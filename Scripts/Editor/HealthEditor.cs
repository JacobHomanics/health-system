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
        private SerializedProperty shieldsProp;
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
        private SerializedProperty onShieldChangedProp;

        private bool showEvents = true;
        private int selectedEventTab = 0;
        private readonly string[] eventTabNames = { "Current Health", "Max Health", "Shield" };

        private float damageAmount = 1f;
        private float healAmount = 1f;
        private float shieldRestoreAmount = 1f;
        private Color newShieldColor = new Color(0f, 0.7f, 1f, 0.7f);

        private void OnEnable()
        {
            health = (Health)target;

            currentProp = serializedObject.FindProperty("current");
            maxProp = serializedObject.FindProperty("max");
            shieldsProp = serializedObject.FindProperty("shields");
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
            onShieldChangedProp = serializedObject.FindProperty("onShieldChanged");
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

            // Display shield total if shields exist
            if (health.ShieldTotal > 0)
            {
                EditorGUILayout.LabelField($"Shield Total: {health.ShieldTotal:F2} ({health.Shields.Count} shields)", EditorStyles.centeredGreyMiniLabel);
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
            EditorGUILayout.LabelField("Shields", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Shield List
            if (shieldsProp != null)
            {
                EditorGUILayout.PropertyField(shieldsProp, new GUIContent("Shield List"), true);
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
            else
            {
                // Shield Events
                EditorGUILayout.PropertyField(onShieldChangedProp);
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

            // Draw shield overlays if shields exist
            if (health.ShieldTotal > 0)
            {
                // Calculate shield as percentage of total (health + shield)
                // Since shield has no max, we'll use a visual representation based on health max
                float totalValue = health.Max + health.ShieldTotal;
                float currentX = rect.x + rect.width;

                // Draw each shield from right to left, stacked
                for (int i = health.Shields.Count - 1; i >= 0; i--)
                {
                    if (health.Shields[i] != null && health.Shields[i].value > 0)
                    {
                        float shieldPercent = totalValue > 0 ? health.Shields[i].value / totalValue : 0;
                        shieldPercent = Mathf.Clamp01(shieldPercent);

                        float shieldWidth = rect.width * shieldPercent;
                        Rect shieldRect = new Rect(currentX - shieldWidth, rect.y, shieldWidth, rect.height);

                        // Use the shield's own color
                        EditorGUI.DrawRect(shieldRect, health.Shields[i].color);

                        currentX -= shieldWidth;
                    }
                }
            }

            // Border
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), Color.black);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 1, rect.width, 1), Color.black);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), Color.black);
            EditorGUI.DrawRect(new Rect(rect.x + rect.width - 1, rect.y, 1, rect.height), Color.black);
        }
    }
}
