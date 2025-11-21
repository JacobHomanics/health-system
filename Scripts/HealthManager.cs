using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace JacobHomanics.HealthSystem
{
    [System.Serializable]
    public class Shield
    {
        [SerializeField] private float _value;
        public float value
        {
            get => _value;
            set => _value = Mathf.Max(0, value);
        }
        public Color color = new Color(0f, 0.7f, 1f, 0.7f); // Default cyan/blue

        public Shield(float value, Color color)
        {
            this.value = value;
            this.color = color;
        }

        public Shield(float value)
        {
            this.value = value;
            this.color = new Color(0f, 0.7f, 1f, 0.7f);
        }
    }

    [System.Serializable]
    public class Health
    {
        [SerializeField] private float current;
        public float Current
        {
            get
            {
                return current;
            }
            set
            {
                var previous = current;
                current = Mathf.Clamp(value, 0, Max);
                onCurrentSet?.Invoke();

                if (current != previous)
                    onCurrentChange?.Invoke(current);

                if (current < previous)
                    onCurrentDown?.Invoke();

                if (current > previous)
                    onCurrentUp?.Invoke();

                if (current == 0)
                    onCurrentZero?.Invoke();

                if (current == Max)
                    onCurrentMax?.Invoke();
            }
        }

        [SerializeField] private float max;
        public float Max
        {
            get
            {
                return max;
            }
            set
            {
                max = value;
            }
        }

        public UnityEvent onCurrentSet;
        public UnityEvent<float> onCurrentChange;

        public UnityEvent onCurrentDown;
        public UnityEvent onCurrentUp;
        public UnityEvent onCurrentMax;
        public UnityEvent onCurrentZero;


        public UnityEvent onMaxSet;
        public UnityEvent onMaxChange;

        public UnityEvent onMaxDown;
        public UnityEvent onMaxUp;
    }

    public class HealthManager : MonoBehaviour
    {
        [SerializeField] private List<Health> healths = new List<Health>();
        [SerializeField] private List<Shield> shields = new List<Shield>();
        // [SerializeField] private int currentHealthIndex = 0;

        public List<Health> Healths
        {
            get
            {
                if (healths == null)
                    healths = new List<Health>();
                return healths;
            }
        }

        public List<Shield> Shields
        {
            get
            {
                if (shields == null)
                    shields = new List<Shield>();
                return shields;
            }
        }

        // public int CurrentHealthIndex
        // {
        //     get => currentHealthIndex;
        //     set
        //     {
        //         if (value >= 0 && value < Healths.Count)
        //         {
        //             currentHealthIndex = value;
        //         }
        //     }
        // }

        public float Current
        {
            get
            {
                float total = 0f;
                foreach (var health in healths)
                {
                    total += health.Current;
                }
                return total;
            }
            set
            {
                // Calculate current total directly to avoid recursion
                float currentTotal = 0f;
                foreach (var health in healths)
                {
                    if (health != null)
                        currentTotal += health.Current;
                }

                float desiredTotal = Mathf.Clamp(value, 0, Max);
                float difference = desiredTotal - currentTotal;

                if (difference > 0)
                {
                    // Increase health - use Heal method (last to first)
                    Heal(difference);
                }
                else if (difference < 0)
                {
                    // Decrease health - use Damage method (first to last)
                    Damage(-difference);
                }
            }
        }

        public float Max
        {
            get
            {
                float total = 0f;
                foreach (var health in healths)
                {
                    total += health.Max;
                }
                return total;
            }
        }

        public float ShieldTotal
        {
            get
            {
                float total = 0f;
                foreach (var shield in Shields)
                {
                    if (shield != null)
                        total += Mathf.Max(0, shield.value);
                }
                return total;
            }
        }

        public UnityEvent onShieldChanged;

        private int GetCurrentHealthIndex()
        {
            // Find the first health that still has health > 0, starting from the beginning
            for (int i = 0; i < Healths.Count; i++)
            {
                if (Healths[i] != null && Healths[i].Current > 0)
                {
                    return i;
                }
            }
            // If no health has health, return the first index (or -1 if list is empty)
            return Healths.Count > 0 ? 0 : -1;
        }

        public void Damage(float amount)
        {
            float remainingDamage = amount;

            // Apply damage to shields in order
            for (int i = 0; i < Shields.Count && remainingDamage > 0; i++)
            {
                if (Shields[i] != null && Shields[i].value > 0)
                {
                    float shieldDamage = Mathf.Min(Shields[i].value, remainingDamage);
                    Shields[i].value -= shieldDamage;
                    remainingDamage -= shieldDamage;
                }
            }

            // Apply remaining damage to healths sequentially, starting from the first health
            if (remainingDamage > 0)
            {
                for (int i = 0; i < Healths.Count && remainingDamage > 0; i++)
                {
                    if (Healths[i] != null)
                    {
                        float healthDamage = Mathf.Min(Healths[i].Current, remainingDamage);
                        Healths[i].Current -= healthDamage;
                        remainingDamage -= healthDamage;
                    }
                }
            }

            onShieldChanged?.Invoke();
        }

        public void Heal(float amount)
        {
            float remainingHeal = amount;

            // Heal healths sequentially (from last to first)
            for (int i = Healths.Count - 1; i >= 0 && remainingHeal > 0; i--)
            {
                if (Healths[i] != null)
                {
                    float missingHealth = Healths[i].Max - Healths[i].Current;
                    if (missingHealth > 0)
                    {
                        float healAmount = Mathf.Min(missingHealth, remainingHeal);
                        Healths[i].Current += healAmount;
                        remainingHeal -= healAmount;
                    }
                }
            }
        }

        public void RestoreShield(float amount)
        {
            if (Shields.Count == 0)
            {
                Shields.Add(new Shield(0f));
            }
            Shields[0].value += amount;
            onShieldChanged?.Invoke();
        }
    }

}

