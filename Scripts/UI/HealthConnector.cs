using UnityEngine;
using JacobHomanics.UI;

namespace JacobHomanics.HealthSystem.UI
{
    public class HealthConnector : BaseCurrentMaxConnector
    {
        public Health health;
        public override float CurrentNum => health.Current;
        public override float MaxNum => health.Max;
    }
}
