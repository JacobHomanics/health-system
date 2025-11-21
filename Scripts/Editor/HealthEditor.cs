using UnityEngine;
using UnityEditor;

namespace JacobHomanics.HealthSystem.Editor
{
    [CustomEditor(typeof(Health))]
    [CanEditMultipleObjects]
    public class HealthEditor : UnityEditor.Editor
    {
        private Health health;
        private SerializedProperty currentProp;
        private SerializedProperty maxProp;
        private SerializedProperty shieldCurrentProp;
        private SerializedProperty shieldMaxProp;
        private SerializedProperty onCurrentSetProp;
        private SerializedProperty onCurrentChangeProp;
        private SerializedProperty onCurrentDownProp;
        private SerializedProperty onCurrentUpProp;
        private SerializedProperty onCurrentMaxProp;
        private SerializedProperty onCurrentZeroProp;
        private SerializedProperty onMaxSetProp;
        private SerializedProperty onMaxChangeProp;
        private SerializedProperty onMaxDownProp;
        private SerializedProperty onMaxUpProp;
        private SerializedProperty onShieldCurrentSetProp;
        private SerializedProperty onShieldCurrentChangeProp;
        private SerializedProperty onShieldCurrentDownProp;
        private SerializedProperty onShieldCurrentUpProp;
        private SerializedProperty onShieldCurrentMaxProp;
        private SerializedProperty onShieldCurrentZeroProp;
        private SerializedProperty onShieldMaxSetProp;
        private SerializedProperty onShieldMaxChangeProp;
        private SerializedProperty onShieldMaxDownProp;
        private SerializedProperty onShieldMaxUpProp;

        private bool showEvents = true;
        private int selectedEventTab = 0;
        private readonly string[] eventTabNames = { "Current Health", "Max Health", "Current Shield", "Max Shield" };

        private float damageAmount = 1f;
        private float healAmount = 1f;
        private float shieldRestoreAmount = 1f;

