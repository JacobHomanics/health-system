using UnityEngine;
using UnityEditor;

namespace JacobHomanics.HealthSystem.Editor
{
    [CustomEditor(typeof(Health))]
    [CanEditMultipleObjects]
    public class HealthManagerEditor : UnityEditor.Editor
    {
        private Health healthManager;
        private SerializedProperty healthsProp;
        private SerializedProperty shieldsProp;
        private SerializedProperty onShieldChangedProp;
        private SerializedProperty onCurrentSetProp;
        private SerializedProperty onCurrentChangeProp;
        private SerializedProperty onCurrentDownProp;
        private SerializedProperty onCurrentUpProp;
        private SerializedProperty onCurrentMaxProp;
        private SerializedProperty onCurrentZeroProp;

        private bool showEvents = true;
        private int selectedEventTab = 0;
        private readonly string[] eventTabNames = { "Current Health", "Shield" };

        private float damageAmount = 1f;
        private float healAmount = 1f;
        private float shieldRestoreAmount = 1f;
        private Color newShieldColor = new Color(0f, 0.7f, 1f, 0.7f);

        private void OnEnable()
        {
            healthManager = (Health)target;

            healthsProp = serializedObject.FindProperty("healths");
            shieldsProp = serializedObject.FindProperty("shields");
            onShieldChangedProp = serializedObject.FindProperty("onShieldChanged");
            onCurrentSetProp = serializedObject.FindProperty("onCurrentSet");
            onCurrentChangeProp = serializedObject.FindProperty("onCurrentChange");
            onCurrentDownProp = serializedObject.FindProperty("onCurrentDown");
            onCurrentUpProp = serializedObject.FindProperty("onCurrentUp");
            onCurrentMaxProp = serializedObject.FindProperty("onCurrentMax");
            onCurrentZeroProp = serializedObject.FindProperty("onCurrentZero");
        }

        private int GetCurrentHealthIndex()
        {
            // Find the first health that still has health > 0, starting from the beginning
            for (int i = 0; i < healthManager.Healths.Count; i++)
            {
                if (healthManager.Healths[i] != null && healthManager.Healths[i].Current > 0)
                {
                    return i;
                }
            }
            // If no health has health, return the first index (or -1 if list is empty)
            return healthManager.Healths.Count > 0 ? 0 : -1;
        }

