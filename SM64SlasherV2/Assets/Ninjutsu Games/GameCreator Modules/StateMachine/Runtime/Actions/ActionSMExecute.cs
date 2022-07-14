#if UNITY_EDITOR
using UnityEditor;
#endif

using GameCreator.Core;
using GameCreator.Variables;
using UnityEngine;

namespace NJG.Graph
{


    [AddComponentMenu("")]
    public class ActionSMExecute : IAction
    {
        public TargetGameObject stateMachine = new TargetGameObject(TargetGameObject.Target.Invoker);

        private StateMachineController sm;

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            GameObject go = null;
            if (sm == null)
            {
                go = stateMachine.GetGameObject(target);
                sm = go.GetComponentInChildren<StateMachineController>();
            }

            if (sm != null)
            {
                sm.Execute();
            }
            else
            {
                Debug.LogWarning("Couldn't find State Machine on: " + go, target);
            }

            return true;
        }

#if UNITY_EDITOR
        public static new string NAME = "State Machine/Execute State Machine";
        private const string NODE_TITLE = "Execute State Machine {0}";
        public const string CUSTOM_ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/StateMachine/Icons/Actions/";

        public override string GetNodeTitle()
        {
            return string.Format(NODE_TITLE, stateMachine.ToString());
        }
        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spState;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        protected override void OnEnableEditorChild()
        {
            this.spState = serializedObject.FindProperty("stateMachine");
        }

        protected override void OnDisableEditorChild()
        {
            this.spState = null;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spState);
            this.serializedObject.ApplyModifiedProperties();
        }

#endif
    }
}