        private void OnEnable()
        {
            health = (Health)target;

            currentProp = serializedObject.FindProperty("current");
            maxProp = serializedObject.FindProperty("max");
            shieldCurrentProp = serializedObject.FindProperty("shieldCurrent");
            shieldMaxProp = serializedObject.FindProperty("shieldMax");
            onCurrentSetProp = serializedObject.FindProperty("onCurrentSet");
            onCurrentChangeProp = serializedObject.FindProperty("onCurrentChange");
            onCurrentDownProp = serializedObject.FindProperty("onCurrentDown");
            onCurrentUpProp = serializedObject.FindProperty("onCurrentUp");
            onCurrentMaxProp = serializedObject.FindProperty("onCurrentMax");
            onCurrentZeroProp = serializedObject.FindProperty("onCurrentZero");
            onMaxSetProp = serializedObject.FindProperty("onMaxSet");
            onMaxChangeProp = serializedObject.FindProperty("onMaxChange");
            onMaxDownProp = serializedObject.FindProperty("onMaxDown");
            onMaxUpProp = serializedObject.FindProperty("onMaxUp");
            onShieldCurrentSetProp = serializedObject.FindProperty("onShieldCurrentSet");
            onShieldCurrentChangeProp = serializedObject.FindProperty("onShieldCurrentChange");
            onShieldCurrentDownProp = serializedObject.FindProperty("onShieldCurrentDown");
            onShieldCurrentUpProp = serializedObject.FindProperty("onShieldCurrentUp");
            onShieldCurrentMaxProp = serializedObject.FindProperty("onShieldCurrentMax");
            onShieldCurrentZeroProp = serializedObject.FindProperty("onShieldCurrentZero");
            onShieldMaxSetProp = serializedObject.FindProperty("onShieldMaxSet");
            onShieldMaxChangeProp = serializedObject.FindProperty("onShieldMaxChange");
            onShieldMaxDownProp = serializedObject.FindProperty("onShieldMaxDown");
            onShieldMaxUpProp = serializedObject.FindProperty("onShieldMaxUp");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            // Health Header
            EditorGUILayout.LabelField("Health System", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Health Bar Visualization
            DrawHealthBar();

            // Display health percentage
            float healthPercent = health.Max > 0 ? (health.Current / health.Max) * 100f : 0f;
            EditorGUILayout.LabelField($"Health: {healthPercent:F2}%", EditorStyles.centeredGreyMiniLabel);

            // Display shield percentage if shield exists
            if (health.ShieldMax > 0)
            {
                float shieldPercent = health.ShieldMax > 0 ? (health.ShieldCurrent / health.ShieldMax) * 100f : 0f;
                EditorGUILayout.LabelField($"Shield: {shieldPercent:F2}%", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.Space();

            // Max Health


            // Health Slider (read-only in play mode to show current state)
            EditorGUI.BeginChangeCheck();
            float newCurrent = EditorGUILayout.Slider("Current Health", health.Current, 0, health.Max);
            if (EditorGUI.EndChangeCheck())
            {
                health.Current = newCurrent;
                EditorUtility.SetDirty(health);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(maxProp, new GUIContent("Max Health"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                // Ensure current doesn't exceed new max
                if (health.Current > health.Max)
                {
                    health.Current = health.Max;
                }
            }

            EditorGUILayout.Space();

            // Shield Section
            EditorGUILayout.LabelField("Shield", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Shield Slider
            EditorGUI.BeginChangeCheck();
            float newShieldCurrent = EditorGUILayout.Slider("Current Shield", health.ShieldCurrent, 0, health.ShieldMax);
            if (EditorGUI.EndChangeCheck())
            {
                health.ShieldCurrent = newShieldCurrent;
                EditorUtility.SetDirty(health);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(shieldMaxProp, new GUIContent("Max Shield"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                // Ensure shield current doesn't exceed new max
                if (health.ShieldCurrent > health.ShieldMax)
                {
                    health.ShieldCurrent = health.ShieldMax;
                }
            }

            EditorGUILayout.Space();


            EditorGUILayout.Space(10);

            // Quick Test Actions
            EditorGUILayout.BeginHorizontal();
            damageAmount = EditorGUILayout.FloatField("Damage Amount", damageAmount);
            if (GUILayout.Button("Apply Damage", GUILayout.Height(18), GUILayout.Width(120)))
            {
                health.Damage(damageAmount);
                EditorUtility.SetDirty(health);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            healAmount = EditorGUILayout.FloatField("Heal Amount", healAmount);
            if (GUILayout.Button("Apply Heal", GUILayout.Height(18), GUILayout.Width(120)))
            {
                health.Heal(healAmount);
                EditorUtility.SetDirty(health);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            shieldRestoreAmount = EditorGUILayout.FloatField("Shield Restore", shieldRestoreAmount);
            if (GUILayout.Button("Restore Shield", GUILayout.Height(18), GUILayout.Width(120)))
            {
                health.RestoreShield(shieldRestoreAmount);
                EditorUtility.SetDirty(health);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Set Health to 0", GUILayout.Height(25)))
            {
                health.Current = 0;
                EditorUtility.SetDirty(health);
            }

            if (GUILayout.Button("Set Health to Max", GUILayout.Height(25)))
            {
                health.Current = health.Max;
                EditorUtility.SetDirty(health);
            }

            EditorGUILayout.EndHorizontal();

            if (health.ShieldMax > 0)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Set Shield to 0", GUILayout.Height(25)))
                {
                    health.ShieldCurrent = 0;
                    EditorUtility.SetDirty(health);
                }

                if (GUILayout.Button("Set Shield to Max", GUILayout.Height(25)))
                {
                    health.ShieldCurrent = health.ShieldMax;
                    EditorUtility.SetDirty(health);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Events Section
            // showEvents = EditorGUILayout.Foldout(showEvents, "Unity Events", true);
            // if (showEvents)
            // {
            EditorGUI.indentLevel++;

            // Tab selection
            selectedEventTab = GUILayout.Toolbar(selectedEventTab, eventTabNames);

            EditorGUILayout.Space(5);

            // Display events based on selected tab
            if (selectedEventTab == 0)
            {
                // Current Health Events
                EditorGUILayout.PropertyField(onCurrentSetProp);
                EditorGUILayout.PropertyField(onCurrentChangeProp);
                EditorGUILayout.PropertyField(onCurrentDownProp);
                EditorGUILayout.PropertyField(onCurrentUpProp);
                EditorGUILayout.PropertyField(onCurrentMaxProp);
                EditorGUILayout.PropertyField(onCurrentZeroProp);
            }
            else if (selectedEventTab == 1)
            {
                // Max Health Events
                EditorGUILayout.PropertyField(onMaxSetProp);
                EditorGUILayout.PropertyField(onMaxChangeProp);
                EditorGUILayout.PropertyField(onMaxDownProp);
                EditorGUILayout.PropertyField(onMaxUpProp);
            }
            else if (selectedEventTab == 2)
            {
                // Current Shield Events
                EditorGUILayout.PropertyField(onShieldCurrentSetProp);
                EditorGUILayout.PropertyField(onShieldCurrentChangeProp);
                EditorGUILayout.PropertyField(onShieldCurrentDownProp);
                EditorGUILayout.PropertyField(onShieldCurrentUpProp);
                EditorGUILayout.PropertyField(onShieldCurrentMaxProp);
                EditorGUILayout.PropertyField(onShieldCurrentZeroProp);
            }
            else
            {
                // Max Shield Events
                EditorGUILayout.PropertyField(onShieldMaxSetProp);
                EditorGUILayout.PropertyField(onShieldMaxChangeProp);
                EditorGUILayout.PropertyField(onShieldMaxDownProp);
                EditorGUILayout.PropertyField(onShieldMaxUpProp);
            }

            EditorGUI.indentLevel--;
            // }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHealthBar()
        {
            Rect rect = GUILayoutUtility.GetRect(18, 18, GUILayout.ExpandWidth(true));

            float healthPercent = health.Max > 0 ? health.Current / health.Max : 0;
            healthPercent = Mathf.Clamp01(healthPercent);

            // Background
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, 1f));

            // Health bar (green to red gradient)
            Rect healthRect = new Rect(rect.x, rect.y, rect.width * healthPercent, rect.height);

            // Color gradient: green (high) -> yellow (mid) -> red (low)
            Color healthColor;
            if (healthPercent > 0.5f)
            {
                // Green to yellow
                float t = (healthPercent - 0.5f) * 2f;
                healthColor = Color.Lerp(Color.yellow, Color.green, t);
            }
            else
            {
                // Yellow to red
                float t = healthPercent * 2f;
                healthColor = Color.Lerp(Color.red, Color.yellow, t);
            }

            EditorGUI.DrawRect(healthRect, healthColor);

            // Draw shield overlay if shield exists
            if (health.ShieldMax > 0 && health.ShieldCurrent > 0)
            {
                // Calculate shield as percentage of total (health + shield)
                float totalMax = health.Max + health.ShieldMax;
                float shieldPercent = totalMax > 0 ? health.ShieldCurrent / totalMax : 0;
                shieldPercent = Mathf.Clamp01(shieldPercent);

                // Draw shield bar on top of health (cyan/blue color)
                float shieldWidth = rect.width * shieldPercent;
                Rect shieldRect = new Rect(rect.x + rect.width - shieldWidth, rect.y, shieldWidth, rect.height);

                // Semi-transparent cyan/blue for shield
                Color shieldColor = new Color(0f, 0.7f, 1f, 0.7f);
                EditorGUI.DrawRect(shieldRect, shieldColor);
            }

            // Border
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), Color.black);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 1, rect.width, 1), Color.black);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), Color.black);
            EditorGUI.DrawRect(new Rect(rect.x + rect.width - 1, rect.y, 1, rect.height), Color.black);
        }
    }
}
