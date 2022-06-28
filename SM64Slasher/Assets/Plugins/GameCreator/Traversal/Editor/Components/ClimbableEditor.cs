using System.Collections.Generic;
using GameCreator.Core;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace GameCreator.Traversal
{
    [CustomEditor(typeof(Climbable), true)]
    public class ClimbableEditor : TraversableComponentEditor
    {
        private static readonly Color COLOR_ANGLE = new Color(0f, 0.25f, 1f);
        private static readonly Color COLOR_PATH = new Color(1f, 0.9f, 0f);
        private static readonly Color COLOR_CONNECTIONS = new Color(0.5f, 1f, .0f);
        private static readonly Color COLOR_REACHABLES = new Color(0f, 1f, .0f);
        
        private const float THICKNESS_PATH = 5f;
        private const float THICKNESS_ANGLE = 10f;
        private const float THICKNESS_CONNECTIONS = 3f;
        private const float THICKNESS_REACHABLES = 1f;
        
        private const float RESOLUTION = 0.1f;

        private static bool HANDLES_EDIT_MODE = false;

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spPath;
        private SerializedProperty spClip;
        private SerializedProperty spCanDrop;
        
        private SerializedProperty spPathA;
        private SerializedProperty spPathB;
        
        private SerializedProperty spPathPositionA;
        private SerializedProperty spPathPositionB;
        
        private SerializedProperty spPathAngleA;
        private SerializedProperty spPathAngleB;

        private SerializedProperty spReachables;

        private SerializedProperty spCommuteToA;
        private SerializedProperty spCommuteToB;

        private SerializedProperty spAutoConnect;

        private Vector3 lastTransformPosition;
        private Quaternion lastTransformRotation;
        private Vector3 lastTransformScale;

        private ReorderableList reachableslist;

        // INITIALIZE: ----------------------------------------------------------------------------

        protected virtual void OnEnable()
        {
            this.spPath = this.serializedObject.FindProperty("path");
            this.spClip = this.serializedObject.FindProperty("clip");
            this.spCanDrop = this.serializedObject.FindProperty("canDrop");

            this.spCommuteToA = this.serializedObject.FindProperty("commuteToA");
            this.spCommuteToB = this.serializedObject.FindProperty("commuteToB");
            
            this.spAutoConnect = this.serializedObject.FindProperty("autoConnect");

            this.spPathA = this.spPath.FindPropertyRelative("pointA");
            this.spPathB = this.spPath.FindPropertyRelative("pointB");
            
            this.spPathPositionA = this.spPathA.FindPropertyRelative("position");
            this.spPathPositionB = this.spPathB.FindPropertyRelative("position");
            
            this.spPathAngleA = this.spPathA.FindPropertyRelative("angle");
            this.spPathAngleB = this.spPathB.FindPropertyRelative("angle");

            this.spReachables = this.serializedObject.FindProperty("reachables");
            this.reachableslist = new ReorderableList(this.serializedObject, this.spReachables)
            {
                displayAdd = true,
                displayRemove = true,
                draggable = true,
                drawHeaderCallback = this.DrawReachables_Header,
                drawElementCallback = this.DrawReachables_Element
            };
            
            Climbable climbable = this.target as Climbable;

            if (climbable != null)
            {
                this.lastTransformPosition = climbable.transform.position;
                this.lastTransformRotation = climbable.transform.rotation;
                this.lastTransformScale = climbable.transform.lossyScale;
            }
        }

        private void DrawReachables_Header(Rect rect)
        {
            EditorGUI.LabelField(rect, "Reachable Elements");
        }

        private void DrawReachables_Element(Rect rect, int index, bool isactive, bool isfocused)
        {
            Rect rectElement = new Rect(
                rect.x,
                rect.y + (rect.height / 2f - EditorGUIUtility.singleLineHeight / 2f),
                rect.width,
                EditorGUIUtility.singleLineHeight
            );
            
            EditorGUI.PropertyField(
                rectElement,
                this.spReachables.GetArrayElementAtIndex(index),
                new GUIContent("Reachable " + index)
            );
        }

        // INSPECTOR GUI: -------------------------------------------------------------------------
        
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            
            EditorGUILayout.PropertyField(this.spClip);
            EditorGUILayout.PropertyField(this.spCanDrop);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Climbable Path A", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(this.spPathA);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Climbable Path B", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(this.spPathB);
            
            EditorGUILayout.PropertyField(this.spCommuteToA, new GUIContent("Commute From A"));
            EditorGUILayout.PropertyField(this.spCommuteToB, new GUIContent("Commute From B"));
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spAutoConnect);

            string editModeLabel = HANDLES_EDIT_MODE
                ? "Exit Edit Mode"
                : "Enter Edit Mode";
            
            if (HANDLES_EDIT_MODE) GUI.backgroundColor = Color.grey;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button(editModeLabel))
            {
                HANDLES_EDIT_MODE = !HANDLES_EDIT_MODE;
                if (HANDLES_EDIT_MODE) Tools.current = Tool.None;
                
                SceneView.RepaintAll();
            }
            
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.Space();
            this.reachableslist.DoLayoutList();
            
            this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            
            Climbable climbable = this.target as Climbable;

            if (this.spAutoConnect.boolValue)
            {
                bool transformHasChanged = (
                    this.lastTransformPosition != climbable.transform.position ||
                    this.lastTransformRotation != climbable.transform.rotation ||
                    this.lastTransformScale != climbable.transform.lossyScale
                );

                if (transformHasChanged)
                {
                    this.GenerateAutoConnections();
                }
            }

            this.lastTransformPosition = climbable.transform.position;
            this.lastTransformRotation = climbable.transform.rotation;
            this.lastTransformScale = climbable.transform.lossyScale;
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void GenerateAutoConnections()
        {
            this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            this.serializedObject.Update();

            Climbable climbable = this.target as Climbable;
            List<TraversableComponent> reachables = climbable.GetAutoConnectionsElements();
                    
            this.spReachables.ClearArray();
            foreach (TraversableComponent reachable in reachables)
            {
                this.spReachables.AddToObjectArray(reachable);
            }
                    
            this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            this.serializedObject.Update();
        }

        // SCENE GUI: ----------------------------------------------------------------------------- 
        
        private void OnSceneGUI()
        {
            Climbable climbable = this.target as Climbable;
            
            Vector3 positionA = climbable.path.GetPointA(climbable);
            Vector3 positionB = climbable.path.GetPointB(climbable);
            
            if (HANDLES_EDIT_MODE)
            {
                this.DrawPathPositionHandle(this.spPathPositionA, climbable);
                this.DrawPathPositionHandle(this.spPathPositionB, climbable);
            
                this.DrawPathRotationHandle(this.spPathAngleA, positionA);
                this.DrawPathRotationHandle(this.spPathAngleB, positionB);
            }

            this.DrawLine(positionA, positionB, COLOR_PATH, THICKNESS_PATH);
            this.DrawReachables(climbable);

            this.DrawPathDirection(positionA, climbable.path.GetDirectionA(climbable) * 0.5f);
            this.DrawPathDirection(positionB, climbable.path.GetDirectionB(climbable) * 0.5f);
        }

        private void DrawReachables(Climbable climbable)
        {
            Vector3 climbablePointA = climbable.path.GetPointA(climbable);
            Vector3 climbablePointB = climbable.path.GetPointB(climbable);
            
            float lengthClimbable = Vector3.Distance(
                climbablePointA,
                climbablePointB
            );
            
            foreach (TraversableComponent reachable in climbable.reachables)
            {
                if (reachable is Climbable reachableClimb)
                { 
                    float divisions = Mathf.Round(lengthClimbable / RESOLUTION);
                    bool firstIteration = true;
                    
                    for (float i = 0f; i < divisions || firstIteration; i++)
                    {
                        firstIteration = false;
                        float t = divisions < 0.01f ? 0f : i / divisions;
                        
                        Vector3 position = Vector3.Lerp(
                            climbablePointA, 
                            climbablePointB, 
                            t
                        );

                        Vector3 position2 = Segment.GetClosestClimbablePoint(position, reachableClimb);
                        float maxDistance = climbable.clip != null
                            ? climbable.clip.maxTraverseDistance
                            : 0f;
                        
                        if (Vector3.Distance(position, position2) > maxDistance)
                        {
                            continue;
                        }
                        
                        this.DrawLine(
                            position, 
                            position2,
                            COLOR_REACHABLES,
                            THICKNESS_REACHABLES
                        );
                    }
                }
                else if (reachable is Obstacle)
                {
                    Vector3 pointObstacle = reachable.transform.position; 
                    Vector3 pointClimbable = Segment.NearestSegmentPoint(
                        climbablePointA,
                        climbablePointB,
                        pointObstacle
                    );
                    
                    this.DrawLine(
                        pointObstacle, 
                        pointClimbable,
                        COLOR_REACHABLES,
                        THICKNESS_REACHABLES
                    );   
                }
            }
        }

        private void DrawPathPositionHandle(SerializedProperty property, Climbable climbable)
        {
            this.serializedObject.Update();
            Vector3 position = property.vector3Value;

            EditorGUI.BeginChangeCheck();
            
            position = Handles.PositionHandle(
                climbable.transform.TransformPoint(position),
                climbable.transform.rotation
            );
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Move Climbable Point");
                
                property.vector3Value = climbable.transform.InverseTransformPoint(position);
                
                this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                this.serializedObject.Update();
                
                this.GenerateAutoConnections();
            }
        }

        private void DrawPathRotationHandle(SerializedProperty property, Vector3 position)
        {
            this.serializedObject.Update();
            float angle = property.floatValue;

            EditorGUI.BeginChangeCheck();
            
            Quaternion rotation = Handles.Disc(
                Quaternion.Euler(0f, angle, 0f), 
                position, Vector3.up, 
                0.5f, false, 0f
            );
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Rotate Climbable Angle");
                
                property.floatValue = rotation.eulerAngles.y;
                this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private void DrawPathDirection(Vector3 position, Vector3 direction)
        {
            Handles.DrawBezier(
                position,
                position + direction,
                position,
                position + direction, 
                COLOR_ANGLE,
                null,
                THICKNESS_ANGLE
            );
        }

        private void DrawLine(Vector3 positionA, Vector3 positionB, Color color, float thickness)
        {
            if (Vector3.Distance(positionA, positionB) < 0.01f) return;
            
            Handles.DrawBezier(
                positionA,
                positionB,
                positionA,
                positionB, 
                color,
                null,
                thickness
            );
        }
    }
}