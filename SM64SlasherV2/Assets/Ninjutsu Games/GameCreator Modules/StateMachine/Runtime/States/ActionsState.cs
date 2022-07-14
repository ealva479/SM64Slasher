using UnityEngine;
using GameCreator.Core;
using System.Collections.Generic;

namespace NJG.Graph
{
    [System.Serializable]
    public class ActionsState : Node
    {
        public bool resetState;
        public System.Action<GraphReactions> onExecute;

        public IActionsList GetActions(GraphReactions reactions)
        {
            // return this.Root.sourceReactions.GetReaction<IActionsList>(this);
            //Debug.Log("Actions reactions " + reactions + " / " + this);
            if (!reactions) reactions = this.Root.sourceReactions;
            return reactions ? reactions.GetReaction<IActionsList>(this) : null;
        }

        /*private void OnDestroy()
        {
#if UNITY_EDITOR
            bool deleteFromSource = Root != null && Root.Owner != null;

            if (Actions || deleteFromSource)
            {
                Root.RemoveReaction(this, deleteFromSource);                
            }
#endif
        }*/

        public override void OnEnter(GraphReactions reactions, GameObject invoker)
        {
            if (Application.isPlaying)
            {
#if UNITY_EDITOR
                //Debug.Log("is Selected " + isSelected);
                internalExecuting = true;
#endif
                base.OnEnter(reactions, invoker);
                IActionsList actions = GetActions(reactions);
                
                if (actions)
                {
                    //if (actions.isExecuting && resetState) actions.Stop();
                    //if (!actions.isExecuting)
                    onExecute?.Invoke(reactions);
                    actions.Execute(invoker, () => OnExit(reactions, invoker));
                }
                //reactions.nodesSubscribed.Add(id);
            }
        }

        public override void OnExit(GraphReactions reactions, GameObject invoker)
        {
            base.OnExit(reactions, invoker);

//            Debug.LogWarning("OnExit ActionsState " + this + " OnExit " + reactions);

            List<Node> nodes = ValidateTransitions(reactions, invoker);
            for (int i = 0, imax = nodes.Count; i < imax; i++)
            {
                // Debug.LogWarning("nodes " + nodes[i]);
                if(nodes[i] is StateMachine)
                {
                    var sm = nodes[i] as StateMachine;
                    if (sm.startNode) sm.startNode.OnEnter(reactions, invoker);
                }
                nodes[i].OnEnter(reactions, invoker);
            }
            //reactions.nodesSubscribed.Remove(id);

            //this.isEntered = false;
        }
    }
}