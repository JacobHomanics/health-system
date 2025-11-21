using UnityEngine;
using UnityEditor;
using JacobHomanics.TrickedOutUI;
using JacobHomanics.TrickedOutUI.Editor;

namespace JacobHomanics.HealthSystem.UI
{
    [CustomEditor(typeof(TMP_TextHealthComponent))]
    public class TMP_TextHealthComponentEditor : TMP_TextVector2ComponentEditor
    {
        protected override string XValueComponentName { get => "Current Health"; }
        protected override string YValueComponentName { get => "Max Health"; }

        protected override string ClampAtYComponentName { get => "Clamp At Max Health"; }
    }
}
