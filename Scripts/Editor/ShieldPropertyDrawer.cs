using UnityEngine;
using UnityEditor;
using JacobHomanics.HealthSystem;

namespace JacobHomanics.HealthSystem.Editor
{
    [CustomPropertyDrawer(typeof(Shield))]
    public class ShieldPropertyDrawer : PropertyDrawer
    {
        private static string draggingPropertyPath;
        private static float dragStartValue;
        private static float dragStartMouseX;
        private static bool isDragging;
        private const float dragThreshold = 3f;

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
                string propertyPath = valueProp.propertyPath;
                float currentValue = valueProp.floatValue;

                // Create draggable area (label + value field)
                Rect dragRect = new Rect(position.x, position.y, valueRect.x + valueRect.width - position.x, EditorGUIUtility.singleLineHeight);

                Event evt = Event.current;
                int controlID = GUIUtility.GetControlID(propertyPath.GetHashCode(), FocusType.Passive);

                // Handle drag events
                if (evt.type == EventType.MouseDown && evt.button == 0 && dragRect.Contains(evt.mousePosition))
                {
                    draggingPropertyPath = propertyPath;
                    dragStartValue = currentValue;
                    dragStartMouseX = evt.mousePosition.x;
                    isDragging = false;
                    // Don't set hotControl yet - wait to see if it's a drag or click
                }
                else if (evt.type == EventType.MouseDrag && draggingPropertyPath == propertyPath)
                {
                    float deltaX = Mathf.Abs(evt.mousePosition.x - dragStartMouseX);
                    
                    if (!isDragging && deltaX > dragThreshold)
                    {
                        // Start dragging after threshold
                        isDragging = true;
                        GUIUtility.hotControl = controlID;
                    }
                    
                    if (isDragging)
                    {
                        float delta = (evt.mousePosition.x - dragStartMouseX) * 0.1f;
                        float newValue2 = dragStartValue + delta;
                        valueProp.floatValue = Mathf.Max(0, newValue2);
                        evt.Use();
                    }
                }
                else if (evt.type == EventType.MouseUp && draggingPropertyPath == propertyPath)
                {
                    if (isDragging)
                    {
                        GUIUtility.hotControl = 0;
                    }
                    draggingPropertyPath = null;
                    isDragging = false;
                }

                // Show drag cursor
                if (dragRect.Contains(evt.mousePosition))
                {
                    EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.SlideArrow);
                }

                // Draw the float field
                EditorGUI.BeginChangeCheck();
                float newValue = EditorGUI.FloatField(valueRect, currentValue);
                if (EditorGUI.EndChangeCheck())
                {
                    valueProp.floatValue = Mathf.Max(0, newValue);
                    // Cancel drag if user typed a value
                    if (draggingPropertyPath == propertyPath)
                    {
                        draggingPropertyPath = null;
                        GUIUtility.hotControl = 0;
                    }
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

