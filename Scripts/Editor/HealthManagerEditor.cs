using UnityEngine;
using UnityEditor;

namespace JacobHomanics.HealthSystem.Editor
{
    [CustomEditor(typeof(HealthManager))]
    [CanEditMultipleObjects]
    public class HealthManagerEditor : UnityEditor.Editor
    {
        private HealthManager healthManager;
        private SerializedProperty healthsProp;
        private SerializedProperty shieldsProp;
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
            healthManager = (HealthManager)target;

            healthsProp = serializedObject.FindProperty("healths");
            shieldsProp = serializedObject.FindProperty("shields");
            onShieldChangedProp = serializedObject.FindProperty("onShieldChanged");
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

        private Health GetCurrentHealth()
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
            EditorGUILayout.LabelField($"Total Health: {totalHealth:F2} / {totalMax:F2} ({healthPercent:F2}%)", EditorStyles.centeredGreyMiniLabel);

            // Display shield total if shields exist
            if (healthManager.ShieldTotal > 0)
            {
                EditorGUILayout.LabelField($"Shield Total: {healthManager.ShieldTotal:F2} ({healthManager.Shields.Count} shields)", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.Space();

            // Health List Section
            EditorGUILayout.LabelField("Healths", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Health List
            if (healthsProp != null)
            {
                EditorGUILayout.PropertyField(healthsProp, new GUIContent("Health List"), true);
            }

            // Current Health Info (read-only display)
            Health currentHealth = GetCurrentHealth();
            int currentIndex = GetCurrentHealthIndex();
            if (currentHealth != null && currentIndex >= 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Current Health (Index {currentIndex})", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Current: {currentHealth.Current:F2} / Max: {currentHealth.Max:F2}");
            }
            else if (healthsProp != null && healthsProp.arraySize > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("No health with health > 0. All healths are depleted.", MessageType.Info);
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
                healthManager.Damage(damageAmount);
                EditorUtility.SetDirty(healthManager);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            healAmount = EditorGUILayout.FloatField("Heal Amount", healAmount);
            if (GUILayout.Button("Apply Heal", GUILayout.Height(18), GUILayout.Width(120)))
            {
                healthManager.Heal(healAmount);
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

            EditorGUILayout.BeginHorizontal();
            newShieldColor = EditorGUILayout.ColorField("Shield Color", newShieldColor);
            if (GUILayout.Button("Add New Shield", GUILayout.Height(18), GUILayout.Width(120)))
            {
                healthManager.Shields.Add(new Shield(shieldRestoreAmount, newShieldColor));
                healthManager.onShieldChanged?.Invoke();
                EditorUtility.SetDirty(healthManager);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            Health currentHealthForButtons = GetCurrentHealth();
            if (currentHealthForButtons != null)
            {
                if (GUILayout.Button("Set Current Health to 0", GUILayout.Height(25)))
                {
                    currentHealthForButtons.Current = 0;
                    EditorUtility.SetDirty(healthManager);
                }

                if (GUILayout.Button("Set Current Health to Max", GUILayout.Height(25)))
                {
                    currentHealthForButtons.Current = currentHealthForButtons.Max;
                    EditorUtility.SetDirty(healthManager);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear All Shields", GUILayout.Height(25)))
            {
                healthManager.Shields.Clear();
                healthManager.onShieldChanged?.Invoke();
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

            // Get current health index once for all event tabs
            int currentHealthIndex = GetCurrentHealthIndex();

            // Display events based on selected tab
            if (selectedEventTab == 0)
            {
                // Current Health Events (from the current health)
                if (currentHealthIndex >= 0 && healthsProp != null && currentHealthIndex < healthsProp.arraySize)
                {
                    SerializedProperty currentHealthProp = healthsProp.GetArrayElementAtIndex(currentHealthIndex);
                    if (currentHealthProp != null)
                    {
                        EditorGUILayout.PropertyField(currentHealthProp.FindPropertyRelative("onCurrentSet"));
                        EditorGUILayout.PropertyField(currentHealthProp.FindPropertyRelative("onCurrentChange"));
                        EditorGUILayout.PropertyField(currentHealthProp.FindPropertyRelative("onCurrentDown"));
                        EditorGUILayout.PropertyField(currentHealthProp.FindPropertyRelative("onCurrentUp"));
                        EditorGUILayout.PropertyField(currentHealthProp.FindPropertyRelative("onCurrentMax"));
                        EditorGUILayout.PropertyField(currentHealthProp.FindPropertyRelative("onCurrentZero"));
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No health with health > 0. Add a health to the list and set its current value > 0.", MessageType.Info);
                }
            }
            else if (selectedEventTab == 1)
            {
                // Max Health Events (from the current health)
                if (currentHealthIndex >= 0 && healthsProp != null && currentHealthIndex < healthsProp.arraySize)
                {
                    SerializedProperty currentHealthProp = healthsProp.GetArrayElementAtIndex(currentHealthIndex);
                    if (currentHealthProp != null)
                    {
                        EditorGUILayout.PropertyField(currentHealthProp.FindPropertyRelative("onMaxSet"));
                        EditorGUILayout.PropertyField(currentHealthProp.FindPropertyRelative("onMaxChange"));
                        EditorGUILayout.PropertyField(currentHealthProp.FindPropertyRelative("onMaxDown"));
                        EditorGUILayout.PropertyField(currentHealthProp.FindPropertyRelative("onMaxUp"));
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No health with health > 0. Add a health to the list and set its current value > 0.", MessageType.Info);
                }
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
