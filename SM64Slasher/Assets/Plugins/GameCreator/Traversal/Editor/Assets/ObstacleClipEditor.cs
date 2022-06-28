using System;

namespace GameCreator.Traversal
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(ObstacleClip))]
    public class ObstacleClipEditor : Editor
    {
        private SerializedProperty spRotationMode;
        private SerializedProperty spRotationDuration;

        private SerializedProperty spIgnoreCollisions;
        private SerializedProperty spAnimation;
        private SerializedProperty spAnimationClip;
        private SerializedProperty spCharacterState;
        private SerializedProperty spRandomAnimationClips;

        private SerializedProperty spHandlesOffsetUpward;
        private SerializedProperty spHandlesOffsetForward;

        private SerializedProperty spCurveOffsetX;
        private SerializedProperty spCurveOffsetY;
        private SerializedProperty spCurveOffsetZ;

        private SerializedProperty spTransitionIn;
        private SerializedProperty spTransitionOut;

        private SerializedProperty spExitSpeed;
        
        // ENABLE: --------------------------------------------------------------------------------
        
        private void OnEnable()
        {
            this.spRotationMode = this.serializedObject.FindProperty("rotationMode");
            this.spRotationDuration = this.serializedObject.FindProperty("rotationDuration");

            this.spIgnoreCollisions = this.serializedObject.FindProperty("ignoreCollisions");
            this.spAnimation = this.serializedObject.FindProperty("animation");
            this.spAnimationClip = this.serializedObject.FindProperty("animationClip");
            this.spCharacterState = this.serializedObject.FindProperty("characterState");
            this.spRandomAnimationClips = this.serializedObject.FindProperty("randomAnimationClips");

            this.spHandlesOffsetUpward = this.serializedObject.FindProperty("handlesOffsetUpward");
            this.spHandlesOffsetForward = this.serializedObject.FindProperty("handlesOffsetForward");

            this.spCurveOffsetX = this.serializedObject.FindProperty("curveOffsetX");
            this.spCurveOffsetY = this.serializedObject.FindProperty("curveOffsetY");
            this.spCurveOffsetZ = this.serializedObject.FindProperty("curveOffsetZ");

            this.spTransitionIn = this.serializedObject.FindProperty("transitionIn");
            this.spTransitionOut = this.serializedObject.FindProperty("transitionOut");

            this.spExitSpeed = this.serializedObject.FindProperty("exitSpeed");
        }
        
        // PAINT: ---------------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            
            EditorGUILayout.PropertyField(this.spRotationMode);
            EditorGUILayout.PropertyField(this.spRotationDuration);

            EditorGUILayout.PropertyField(this.spIgnoreCollisions);
            EditorGUILayout.PropertyField(this.spAnimation);

            EditorGUI.indentLevel++;
            switch ((ObstacleClip.Animation)this.spAnimation.enumValueIndex)
            {
                case ObstacleClip.Animation.AnimationClip:
                    EditorGUILayout.PropertyField(this.spAnimationClip);
                    break;
                
                case ObstacleClip.Animation.CharacterState:
                    EditorGUILayout.PropertyField(this.spCharacterState);
                    break;
                
                case ObstacleClip.Animation.RandomAnimationClip:
                    EditorGUILayout.PropertyField(this.spRandomAnimationClips);
                    break;
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spHandlesOffsetUpward);
            EditorGUILayout.PropertyField(this.spHandlesOffsetForward);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tweak Movement:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(this.spCurveOffsetX);
            EditorGUILayout.PropertyField(this.spCurveOffsetY);
            EditorGUILayout.PropertyField(this.spCurveOffsetZ);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spTransitionIn);
            EditorGUILayout.PropertyField(this.spTransitionOut);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spExitSpeed);

            this.serializedObject.ApplyModifiedProperties();
        }
    }   
}
