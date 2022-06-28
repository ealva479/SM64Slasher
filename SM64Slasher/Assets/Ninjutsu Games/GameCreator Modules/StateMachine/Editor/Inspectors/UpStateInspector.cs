using UnityEngine;
using UnityEditor;
using GameCreator.Core;
using UnityEditor.Callbacks;

namespace NJG.Graph
{
    [CustomEditor(typeof(UpState), true)]
    public class UpStateInspector : NodeInspector
    {
        protected static UpStateInspector stateInspector;
        protected UpState state;

        public override void OnEnable()
        {
            base.OnEnable();

            state = node as UpState;
            stateInspector = this;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            stateInspector = null;
        }

        protected override void DrawCustomHeader()
        {
            state = node as UpState;

            GUILayout.BeginVertical("IN BigTitle");
            EditorGUIUtility.labelWidth = 50;

            GUILayout.BeginHorizontal(GUILayout.Height(35));
            GUILayout.Label(icon, GUILayout.Width(32), GUILayout.Height(32));
            GUIContent content = new GUIContent(state.parent.parent == state.Root ? "(Up) Base" : "(Up) " + state.parent.parent.name);
            EditorGUILayout.LabelField(content);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("This node cannot have transitions or actions.", MessageType.Info);

            base.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}