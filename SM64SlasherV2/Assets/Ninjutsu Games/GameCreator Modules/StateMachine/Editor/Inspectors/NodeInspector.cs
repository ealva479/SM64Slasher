using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace NJG.Graph
{
    [CustomEditor(typeof(Node), true)]
    public class NodeInspector : Editor
    {
        protected static string GUI_WARNING = "Some of your actions require scene references. " +
            "Drag and Drop this State Machine to the scene in order to be able to set scene references";

        private const string ICONS_PATH = "Assets/Ninjutsu Games/GameCreator Modules/StateMachine/Icons/Core/{0}";
        //private const string ICONS_PATH = "Assets/Plugins/GameCreator/Extra/Icons/Actions/{0}";

        private static GUIContent GUI_TRANSITION = new GUIContent("Transition Mode", "Parallel = All transitions are executed at once.\n\n" +
            "Selective = Tries executing the first transition if its not successful it will continue.\nUseful for If/Else cases.");

        public static NodeInspector Instance { get; private set; }

        protected TransitionInspector transitionEditor;
        protected Node node;

        protected Texture2D icon;
        protected SerializedProperty spComments;
        protected SerializedProperty spDescription;
        protected SerializedProperty spTransitionMode;
        protected static GUIContent GUIDescription;
        protected static GUIContent GUIComment;
        protected bool throwReferenceWarning;

        public virtual void OnEnable()
        {
            if (serializedObject == null) return;

            node = target as Node;
            transitionEditor = new TransitionInspector(node, this);
            transitionEditor.OnEnable();
            //Undo.undoRedoPerformed += OnUndoRedo;
            //Editor.finishedDefaultHeaderGUI += CustomHeaderGUI;

            if (GUIDescription == null)
            {
                Texture2D iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(ICONS_PATH, "Description.png"));
                GUIDescription = new GUIContent(" Description", iconTexture);
            }
            if (GUIComment == null)
            {
                Texture2D iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(ICONS_PATH, "Comment.png"));
                GUIComment = new GUIContent(" Comment", iconTexture);
            }
            spComments = serializedObject.FindProperty("enableComments");
            spDescription = serializedObject.FindProperty("description");
            spTransitionMode = serializedObject.FindProperty("transitionMode");

            icon = AssetPreview.GetMiniThumbnail(target);

            Instance = this;
        }

        private void CustomHeaderGUI(Editor obj)
        {
        }

        private void OnUndoRedo()
        {
        }

        protected virtual bool ShouldDrawCustomHeader()
        {
            return true;
        }

        public virtual void OnDisable()
        {
            //Undo.undoRedoPerformed -= OnUndoRedo;
            //Editor.finishedDefaultHeaderGUI -= CustomHeaderGUI;

            // transitionEditor?.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (node.transitions.Length > 0)
            {
                EditorGUILayout.PropertyField(spTransitionMode, GUI_TRANSITION);
                EditorGUILayout.Space();
                transitionEditor.OnInspectorGUI();
            }
            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(node);
                GraphEditor.RepaintAll();
            }
        }

        protected void DrawDescription()
        {
            if(node is StateMachine)
            {
                // EditorGUILayout.LabelField(GUIDescription);
                // node.description = EditorGUILayout.TextArea(node.description, GraphStyles.DescriptionTextArea,
                //     GUILayout.MinHeight(44));
            }
            else
            {
                // EditorGUILayout.LabelField(GUIComment);
                // node.description = EditorGUILayout.TextArea(node.description, GraphStyles.DescriptionTextArea,
                //     GUILayout.MinHeight(44));
            }
            serializedObject.Update();
            EditorGUILayout.PropertyField(spDescription, node is StateMachine ? GUIDescription : GUIComment);
            if(!(node is StateMachine) && (node as StateMachine) != node.Root) node.Root.disableComments = EditorGUILayout.ToggleLeft("Hide Comments", node.Root.disableComments);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();

        }

        public void Refresh()
        {
            transitionEditor.ResetTransitionList();
            Repaint();
        }

        protected override void OnHeaderGUI()
        {
            if (ShouldDrawCustomHeader()) DrawCustomHeader();
            else base.OnHeaderGUI();
        }

        protected virtual void DrawCustomHeader()
        {
            GUILayout.BeginVertical("IN BigTitle");
            EditorGUIUtility.labelWidth = 50;

            GUILayout.BeginHorizontal(GUILayout.Height(35));
            GUILayout.Label(icon, GUILayout.Width(32), GUILayout.Height(32));
            node.name = EditorGUILayout.TextField(string.Empty, node.name);
            GUILayout.EndHorizontal();

            //EditorGUILayout.LabelField(GUIComment);
            //EditorGUILayout.PropertyField(spComment, GUIContent.none); 

            GUILayout.EndVertical();

            if (GUI.changed)
            {
                node.hasCustomName = !string.IsNullOrEmpty(node.name);
                EditorUtility.SetDirty(node);
                GraphEditor.RepaintAll();
            }
        }

        public static void Dirty()
        {
            NodeInspector[] editors = (NodeInspector[])Resources.FindObjectsOfTypeAll(typeof(NodeInspector));
            foreach (NodeInspector inspector in editors)
            {
                inspector.MarkDirty();
            }
        }

        protected virtual void MarkDirty()
        {
            transitionEditor?.OnEnable();

        }

    }
}