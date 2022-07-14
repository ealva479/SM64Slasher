using UnityEditor;
using UnityEngine;

namespace NJG.Graph
{
    [CustomEditor(typeof(TriggerState), true)]
    public class TriggerStateInspector : NodeInspector
    {
        private GraphTriggerInspector triggerEditor;
        private TriggerState state;
        private int lastCount;

        public override void OnEnable()
        {
            base.OnEnable();

            state = node as TriggerState;
            triggerEditor = (GraphTriggerInspector)CreateEditor(state.GetTrigger(GraphEditor.Reactions));

            if (triggerEditor)
            {
                GraphTrigger trigger = triggerEditor.target as GraphTrigger;
                lastCount = trigger.igniters.Count;
            }

            if (!GraphEditor.Controller)
            {
                var controllers = FindObjectsOfType<StateMachineController>();
                foreach (var controller in controllers)
                {
                    if (controller.stateMachine == GraphEditor.Active)
                    {
                        GraphEditor.SelectGameObject(controller.gameObject);
                    }
                }
            }

            if (GraphEditor.Controller)
            {
                GraphEditor.Controller.canDrawCollider = true;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (GraphEditor.Controller)
            {
                GraphEditor.Controller.canDrawCollider = false;
            }
        }

        public override void OnInspectorGUI()
        {
            state = node as TriggerState;
            serializedObject.Update();
            
            DrawDescription();

            if (triggerEditor)
            {
                EditorGUI.BeginChangeCheck();
                triggerEditor.OnInspectorGUI();

                GraphTrigger trigger = triggerEditor.target as GraphTrigger;
                bool hasChanged = EditorGUI.EndChangeCheck() || lastCount != trigger.igniters.Count || GraphTriggerInspector.HasChanged;
                bool isInstance = GraphEditor.Controller != null && GraphEditor.Controller.stateMachine && GraphEditor.Controller.reactions;
               
                if (hasChanged)
                {
                    lastCount = trigger.igniters.Count;
                    GraphTriggerInspector.HasChanged = false;
                    GraphUtility.ReplaceLocalVarTargets(trigger.igniters.Values);
                    if (isInstance)
                    {
                        GraphUtility.CopyTrigger(GraphEditor.Controller.reactions.GetReaction<GraphTrigger>(state),
                            GraphEditor.Controller.stateMachine.sourceReactions.GetReaction<GraphTrigger>(state));
                    }
                }
            }
            EditorGUILayout.Space();
            base.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}