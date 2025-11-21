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
        private Color newShieldColor = new Color(0f, 0.7f, 0.7f);

        // Animation for lagged lost health bar
        private float displayedHealth = 0f; // The lagged health value (what we display)
        private float previousCurrentHealth = 0f;
        private float startHealth = 0f; // Starting health value for animation
        private float targetHealth = 0f; // Target health value for animation
        private float animationElapsed = 0f; // Elapsed time during animation
        private float animationDelayRemaining = 0f; // Remaining delay time
        private bool isAnimating = false;
        private const float animationDuration = 0.5f; // Duration in seconds for the animation
        private const float animationDelay = 0.2f; // Delay in seconds before animation starts
        private double lastUpdateTime = 0f; // For calculating deltaTime

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

            // Initialize lagged health animation
            displayedHealth = health.Current;
            previousCurrentHealth = health.Current;
            startHealth = health.Current;
            targetHealth = health.Current;
            isAnimating = false;
            animationElapsed = 0f;
            animationDelayRemaining = 0f;
            lastUpdateTime = EditorApplication.timeSinceStartup;
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

            // Animate lagged health bar (duration-based with delay, similar to BaseAnimatedFill)
            float currentHealth = health.Current;
            bool healthIncreased = currentHealth > previousCurrentHealth;
            bool healthDecreased = currentHealth < previousCurrentHealth;

            // Calculate deltaTime and clamp it to prevent large jumps
            double currentTime = EditorApplication.timeSinceStartup;
            float deltaTime = (float)(currentTime - lastUpdateTime);
            deltaTime = Mathf.Clamp(deltaTime, 0f, 0.1f); // Clamp to max 100ms to prevent large jumps
            lastUpdateTime = currentTime;

            // If health increased (healing), snap immediately to target - no animation or delay
            if (healthIncreased)
            {
                displayedHealth = currentHealth;
                startHealth = currentHealth;
                targetHealth = currentHealth;
                isAnimating = false;
                animationDelayRemaining = 0f;
                animationElapsed = 0f;
            }
            // If health decreased (damage), start new animation with delay
            else if (healthDecreased)
            {
                // If already animating, calculate current value to continue from there
                float startValue;
                if (isAnimating)
                {
                    // Calculate current value based on animation progress
                    if (animationDelayRemaining > 0f)
                    {
                        // Still in delay, use the starting value
                        startValue = startHealth;
                    }
                    else if (animationDuration > 0f)
                    {
                        // Calculate current interpolated value
                        float t = Mathf.Clamp01(animationElapsed / animationDuration);
                        startValue = Mathf.Lerp(startHealth, targetHealth, t);
                    }
                    else
                    {
                        // Duration was 0, should be at target
                        startValue = targetHealth;
                    }
                }
                else
                {
                    // Not animating, use current displayed health
                    startValue = displayedHealth;
                }

                // Start new animation from current position to new target
                startHealth = startValue;
                targetHealth = currentHealth;
                displayedHealth = startHealth;
                animationDelayRemaining = animationDelay;
                animationElapsed = 0f;
                isAnimating = true;
            }

            // Update animation (handles both delay and animation phases)
            if (isAnimating)
            {
                // Handle delay before animation starts
                if (animationDelayRemaining > 0f)
                {
                    animationDelayRemaining -= deltaTime;
                    // During delay, maintain displayedHealth at start value
                    displayedHealth = startHealth;

                    // If delay just completed, reset animation elapsed
                    if (animationDelayRemaining <= 0f)
                    {
                        animationElapsed = 0f;
                        animationDelayRemaining = 0f; // Ensure it's exactly 0
                    }
                    else
                    {
                        // Force repaint to continue checking delay
                        Repaint();
                    }
                }
                // Animation phase (after delay)
                else
                {
                    // Update animation elapsed time
                    animationElapsed += deltaTime;
                    float progress = Mathf.Clamp01(animationElapsed / animationDuration);

                    // Interpolate from start to target
                    displayedHealth = Mathf.Lerp(startHealth, targetHealth, progress);

                    // Check if animation is complete
                    if (progress >= 1f || Mathf.Abs(displayedHealth - targetHealth) < 0.01f)
                    {
                        displayedHealth = targetHealth;
                        isAnimating = false;
                    }
                    else
                    {
                        // Force repaint for smooth animation
                        Repaint();
                    }
                }
            }

            previousCurrentHealth = currentHealth;

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

            // Display health and shield information
            float currentHealth = health.Current;
            float maxHealth = health.Max;
            float totalShield = health.Shield;

            EditorGUILayout.LabelField($"Current Health: {currentHealth:F2}");
            EditorGUILayout.LabelField($"Max Health: {maxHealth:F2}");
            EditorGUILayout.LabelField($"Shield: {totalShield:F2}");

            EditorGUILayout.Space(10);

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

            // Health bar (green to red gradient) - draw first so lost health bar appears on top
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

            // Health lost bar (lagged behind version - shows missing health that's animating down)
            // The lost health bar shows the gap between current health and the lagged displayed health
            float displayedHealthPercent = totalMax > 0 ? displayedHealth / totalMax : 0;
            displayedHealthPercent = Mathf.Clamp01(displayedHealthPercent);

            // Calculate the gap between current health and displayed health (the lagged portion)
            // When displayedHealth > currentHealth, there's a gap showing the lost health animating down
            if (displayedHealth > totalHealth + 0.01f) // Add small epsilon to avoid floating point issues
            {
                // Draw lost health bar between current health position and displayed health position
                float lostStartX = rect.x + rect.width * healthPercent;
                float lostEndX = rect.x + rect.width * displayedHealthPercent;
                float lostWidth = lostEndX - lostStartX;

                if (lostWidth > 0.1f)
                {
                    Rect lostRect = new Rect(lostStartX, rect.y, lostWidth, rect.height);
                    Color lostColor = new Color(0.4f, 0.1f, 0.1f, 0.8f); // Dark red, more opaque so it's visible
                    EditorGUI.DrawRect(lostRect, lostColor);
                }
            }

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
