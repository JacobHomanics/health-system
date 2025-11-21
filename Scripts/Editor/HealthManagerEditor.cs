using UnityEngine;
using UnityEditor;

namespace JacobHomanics.HealthSystem.Editor
{
    [CustomEditor(typeof(Health))]
    [CanEditMultipleObjects]
    public class HealthManagerEditor : UnityEditor.Editor
    {
        private Health health;
        private SerializedProperty healthsProp;
        private SerializedProperty shieldsProp;
        private SerializedProperty onShieldChangedProp;
        private SerializedProperty onCurrentSetProp;
        private SerializedProperty onCurrentChangeProp;
        private SerializedProperty onCurrentDownProp;
        private SerializedProperty onCurrentUpProp;
        private SerializedProperty onCurrentMaxProp;
        private SerializedProperty onCurrentZeroProp;

        private int selectedMainTab = 0;
        private readonly string[] mainTabNames = { "System", "Health", "Shield" };

        private float damageAmount = 1f;
        private float healAmount = 1f;
        private float shieldRestoreAmount = 1f;
        private float shieldDamageAmount = 1f;
        private Color newShieldColor = new Color(0f, 0.7f, 1f, 0.7f);

        private void OnEnable()
        {
            health = (Health)target;

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
            for (int i = 0; i < health.Healths.Count; i++)
            {
                if (health.Healths[i] != null && health.Healths[i].Current > 0)
                {
                    return i;
                }
            }
            // If no health has health, return the first index (or -1 if list is empty)
            return health.Healths.Count > 0 ? 0 : -1;
        }

