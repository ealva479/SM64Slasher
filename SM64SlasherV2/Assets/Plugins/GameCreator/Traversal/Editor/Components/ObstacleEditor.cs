using GameCreator.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GameCreator.Traversal
{
    [CustomEditor(typeof(Obstacle), true)]
    public class ObstacleEditor : TraversableComponentEditor
    {
        private static readonly Color COLOR_ALTERNATE_ROW = new Color(0,0,0, 0.1f);
        
        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spClip;
        private SerializedProperty spDelayIn;

        private SerializedProperty spObstaclePath;
        private SerializedProperty spObstaclePathMode;
        private SerializedProperty spObstaclePathPath;
        
        private ReorderableList pathList;
        
        // INITIALIZE: ----------------------------------------------------------------------------

        protected virtual void OnEnable()
        {
            this.spClip = this.serializedObject.FindProperty("clip");
            this.spDelayIn = this.serializedObject.FindProperty("delayIn");
            
            this.spObstaclePath = this.serializedObject.FindProperty("obstaclePath");
            this.spObstaclePathMode = this.spObstaclePath.FindPropertyRelative("mode");
            this.spObstaclePathPath = this.spObstaclePath.FindPropertyRelative("path");

            this.pathList = new ReorderableList(this.serializedObject, this.spObstaclePathPath)
            {
                displayAdd = true,
                displayRemove = true,
                draggable = true,
                elementHeight = (
                    ObstaclePathPointDrawer.PropertyHeight() + 
                    EditorGUIUtility.standardVerticalSpacing +
                    EditorGUIUtility.standardVerticalSpacing
                ),
                drawHeaderCallback = this.DrawPath_Header,
                drawElementCallback = this.DrawPath_Element,
            };
        }

        private void DrawPath_Header(Rect rect)
        {
            EditorGUI.LabelField(rect, "Obstacle Path");
        }

        private void DrawPath_Element(Rect rect, int index, bool isactive, bool isfocused)
        {
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            rect.height -= EditorGUIUtility.standardVerticalSpacing * 2f;

            bool isEdge = index == 0 || index == (this.spObstaclePathPath.arraySize - 1);
            
            ObstaclePathPointDrawer.DrawGUI(
                rect, 
                this.spObstaclePathPath.GetArrayElementAtIndex(index),
                new GUIContent("Point " + index + (isEdge ? " (Edge)" : "")), 
                isEdge
            );
        }

        // INSPECTOR GUI: -------------------------------------------------------------------------
        
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spClip);
            EditorGUILayout.PropertyField(this.spDelayIn);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Traverse", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(this.spObstaclePathMode);
            
            EditorGUILayout.Space();
            this.pathList.DoLayoutList();
            
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}