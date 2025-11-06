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

    // Foldout states
    private bool showReferences = true;
    private bool showTextDisplay = true;
    private bool showColorGradient = true;
    private bool showBackgroundFill = true;
    private bool showAnimation = true;

    private void OnEnable()
    {
        healthSlider = (HealthSlider)target;

        // Find all serialized properties
        healthProp = serializedObject.FindProperty("health");
        sliderProp = serializedObject.FindProperty("slider");
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

        // Text Display Section
        showTextDisplay = EditorGUILayout.Foldout(showTextDisplay, "Text Display", true);
        if (showTextDisplay)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(showTextProp, new GUIContent("Show Text"));

            if (showTextProp.boolValue)
            {
                EditorGUILayout.PropertyField(textCurrentProp, new GUIContent("Current Text"));
                EditorGUILayout.PropertyField(textMaxProp, new GUIContent("Max Text"));
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        // Color Gradient Section
        showColorGradient = EditorGUILayout.Foldout(showColorGradient, "Color Gradient", true);
        if (showColorGradient)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(showColorGradientProp, new GUIContent("Show Color Gradient"));

            if (showColorGradientProp.boolValue)
            {
                EditorGUILayout.PropertyField(colorAtMinProp, new GUIContent("Color at Min (Red)"));
                EditorGUILayout.PropertyField(colorAtHalfwayProp, new GUIContent("Color at Halfway (Yellow)"));
                EditorGUILayout.PropertyField(colorAtMaxProp, new GUIContent("Color at Max (Green)"));
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        // Background Fill Section
        showBackgroundFill = EditorGUILayout.Foldout(showBackgroundFill, "Background Fill", true);
        if (showBackgroundFill)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(showBackgroundFillProp, new GUIContent("Show Background Fill"));

            if (showBackgroundFillProp.boolValue)
            {
                EditorGUILayout.PropertyField(backgroundFillProp, new GUIContent("Background Fill Image"));
                EditorGUILayout.PropertyField(keepSizeConsistentProp, new GUIContent("Keep Size Consistent"));
                EditorGUILayout.PropertyField(animationSpeedProp, new GUIContent("Animation Speed"));
                EditorGUILayout.PropertyField(speedCurveProp, new GUIContent("Speed Curve"));
                EditorGUILayout.PropertyField(delayProp, new GUIContent("Delay"));
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        serializedObject.ApplyModifiedProperties();
    }
}

