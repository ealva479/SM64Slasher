using UnityEngine;
using System.Collections;
using GameCreator.Core;
using System;

namespace NJG.Graph
{
    [System.Serializable]
    public class Transition : ScriptableObject
    {
#if UNITY_EDITOR
        [NonSerialized]
        public float progress;

        //[NonSerialized]
        //public IConditionsList conditionList;

#endif
        
        public Node toNode;
        public Node fromNode;
        public bool useConditions;
        public bool isNegative;
        public bool mute;
        public IConditionsList GetConditions(GraphReactions reactions)
        {
            //Debug.Log("Conditions reactions " + reactions+" / "+this);
            if (!reactions) reactions = this.fromNode.Root.sourceReactions;
            return reactions ? reactions.GetCondition<IConditionsList>(this) : null;
        }

        public int id;

        [NonSerialized]
        public bool entered;

        // PROPERTIES: ----------------------------------------------------------------------------

        public void Init(Node toNode, Node fromNode)
        {
            this.toNode = toNode;
            this.fromNode = fromNode;
        }

        public Node Validate(Node nodeInvoker, GraphReactions reactions, GameObject invoker)
        {
            if (mute)
            {
                return null;
            }

            if (useConditions)
            {
                IConditionsList conditions = GetConditions(reactions);

                if (conditions == null)
                {
                    Debug.Log("Transition conditions are null.", reactions);
                    return null;
                }

                bool conditionsMet = conditions.Check(invoker);
                if (isNegative && conditionsMet) return null;
                if (!isNegative && !conditionsMet) return null;
            }

            if (nodeInvoker.Equals(fromNode))
            {
                entered = true;
            }

            return toNode;
        }

#if UNITY_EDITOR
        private void OnDestroy()
        {
            if (!Application.isPlaying && fromNode && fromNode.Root && fromNode.Root.sourceReactions)
            {
                IConditionsList conditions = GetConditions(fromNode.Root.sourceReactions);
                if (conditions)
                {
                    foreach (var c in conditions.conditions)
                    {
                        DestroyImmediate(c, true);
                    }
                    DestroyImmediate(conditions, true);
                }
            }
        }
#endif
    }
}