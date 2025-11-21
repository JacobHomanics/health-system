using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    public class HealthData
    {
        public HealthData(float current, float max)
        {
            this.current = current;
            this.max = max;
        }

        [SerializeField] private float current;
        public float Current
        {
            get
            {
                return current;
            }
            set
            {
                current = Mathf.Clamp(value, 0, Max);
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
                max = Mathf.Max(0, value);

                // Clamp current to new max value
                if (current > max)
                {
                    Current = max;
                }
            }
        }
    }

    public class Health : MonoBehaviour
    {
        [SerializeField] private List<HealthData> healths = new();
        [SerializeField] private List<Shield> shields = new();
        // [SerializeField] private int currentHealthIndex = 0;

        public List<HealthData> Healths
        {
            get
            {
                if (healths == null)
                    healths = new List<HealthData>();
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
                    currentTotal += health.Current;
                }

                float desiredTotal = Mathf.Clamp(value, 0, Max);
                float difference = desiredTotal - currentTotal;

                if (difference > 0)
                {
                    // Increase health - fill sequentially from last to first (without affecting shields)
                    float remainingHeal = difference;
                    for (int i = Healths.Count - 1; i >= 0 && remainingHeal > 0; i--)
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
                else if (difference < 0)
                {
                    // Decrease health - remove sequentially from first to last (without affecting shields)
                    float remainingDamage = -difference;
                    for (int i = 0; i < Healths.Count && remainingDamage > 0; i++)
                    {

                        float healthDamage = Mathf.Min(Healths[i].Current, remainingDamage);
                        Healths[i].Current -= healthDamage;
                        remainingDamage -= healthDamage;

                    }
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
            set
            {
                // Calculate current total max
                float currentTotalMax = 0f;
                foreach (var health in healths)
                {
                    currentTotalMax += health.Max;
                }

                float desiredTotal = Mathf.Max(0, value);
                float difference = desiredTotal - currentTotalMax;

                if (difference > 0)
                {
                    // Increase max - fill sequentially from last to first
                    float remaining = difference;
                    for (int i = healths.Count - 1; i >= 0 && remaining > 0; i--)
                    {
                        healths[i].Max += remaining;
                        remaining = 0;
                        break; // Only fill the last health
                    }
                }
                else if (difference < 0)
                {
                    // Decrease max - remove sequentially from last to first
                    float remaining = -difference;
                    for (int i = healths.Count - 1; i >= 0 && remaining > 0; i--)
                    {
                        if (healths[i] != null && remaining > 0)
                        {
                            float reduction = Mathf.Min(healths[i].Max, remaining);
                            healths[i].Max -= reduction;
                            remaining -= reduction;
                        }
                    }
                }
            }
        }

        public float Shield
        {
            get
            {
                float total = 0f;
                foreach (var shield in Shields)
                {
                    total += Mathf.Max(0, shield.value);
                }
                return total;
            }
            set
            {
                // Calculate current total directly to avoid recursion
                float currentTotal = 0f;
                foreach (var shield in shields)
                {
                    currentTotal += shield.value;
                }

                float desiredTotal = Mathf.Max(0, value);
                float difference = desiredTotal - currentTotal;

                if (difference > 0)
                {
                    // Increase shield - fill sequentially from last to first
                    float remainingHeal = difference;

                    Shields[0].value += remainingHeal;
                }
                else if (difference < 0)
                {
                    // Decrease health - remove sequentially from first to last
                    float remainingDamage = -difference;
                    for (int i = 0; i < Shields.Count && remainingDamage > 0; i++)
                    {
                        float healthDamage = Mathf.Min(Shields[i].value, remainingDamage);
                        Shields[i].value -= healthDamage;
                        remainingDamage -= healthDamage;
                    }
                }
            }
        }

        public void Damage(float amount)
        {
            float remainingDamage = amount;

            // Apply damage to shields in order
            for (int i = 0; i < Shields.Count && remainingDamage > 0; i++)
            {
                if (Shields[i].value > 0)
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
                    float healthDamage = Mathf.Min(Healths[i].Current, remainingDamage);
                    Healths[i].Current -= healthDamage;
                    remainingDamage -= healthDamage;
                }
            }
        }

        public void Heal(float amount)
        {
            float remainingHeal = amount;

            // Heal healths sequentially (from last to first)
            for (int i = Healths.Count - 1; i >= 0 && remainingHeal > 0; i--)
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

        public void RestoreShield(float amount)
        {
            if (Shields.Count == 0)
            {
                Shields.Add(new Shield(0f));
            }


            Shields[0].value += amount;
        }


        void Reset()
        {
            healths.Add(new HealthData(100, 100));

            shields.Add(new Shield(0));
        }

    }
}