        private HealthData GetCurrentHealth()
        {
            int index = GetCurrentHealthIndex();
            if (index >= 0 && index < health.Healths.Count)
            {
                return health.Healths[index];
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

            // Health Bar Visualization (always visible)
            DrawHealthBar();

            // Display total health percentage
            float totalHealth = health.Current;
            float totalMax = health.Max;
            float healthPercent = totalMax > 0 ? (totalHealth / totalMax) * 100f : 0f;
            EditorGUILayout.LabelField($"Total: {totalHealth:F2} / {totalMax:F2} ({healthPercent:F2}%)", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.Space(10);

            // Main Tab Selection
            selectedMainTab = GUILayout.Toolbar(selectedMainTab, mainTabNames);

            EditorGUILayout.Space(5);

            // Display content based on selected main tab
            if (selectedMainTab == 0)
            {
                // System Tab
                DrawSystemTab();
            }
            else if (selectedMainTab == 1)
            {
                // Health Tab
                DrawHealthTab();
            }
            else
            {
                // Shield Tab
                DrawShieldTab();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSystemTab()
        {
            EditorGUILayout.LabelField("System Overview", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            damageAmount = EditorGUILayout.FloatField("Damage", damageAmount);
            if (GUILayout.Button("Apply", GUILayout.Height(18), GUILayout.Width(120)))
            {
                health.Damage(damageAmount);
                EditorUtility.SetDirty(health);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            healAmount = EditorGUILayout.FloatField("Heal", healAmount);
            if (GUILayout.Button("Apply", GUILayout.Height(18), GUILayout.Width(120)))
            {
                health.Heal(healAmount);
                EditorUtility.SetDirty(health);
            }
            EditorGUILayout.EndHorizontal();
            // System Statistics
            // GUIStyle centeredStyle = new GUIStyle(EditorStyles.label);
            // centeredStyle.alignment = TextAnchor.MiddleCenter;

            // EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
            // EditorGUI.indentLevel++;

            // float totalHealth = health.Current;
            // float totalMax = health.Max;
            // float healthPercent = totalMax > 0 ? (totalHealth / totalMax) * 100f : 0f;
            // float totalShield = health.Shield;

            // EditorGUILayout.LabelField($"Total Health: {totalHealth:F2} / {totalMax:F2} ({healthPercent:F2}%)");
            // EditorGUILayout.LabelField($"Total Shield: {totalShield:F2}");
            // EditorGUILayout.LabelField($"Health Count: {health.Healths.Count}");
            // EditorGUILayout.LabelField($"Shield Count: {health.Shields.Count}");

            // EditorGUI.indentLevel--;

            // EditorGUILayout.Space(10);

            // // System Status
            // EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            // EditorGUI.indentLevel++;

            // string healthStatus = totalHealth <= 0 ? "Dead" : (totalHealth >= totalMax ? "Full Health" : "Injured");
            // string shieldStatus = totalShield > 0 ? "Active" : "None";

            // EditorGUILayout.LabelField($"Health Status: {healthStatus}");
            // EditorGUILayout.LabelField($"Shield Status: {shieldStatus}");

            // EditorGUI.indentLevel--;

            // EditorGUILayout.Space(10);

            // // Quick Actions
            // EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            // EditorGUILayout.BeginHorizontal();
            // if (GUILayout.Button("Reset to Full Health", GUILayout.Height(25)))
            // {
            //     health.Current = health.Max;
            //     EditorUtility.SetDirty(health);
            // }

            // if (GUILayout.Button("Reset All", GUILayout.Height(25)))
            // {
            //     health.Current = health.Max;
            //     health.Shield = 0;
            //     EditorUtility.SetDirty(health);
            // }
            // EditorGUILayout.EndHorizontal();

            // EditorGUILayout.Space(5);

            // EditorGUILayout.BeginHorizontal();
            // if (GUILayout.Button("Kill (Set to 0)", GUILayout.Height(25)))
            // {
            //     health.Current = 0;
            //     health.Shield = 0;
            //     EditorUtility.SetDirty(health);
            // }
            // EditorGUILayout.EndHorizontal();
        }

        private void DrawHealthTab()
        {
            GUIStyle centeredStyle = new GUIStyle(EditorStyles.label);
            centeredStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("Current Health", centeredStyle);

            // Editable Current Health Field
            EditorGUI.BeginChangeCheck();
            float newCurrent = EditorGUILayout.Slider(health.Current, 0, health.Max);
            if (EditorGUI.EndChangeCheck())
            {
                health.Current = newCurrent;
                EditorUtility.SetDirty(health);
            }

            EditorGUILayout.Space();

            // Editable Max Health Field
            EditorGUI.BeginChangeCheck();
            float newMax = EditorGUILayout.FloatField("Max Health", health.Max);
            if (EditorGUI.EndChangeCheck())
            {
                health.Max = newMax;
                EditorUtility.SetDirty(health);
            }

            EditorGUILayout.Space();

            // Health List
            if (healthsProp != null)
            {
                if (healthsProp.arraySize > 1)
                    EditorGUILayout.PropertyField(healthsProp, new GUIContent("Healths"), true);
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            damageAmount = EditorGUILayout.FloatField("Damage", damageAmount);
            if (GUILayout.Button("Apply", GUILayout.Height(18), GUILayout.Width(120)))
            {
                health.Current -= damageAmount;
                EditorUtility.SetDirty(health);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            healAmount = EditorGUILayout.FloatField("Heal", healAmount);
            if (GUILayout.Button("Apply", GUILayout.Height(18), GUILayout.Width(120)))
            {
                health.Current += healAmount;
                EditorUtility.SetDirty(health);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Set Current Health to 0", GUILayout.Height(25)))
            {
                health.Current = 0;
                EditorUtility.SetDirty(health);
            }

            if (GUILayout.Button("Set Current Health to Max", GUILayout.Height(25)))
            {
                health.Current = health.Max;
                EditorUtility.SetDirty(health);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(onCurrentSetProp);
            EditorGUILayout.PropertyField(onCurrentChangeProp);
            EditorGUILayout.PropertyField(onCurrentDownProp);
            EditorGUILayout.PropertyField(onCurrentUpProp);
            EditorGUILayout.PropertyField(onCurrentMaxProp);
            EditorGUILayout.PropertyField(onCurrentZeroProp);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            if (health.Healths.Count == 1)
            {
                if (GUILayout.Button("Enable Multi-Health Mode", GUILayout.Height(25)))
                {
                    health.Healths.Add(new HealthData(100, 100));
                    EditorUtility.SetDirty(health);
                }
            }
            else
            {
                if (GUILayout.Button("Disable Multi-Health Mode", GUILayout.Height(25)))
                {
                    for (int i = health.Healths.Count - 1; i >= 1; i--)
                    {
                        health.Healths.Remove(health.Healths[i]);
                    }
                    EditorUtility.SetDirty(health);
                }
            }
        }

        private void DrawShieldTab()
        {
            GUIStyle centeredStyle = new GUIStyle(EditorStyles.label);
            centeredStyle.alignment = TextAnchor.MiddleCenter;

            // Shield List or Simplified View
            if (shieldsProp != null)
            {
                if (shieldsProp.arraySize == 1)
                {
                    // Simplified view for single shield
                    SerializedProperty shieldProp = shieldsProp.GetArrayElementAtIndex(0);
                    if (shieldProp != null)
                    {
                        SerializedProperty valueProp = shieldProp.FindPropertyRelative("_value");
                        SerializedProperty colorProp = shieldProp.FindPropertyRelative("color");

                        EditorGUILayout.LabelField("Shield", centeredStyle);

                        if (valueProp != null)
                        {
                            EditorGUI.BeginChangeCheck();
                            float newValue = EditorGUILayout.FloatField("Value", valueProp.floatValue);
                            if (EditorGUI.EndChangeCheck())
                            {
                                valueProp.floatValue = Mathf.Max(0, newValue);
                                serializedObject.ApplyModifiedProperties();
                            }
                        }

                        if (colorProp != null)
                        {
                            EditorGUILayout.PropertyField(colorProp, new GUIContent("Color"));
                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
                else
                {
                    // List view for multiple shields
                    EditorGUILayout.PropertyField(shieldsProp, new GUIContent("Shields"), true);
                }
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            shieldDamageAmount = EditorGUILayout.FloatField("Damage", shieldDamageAmount);
            if (GUILayout.Button("Apply", GUILayout.Height(18), GUILayout.Width(120)))
            {
                health.Shield -= shieldDamageAmount;
                EditorUtility.SetDirty(health);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            shieldRestoreAmount = EditorGUILayout.FloatField("Restore", shieldRestoreAmount);
            if (GUILayout.Button("Apply", GUILayout.Height(18), GUILayout.Width(120)))
            {
                health.Shield += shieldRestoreAmount;
                EditorUtility.SetDirty(health);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Set Shield to 0", GUILayout.Height(25)))
            {
                health.Shield = 0;
                EditorUtility.SetDirty(health);
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(onShieldChangedProp);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            if (health.Shields.Count == 1)
            {
                if (GUILayout.Button("Enable Multi-Shield Mode", GUILayout.Height(25)))
                {
                    health.Shields.Add(new Shield(100));
                    EditorUtility.SetDirty(health);
                }
            }
            else
            {
                if (GUILayout.Button("Disable Multi-Shield Mode", GUILayout.Height(25)))
                {
                    for (int i = health.Shields.Count - 1; i >= 1; i--)
                    {
                        health.Shields.Remove(health.Shields[i]);
                    }
                    EditorUtility.SetDirty(health);
                }
            }
        }

        private void DrawHealthBar()
        {
            Rect rect = GUILayoutUtility.GetRect(18, 18, GUILayout.ExpandWidth(true));

            float totalHealth = health.Current;
            float totalMax = health.Max;
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
            if (health.Shield > 0)
            {
                // Calculate shield as percentage of total (health + shield)
                // Since shield has no max, we'll use a visual representation based on health max
                float totalValue = totalMax + health.Shield;
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
