using UnityEngine;
using UnityEditor;
using JacobHomanics.HealthSystem;

namespace JacobHomanics.HealthSystem.Editor
{
    [CustomPropertyDrawer(typeof(Shield))]
    public class ShieldPropertyDrawer : PropertyDrawer
    {
        private static int dragControlID = -1;
        private static float dragStartValue;
        private static float dragStartMouseX;
        private static string dragPropertyPath;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Calculate rects
            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            Rect valueRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, (position.width - EditorGUIUtility.labelWidth) * 0.5f, EditorGUIUtility.singleLineHeight);
            Rect colorRect = new Rect(position.x + EditorGUIUtility.labelWidth + (position.width - EditorGUIUtility.labelWidth) * 0.5f, position.y, (position.width - EditorGUIUtility.labelWidth) * 0.5f, EditorGUIUtility.singleLineHeight);

            // Draw label
            EditorGUI.LabelField(labelRect, label);

            // Draw value field with clamping and drag support
            SerializedProperty valueProp = property.FindPropertyRelative("_value");
            if (valueProp != null)
            {
                float currentValue = valueProp.floatValue;
                int controlID = GUIUtility.GetControlID(FocusType.Passive);

                // Create a draggable rect that includes both label and value
                Rect dragRect = new Rect(valueRect.x - EditorGUIUtility.labelWidth, valueRect.y, EditorGUIUtility.labelWidth + valueRect.width, valueRect.height);

                Event evt = Event.current;

                string propertyPath = valueProp.propertyPath;

                switch (evt.type)
                {
                    case EventType.MouseDown:
                        if (dragRect.Contains(evt.mousePosition) && evt.button == 0)
                        {
                            dragControlID = controlID;
                            dragStartValue = currentValue;
                            dragStartMouseX = evt.mousePosition.x;
                            dragPropertyPath = propertyPath;
                            GUIUtility.hotControl = controlID;
                            evt.Use();
                        }
                        break;

                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == controlID && dragControlID == controlID && dragPropertyPath == propertyPath)
                        {
                            float delta = (evt.mousePosition.x - dragStartMouseX) * 0.1f; // Sensitivity
                            float newValue2 = dragStartValue + delta;
                            valueProp.floatValue = Mathf.Max(0, newValue2);
                            evt.Use();
                        }
                        break;

                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == controlID && dragControlID == controlID && dragPropertyPath == propertyPath)
                        {
                            dragControlID = -1;
                            dragPropertyPath = null;
                            GUIUtility.hotControl = 0;
                            evt.Use();
                        }
                        break;
                }

                // Show drag cursor when hovering over draggable area
                if (dragRect.Contains(evt.mousePosition))
                {
                    EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.SlideArrow);
                }

                // Draw the float field (still allows direct editing)
                EditorGUI.BeginChangeCheck();
                float newValue = EditorGUI.FloatField(valueRect, currentValue);
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

