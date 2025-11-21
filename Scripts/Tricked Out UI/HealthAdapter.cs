using UnityEngine;
using JacobHomanics.TrickedOutUI;

namespace JacobHomanics.HealthSystem.UI
{
    public class HealthAdapter : BaseVector2Adapter
    {
        public HealthManager health;
        public override float X => health.Current;
        public override float Y => health.Max;
    }
}
