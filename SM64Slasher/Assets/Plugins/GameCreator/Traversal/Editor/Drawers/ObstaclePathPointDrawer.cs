using UnityEditor;
using UnityEngine;

namespace GameCreator.Traversal
{
    [CustomPropertyDrawer(typeof(ObstaclePoint))]
    public class ObstaclePathPointDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawGUI(position, property, label, true);
        }

        public static void DrawGUI(
            Rect position, SerializedProperty property, 
            GUIContent label, bool isEdge)
        {
            SerializedProperty spAngle = property.FindPropertyRelative("angle");
            SerializedProperty spCommuteTo = property.FindPropertyRelative("commuteTo");
            SerializedProperty spTransition = property.FindPropertyRelative("transition");
            SerializedProperty spEasing = property.FindPropertyRelative("easing");
            SerializedProperty spRestDuration = property.FindPropertyRelative("restDuration");
            SerializedProperty spLocation = property.FindPropertyRelative("location");
            
            Rect rectLabel = new Rect(
                position.x, position.y, 
                position.width, EditorGUIUtility.singleLineHeight
            );
            
            EditorGUI.LabelField(rectLabel, label, EditorStyles.boldLabel);
            
            Rect rectAngle = new Rect(
                rectLabel.x, rectLabel.y + rectLabel.height + EditorGUIUtility.standardVerticalSpacing, 
                position.width, EditorGUIUtility.singleLineHeight
            );
            
            EditorGUI.BeginDisabledGroup(!isEdge);
            EditorGUI.PropertyField(rectAngle, spAngle);
            EditorGUI.EndDisabledGroup();
            
            Rect rectCommuteTo = new Rect(
                rectAngle.x, rectAngle.y + rectAngle.height + EditorGUIUtility.standardVerticalSpacing, 
                position.width, EditorGUIUtility.singleLineHeight
            );
            
            EditorGUI.BeginDisabledGroup(!isEdge);
            EditorGUI.PropertyField(rectCommuteTo, spCommuteTo);
            EditorGUI.EndDisabledGroup();
            
            Rect rectTransition = new Rect(
                rectCommuteTo.x, rectCommuteTo.y + rectCommuteTo.height + EditorGUIUtility.standardVerticalSpacing, 
                position.width, TranslationDrawer.PropertyHeight()
            );
            
            EditorGUI.PropertyField(rectTransition, spTransition);
            
            Rect rectEasing = new Rect(
                rectTransition.x, rectTransition.y + rectTransition.height + EditorGUIUtility.standardVerticalSpacing, 
                position.width, EditorGUIUtility.singleLineHeight
            );
            
            EditorGUI.PropertyField(rectEasing, spEasing);
            
            Rect rectRestDuration = new Rect(
                rectEasing.x, rectEasing.y + rectEasing.height + EditorGUIUtility.standardVerticalSpacing, 
                position.width, EditorGUIUtility.singleLineHeight
            );
            
            EditorGUI.PropertyField(rectRestDuration, spRestDuration);
            
            Rect rectLocation = new Rect(
                rectRestDuration.x, rectRestDuration.y + rectRestDuration.height + EditorGUIUtility.standardVerticalSpacing, 
                position.width, LocationDrawer.PropertyHeight()
            );
            
            EditorGUI.PropertyField(rectLocation, spLocation);
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return PropertyHeight();
        }

        public static float PropertyHeight()
        {
            return (
                EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing + TranslationDrawer.PropertyHeight() +
                EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing + LocationDrawer.PropertyHeight()
            );
        }
    }
}