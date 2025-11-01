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





        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
