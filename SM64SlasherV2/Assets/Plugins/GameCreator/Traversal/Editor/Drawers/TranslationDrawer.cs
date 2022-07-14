namespace GameCreator.Traversal
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(Translation))]
    public class TranslationDrawer : PropertyDrawer
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

            SerializedProperty propertyMode = property.FindPropertyRelative("mode");
            SerializedProperty propertySpeed = property.FindPropertyRelative("speed");
            SerializedProperty propertyDuration = property.FindPropertyRelative("duration");
            
            EditorGUI.PropertyField(rectMode, propertyMode, label);

            EditorGUI.indentLevel++;
            switch (propertyMode.intValue)
            {
                case 0: // BySpeed
                    EditorGUI.PropertyField(rectValue, propertySpeed);
                    break;
                
                case 1: // ByDuration
                    EditorGUI.PropertyField(rectValue, propertyDuration);
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
