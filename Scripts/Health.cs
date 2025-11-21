using UnityEngine;
using UnityEngine.Events;

namespace JacobHomanics.HealthSystem
{

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

        [SerializeField] private float shieldCurrent;
        public float ShieldCurrent
        {
            get
            {
                return shieldCurrent;
            }
            set
            {
                var previous = shieldCurrent;
                shieldCurrent = Mathf.Max(0, value);
                onShieldCurrentSet?.Invoke();

                if (shieldCurrent != previous)
                    onShieldCurrentChange?.Invoke(shieldCurrent);

                if (shieldCurrent < previous)
                    onShieldCurrentDown?.Invoke();

                if (shieldCurrent > previous)
                    onShieldCurrentUp?.Invoke();

                if (shieldCurrent == 0)
                    onShieldCurrentZero?.Invoke();
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

        public UnityEvent onShieldCurrentSet;
        public UnityEvent<float> onShieldCurrentChange;

        public UnityEvent onShieldCurrentDown;
        public UnityEvent onShieldCurrentUp;
        public UnityEvent onShieldCurrentZero;

        public void Damage(float amount)
        {
            if (ShieldCurrent > 0)
            {
                float remainingDamage = amount - ShieldCurrent;
                ShieldCurrent -= amount;

                if (remainingDamage > 0)
                {
                    Current -= remainingDamage;
                }
            }
            else
            {
                Current -= amount;
            }
        }

        public void Heal(float amount)
        {
            Current += amount;
        }

        public void RestoreShield(float amount)
        {
            ShieldCurrent += amount;
        }
    }

}
