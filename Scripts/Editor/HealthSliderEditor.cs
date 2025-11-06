using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using TMPro;

[CustomEditor(typeof(HealthSlider))]
[CanEditMultipleObjects]
public class HealthSliderEditor : UnityEditor.Editor
{
    private HealthSlider healthSlider;

    // Serialized Properties
    private SerializedProperty healthProp;
    private SerializedProperty sliderProp;
    private SerializedProperty featureTogglesProp;
    private SerializedProperty showTextProp;
    private SerializedProperty textCurrentProp;
    private SerializedProperty textMaxProp;
    private SerializedProperty showColorGradientProp;
    private SerializedProperty colorAtMinProp;
    private SerializedProperty colorAtHalfwayProp;
    private SerializedProperty colorAtMaxProp;
    private SerializedProperty showBackgroundFillProp;
    private SerializedProperty backgroundFillProp;
    private SerializedProperty keepSizeConsistentProp;
    private SerializedProperty animationSpeedProp;
    private SerializedProperty speedCurveProp;
    private SerializedProperty delayProp;

    // ReorderableList
    private ReorderableList featureTogglesList;

    // Foldout states
    private bool showReferences = true;
    private bool showFeatureList = true;
    private bool showTextDisplay = true;
    private bool showColorGradient = true;
    private bool showBackgroundFill = true;

    private void OnEnable()
    {
        healthSlider = (HealthSlider)target;

        // Find all serialized properties
        healthProp = serializedObject.FindProperty("health");
        sliderProp = serializedObject.FindProperty("slider");
        featureTogglesProp = serializedObject.FindProperty("featureToggles");
        showTextProp = serializedObject.FindProperty("showText");
        textCurrentProp = serializedObject.FindProperty("textCurrent");
        textMaxProp = serializedObject.FindProperty("textMax");
        showColorGradientProp = serializedObject.FindProperty("showColorGradient");
        colorAtMinProp = serializedObject.FindProperty("colorAtMin");
        colorAtHalfwayProp = serializedObject.FindProperty("colorAtHalfway");
        colorAtMaxProp = serializedObject.FindProperty("colorAtMax");
        showBackgroundFillProp = serializedObject.FindProperty("showBackgroundFill");
        backgroundFillProp = serializedObject.FindProperty("backgroundFill");
        keepSizeConsistentProp = serializedObject.FindProperty("keepSizeConsistent");
        animationSpeedProp = serializedObject.FindProperty("animationSpeed");
        speedCurveProp = serializedObject.FindProperty("speedCurve");
        delayProp = serializedObject.FindProperty("delay");

        // Setup ReorderableList
        featureTogglesList = new ReorderableList(serializedObject, featureTogglesProp, true, true, true, true);
        featureTogglesList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Feature Toggles");
        };
        featureTogglesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = featureTogglesProp.GetArrayElementAtIndex(index);
            rect.y += 2;
            float width = rect.width;
            float nameWidth = width * 0.6f;
            float enabledWidth = width * 0.4f - 5;

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, nameWidth, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("featureName"),
                GUIContent.none
            );
            EditorGUI.PropertyField(
                new Rect(rect.x + nameWidth + 5, rect.y, enabledWidth, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("enabled"),
                new GUIContent("Enabled")
            );
        };
        featureTogglesList.onAddCallback = (ReorderableList list) =>
        {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("featureName").stringValue = "New Feature";
            element.FindPropertyRelative("enabled").boolValue = false;
        };
        featureTogglesList.elementHeight = EditorGUIUtility.singleLineHeight + 4;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Health Slider", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // References Section
        showReferences = EditorGUILayout.Foldout(showReferences, "References", true);
        if (showReferences)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(healthProp, new GUIContent("Health"));
            EditorGUILayout.PropertyField(sliderProp, new GUIContent("Slider"));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        // Feature Toggles List
        showFeatureList = EditorGUILayout.Foldout(showFeatureList, "Feature Toggles", true);
        if (showFeatureList)
        {
            EditorGUI.indentLevel++;
            featureTogglesList.DoLayoutList();

            // Sync feature toggles with boolean properties
            SyncFeatureToggles();

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        // Display sections based on feature toggles
        DrawFeatureSections();

        serializedObject.ApplyModifiedProperties();
    }

    private void SyncFeatureToggles()
    {
        // Sync the list with the existing boolean properties
        for (int i = 0; i < featureTogglesProp.arraySize; i++)
        {
            var element = featureTogglesProp.GetArrayElementAtIndex(i);
            string featureName = element.FindPropertyRelative("featureName").stringValue;
            bool enabled = element.FindPropertyRelative("enabled").boolValue;

            // Map feature names to boolean properties
            if (featureName == "Text Display")
            {
                showTextProp.boolValue = enabled;
            }
            else if (featureName == "Color Gradient")
            {
                showColorGradientProp.boolValue = enabled;
            }
            else if (featureName == "Background Fill")
            {
                showBackgroundFillProp.boolValue = enabled;
            }
        }
    }

    private bool IsFeatureInList(string featureName)
    {
        for (int i = 0; i < featureTogglesProp.arraySize; i++)
        {
            var element = featureTogglesProp.GetArrayElementAtIndex(i);
            if (element.FindPropertyRelative("featureName").stringValue == featureName)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsFeatureEnabled(string featureName)
    {
        for (int i = 0; i < featureTogglesProp.arraySize; i++)
        {
            var element = featureTogglesProp.GetArrayElementAtIndex(i);
            if (element.FindPropertyRelative("featureName").stringValue == featureName)
            {
                return element.FindPropertyRelative("enabled").boolValue;
            }
        }
        return false;
    }

    private void DrawFeatureSections()
    {
        // Text Display Section
        if (IsFeatureInList("Text Display"))
        {
            showTextDisplay = EditorGUILayout.Foldout(showTextDisplay, "Text Display", true);
            if (showTextDisplay)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(textCurrentProp, new GUIContent("Current Text"));
                EditorGUILayout.PropertyField(textMaxProp, new GUIContent("Max Text"));
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        // Color Gradient Section
        if (IsFeatureInList("Color Gradient"))
        {
            showColorGradient = EditorGUILayout.Foldout(showColorGradient, "Color Gradient", true);
            if (showColorGradient)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(colorAtMinProp, new GUIContent("Color at Min (Red)"));
                EditorGUILayout.PropertyField(colorAtHalfwayProp, new GUIContent("Color at Halfway (Yellow)"));
                EditorGUILayout.PropertyField(colorAtMaxProp, new GUIContent("Color at Max (Green)"));
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        // Background Fill Section
        if (IsFeatureInList("Background Fill"))
        {
            showBackgroundFill = EditorGUILayout.Foldout(showBackgroundFill, "Background Fill", true);
            if (showBackgroundFill)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(backgroundFillProp, new GUIContent("Background Fill Image"));
                EditorGUILayout.PropertyField(keepSizeConsistentProp, new GUIContent("Keep Size Consistent"));
                EditorGUILayout.PropertyField(animationSpeedProp, new GUIContent("Animation Speed"));
                EditorGUILayout.PropertyField(speedCurveProp, new GUIContent("Speed Curve"));
                EditorGUILayout.PropertyField(delayProp, new GUIContent("Delay"));
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }
    }
}

