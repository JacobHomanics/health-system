using UnityEngine;
using JacobHomanics.TrickedOutUI;

namespace JacobHomanics.HealthSystem.UI
{
    public class HealthConnector : BaseCurrentMaxConnector
    {
        public Health health;
        public override float CurrentNum => health.Current;
        public override float MaxNum => health.Max;
    }
}
