using UnityEngine;
using UnityEditor;
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

    // Available features
    private static readonly string[] AvailableFeatures = { "Text Display", "Color Gradient", "Background Fill" };

    // Foldout states
    private bool showReferences = true;
    private bool showTextDisplay = true;
    private bool showColorGradient = true;
    private bool showBackgroundFill = true;

    // Confirmation state - tracks which feature is waiting for confirmation
    private string featureAwaitingConfirmation = null;

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

        // Sync feature toggles with boolean properties
        SyncFeatureToggles();

        // Add Feature Button
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Feature"))
        {
            ShowAddFeatureMenu();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        // Display sections based on feature toggles
        DrawFeatureSections();

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowAddFeatureMenu()
    {
        GenericMenu menu = new GenericMenu();

        // Get currently added features
        System.Collections.Generic.HashSet<string> addedFeatures = new System.Collections.Generic.HashSet<string>();
        for (int i = 0; i < featureTogglesProp.arraySize; i++)
        {
            var element = featureTogglesProp.GetArrayElementAtIndex(i);
            string featureName = element.FindPropertyRelative("featureName").stringValue;
            addedFeatures.Add(featureName);
        }

        // Add menu items for available features that aren't already added
        foreach (string feature in AvailableFeatures)
        {
            if (!addedFeatures.Contains(feature))
            {
                menu.AddItem(new GUIContent(feature), false, () => AddFeature(feature));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(feature + " (Already Added)"));
            }
        }

        // If all features are added, show a message
        if (addedFeatures.Count >= AvailableFeatures.Length)
        {
            menu.AddDisabledItem(new GUIContent("All features added"));
        }

        menu.ShowAsContext();
    }

    private void AddFeature(string featureName)
    {
        serializedObject.Update();
        int index = featureTogglesProp.arraySize;
        featureTogglesProp.arraySize++;
        var element = featureTogglesProp.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("featureName").stringValue = featureName;
        element.FindPropertyRelative("enabled").boolValue = false;
        serializedObject.ApplyModifiedProperties();
    }

    private void RequestRemoveFeature(string featureName)
    {
        // If a different feature is already awaiting confirmation, cancel it first
        if (featureAwaitingConfirmation != null && featureAwaitingConfirmation != featureName)
        {
            featureAwaitingConfirmation = null;
        }
        featureAwaitingConfirmation = featureName;
    }

    private void ConfirmRemoveFeature(string featureName)
    {
        serializedObject.Update();
        for (int i = featureTogglesProp.arraySize - 1; i >= 0; i--)
        {
            var element = featureTogglesProp.GetArrayElementAtIndex(i);
            if (element.FindPropertyRelative("featureName").stringValue == featureName)
            {
                featureTogglesProp.DeleteArrayElementAtIndex(i);
                break;
            }
        }
        serializedObject.ApplyModifiedProperties();
        featureAwaitingConfirmation = null;
    }

    private void CancelRemoveFeature()
    {
        featureAwaitingConfirmation = null;
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
            EditorGUILayout.BeginHorizontal();
            showTextDisplay = EditorGUILayout.Foldout(showTextDisplay, "Text Display", true);

            if (featureAwaitingConfirmation == "Text Display")
            {
                EditorGUILayout.LabelField("Are you sure?", GUILayout.Width(100));
                if (GUILayout.Button("No", GUILayout.Width(60)))
                {
                    CancelRemoveFeature();
                }
                if (GUILayout.Button("Yes", GUILayout.Width(60)))
                {
                    ConfirmRemoveFeature("Text Display");
                }
            }
            else
            {
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    RequestRemoveFeature("Text Display");
                }
            }
            EditorGUILayout.EndHorizontal();
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
            EditorGUILayout.BeginHorizontal();
            showColorGradient = EditorGUILayout.Foldout(showColorGradient, "Color Gradient", true);

            if (featureAwaitingConfirmation == "Color Gradient")
            {
                EditorGUILayout.LabelField("Are you sure?", GUILayout.Width(100));
                if (GUILayout.Button("No", GUILayout.Width(60)))
                {
                    CancelRemoveFeature();
                }
                if (GUILayout.Button("Yes", GUILayout.Width(60)))
                {
                    ConfirmRemoveFeature("Color Gradient");
                }
            }
            else
            {
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    RequestRemoveFeature("Color Gradient");
                }
            }
            EditorGUILayout.EndHorizontal();
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
            EditorGUILayout.BeginHorizontal();
            showBackgroundFill = EditorGUILayout.Foldout(showBackgroundFill, "Background Fill", true);

            if (featureAwaitingConfirmation == "Background Fill")
            {
                EditorGUILayout.LabelField("Are you sure?", GUILayout.Width(100));
                if (GUILayout.Button("No", GUILayout.Width(60)))
                {
                    CancelRemoveFeature();
                }
                if (GUILayout.Button("Yes", GUILayout.Width(60)))
                {
                    ConfirmRemoveFeature("Background Fill");
                }
            }
            else
            {
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    RequestRemoveFeature("Background Fill");
                }
            }
            EditorGUILayout.EndHorizontal();
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

