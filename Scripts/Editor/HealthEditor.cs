using UnityEngine;
using UnityEditor;
using JacobHomanics.HealthSystem;

namespace JacobHomanics.HealthSystem.Editor
{
    [CustomEditor(typeof(Health))]
    [CanEditMultipleObjects]
    public class HealthEditor : UnityEditor.Editor
    {
        private Health health;
        private SerializedProperty currentProp;
        private SerializedProperty maxProp;
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

        private bool showEvents = true;
        private int selectedEventTab = 0;
        private readonly string[] eventTabNames = { "Current Health", "Max Health" };

        private void OnEnable()
        {
            health = (Health)target;

            currentProp = serializedObject.FindProperty("current");
            maxProp = serializedObject.FindProperty("max");
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
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            // Health Header
            EditorGUILayout.LabelField("Health System", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // EditorGUILayout.LabelField($"{health.Current:F1} / {health.Max:F1}", EditorStyles.centeredGreyMiniLabel);
            // Health Bar Visualization
            DrawHealthBar();

            EditorGUILayout.Space();

            // Max Health
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

            // EditorGUILayout.Space();

            // Current Health (read-only display with slider)
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            // EditorGUILayout.PropertyField(currentProp, new GUIContent("Current Health"));
            EditorGUI.EndDisabledGroup();

            // Health Slider (read-only in play mode to show current state)
            EditorGUI.BeginChangeCheck();
            float newCurrent = EditorGUILayout.Slider("Current Health", health.Current, 0, health.Max);
            if (EditorGUI.EndChangeCheck())
            {
                health.Current = newCurrent;
                EditorUtility.SetDirty(health);
            }

            EditorGUILayout.Space();


            EditorGUILayout.Space(10);

            // Quick Test Actions
            // if (Application.isPlaying)
            // {
            // EditorGUILayout.LabelField("Quick Test Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Damage 1", GUILayout.Height(25)))
            {
                health.Current -= 1;
                EditorUtility.SetDirty(health);
            }

            if (GUILayout.Button("Damage 10", GUILayout.Height(25)))
            {
                health.Current -= 10;
                EditorUtility.SetDirty(health);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Heal 1", GUILayout.Height(25)))
            {
                health.Current += 1;
                EditorUtility.SetDirty(health);
            }

            if (GUILayout.Button("Heal 10", GUILayout.Height(25)))
            {
                health.Current += 10;
                EditorUtility.SetDirty(health);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Set to 0", GUILayout.Height(25)))
            {
                health.Current = 0;
                EditorUtility.SetDirty(health);
            }

            if (GUILayout.Button("Set to Max", GUILayout.Height(25)))
            {
                health.Current = health.Max;
                EditorUtility.SetDirty(health);
            }

            EditorGUILayout.EndHorizontal();
            // }
            // else
            // {
            //     // EditorGUILayout.HelpBox("Quick test actions are available in Play Mode", MessageType.Info);
            // }

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
            else
            {
                // Max Health Events
                EditorGUILayout.PropertyField(onMaxSetProp);
                EditorGUILayout.PropertyField(onMaxChangeProp);
                EditorGUILayout.PropertyField(onMaxDownProp);
                EditorGUILayout.PropertyField(onMaxUpProp);
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

            // Border
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), Color.black);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 1, rect.width, 1), Color.black);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), Color.black);
            EditorGUI.DrawRect(new Rect(rect.x + rect.width - 1, rect.y, 1, rect.height), Color.black);
        }
    }
}