        private HealthData GetCurrentHealth()
        {
            int index = GetCurrentHealthIndex();
            if (index >= 0 && index < healthManager.Healths.Count)
            {
                return healthManager.Healths[index];
            }
            return null;
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


            // Display total health percentage
            float totalHealth = healthManager.Current;
            float totalMax = healthManager.Max;
            float healthPercent = totalMax > 0 ? (totalHealth / totalMax) * 100f : 0f;
            EditorGUILayout.LabelField($"Total: {totalHealth:F2} / {totalMax:F2} ({healthPercent:F2}%)", EditorStyles.centeredGreyMiniLabel);

            // Display shield total if shields exist
            if (healthManager.ShieldTotal > 0)
            {
                EditorGUILayout.LabelField($"Shield Total: {healthManager.ShieldTotal:F2} ({healthManager.Shields.Count} shields)", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.Space();

            EditorGUILayout.Space();
            GUIStyle centeredStyle = new GUIStyle(EditorStyles.label);
            centeredStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("Current Health", centeredStyle);

            // Editable Current Health Field
            EditorGUI.BeginChangeCheck();
            float newCurrent = EditorGUILayout.Slider(healthManager.Current, 0, healthManager.Max);
            if (EditorGUI.EndChangeCheck())
            {
                healthManager.Current = newCurrent;
                EditorUtility.SetDirty(healthManager);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Max Health", centeredStyle);

            // Editable Max Health Field
            EditorGUI.BeginChangeCheck();
            float newMax = EditorGUILayout.FloatField(healthManager.Max);
            if (EditorGUI.EndChangeCheck())
            {
                healthManager.Max = newMax;
                EditorUtility.SetDirty(healthManager);
            }



            // Health List
            if (healthsProp != null)
            {
                if (healthsProp.arraySize > 1)
                    EditorGUILayout.PropertyField(healthsProp, new GUIContent("Healths"), true);
            }

            EditorGUILayout.Space();

            // Shield List
            if (shieldsProp != null)
            {
                EditorGUILayout.PropertyField(shieldsProp, new GUIContent("Shields"), true);
            }


            EditorGUILayout.Space();


            EditorGUILayout.Space(10);

            // Quick Test Actions
            EditorGUILayout.BeginHorizontal();
            damageAmount = EditorGUILayout.FloatField("Damage Amount", damageAmount);
            if (GUILayout.Button("Apply Damage", GUILayout.Height(18), GUILayout.Width(120)))
            {
                healthManager.Current -= damageAmount;
                EditorUtility.SetDirty(healthManager);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            healAmount = EditorGUILayout.FloatField("Heal Amount", healAmount);
            if (GUILayout.Button("Apply Heal", GUILayout.Height(18), GUILayout.Width(120)))
            {
                healthManager.Current += healAmount;
                EditorUtility.SetDirty(healthManager);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            shieldRestoreAmount = EditorGUILayout.FloatField("Shield Amount", shieldRestoreAmount);
            if (GUILayout.Button("Restore Shield", GUILayout.Height(18), GUILayout.Width(120)))
            {
                healthManager.RestoreShield(shieldRestoreAmount);
                EditorUtility.SetDirty(healthManager);
            }
            EditorGUILayout.EndHorizontal();



            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Set Current Health to 0", GUILayout.Height(25)))
            {
                healthManager.Current = 0;
                EditorUtility.SetDirty(healthManager);
            }

            if (GUILayout.Button("Set Current Health to Max", GUILayout.Height(25)))
            {
                healthManager.Current = healthManager.Max;
                EditorUtility.SetDirty(healthManager);
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
                // Current Health Events (from HealthManager)
                EditorGUILayout.PropertyField(onCurrentSetProp);
                EditorGUILayout.PropertyField(onCurrentChangeProp);
                EditorGUILayout.PropertyField(onCurrentDownProp);
                EditorGUILayout.PropertyField(onCurrentUpProp);
                EditorGUILayout.PropertyField(onCurrentMaxProp);
                EditorGUILayout.PropertyField(onCurrentZeroProp);
            }
            else
            {
                // Shield Events
                EditorGUILayout.PropertyField(onShieldChangedProp);
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            if (healthManager.Healths.Count == 1)
            {
                if (GUILayout.Button("Enable Multi-Health Mode", GUILayout.Height(25)))
                {
                    healthManager.Healths.Add(new HealthData(100, 100));
                    EditorUtility.SetDirty(healthManager);
                }
            }

            if (GUILayout.Button("Enable Multi-Shield Mode", GUILayout.Height(25)))
            {
                healthManager.Shields.Add(new Shield(100));
                EditorUtility.SetDirty(healthManager);
            }




            // }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHealthBar()
        {
            Rect rect = GUILayoutUtility.GetRect(18, 18, GUILayout.ExpandWidth(true));

            float totalHealth = healthManager.Current;
            float totalMax = healthManager.Max;
            float healthPercent = totalMax > 0 ? totalHealth / totalMax : 0;
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
            if (healthManager.ShieldTotal > 0)
            {
                // Calculate shield as percentage of total (health + shield)
                // Since shield has no max, we'll use a visual representation based on health max
                float totalValue = totalMax + healthManager.ShieldTotal;
                float currentX = rect.x + rect.width;

                // Draw each shield from right to left, stacked
                for (int i = healthManager.Shields.Count - 1; i >= 0; i--)
                {
                    if (healthManager.Shields[i] != null && healthManager.Shields[i].value > 0)
                    {
                        float shieldPercent = totalValue > 0 ? healthManager.Shields[i].value / totalValue : 0;
                        shieldPercent = Mathf.Clamp01(shieldPercent);

                        float shieldWidth = rect.width * shieldPercent;
                        Rect shieldRect = new Rect(currentX - shieldWidth, rect.y, shieldWidth, rect.height);

                        // Use the shield's own color
                        EditorGUI.DrawRect(shieldRect, healthManager.Shields[i].color);

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
