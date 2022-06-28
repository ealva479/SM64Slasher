using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace NJG.Graph
{
    public static class Pasteboard
    {
        private static List<Node> nodes = new List<Node>();
        private static GraphReactions reactions;
        private static GraphReactions transitionReactions;
        public static Transition copyTransition;

        public static void CopyConditions(Transition transition, GraphReactions fromReactions)
        {
            transitionReactions = fromReactions;
            copyTransition = transition;
        }

        public static void PasteConditions(Transition targetTransition, GraphReactions toReactions)
        {
            targetTransition.useConditions = true;
            GraphUtility.CopyIConditionList(copyTransition.GetConditions(transitionReactions), targetTransition.GetConditions(toReactions));
            EditorUtility.SetDirty(targetTransition);
            EditorUtility.SetDirty(targetTransition.fromNode);
            EditorUtility.SetDirty(targetTransition.fromNode.Root);
            EditorUtility.SetDirty(toReactions);
            copyTransition = null;
        }

        public static void ClearConditions()
        {
            copyTransition = null;
        }

        public static void Copy(List<Node> copiedNodes, GraphReactions copyFromReactions)
        {
            nodes = new List<Node>(copiedNodes);
            reactions = copyFromReactions;
        }

        public static void Paste(Vector2 position, StateMachine stateMachine, GraphReactions overrideReactions, bool checkNames)// = null
        {
            List<Node> copiedNodes = new List<Node>();
            Vector2 center = GetCenter(nodes);

            for (int i = 0; i < nodes.Count; i++)
            {
                Node origNode = nodes[i];

                if (!origNode) continue;
                
                if(origNode is UpState)
                {
                    if(stateMachine.parent == null)
                    {
                        Debug.LogWarningFormat("Trying to copy upNode in root");
                        continue;
                    }
                }

                Node copiedNode = (Node)GraphUtility.Copy(origNode, reactions, overrideReactions); // ?? stateMachine.sourceReactions
                copiedNode.parent = stateMachine;
                copiedNode.hideFlags = HideFlags.HideInHierarchy;
                if (!stateMachine.startNode)
                {
                    GraphUtility.SetDefaultNode(copiedNode, stateMachine);
                }
                else
                {
                    copiedNode.IsStartNode = false;
                }

                var updatedNodes = stateMachine.nodes;
                ArrayUtility.Add<Node>(ref updatedNodes, copiedNode);
                stateMachine.nodes = updatedNodes;

                copiedNode.position = new Rect(-(center.x - origNode.position.x) + position.x, -(center.y - origNode.position.y) + position.y, GraphStyles.StateWidth, GraphStyles.StateHeight);

                if (copiedNode is StateMachine || copiedNode is TriggerState)
                {
                    copiedNode.position.width = GraphStyles.StateMachineWidth;
                    copiedNode.position.height = GraphStyles.StateMachineHeight;
                }

                GraphUtility.UpdateNodeColor(copiedNode);
                copiedNodes.Add(copiedNode);
            }

            for (int i = 0; i < copiedNodes.Count; i++)
            {
                Node node = copiedNodes[i];
                if (node is UpState)
                {
                    bool mOverride = EditorUtility.DisplayDialog("Override UpState", "UpState can only exist once per state machine. Do you want to override it?", "Yes", "No");
                    UpState upState = stateMachine.nodes.ToList().Find(x => x.GetType() == typeof(UpState) && (mOverride && x != node || !mOverride && x == node)) as UpState;

                    var updatedNodes = stateMachine.nodes;
                    ArrayUtility.Remove(ref updatedNodes, upState);
                    stateMachine.nodes = updatedNodes;

                    GraphUtility.DestroyImmediate(upState);
                    GraphEditor.SelectedNodes.Clear();
                    stateMachine.upNode = node;
                }

                if (node.IsStartNode)
                {
                    stateMachine.startNode = node;
                }

                if (node is StateMachine machine)
                {
                    if(!machine.upNode && machine.Root != machine)
                    {
                        machine.upNode = GraphUtility.AddNode<UpState>(
                            GraphEditor.Center + new Vector2(0, -(GraphStyles.StateMachineHeight + 5)),
                            machine);
                    }
                }
            }

            //Debug.LogFormat("copiedNodes: {0}", copiedNodes.Count);
            for (int i = 0; i < copiedNodes.Count; i++)
            {
                Node node = copiedNodes[i];
                //Debug.LogFormat("transitions: {0}", node.transitions.Length);
                foreach (Transition transition in node.transitions)
                {
                    
                    /*foreach (var n in dest.parent.nodes)
                    {
                        Debug.LogFormat("Looking for: {0} on: {1}", (origTransition.toNode.id + 1), n.id);
                    }*/
                    //Node toNode = dest.parent.nodes.ToList().Find(x => x.id == (origTransition.toNode.id + 1));
                    Node toNode = node.parent.nodes.ToList().Find(x => x.id == transition.toNode.id + 1) ?? 
                        stateMachine.NodesRecursive.ToList().Find(x => x.id == transition.toNode.id + 1) ??
                        node.parent.nodes.ToList().Find(x => x.id == (transition.toNode.id + 1));

                    if(toNode == null && checkNames)
                    {
                        toNode = node.parent.nodes.ToList().Find(x => x.name == transition.toNode.name) ??
                        stateMachine.NodesRecursive.ToList().Find(x => x.name == transition.toNode.name) ??
                        node.parent.nodes.ToList().Find(x => x.name == transition.toNode.name);
                    }

                    //Debug.LogFormat("Looking for: {0} on: {1} toNode: {2}", transition.toNode.id, node.id, toNode);

                    if (toNode != null)
                    {
                        transition.toNode = toNode;
                    }
                    else
                    {
                        GraphUtility.DestroyImmediate(transition);

                        var transitions = node.transitions;
                        ArrayUtility.Remove(ref transitions, transition);
                        node.transitions = transitions;
                    }
                }
            }

            for (int i = 0; i < copiedNodes.Count; i++)
            {
                Node node = copiedNodes[i];
                if (!node) continue;
                if (!overrideReactions.components.ContainsKey(node.id)) continue;

                var val = overrideReactions.components[node.id];
                overrideReactions.components.Remove(node.id);
                var guid = System.Guid.NewGuid();
                node.id = Mathf.Abs(guid.GetHashCode());
                overrideReactions.components.Add(node.id, val);

                Node existingNode = stateMachine.nodes.ToList().Find(x => x.name == node.name && x != node);
                if (existingNode)
                {
                    node.name = GraphUtility.GenerateUniqueNodeName(node.GetType(), stateMachine, node.name);
                }
            }

            GraphUtility.ParentChilds(stateMachine);
            nodes.Clear();
            EditorUtility.SetDirty(stateMachine);
            EditorUtility.SetDirty(reactions);
            // AssetDatabase.SaveAssets();
        }

        private static Vector2 GetCenter(List<Node> nodes)
        {
            Vector2 center = Vector2.zero;
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                center += new Vector2(node.position.center.x, node.position.center.y);
            }
            center /= nodes.Count;
            return center;
        }
        
        public static bool CanPaste()
        {
            return nodes.Count > 0;
        }
    }
}