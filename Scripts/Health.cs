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
                    onCurrentChange?.Invoke();

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
        public UnityEvent onCurrentChange;

        public UnityEvent onCurrentDown;
        public UnityEvent onCurrentUp;
        public UnityEvent onCurrentMax;
        public UnityEvent onCurrentZero;


        public UnityEvent onMaxSet;
        public UnityEvent onMaxChange;

        public UnityEvent onMaxDown;
        public UnityEvent onMaxUp;

        [ContextMenu("-- Current Health --")]
        private void CurrentHealthHeader() { }

        [ContextMenu("Damage 1")]
        private void Take1Damage()
        {
            Current -= 1;
        }

        [ContextMenu("Damage 10")]
        private void Take10Damage()
        {
            Current -= 10;
        }

        [ContextMenu("Heal 1")]
        private void Heal1()
        {
            Current += 1;
        }

        [ContextMenu("Heal 10")]
        private void Heal10()
        {
            Current += 10;
        }
    }

}
