namespace GameCreator.Traversal
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(Location))]
    public class LocationDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect rectMode = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight
            );
            
            Rect rectValue = new Rect(
                rectMode.x,
                rectMode.y + rectMode.height + EditorGUIUtility.standardVerticalSpacing,
                rectMode.width,
                EditorGUIUtility.singleLineHeight
            );

            SerializedProperty propertyType = property.FindPropertyRelative("type");
            SerializedProperty propertyPosition = property.FindPropertyRelative("position");
            SerializedProperty propertyTransform = property.FindPropertyRelative("transform");
            
            EditorGUI.PropertyField(rectMode, propertyType, label);

            EditorGUI.indentLevel++;
            switch (propertyType.intValue)
            {
                case 0: // Position
                    EditorGUI.PropertyField(rectValue, propertyPosition);
                    break;
                
                case 1: // Transform
                    EditorGUI.PropertyField(rectValue, propertyTransform);
                    break;
            }
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return PropertyHeight();
        }

        public static float PropertyHeight()
        {
            return (
                EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing
            );
        }
    }   
}
