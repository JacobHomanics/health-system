using UnityEngine;
using UnityEditor;
using JacobHomanics.HealthSystem;

namespace JacobHomanics.HealthSystem.Editor
{
    [CustomPropertyDrawer(typeof(Health))]
    public class HealthPropertyDrawer : PropertyDrawer
    {
        private static System.Collections.Generic.Dictionary<string, bool> currentEventsExpanded = new System.Collections.Generic.Dictionary<string, bool>();
        private static System.Collections.Generic.Dictionary<string, bool> maxEventsExpanded = new System.Collections.Generic.Dictionary<string, bool>();

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

                // Current field with clamping
                if (currentProp != null && maxProp != null)
                {
                    EditorGUI.BeginChangeCheck();
                    float maxValue = maxProp.floatValue;
                    float newCurrent = EditorGUI.FloatField(new Rect(position.x, yPos, position.width, lineHeight), new GUIContent("Current"), currentProp.floatValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        // Clamp to 0 and Max
                        currentProp.floatValue = Mathf.Clamp(newCurrent, 0, maxValue);
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

                // Events - Current Health (with foldout)
                string currentEventsKey = property.propertyPath + "_currentEvents";
                if (!currentEventsExpanded.ContainsKey(currentEventsKey))
                    currentEventsExpanded[currentEventsKey] = false;

                currentEventsExpanded[currentEventsKey] = EditorGUI.Foldout(new Rect(position.x, yPos, position.width, lineHeight), currentEventsExpanded[currentEventsKey], "Current Health Events", true);
                yPos += lineHeight + spacing;

                if (currentEventsExpanded[currentEventsKey])
                {
                    EditorGUI.indentLevel++;

                    SerializedProperty onCurrentSetProp = property.FindPropertyRelative("onCurrentSet");
                    float propHeight = EditorGUI.GetPropertyHeight(onCurrentSetProp);
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, propHeight), onCurrentSetProp);
                    yPos += propHeight + spacing;

                    SerializedProperty onCurrentChangeProp = property.FindPropertyRelative("onCurrentChange");
                    propHeight = EditorGUI.GetPropertyHeight(onCurrentChangeProp);
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, propHeight), onCurrentChangeProp);
                    yPos += propHeight + spacing;

                    SerializedProperty onCurrentDownProp = property.FindPropertyRelative("onCurrentDown");
                    propHeight = EditorGUI.GetPropertyHeight(onCurrentDownProp);
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, propHeight), onCurrentDownProp);
                    yPos += propHeight + spacing;

                    SerializedProperty onCurrentUpProp = property.FindPropertyRelative("onCurrentUp");
                    propHeight = EditorGUI.GetPropertyHeight(onCurrentUpProp);
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, propHeight), onCurrentUpProp);
                    yPos += propHeight + spacing;

                    SerializedProperty onCurrentMaxProp = property.FindPropertyRelative("onCurrentMax");
                    propHeight = EditorGUI.GetPropertyHeight(onCurrentMaxProp);
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, propHeight), onCurrentMaxProp);
                    yPos += propHeight + spacing;

                    SerializedProperty onCurrentZeroProp = property.FindPropertyRelative("onCurrentZero");
                    propHeight = EditorGUI.GetPropertyHeight(onCurrentZeroProp);
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, propHeight), onCurrentZeroProp);
                    yPos += propHeight + spacing;

                    EditorGUI.indentLevel--;
                }

                // Events - Max Health (with foldout)
                string maxEventsKey = property.propertyPath + "_maxEvents";
                if (!maxEventsExpanded.ContainsKey(maxEventsKey))
                    maxEventsExpanded[maxEventsKey] = false;

                maxEventsExpanded[maxEventsKey] = EditorGUI.Foldout(new Rect(position.x, yPos, position.width, lineHeight), maxEventsExpanded[maxEventsKey], "Max Health Events", true);
                yPos += lineHeight + spacing;

                if (maxEventsExpanded[maxEventsKey])
                {
                    EditorGUI.indentLevel++;

                    SerializedProperty onMaxSetProp = property.FindPropertyRelative("onMaxSet");
                    float propHeight = EditorGUI.GetPropertyHeight(onMaxSetProp);
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, propHeight), onMaxSetProp);
                    yPos += propHeight + spacing;

                    SerializedProperty onMaxChangeProp = property.FindPropertyRelative("onMaxChange");
                    propHeight = EditorGUI.GetPropertyHeight(onMaxChangeProp);
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, propHeight), onMaxChangeProp);
                    yPos += propHeight + spacing;

                    SerializedProperty onMaxDownProp = property.FindPropertyRelative("onMaxDown");
                    propHeight = EditorGUI.GetPropertyHeight(onMaxDownProp);
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, propHeight), onMaxDownProp);
                    yPos += propHeight + spacing;

                    SerializedProperty onMaxUpProp = property.FindPropertyRelative("onMaxUp");
                    propHeight = EditorGUI.GetPropertyHeight(onMaxUpProp);
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, propHeight), onMaxUpProp);

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
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

