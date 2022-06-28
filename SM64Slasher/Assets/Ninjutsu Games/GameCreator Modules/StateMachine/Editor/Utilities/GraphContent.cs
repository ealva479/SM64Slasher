using UnityEngine;
using System.Collections;

namespace NJG.Graph
{
    public static class GraphContent
    {
        public static readonly GUIContent CreateActions;
        public static readonly GUIContent CreateTrigger;
        public static GUIContent CreateStateMachine;
        public static GUIContent CreateSubFsm;

        public static GUIContent MakeTransition;
        public static GUIContent SetAsDefault;
        public static GUIContent Copy;
        public static GUIContent CopyCurrent;
        public static GUIContent Paste;
        public static GUIContent Delete;
        public static GUIContent moveToSubStateMachine;
        public static GUIContent moveToParentStateMachine;

        static GraphContent()
        {
            CreateActions = new GUIContent("Create Actions");
            CreateTrigger = new GUIContent("Create Trigger");
            CreateStateMachine = new GUIContent("Create State Machine");
            CreateSubFsm = new GUIContent("Create Sub-State Machine");
            MakeTransition = new GUIContent("Make Transition");
            SetAsDefault = new GUIContent("Set As Default");
            Copy = new GUIContent("Copy");
            CopyCurrent = new GUIContent("Copy current StateMachine");
            Delete = new GUIContent("Delete");
            Paste = new GUIContent("Paste");
            moveToSubStateMachine = new GUIContent("Move To Sub-State Machine");
            moveToParentStateMachine = new GUIContent("Move To Parent-Sate Machine");
        }
    }
}