using UnityEngine;
using UnityEditor;
using JacobHomanics.HealthSystem;

namespace JacobHomanics.HealthSystem.Editor
{
    [CustomPropertyDrawer(typeof(Shield))]
    public class ShieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Calculate rects
            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            Rect valueRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, (position.width - EditorGUIUtility.labelWidth) * 0.5f, EditorGUIUtility.singleLineHeight);
            Rect colorRect = new Rect(position.x + EditorGUIUtility.labelWidth + (position.width - EditorGUIUtility.labelWidth) * 0.5f, position.y, (position.width - EditorGUIUtility.labelWidth) * 0.5f, EditorGUIUtility.singleLineHeight);

            // Draw label
            EditorGUI.LabelField(labelRect, label);

            // Draw value field with clamping
            SerializedProperty valueProp = property.FindPropertyRelative("_value");
            if (valueProp != null)
            {
                EditorGUI.BeginChangeCheck();
                float newValue = EditorGUI.FloatField(valueRect, valueProp.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    valueProp.floatValue = Mathf.Max(0, newValue);
                }
            }
            else
            {
                // Fallback if _value property doesn't exist
                EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), GUIContent.none);
            }

            // Draw color field
            EditorGUI.PropertyField(colorRect, property.FindPropertyRelative("color"), GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}

