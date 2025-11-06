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

    // Feature type identifiers
    private const string TextDisplayType = "TextDisplayFeature";
    private const string ColorGradientType = "ColorGradientFeature";
    private const string BackgroundFillType = "BackgroundFillFeature";
    private const string FlashingType = "FlashingFeature";

    // Available features mapping
    private static readonly System.Collections.Generic.Dictionary<string, string> AvailableFeatures = new System.Collections.Generic.Dictionary<string, string>
    {
        { "Text Display", TextDisplayType },
        { "Color Gradient", ColorGradientType },
        { "Background Fill", BackgroundFillType },
        { "Flashing", FlashingType }
    };

    // Foldout states
    private bool showReferences = true;
    private bool showTextDisplay = true;
    private bool showColorGradient = true;
    private bool showBackgroundFill = true;
    private bool showFlashing = true;

    // Confirmation state - tracks which feature type is waiting for confirmation
    private string featureTypeAwaitingConfirmation = null;

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

        // Add Feature Button
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Feature"))
        {
            ShowAddFeatureMenu();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        // Display sections based on feature toggles
        bool featureRemoved = DrawFeatureSections();

        // If a feature was removed, apply changes and exit early to avoid accessing deleted properties
        if (featureRemoved)
        {
            serializedObject.ApplyModifiedProperties();
            return;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowAddFeatureMenu()
    {
        GenericMenu menu = new GenericMenu();

        // Get currently added feature types
        System.Collections.Generic.HashSet<string> addedFeatureTypes = new System.Collections.Generic.HashSet<string>();
        for (int i = 0; i < featureTogglesProp.arraySize; i++)
        {
            var element = featureTogglesProp.GetArrayElementAtIndex(i);
            string featureType = GetFeatureType(element);
            if (featureType != null)
            {
                addedFeatureTypes.Add(featureType);
            }
        }

        // Add menu items for available features that aren't already added
        foreach (var kvp in AvailableFeatures)
        {
            string displayName = kvp.Key;
            string featureType = kvp.Value;

            if (!addedFeatureTypes.Contains(featureType))
            {
                menu.AddItem(new GUIContent(displayName), false, () => AddFeature(featureType));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(displayName + " (Already Added)"));
            }
        }

        // If all features are added, show a message
        if (addedFeatureTypes.Count >= AvailableFeatures.Count)
        {
            menu.AddDisabledItem(new GUIContent("All features added"));
        }

        menu.ShowAsContext();
    }

    private void AddFeature(string featureType)
    {
        serializedObject.Update();
        int index = featureTogglesProp.arraySize;
        featureTogglesProp.arraySize++;
        var element = featureTogglesProp.GetArrayElementAtIndex(index);

        // Set the managed reference type based on feature type
        string managedReferenceType = GetManagedReferenceType(featureType);
        element.managedReferenceValue = CreateFeatureInstance(featureType);

        serializedObject.ApplyModifiedProperties();
    }

    private object CreateFeatureInstance(string featureType)
    {
        if (featureType == TextDisplayType)
        {
            return new TextDisplayFeature();
        }
        else if (featureType == ColorGradientType)
        {
            return new ColorGradientFeature();
        }
        else if (featureType == BackgroundFillType)
        {
            return new BackgroundFillFeature();
        }
        else if (featureType == FlashingType)
        {
            return new FlashingFeature();
        }
        return null;
    }

    private string GetManagedReferenceType(string featureType)
    {
        // Unity uses assembly qualified names for managed references
        string assemblyName = typeof(FeatureToggle).Assembly.GetName().Name;
        return $"{featureType}, {assemblyName}";
    }

    private string GetFeatureType(SerializedProperty element)
    {
        // Check for type-specific properties to determine the feature type
        if (element.FindPropertyRelative("textCurrent") != null)
        {
            return TextDisplayType;
        }
        else if (element.FindPropertyRelative("colorAtMin") != null)
        {
            return ColorGradientType;
        }
        else if (element.FindPropertyRelative("backgroundFill") != null)
        {
            return BackgroundFillType;
        }
        else if (element.FindPropertyRelative("thresholdPercent") != null)
        {
            return FlashingType;
        }
        return null;
    }

    private SerializedProperty GetFeatureElementByType(string featureType)
    {
        for (int i = 0; i < featureTogglesProp.arraySize; i++)
        {
            var element = featureTogglesProp.GetArrayElementAtIndex(i);
            if (GetFeatureType(element) == featureType)
            {
                return element;
            }
        }
        return null;
    }

    private bool IsFeatureInList(string featureType)
    {
        return GetFeatureElementByType(featureType) != null;
    }

    private void RequestRemoveFeature(string featureType)
    {
        // If a different feature is already awaiting confirmation, cancel it first
        if (featureTypeAwaitingConfirmation != null && featureTypeAwaitingConfirmation != featureType)
        {
            featureTypeAwaitingConfirmation = null;
        }
        featureTypeAwaitingConfirmation = featureType;
    }

    private void ConfirmRemoveFeature(string featureType)
    {
        serializedObject.Update();
        for (int i = featureTogglesProp.arraySize - 1; i >= 0; i--)
        {
            var element = featureTogglesProp.GetArrayElementAtIndex(i);
            if (GetFeatureType(element) == featureType)
            {
                featureTogglesProp.DeleteArrayElementAtIndex(i);
                break;
            }
        }
        serializedObject.ApplyModifiedProperties();
        featureTypeAwaitingConfirmation = null;
    }

    private void CancelRemoveFeature()
    {
        featureTypeAwaitingConfirmation = null;
    }

    private bool DrawFeatureSections()
    {
        bool featureRemoved = false;

        // Text Display Section
        var textFeature = GetFeatureElementByType(TextDisplayType);
        if (textFeature != null)
        {
            EditorGUILayout.BeginHorizontal();
            showTextDisplay = EditorGUILayout.Foldout(showTextDisplay, "Text Display", true);

            if (featureTypeAwaitingConfirmation == TextDisplayType)
            {
                EditorGUILayout.LabelField("Are you sure?", GUILayout.Width(100));
                if (GUILayout.Button("No", GUILayout.Width(60)))
                {
                    CancelRemoveFeature();
                }
                if (GUILayout.Button("Yes", GUILayout.Width(60)))
                {
                    ConfirmRemoveFeature(TextDisplayType);
                    featureRemoved = true;
                }
            }
            else
            {
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    RequestRemoveFeature(TextDisplayType);
                }
            }
            EditorGUILayout.EndHorizontal();
            if (showTextDisplay && !featureRemoved)
            {
                EditorGUI.indentLevel++;
                var textCurrentProp = textFeature.FindPropertyRelative("textCurrent");
                var textMaxProp = textFeature.FindPropertyRelative("textMax");
                if (textCurrentProp != null)
                    EditorGUILayout.PropertyField(textCurrentProp, new GUIContent("Current Text"));
                if (textMaxProp != null)
                    EditorGUILayout.PropertyField(textMaxProp, new GUIContent("Max Text"));
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        if (featureRemoved)
            return true;

        // Color Gradient Section
        var colorFeature = GetFeatureElementByType(ColorGradientType);
        if (colorFeature != null)
        {
            EditorGUILayout.BeginHorizontal();
            showColorGradient = EditorGUILayout.Foldout(showColorGradient, "Color Gradient", true);

            if (featureTypeAwaitingConfirmation == ColorGradientType)
            {
                EditorGUILayout.LabelField("Are you sure?", GUILayout.Width(100));
                if (GUILayout.Button("No", GUILayout.Width(60)))
                {
                    CancelRemoveFeature();
                }
                if (GUILayout.Button("Yes", GUILayout.Width(60)))
                {
                    ConfirmRemoveFeature(ColorGradientType);
                    featureRemoved = true;
                }
            }
            else
            {
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    RequestRemoveFeature(ColorGradientType);
                }
            }
            EditorGUILayout.EndHorizontal();
            if (showColorGradient && !featureRemoved)
            {
                EditorGUI.indentLevel++;
                var colorAtMinProp = colorFeature.FindPropertyRelative("colorAtMin");
                var colorAtHalfwayProp = colorFeature.FindPropertyRelative("colorAtHalfway");
                var colorAtMaxProp = colorFeature.FindPropertyRelative("colorAtMax");
                if (colorAtMinProp != null)
                    EditorGUILayout.PropertyField(colorAtMinProp, new GUIContent("Color at Min"));
                if (colorAtHalfwayProp != null)
                    EditorGUILayout.PropertyField(colorAtHalfwayProp, new GUIContent("Color at Halfway"));
                if (colorAtMaxProp != null)
                    EditorGUILayout.PropertyField(colorAtMaxProp, new GUIContent("Color at Max"));
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        if (featureRemoved)
            return true;

        // Background Fill Section
        var bgFeature = GetFeatureElementByType(BackgroundFillType);
        if (bgFeature != null)
        {
            EditorGUILayout.BeginHorizontal();
            showBackgroundFill = EditorGUILayout.Foldout(showBackgroundFill, "Background Fill", true);

            if (featureTypeAwaitingConfirmation == BackgroundFillType)
            {
                EditorGUILayout.LabelField("Are you sure?", GUILayout.Width(100));
                if (GUILayout.Button("No", GUILayout.Width(60)))
                {
                    CancelRemoveFeature();
                }
                if (GUILayout.Button("Yes", GUILayout.Width(60)))
                {
                    ConfirmRemoveFeature(BackgroundFillType);
                    featureRemoved = true;
                }
            }
            else
            {
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    RequestRemoveFeature(BackgroundFillType);
                }
            }
            EditorGUILayout.EndHorizontal();
            if (showBackgroundFill && !featureRemoved)
            {
                EditorGUI.indentLevel++;
                var backgroundFillProp = bgFeature.FindPropertyRelative("backgroundFill");
                var keepSizeConsistentProp = bgFeature.FindPropertyRelative("keepSizeConsistent");
                var animationSpeedProp = bgFeature.FindPropertyRelative("animationSpeed");
                var speedCurveProp = bgFeature.FindPropertyRelative("speedCurve");
                var delayProp = bgFeature.FindPropertyRelative("delay");

                if (backgroundFillProp != null)
                    EditorGUILayout.PropertyField(backgroundFillProp, new GUIContent("Background Fill Image"));
                if (keepSizeConsistentProp != null)
                    EditorGUILayout.PropertyField(keepSizeConsistentProp, new GUIContent("Keep Size Consistent"));
                if (animationSpeedProp != null)
                    EditorGUILayout.PropertyField(animationSpeedProp, new GUIContent("Animation Speed"));
                if (speedCurveProp != null)
                    EditorGUILayout.PropertyField(speedCurveProp, new GUIContent("Speed Curve"));
                if (delayProp != null)
                    EditorGUILayout.PropertyField(delayProp, new GUIContent("Delay"));
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        if (featureRemoved)
            return true;

        // Flashing Section
        var flashingFeature = GetFeatureElementByType(FlashingType);
        if (flashingFeature != null)
        {
            EditorGUILayout.BeginHorizontal();
            showFlashing = EditorGUILayout.Foldout(showFlashing, "Flashing", true);

            if (featureTypeAwaitingConfirmation == FlashingType)
            {
                EditorGUILayout.LabelField("Are you sure?", GUILayout.Width(100));
                if (GUILayout.Button("No", GUILayout.Width(60)))
                {
                    CancelRemoveFeature();
                }
                if (GUILayout.Button("Yes", GUILayout.Width(60)))
                {
                    ConfirmRemoveFeature(FlashingType);
                    featureRemoved = true;
                }
            }
            else
            {
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    RequestRemoveFeature(FlashingType);
                }
            }
            EditorGUILayout.EndHorizontal();
            if (showFlashing && !featureRemoved)
            {
                EditorGUI.indentLevel++;
                var thresholdPercentProp = flashingFeature.FindPropertyRelative("thresholdPercent");
                var flashColor1Prop = flashingFeature.FindPropertyRelative("flashColor1");
                var flashColor2Prop = flashingFeature.FindPropertyRelative("flashColor2");
                var flashSpeedProp = flashingFeature.FindPropertyRelative("flashSpeed");

                if (thresholdPercentProp != null)
                    EditorGUILayout.PropertyField(thresholdPercentProp, new GUIContent("Threshold Percent"));
                if (flashColor1Prop != null)
                    EditorGUILayout.PropertyField(flashColor1Prop, new GUIContent("Flash Color 1"));
                if (flashColor2Prop != null)
                    EditorGUILayout.PropertyField(flashColor2Prop, new GUIContent("Flash Color 2"));
                if (flashSpeedProp != null)
                    EditorGUILayout.PropertyField(flashSpeedProp, new GUIContent("Flash Speed"));
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        return featureRemoved;
    }
}


