using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace JacobHomanics.HealthSystem
{
    [System.Serializable]
    public class Shield
    {
        public float value;
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

    public class Health : MonoBehaviour
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

        [SerializeField] private List<Shield> shields = new List<Shield>();

        public List<Shield> Shields
        {
            get
            {
                if (shields == null)
                    shields = new List<Shield>();
                return shields;
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
                        total += shield.value;
                }
                return total;
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

        public UnityEvent onShieldChanged;

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

                    // Remove empty shields
                    if (Shields[i].value <= 0)
                    {
                        Shields.RemoveAt(i);
                        i--; // Adjust index after removal
                    }
                }
            }

            // Clean up any zero or negative shields
            for (int i = Shields.Count - 1; i >= 0; i--)
            {
                if (Shields[i] == null || Shields[i].value <= 0)
                {
                    Shields.RemoveAt(i);
                }
            }

            // Apply remaining damage to health
            if (remainingDamage > 0)
            {
                Current -= remainingDamage;
            }

            onShieldChanged?.Invoke();
        }

        public void Heal(float amount)
        {
            Current += amount;
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

        public void AddShield(float amount)
        {
            Shields.Add(new Shield(amount));
            onShieldChanged?.Invoke();
        }

        public void AddShield(float amount, Color color)
        {
            Shields.Add(new Shield(amount, color));
            onShieldChanged?.Invoke();
        }

        public void RemoveShield(int index)
        {
            if (index >= 0 && index < Shields.Count)
            {
                Shields.RemoveAt(index);
                onShieldChanged?.Invoke();
            }
        }
    }

}

