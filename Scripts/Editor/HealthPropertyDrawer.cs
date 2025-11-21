using UnityEngine;
using UnityEditor;
using JacobHomanics.HealthSystem;

namespace JacobHomanics.HealthSystem.Editor
{
    [CustomPropertyDrawer(typeof(HealthData))]
    public class HealthPropertyDrawer : PropertyDrawer
    {
        private static System.Collections.Generic.Dictionary<string, bool> currentEventsExpanded = new System.Collections.Generic.Dictionary<string, bool>();
        private static System.Collections.Generic.Dictionary<string, bool> maxEventsExpanded = new System.Collections.Generic.Dictionary<string, bool>();
        private static System.Collections.Generic.Dictionary<string, float> damageAmounts = new System.Collections.Generic.Dictionary<string, float>();
        private static System.Collections.Generic.Dictionary<string, float> healAmounts = new System.Collections.Generic.Dictionary<string, float>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get properties
            SerializedProperty currentProp = property.FindPropertyRelative("current");
            SerializedProperty maxProp = property.FindPropertyRelative("max");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float yPos = position.y;

            // Draw foldout
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, yPos, position.width, lineHeight), property.isExpanded, label);
            yPos += lineHeight + spacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Current field as slider
                if (currentProp != null && maxProp != null)
                {
                    EditorGUI.BeginChangeCheck();
                    float maxValue = maxProp.floatValue;
                    float currentValue = currentProp.floatValue;
                    float newCurrent = EditorGUI.Slider(new Rect(position.x, yPos, position.width, lineHeight), new GUIContent("Current"), currentValue, 0, maxValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        currentProp.floatValue = newCurrent;
                    }
                }
                else
                {
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), currentProp, new GUIContent("Current"));
                }
                yPos += lineHeight + spacing;

                // Max field
                EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), maxProp, new GUIContent("Max"));
                yPos += lineHeight + spacing;

                // Damage and Heal buttons
                string damageKey = property.propertyPath + "_damage";
                string healKey = property.propertyPath + "_heal";

                if (!damageAmounts.ContainsKey(damageKey))
                    damageAmounts[damageKey] = 1f;
                if (!healAmounts.ContainsKey(healKey))
                    healAmounts[healKey] = 1f;

                float buttonWidth = (position.width - spacing) / 2f;
                float inputWidth = buttonWidth * 0.6f;
                float btnWidth = buttonWidth * 0.4f;

                // Damage section
                EditorGUI.LabelField(new Rect(position.x, yPos, 60, lineHeight), "Damage:");
                damageAmounts[damageKey] = EditorGUI.FloatField(new Rect(position.x + 65, yPos, inputWidth - 65, lineHeight), damageAmounts[damageKey]);
                if (GUI.Button(new Rect(position.x + inputWidth, yPos, btnWidth, lineHeight), "Apply"))
                {
                    if (currentProp != null)
                    {
                        float currentValue = currentProp.floatValue;
                        currentProp.floatValue = Mathf.Max(0, currentValue - damageAmounts[damageKey]);
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
                yPos += lineHeight + spacing;

                // Heal section
                EditorGUI.LabelField(new Rect(position.x, yPos, 60, lineHeight), "Heal:");
                healAmounts[healKey] = EditorGUI.FloatField(new Rect(position.x + 65, yPos, inputWidth - 65, lineHeight), healAmounts[healKey]);
                if (GUI.Button(new Rect(position.x + inputWidth, yPos, btnWidth, lineHeight), "Apply"))
                {
                    if (currentProp != null && maxProp != null)
                    {
                        float currentValue = currentProp.floatValue;
                        float maxValue = maxProp.floatValue;
                        currentProp.floatValue = Mathf.Min(maxValue, currentValue + healAmounts[healKey]);
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float height = EditorGUIUtility.singleLineHeight; // Foldout

            // Current and Max fields
            height += EditorGUIUtility.singleLineHeight + spacing; // Current
            height += EditorGUIUtility.singleLineHeight + spacing; // Max

            // Damage and Heal buttons
            height += EditorGUIUtility.singleLineHeight + spacing; // Damage
            height += EditorGUIUtility.singleLineHeight + spacing; // Heal

            // Current Health Events section (foldout)
            string currentEventsKey = property.propertyPath + "_currentEvents";
            bool currentExpanded = currentEventsExpanded.ContainsKey(currentEventsKey) && currentEventsExpanded[currentEventsKey];
            height += EditorGUIUtility.singleLineHeight + spacing; // Foldout
            if (currentExpanded)
            {
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onCurrentSet")) + spacing;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onCurrentChange")) + spacing;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onCurrentDown")) + spacing;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onCurrentUp")) + spacing;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onCurrentMax")) + spacing;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onCurrentZero")) + spacing;
            }

            // Max Health Events section (foldout)
            string maxEventsKey = property.propertyPath + "_maxEvents";
            bool maxExpanded = maxEventsExpanded.ContainsKey(maxEventsKey) && maxEventsExpanded[maxEventsKey];
            height += EditorGUIUtility.singleLineHeight + spacing; // Foldout
            if (maxExpanded)
            {
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onMaxSet")) + spacing;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onMaxChange")) + spacing;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onMaxDown")) + spacing;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onMaxUp"));
            }

            return height;
        }
    }
}

