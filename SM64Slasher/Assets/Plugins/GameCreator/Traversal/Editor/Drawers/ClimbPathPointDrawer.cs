using UnityEditor;
using UnityEngine;

namespace GameCreator.Traversal
{
    [CustomPropertyDrawer(typeof(ClimbPath.Point))]
    public class ClimbPathPointDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty spPosition = property.FindPropertyRelative("position");
            SerializedProperty spAngle = property.FindPropertyRelative("angle");
            
            Rect rectPosition = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight
            );
            
            Rect rectAngle = new Rect(
                rectPosition.x,
                rectPosition.y + rectPosition.height + EditorGUIUtility.standardVerticalSpacing,
                rectPosition.width,
                EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(rectPosition, spPosition);
            EditorGUI.PropertyField(rectAngle, spAngle);
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (
                EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing
            );
        }
    }
}