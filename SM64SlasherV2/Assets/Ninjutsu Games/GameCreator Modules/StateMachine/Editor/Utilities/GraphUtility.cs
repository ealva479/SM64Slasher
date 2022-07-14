#if SM_RPG
using NJG.RPG;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameCreator.Core;
using GameCreator.Variables;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;
#if PHOTON_MODULE
using NJG.PUN;

#endif

namespace NJG.Graph
{
    public static class GraphUtility
    {
        public static bool DestroyImmediate(ScriptableObject obj)
        {
            if (obj == null)
            {
                return false;
            }

            bool result = DeleteChilds(obj);
            Undo.DestroyObjectImmediate(obj);
            // AssetDatabase.SaveAssets();
            return result;
        }

        public static bool DeleteNode(Node node, GraphReactions reactions)
        {
            bool shouldSave = false;

            if (node is TriggerState)
            {
                var state = node as TriggerState;
                var trigger = state.GetTrigger(reactions);
                if (reactions && node) reactions.RemoveReaction(node);

                if (trigger != null && trigger.igniters.Count > 0)
                {
                    foreach (var ig in trigger.igniters)
                    {
                        if (ig.Value == null) continue;
                        ig.Value.hideFlags = 0;
                        Object.DestroyImmediate(ig.Value, true);
                    }
                }
            }

            if (node is ActionsState)
            {
                var state = node as ActionsState;
                var trigger = state.GetActions(reactions);
                if (reactions && node) reactions.RemoveReaction(node);
                if (trigger != null && trigger.actions.Length > 0)
                {
                    foreach (var ig in trigger.actions)
                    {
                        if (ig == null) continue;
                        ig.hideFlags = 0;
                        Object.DestroyImmediate(ig, true);
                    }
                }
            }

            var nodes = node.parent.nodes;
            ArrayUtility.Remove(ref nodes, node);
            node.parent.nodes = nodes;

            foreach (Transition transition in node.InTransitions)
            {
                bool res = DestroyImmediate(transition);
                if (res) shouldSave = true;

                reactions.RemoveCondition(transition); //if (reactions && transition) 

                var transitions = transition.fromNode.transitions;
                if (transitions != null)
                {
                    var list = transition.GetConditions(reactions);
                    if (list != null && list.conditions != null)
                    {
                        foreach (var c in list.conditions)
                        {
                            if (c == null) continue;
                            c.hideFlags = 0;
                            Object.DestroyImmediate(c, true);
                        }
                    }

                    ArrayUtility.Remove(ref transitions, transition);
                    transition.fromNode.transitions = transitions;
                }
            }

            shouldSave = DestroyImmediate(node);
            // GraphReactions.CleanUpRogueComponents(reactions);

            if (shouldSave)
            {
                // AssetDatabase.Refresh();            
                // AssetDatabase.SaveAssets();
                //Debug.LogWarning("We should save Assets");
            }

            return shouldSave;
        }

        public static T AddNode<T>(Vector2 position, StateMachine parent, bool addStartNode = true)
        {
            if (parent == null)
            {
                Debug.LogWarning("Can't add node to parent state machine, because the parent state machine is null!");
                return default(T);
            }

            Node node = (Node) ScriptableObject.CreateInstance(typeof(T));
            Undo.RegisterCreatedObjectUndo(node, "Add Node");
            // Undo.RegisterCompleteObjectUndo(node, "Add Node 2");


            node.hideFlags = HideFlags.HideInHierarchy;
            Guid guid = Guid.NewGuid();
            node.id = Mathf.Abs(guid.GetHashCode());

            if (!(node is EntryState) && !(node is ExitState))
            {
                node.name = GenerateUniqueNodeName<T>(parent.Root);
            }

            node.parent = parent;

            var nodes = parent.nodes;
            ArrayUtility.Add(ref nodes, node);
            parent.nodes = nodes;

            Undo.RecordObject(parent, "Add Node");

            node.position = new Rect(position.x - GraphStyles.StateWidth / 2, position.y - GraphStyles.StateHeight / 2,
                GraphStyles.StateWidth, GraphStyles.StateHeight);
            UpdateNodeColor(node);

            if (node is UpState)
            {
                parent.upNode = node;
            }
            /*else if (node is EntryState)
            {
                node.name = "Entry";
                parent.entryNode = node;
            }
            else if (node is ExitState)
            {
                node.name = "Exit";
                parent.exitNode = node;
            }*/
            else if (node is ActionsState)
            {
                parent.Root.sourceReactions.GetReaction<IActionsList>(node);
                //parent.GetReaction<GameCreator.Core.IActionsList>(node);
            }
            else if (node is TriggerState)
            {
                try
                {
                    var gt = parent.Root.sourceReactions.GetReaction<GraphTrigger>(node);
                    var ed = Editor.CreateEditor(gt) as GraphTriggerInspector;
                    ed.OnEnable();
                    ed.UpdateIgniters();
                }
                catch (Exception e)
                {
                    Debug.LogWarningFormat("Trigger exception: {0}", e.Message);
                }
            }

            if (node is StateMachine)
            {
                node.position.width = GraphStyles.StateMachineWidth;
                node.position.height = GraphStyles.StateMachineHeight;

                (node as StateMachine).upNode =
                    AddNode<UpState>(GraphEditor.Center + new Vector2(0, -(GraphStyles.StateMachineHeight + 5)),
                        (StateMachine) node);
                //AddNode<EntryState>(GraphEditor.Center - new Vector2(GraphStyles.StateWidth, 0), (StateMachine)node);
                //AddNode<ExitState>(GraphEditor.Center + new Vector2(GraphStyles.StateWidth, 0), (StateMachine)node);
                if ((node as StateMachine).startNode == null && addStartNode)
                {
                    var startNode = AddNode<ActionsState>(GraphEditor.Center, (StateMachine) node);
                    startNode.name = "Start";
                }

                //ParentChilds(parent);
                // AssetDatabase.SaveAssets();
            }
            else if (node is TriggerState || node is UpState)
            {
                node.position.width = GraphStyles.StateMachineWidth;
                node.position.height = GraphStyles.StateMachineHeight;
            }

            if (!(node is TriggerState) && !(node is UpState) && !(node is EntryState) && !(node is ExitState))
            {
                /*if(parent.entryNode && parent.entryNode.transitions.Length == 0)
                {
                    AddTransition(parent.entryNode, node, false);
                }
                */
                if (!parent.startNode)
                {
                    SetDefaultNode(node, parent);
                }
            }

            GraphEditor.RepaintAll();

            if (EditorUtility.IsPersistent(parent))
            {
                // ParentChilds(parent);
                if (node is StateMachine) ParentChilds(parent);
                else AssetDatabase.AddObjectToAsset(node, parent);
                //AssetDatabase.SaveAssets();
            }

            EditorUtility.SetDirty(parent.Root.sourceReactions.gameObject);

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            return (T) (object) node;
        }

        public static Transition AddTransition(Node fromNode, Node toNode, bool selectTransition = true)
        {
            if (!fromNode || !toNode || toNode is UpState || toNode is TriggerState)
            {
                return null;
            }

            Transition transition = ScriptableObject.CreateInstance<Transition>();
            Undo.RegisterCreatedObjectUndo(transition, "Add Transition");

            transition.hideFlags = HideFlags.HideInHierarchy;
            Guid guid = Guid.NewGuid();
            transition.id = Mathf.Abs(guid.GetHashCode());

            //GraphEditor.Reactions.GetCondition<GameCreator.Core.IConditionsList>(transition);
            //fromNode.parent.GetCondition<GameCreator.Core.IConditionsList>(transition);
            transition.Init(toNode, fromNode);

            if (EditorUtility.IsPersistent(fromNode))
            {
                AssetDatabase.AddObjectToAsset(transition, fromNode);
                //AssetDatabase.SaveAssets();
            }

            Undo.RecordObject(fromNode, "Add Transition");

            var transitions = fromNode.transitions;
            ArrayUtility.Add(ref transitions, transition);
            fromNode.transitions = transitions;

            NodeInspector.Dirty();

            // Undo.RegisterCompleteObjectUndo(transition, "Add Transition");
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            if (selectTransition && GraphEditor.Instance) GraphEditor.SelectTransition(transition);
            EditorUtility.SetDirty(fromNode.Root.sourceReactions.gameObject);

            return transition;
        }

        public static void SetDefaultNode(Node node, StateMachine parent)
        {
            if (!parent || !node || !parent.nodes.Contains(node))
            {
                return;
            }

            foreach (Node mNode in parent.nodes)
            {
                mNode.IsStartNode = mNode == node;
                UpdateNodeColor(mNode);
            }

            Undo.RegisterCompleteObjectUndo(node, "Set Default Node");

            //node.IsStartNode = true; 
            //UpdateNodeColor(node);
        }

        public static void UpdateNodeColor(Node node)
        {
            if (node.IsStartNode)
            {
                node.color = GraphStyles.startNodeColor;
            }
            else if (node is StateMachine)
            {
                node.color = GraphStyles.StateMachineColor;
            }
            else if (node is EntryState)
            {
                node.color = GraphStyles.EntryNodeColor;
            }
            else if (node is ExitState)
            {
                node.color = GraphStyles.ExitNodeColor;
            }
            else if (node is TriggerState)
            {
                node.color = GraphStyles.TriggerNodeColor;
            }
            else
            {
                node.color = GraphStyles.defaultNodeColor;
            }
        }

        public static bool DeleteChilds(ScriptableObject root)
        {
            if (!root)
            {
                return false;
            }

            bool shouldSave = false;
            FieldInfo[] fields = root.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (!CanBeCopied(field)) continue;

                if (field.FieldType.IsSubclassOf(typeof(ScriptableObject)))
                {
                    ScriptableObject value = (ScriptableObject) field.GetValue(root);
                    if (value)
                    {
                        DeleteChilds(value);
                        //Debug.Log("Delete " + value);
                        Undo.DestroyObjectImmediate(value);
                        shouldSave = true;
                    }
                }
                else if (field.FieldType.IsArray)
                {
                    var array = field.GetValue(root) as Array;
                    Type elementType = field.FieldType.GetElementType();
                    if (elementType.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        foreach (ScriptableObject value in array)
                        {
                            if (value)
                            {
                                DeleteChilds(value);
                                //Debug.Log("Delete " + value);
                                Undo.DestroyObjectImmediate(value);

                                //UnityEngine.Object.DestroyImmediate(value, true);
                                shouldSave = true;
                            }
                        }
                    }
                }
            }

            // if(shouldSave) AssetDatabase.SaveAssets();
            return shouldSave;
        }

        private static bool CanBeCopied(FieldInfo field)
        {
#if NET_4_6
            var attribute = field.GetCustomAttribute<NonSerializedAttribute>();
            if (attribute != null || field.Name == "parent" || field.Name == "toNode" || field.Name == "fromNode")
            {
                return false;
            }
#else
            var attrs = (NonSerializedAttribute[])field.GetCustomAttributes
                (typeof(NonSerializedAttribute), false);

            if ((attrs != null && attrs.Length > 0) || field.Name == "parent" || field.Name == "toNode" || field.Name == "fromNode")
            {
                return false;
            }
#endif
            return true;
        }

        public static EventType ReserveEvent(params Rect[] areas)
        {
            EventType eventType = Event.current.type;
            foreach (Rect area in areas)
            {
                if ((area.Contains(Event.current.mousePosition) &&
                     (eventType == EventType.MouseDown || eventType == EventType.ScrollWheel)))
                {
                    Event.current.type = EventType.Ignore;
                }
            }

            return eventType;
        }

        public static void ReleaseEvent(EventType type)
        {
            if (Event.current.type != EventType.Used)
            {
                Event.current.type = type;
            }
        }

        public static string GenerateUniqueNodeName<T>(StateMachine stateMachine, string optionalName = "")
        {
            return GenerateUniqueNodeName(typeof(T), stateMachine, optionalName);
        }

        public static string GenerateUniqueNodeName(Type type, StateMachine stateMachine, string optionalName = "")
        {
            int cnt = 0;
            string uniqueName = string.Empty;

            if (!string.IsNullOrEmpty(optionalName))
            {
                uniqueName = optionalName;
            }
            else
            {
                uniqueName = (type == typeof(ActionsState) ? "Actions" : "StateMachine");
                if (type == typeof(TriggerState)) uniqueName = "Trigger";
            }

            Node[] nodes = stateMachine.nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                Node node = nodes[i];
                if (node.GetType() != type) continue;
                cnt++;
                /*if (node.name == uniqueName)
                {
                    return node;
                }*/
            }

            /*while (NodeExists(stateMachine.Root, uniqueName + (cnt > 0 ? " " + cnt : string.Empty)))
            {
                cnt++;
            }*/
            return uniqueName + (cnt >= 0 ? " " + cnt : string.Empty);
        }

        public static string GenerateUniqueNodeName(string previousName)
        {
            int cnt = 0;
            Node[] nodes = GraphEditor.Active.nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                Node node = nodes[i];
                if (node.name.Equals(previousName) || node.name.Contains(previousName))
                {
                    cnt++;
                }
            }
            return previousName + (cnt > 0 ? " " + cnt : string.Empty);
        }

        public static GraphReactions CopyReactions(GraphReactions original, GraphReactions dest)
        {
            //Debug.Log("CopyReactions from " + original + " to " + dest);
            if (!original || !dest) return null;

            var buffer = new List<int>(dest.components.Keys);

            foreach (var key in buffer)
            {
                var ig = dest.components[key];
                if (!original.components.ContainsKey(key))
                {
                    Object.DestroyImmediate(ig, true);
                    dest.components.Remove(key);
                }
            }

            //dest.components.Clear();

            foreach (var b in original.components)
            {
                bool destHasIt = dest.components.ContainsKey(b.Key);
                if (dest)
                {
                    Behaviour comp = null;
                    if (b.Value is IActionsList)
                    {
                        comp = destHasIt ? dest.components[b.Key] : dest.gameObject.AddComponent<IActionsList>();
                        if (comp) CopyIActionList(b.Value as IActionsList, comp as IActionsList, false);
                    }
                    else if (b.Value is GraphTrigger)
                    {
                        comp = destHasIt ? dest.components[b.Key] : dest.gameObject.AddComponent<GraphTrigger>();
                        if (comp)
                        {
                            comp.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                            CopyTrigger(b.Value as GraphTrigger, comp as GraphTrigger);
                        }
                    }
                    else if (b.Value is IConditionsList)
                    {
                        comp = destHasIt ? dest.components[b.Key] : dest.gameObject.AddComponent<IConditionsList>();
                        if (comp) CopyIConditionList(b.Value as IConditionsList, comp as IConditionsList);
                    }

                    if (comp && !destHasIt) dest.components.Add(b.Key, comp);
                }
            }

            return dest;
        }

        /// <summary>
        /// Creates and return a copy of a Node or StateMachine.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Node Copy(Node original, GraphReactions sourceReactions, GraphReactions destinyReactions)
        {
            Node dest = null;
            Guid guid;
            int newID = original.id + 1;

            //Debug.LogFormat("Copy oldId: {0} newID: {1}", original.id, newID);

            //if (existing == null)
            //{
            dest = (Node) ScriptableObject.CreateInstance(original.GetType());
            //guid = System.Guid.NewGuid();
            //dest.id = Mathf.Abs(guid.GetHashCode());
            dest.id = newID;
            dest.parent = original.parent;
            dest.transitionMode = original.transitionMode;
            /*}
            else
            {
                dest = existing;
                dest.id = original.id;
            }*/
            dest.color = original.color;
            dest.description = original.description;
            dest.name = GenerateUniqueNodeName(original.name);
            dest.position = original.position;
            dest.hideFlags = original.hideFlags;
            dest.IsStartNode = original.IsStartNode;

            if (original is StateMachine)
            {
                if (destinyReactions) ((StateMachine) dest).sourceReactions = destinyReactions;
                //(dest as StateMachine).startNode = Copy((original as StateMachine).startNode, existing);
                ((StateMachine) dest).nodes = CopyNodes((original as StateMachine).nodes, dest as StateMachine,
                    sourceReactions, destinyReactions);
                foreach (var n in (dest as StateMachine).nodes)
                {
                    if (n.id == (original as StateMachine).startNode.id)
                    {
                        (dest as StateMachine).startNode = n;
                    }
                }
            }
            else if (original is TriggerState)
            {
                CopyTrigger((original as TriggerState).GetTrigger(sourceReactions),
                    (dest as TriggerState)?.GetTrigger(destinyReactions));
            }
            else if (original is ActionsState && dest is ActionsState)
            {
                //Debug.Log("ActionState copy " + original + " / " + dest);
                CopyIActionList((original as ActionsState).GetActions(sourceReactions),
                    (dest as ActionsState).GetActions(destinyReactions));
            }

            /*if (EditorUtility.IsPersistent(original.parent))
            {
                AssetDatabase.AddObjectToAsset(copiedNode, original.parent);
                //AssetDatabase.SaveAssets();
            }*/

            foreach (Transition origTransition in original.transitions)
            {
                //Node toNode = dest.parent.nodes.ToList().Find(x => x.name == origTransition.toNode.name && x.GetType() == origTransition.toNode.GetType());
                Node toNode = dest.parent.nodes.ToList().Find(x => x.id == (origTransition.toNode.id + 1) ||
                                                                   x.id == (origTransition.toNode.id - 1) ||
                                                                   x.id == origTransition.toNode.id);
                //Debug.LogFormat("origTransition origID: {0} newID: {1} toNode: {2}", origTransition.toNode.id, origTransition.toNode.id + 1, toNode);
                if (toNode)
                {
                    Transition destTrasition = ScriptableObject.CreateInstance<Transition>();
                    guid = Guid.NewGuid();
                    destTrasition.id = Mathf.Abs(guid.GetHashCode());
                    destTrasition.hideFlags = HideFlags.HideInHierarchy;
                    destTrasition.toNode = toNode;
                    destTrasition.fromNode = dest;
                    destTrasition.mute = origTransition.mute;
                    destTrasition.isNegative = origTransition.isNegative;
                    destTrasition.useConditions = origTransition.useConditions;

                    var transitions = dest.transitions;
                    ArrayUtility.Add(ref transitions, destTrasition);
                    dest.transitions = transitions;

                    CopyIConditionList(origTransition.GetConditions(sourceReactions),
                        destTrasition.GetConditions(destinyReactions));
                }
                /*else if (existing)
                {
                    //Transition destTrasition = existing.transitions.ToList().Find(x => x.name == origTransition.name && x.GetType() == origTransition.toNode.GetType());
                    Transition destTrasition = existing.transitions.ToList().Find(x => x.id == origTransition.id);
                    CopyIConditionList(origTransition.Conditions, destTrasition.Conditions);
                }*/
            }

            return dest;
        }

        /// <summary>
        /// Copies a list of nodes to a StateMachine.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Node[] CopyNodes(Node[] nodes, StateMachine parent, GraphReactions sourceReactions,
            GraphReactions destinyReactions)
        {
            //Debug.LogFormat("CopyNodes: {0}", nodes.Length);
            List<Node> mNodes = new List<Node>();
            int add = parent.id / 10;

            for (int i = 0, imax = nodes.Length; i < imax; i++)
            {
                Node original = nodes[i];
                if (!original) continue;

                Node
                    dest = null; // existing != null && existing.nodes.Length > 0 && i < existing.nodes.Length ? existing.nodes[i] : null;
                Guid guid = Guid.NewGuid();
                int ident = Mathf.Abs(guid.GetHashCode());

                int newID = original.id + add;

                //Debug.LogWarningFormat("Node {0} id: {1} newID: {2} orig: {3}", i, ident, newID, original.id);

                if (!dest)
                {
                    dest = (Node) ScriptableObject.CreateInstance(original.GetType());
                    //guid = System.Guid.NewGuid();
                    //dest.id = Mathf.Abs(guid.GetHashCode());
                    dest.parent = parent;
                }

                //else
                {
                    dest.id = newID; // original.id;
                }

                dest.color = original.color;
                dest.description = original.description;
                dest.name = GenerateUniqueNodeName(original.name);
                dest.position = original.position;
                dest.IsStartNode = original.IsStartNode;
                dest.hideFlags = HideFlags.HideInHierarchy;

                if (dest.IsStartNode) parent.startNode = dest;

                if (original is ActionsState)
                {
                    CopyIActionList((original as ActionsState).GetActions(sourceReactions),
                        ((ActionsState) dest).GetActions(destinyReactions));
                }
                else if (original is TriggerState)
                {
                    CopyTrigger((original as TriggerState).GetTrigger(sourceReactions),
                        ((TriggerState) dest).GetTrigger(destinyReactions));
                }
                else if (original is StateMachine)
                {
                    ((StateMachine) dest).nodes = CopyNodes((original as StateMachine).nodes, dest as StateMachine,
                        sourceReactions, destinyReactions);
                }

                mNodes.Add(dest);
            }

            foreach (Node original in nodes)
            {
                foreach (Transition origTransition in original.transitions)
                {
                    Transition destTrasition = null;
                    Node dest = null;
                    /*if (existing)
                    {
                        //destTrasition = existing.transitions.ToList().Find(x => x.name == origTransition.name && x.GetType() == origTransition.toNode.GetType());
                        destTrasition = existing.transitions.ToList().Find(x => x.id == origTransition.id);
                    }*/
                    if (destTrasition == null)
                    {
                        destTrasition = ScriptableObject.CreateInstance<Transition>();
                        //destTrasition.id = origTransition.id + add;
                        Guid guid = Guid.NewGuid();
                        destTrasition.id = Mathf.Abs(guid.GetHashCode());
                        dest = mNodes.Find(x => (x.id - add) == original.id);
                        destTrasition.hideFlags = HideFlags.HideInHierarchy;
                        destTrasition.useConditions = origTransition.useConditions;
                        destTrasition.isNegative = origTransition.isNegative;

                        //destTrasition.toNode = mNodes.ToList().Find(x => x.name == origTransition.toNode.name && x.GetType() == origTransition.toNode.GetType());
                        destTrasition.toNode = mNodes.ToList().Find(x => (x.id - add) == origTransition.toNode.id);
                        destTrasition.fromNode = dest;

                        var transitions = dest.transitions;
                        ArrayUtility.Add(ref transitions, destTrasition);
                        dest.transitions = transitions;
                    }

                    CopyIConditionList(origTransition.GetConditions(sourceReactions),
                        destTrasition.GetConditions(destinyReactions));
                }
            }

            return mNodes.ToArray();
        }

        /// <summary>
        /// Copies a Trigger to another.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void CopyTrigger(GraphTrigger source, GraphTrigger dest)
        {
            //Debug.LogWarning("CopyTrigger from " + source + " to " + dest);
            if (source == null || dest == null) return;

            dest.minDistance = source.minDistance;
            dest.minDistanceToPlayer = source.minDistanceToPlayer;

            var buffer = new List<int>(dest.igniters.Keys);

            foreach (var key in buffer)
            {
                var ig = dest.igniters[key];
                if (!source.igniters.ContainsKey(key))
                {
                    Debug.LogWarning("1 Trigger Destroy " + ig);
                    Object.DestroyImmediate(ig, true);
                    dest.igniters.Remove(key);
                }
            }

            buffer = new List<int>(source.igniters.Keys);

            SerializedObject serializedObject = new SerializedObject(dest);
            SerializedProperty spIgniters = serializedObject.FindProperty("igniters");
            SerializedProperty spValues = spIgniters.FindPropertyRelative("values");
            SerializedProperty spIgnitersKeys = spIgniters.FindPropertyRelative("keys");
            SerializedProperty spIgnitersValues = spIgniters.FindPropertyRelative("values");

            int index = 0;
            foreach (var key in buffer)
            {
                var ig = source.igniters[key];

                if (ig == null) continue;

                if (dest.igniters != null && dest.igniters.Count > 0 && dest.igniters.ContainsKey(key) &&
                    dest.igniters[key] != null && dest.igniters[key].GetType() != ig.GetType())
                {
                    Object.DestroyImmediate(dest.igniters[key], true);
                }

                if (!dest.igniters.ContainsKey(key) || dest.igniters[key] == null)
                {
                    Igniter newIgniter = dest.gameObject.AddComponent(ig.GetType()) as Igniter;

                    spIgnitersKeys.InsertArrayElementAtIndex(index);
                    spIgnitersValues.InsertArrayElementAtIndex(index);

                    spIgnitersKeys.GetArrayElementAtIndex(index).intValue = key;
                    spIgnitersValues.GetArrayElementAtIndex(index).objectReferenceValue = newIgniter;

                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    serializedObject.Update();

                    if (ComponentUtility.CopyComponent(source))
                    {
                        ComponentUtility.PasteComponentValues(newIgniter);
                    }

                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    serializedObject.Update();

                    newIgniter.Setup(dest);
                    if (newIgniter.enabled)
                    {
                        newIgniter.enabled = Application.isPlaying;
                    }
                }
                else
                {
                    if (ComponentUtility.CopyComponent(source))
                    {
                        ComponentUtility.PasteComponentValues(dest.igniters[key]);
                    }

                    dest.igniters[key].Setup(dest);
                    if (dest.igniters[key].enabled)
                    {
                        dest.igniters[key].enabled = Application.isPlaying;
                    }
                }

                if (ig != null) dest.igniters[key].GetCopyOf(ig);
                index++;
            }

            ReplaceLocalVarTargets(dest.igniters);

            //EditorUtility.SetDirty(dest);
        }

        /// <summary>
        /// Copies a Trigger to another.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static List<MonoBehaviour> CopyTrigger(Trigger source, Trigger dest, Vector2 position,
            TriggerState triggerState, List<Node> nodes)
        {
            List<MonoBehaviour> monoBehaviours = new List<MonoBehaviour>();

            //Debug.LogWarning("CopyTrigger from " + source + " to " + dest);
            if (source == null || dest == null) return monoBehaviours;

            dest.minDistance = source.minDistance;
            dest.minDistanceToPlayer = source.minDistanceToPlayer;

            var buffer = new List<int>(dest.igniters.Keys);

            foreach (var key in buffer)
            {
                var ig = dest.igniters[key];
                if (!source.igniters.ContainsKey(key))
                {
                    Object.DestroyImmediate(ig, true);
                    dest.igniters.Remove(key);
                }
            }

            buffer = new List<int>(source.igniters.Keys);

            SerializedObject serializedObject = new SerializedObject(dest);
            SerializedProperty spIgniters = serializedObject.FindProperty("igniters");
            SerializedProperty spValues = spIgniters.FindPropertyRelative("values");
            SerializedProperty spIgnitersKeys = spIgniters.FindPropertyRelative("keys");
            SerializedProperty spIgnitersValues = spIgniters.FindPropertyRelative("values");

            int index = 0;
            foreach (var key in buffer)
            {
                var ig = source.igniters[key];

                if (ig == null) continue;

                if (dest.igniters != null && dest.igniters.Count > 0 && dest.igniters.ContainsKey(key) &&
                    dest.igniters[key] != null && dest.igniters[key].GetType() != ig.GetType())
                {
                    Object.DestroyImmediate(dest.igniters[key], true);
                }

                if (!dest.igniters.ContainsKey(key) || dest.igniters[key] == null)
                {
                    Igniter newIgniter = dest.gameObject.AddComponent(ig.GetType()) as Igniter;
                    spIgnitersKeys.InsertArrayElementAtIndex(index);
                    spIgnitersValues.InsertArrayElementAtIndex(index);

                    spIgnitersKeys.GetArrayElementAtIndex(index).intValue = key;
                    spIgnitersValues.GetArrayElementAtIndex(index).objectReferenceValue = newIgniter;

                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    serializedObject.Update();

                    if (ComponentUtility.CopyComponent(source))
                    {
                        ComponentUtility.PasteComponentValues(newIgniter);
                    }

                    dest.igniters[key] = newIgniter;

                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    serializedObject.Update();

                    newIgniter.Setup(dest);
                    if (newIgniter.enabled)
                    {
                        newIgniter.enabled = Application.isPlaying;
                    }
                }
                else
                {
                    if (ComponentUtility.CopyComponent(source))
                    {
                        ComponentUtility.PasteComponentValues(dest.igniters[key]);
                    }

                    dest.igniters[key].Setup(dest);
                    if (dest.igniters[key].enabled)
                    {
                        dest.igniters[key].enabled = Application.isPlaying;
                    }
                }

                if (ig != null) dest.igniters[key].GetCopyOf(ig);
                index++;
            }

            float padding = 10;
            var pos = position;
            pos.y += GraphStyles.StateHeight * 2;

            float totalWidth = 0;
            int count = 0;
            for (int i = 0; i < source.items.Count; i++)
            {
                var item = source.items[i];
                if (item.option == Trigger.ItemOpts.Conditions)
                {
                    if (item.conditions.clauses.Length > 0)
                    {
                        totalWidth += GraphStyles.StateWidth + padding;
                        count++;
                    }
                }
                else if (item.option == Trigger.ItemOpts.Actions)
                {
                    totalWidth += GraphStyles.StateWidth + padding;
                    count++;
                }
            }

            if (count > 1) pos.x -= (totalWidth / 2) - (GraphStyles.StateMachineWidth / 2) - 5;

            int conditionIndex = 0;
            for (int i = 0; i < source.items.Count; i++)
            {
                var item = source.items[i];
                if (item.option == Trigger.ItemOpts.Actions)
                {
                    ActionsState state = AddNode<ActionsState>(pos, GraphEditor.Active);
                    nodes.Add(state);
                    UpdateNodeColor(state);
                    if (item.actions)
                        CopyIActionList(item.actions.actionsList, state.GetActions(GraphEditor.Reactions));
                    AddTransition(triggerState, state, false);
                    pos.x += GraphStyles.StateWidth + padding;
                    if (item.actions) monoBehaviours.Add(item.actions);
                }

                if (item.option == Trigger.ItemOpts.Conditions)
                {
                    monoBehaviours.Add(item.conditions);

                    conditionIndex++;
                    ActionsState state = AddNode<ActionsState>(pos, GraphEditor.Active);
                    nodes.Add(state);
                    state.transitionMode = Node.TransitionMode.Selective;
                    state.name = $"Conditions {conditionIndex}";
                    UpdateNodeColor(state);
                    AddTransition(triggerState, state, false);

                    float totalWidth2 = 0;
                    int count2 = 0;
                    var pos2 = pos;
                    pos2.y += GraphStyles.StateHeight * 2;

                    for (int j = 0; j < item.conditions.clauses.Length; j++)
                    {
                        var clause = item.conditions.clauses[j];
                        if (clause.actions)
                        {
                            totalWidth2 += GraphStyles.StateWidth + padding;
                            count2++;
                        }
                    }

                    if (item.conditions.defaultActions)
                    {
                        totalWidth2 += GraphStyles.StateWidth + padding;
                        count2++;
                    }

                    if (count2 > 1) pos2.x -= (totalWidth2 / 2) - (GraphStyles.StateWidth / 2) - 5;

                    foreach (Clause clause in item.conditions.clauses)
                    {
                        if (!clause.actions) continue;

                        ActionsState subState = AddNode<ActionsState>(pos2, GraphEditor.Active);
                        nodes.Add(subState);
                        UpdateNodeColor(subState);
                        CopyIActionList(clause.actions.actionsList, subState.GetActions(GraphEditor.Reactions));
                        Transition t = AddTransition(state, subState, false);
                        t.useConditions = true;
                        Editor.CreateEditor(GraphEditor.Reactions.GetCondition<IConditionsList>(t));
                        CopyIConditionList(clause.conditionsList, t.GetConditions(GraphEditor.Reactions));
                        pos2.x += GraphStyles.StateWidth + padding;

                        monoBehaviours.Add(clause.actions);
                    }

                    if (item.conditions.defaultActions)
                    {
                        ActionsState subState = AddNode<ActionsState>(pos2, GraphEditor.Active);
                        nodes.Add(subState);
                        UpdateNodeColor(subState);
                        CopyIActionList(item.conditions.defaultActions.actionsList,
                            subState.GetActions(GraphEditor.Reactions));
                        AddTransition(state, subState, false);
                        pos2.x += GraphStyles.StateWidth + padding;
                        monoBehaviours.Add(item.conditions.defaultActions);
                    }

                    pos.x += GraphStyles.StateWidth + padding;
                }
            }

            return monoBehaviours;
        }

        /// <summary>
        /// Copies an IConditionsList to another.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void CopyIConditionList(IConditionsList source, IConditionsList dest)
        {
            if (source == null || source.conditions == null || dest == null) return;

            ICondition[] conditions = new ICondition[source.conditions.Length];
            for (int i = 0; i < dest.conditions.Length; i++)
            {
                var curr = dest.conditions[i];
                if (curr == null) continue;

                if (i < source.conditions.Length)
                {
                    var src = source.conditions[i];
                    if ((src != null && curr.GetType() != src.GetType()))
                    {
                        MonoBehaviour.DestroyImmediate(curr, true);
                        curr = dest.gameObject.AddComponent(src.GetType()) as ICondition;
                        if (ComponentUtility.CopyComponent(src))
                        {
                            ComponentUtility.PasteComponentValues(curr);
                        }
                    }
                }
                else if (i >= source.conditions.Length) MonoBehaviour.DestroyImmediate(curr, true);
            }

            for (int i = 0; i < source.conditions.Length; i++)
            {
                ICondition sourceAction = source.conditions[i];
                if (sourceAction == null) continue;
                conditions[i] = dest.gameObject.AddComponent(sourceAction.GetType()) as ICondition;
                if (ComponentUtility.CopyComponent(sourceAction))
                {
                    ComponentUtility.PasteComponentValues(conditions[i]);
                }
            }

            dest.conditions = conditions;
            ReplaceLocalVarTargets(dest.conditions);
        }

        /// <summary>
        /// Copies an IActionsList to another.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void CopyIActionList(IActionsList source, IActionsList dest, bool forceCreate = true)
        {
            bool cannotCopy = source == null || source.actions == null || dest == null;
            if (cannotCopy) return;

            IAction[] actions = new IAction[source.actions.Length];

            for (int i = 0; i < dest.actions.Length; i++)
            {
                var curr = dest.actions[i];
                if (curr == null) continue;

                if (i < source.actions.Length)
                {
                    var src = source.actions[i];
                    if (src != null && curr.GetType() != src.GetType())
                    {
                        MonoBehaviour.DestroyImmediate(curr, true);
                    }
                }
                else if (i >= source.actions.Length)
                {
                    MonoBehaviour.DestroyImmediate(curr, true);
                }
            }

            for (int i = 0; i < source.actions.Length; i++)
            {
                IAction sourceAction = source.actions[i];
                if (sourceAction == null) continue;
                actions[i] = dest.gameObject.AddComponent(sourceAction.GetType()) as IAction;
                if (ComponentUtility.CopyComponent(sourceAction))
                {
                    ComponentUtility.PasteComponentValues(actions[i]);
                }
            }

            dest.actions = actions;
            ReplaceLocalVarTargets(dest.actions);
        }


        public static T GetCopyOf<T>(this MonoBehaviour comp, T source) where T : MonoBehaviour
        {
            if (comp == null) return null;

            Type type = comp.GetType();

            if (comp == null || source == null || type != source.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                 BindingFlags.Default | BindingFlags.DeclaredOnly;

            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                var v = finfo.GetValue(source);
                if (v == null) continue;
                if (v is SerializedProperty) continue;

                // Game Creator default Target types
                /*if (v is TargetPosition && (v as TargetPosition).target == TargetPosition.Target.Transform)
                {
                    var t = v as TargetPosition;
                    if (t.targetTransform.optionIndex != GameCreator.Variables.BaseProperty<Transform>.OPTION.UseGlobalVariable &&
                        t.targetTransform.optionIndex != GameCreator.Variables.BaseProperty<Transform>.OPTION.UseLocalVariable)
                    {
                        var go = t.targetTransform.GetValue();
                        if (go == null) continue;
                        var goType = PrefabUtility.GetPrefabAssetType(go);
                        if (goType != PrefabAssetType.Regular && goType != PrefabAssetType.Variant) continue;
                    }
                }*/
                /*if (v is TargetTransform && (v as TargetTransform).target == TargetTransform.Target.Transform)
                {
                    var t = v as TargetTransform;
                    var go = t.GetTransform(comp.gameObject);
                    if (go == null) continue;
                    var goType = PrefabUtility.GetPrefabAssetType(go);
                    if (goType != PrefabAssetType.Regular && goType != PrefabAssetType.Variant) continue;
                }*/
                if (v is TargetGameObject && (v as TargetGameObject).target == TargetGameObject.Target.GameObject)
                {
                    var t = v as TargetGameObject;
                    var go = t.GetGameObject(comp.gameObject);
                    if (go == null) continue;
                    var goType = PrefabUtility.GetPrefabAssetType(go);
                    if (goType != PrefabAssetType.Regular && goType != PrefabAssetType.Variant) continue;
                }

                if (v is TargetCharacter && (v as TargetCharacter).target == TargetCharacter.Target.Character)
                {
                    var t = v as TargetCharacter;
                    var go = t.GetCharacter(comp.gameObject);
                    if (go == null) continue;
                    var goType = PrefabUtility.GetPrefabAssetType(go);
                    if (goType != PrefabAssetType.Regular && goType != PrefabAssetType.Variant) continue;
                }
                /*if (v is TargetRigidbody && (v as TargetRigidbody).target == TargetRigidbody.Target.Rigidbody)
                {
                    var t = v as TargetRigidbody;
                    var go = t.GetRigidbody(comp.gameObject);
                    if (go == null) continue;
                    var goType = PrefabUtility.GetPrefabAssetType(go);
                    if (goType != PrefabAssetType.Regular && goType != PrefabAssetType.Variant) continue;
                }*/
#if PHOTON_RPG
                // Photon target types
                if (v is TargetPrefabId && (v as TargetPrefabId).target == TargetPrefabId.Target.GameObject)
                {
                    var t = v as TargetPrefabId;
                    if (t.gameObject.optionIndex != GameCreator.Variables.BaseProperty<GameObject>.OPTION.UseGlobalVariable &&
                        t.gameObject.optionIndex != GameCreator.Variables.BaseProperty<GameObject>.OPTION.UseLocalVariable)
                    {
                        var go = t.gameObject.GetValue();
                        if (go == null) continue;
                        var goType = PrefabUtility.GetPrefabAssetType(go);
                        if (goType != PrefabAssetType.Regular && goType != PrefabAssetType.Variant) continue;
                    }
                }
#endif
#if SM_RPG
                // RPG module target types
                if (v is TargetRPGGameObject && (v as TargetRPGGameObject).target == TargetRPGGameObject.Target.GameObject)
                {
                    var t = v as TargetRPGGameObject;
                    var go = t.GetGameObject(comp.gameObject);
                    if (go == null) continue;
                    var goType = PrefabUtility.GetPrefabAssetType(go);
                    if (goType != PrefabAssetType.Regular && goType != PrefabAssetType.Variant) continue;
                }
#endif

                //if((v is Object)) Debug.Log("finfo.GetValue(dest) " + finfo.Name + " = " + v+" / "+v.GetType());
                //if((v is Object)) Debug.Log("finfo.GetValue(dest) " + finfo.Name + " = " + v+" / "+v.GetType() +" is Self? "+((v is Object) ? TestIfSelf(v as Object, v2 as Object) : false)+" other "+v2);
                finfo.SetValue(comp, finfo.GetValue(source));
            }

            return comp as T;
        }

        public static bool ReplaceLocalVarTargets<T>(ICollection<T> array)
        {
            return ReplaceLocalVarTargets(array.ToArray());
        }

        public static bool ReplaceLocalVarTargets<T>(T[] array)
        {
            bool result = false;
            //IActionsList actions = actionEditor.target as IActionsList;
            for (int i = 0; i < array.Length; i++)
            {
                var target = array[i];
                if (target == null) continue;

                // Get the type handle of a specified class.
                Type myType = target.GetType();

                // Get the fields of the specified class.
                FieldInfo[] myField = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic
                    | BindingFlags.Instance |
                    BindingFlags.Default); // | BindingFlags.DeclaredOnly | BindingFlags.SetProperty

                for (int e = 0; e < myField.Length; e++)
                {
                    var field = myField[e];

                    // Determine whether or not each field is a special name.
                    var obj = field.GetValue(target);
                    if (obj == null) continue;

                    FieldInfo[] subFields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic
                        | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly |
                        BindingFlags.SetProperty);

                    //Debug.Log("myField[e] " + field.Name + " / " + field.FieldType + " / obj " + obj.GetType()+ " myType "+ field.GetType()
                    //    +" - "+ (myType == typeof(IActionVariablesAssign)));

                    try
                    {
                        bool changed = CheckField(field, target, field.GetValue(target));
                        if (changed) result = true;

                        for (int f = 0; f < subFields.Length; f++)
                        {
                            var subField = subFields[f];
                            //Debug.Log("subField[e] " + subField.Name + " / " + subField.GetValue(obj)+" / "+subField.FieldType);
                            changed = CheckField(subField, target, subField.GetValue(obj));
                            if (changed) result = true;
                        }
                    }
                    catch
                    {
                        //Do nothing
                    }
                }
            }

            return result;
        }

        private static bool CheckField<T>(FieldInfo field, T target, object value)
        {
            bool result = false;
            var obj = value;
            if (obj == null) return false;
            if (target == null) return false;
            if (field.FieldType == typeof(IActionVariablesAssign))
            {
                if (((IActionVariablesAssign) obj).valueFrom == IActionVariablesAssign.ValueFrom.LocalVariable &&
                    ((IActionVariablesAssign) obj).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as IActionVariablesAssign).local.targetObject,
                        (target as MonoBehaviour).gameObject))
                {
                    (obj as IActionVariablesAssign).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(BaseProperty<T>))
            {
                if (((BaseProperty<T>) obj).optionIndex == BaseProperty<T>.OPTION.UseLocalVariable &&
                    ((BaseProperty<T>) obj).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as BaseProperty<T>).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    ((BaseProperty<T>) obj).local.targetObject = (target as MonoBehaviour)?.gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(VariableProperty))
            {
                if (((VariableProperty) obj).GetVariableType() == Variable.VarType.LocalVariable &&
                    (obj as VariableProperty).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as VariableProperty).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as VariableProperty).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(NumberProperty))
            {
                if (((NumberProperty) obj).optionIndex == BaseProperty<float>.OPTION.UseLocalVariable &&
                    ((NumberProperty) obj).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as NumberProperty).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as NumberProperty).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(StringProperty))
            {
                if ((obj as StringProperty).optionIndex == BaseProperty<string>.OPTION.UseLocalVariable &&
                    (obj as StringProperty).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as StringProperty).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as StringProperty).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(Vector3Property))
            {
                if ((obj as Vector3Property).optionIndex == BaseProperty<Vector3>.OPTION.UseLocalVariable &&
                    (obj as Vector3Property).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as Vector3Property).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as Vector3Property).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(Vector2Property))
            {
                if ((obj as Vector2Property).optionIndex == BaseProperty<Vector2>.OPTION.UseLocalVariable &&
                    (obj as Vector2Property).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as Vector2Property).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as Vector2Property).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(BoolProperty))
            {
                if ((obj as BoolProperty).optionIndex == BaseProperty<bool>.OPTION.UseLocalVariable &&
                    (obj as BoolProperty).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as BoolProperty).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as BoolProperty).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(SpriteProperty))
            {
                if ((obj as SpriteProperty).optionIndex == BaseProperty<Sprite>.OPTION.UseLocalVariable &&
                    (obj as SpriteProperty).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as SpriteProperty).local.targetObject, (target as MonoBehaviour)?.gameObject))
                {
                    (obj as SpriteProperty).local.targetObject = (target as MonoBehaviour)?.gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(ColorProperty))
            {
                if ((obj as ColorProperty).optionIndex == BaseProperty<Color>.OPTION.UseLocalVariable &&
                    (obj as ColorProperty).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as ColorProperty).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as ColorProperty).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(Texture2DProperty))
            {
                if ((obj as Texture2DProperty).optionIndex == BaseProperty<Texture2D>.OPTION.UseLocalVariable &&
                    (obj as Texture2DProperty).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as Texture2DProperty).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as Texture2DProperty).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(GameObjectProperty))
            {
                // Debug.LogWarningFormat("1 GameObjectProperty {0} match: {1}",obj, (((GameObjectProperty) obj).optionIndex == BaseProperty<GameObject>.OPTION.UseLocalVariable &&
                //     ((GameObjectProperty) obj).local.targetType == HelperLocalVariable.Target.GameObject &&
                //     CheckTarget((obj as GameObjectProperty)?.local.targetObject, (target as MonoBehaviour)?.gameObject)));
                //
                if (((GameObjectProperty) obj).optionIndex == BaseProperty<GameObject>.OPTION.UseLocalVariable &&
                    ((GameObjectProperty) obj).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as GameObjectProperty)?.local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    //Debug.LogWarningFormat("2 GameObjectProperty set go {0} to {1}", ((GameObjectProperty) obj).local.targetObject, (target as MonoBehaviour).gameObject);

                    (obj as GameObjectProperty).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                    //((GameObjectProperty) obj).local.Set((target as MonoBehaviour).gameObject);
                }
            }
            else if (field.FieldType == typeof(TargetGameObject))
            {
                if ((obj as TargetGameObject).target == TargetGameObject.Target.LocalVariable &&
                    (obj as TargetGameObject).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as TargetGameObject).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as TargetGameObject).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(TargetPosition))
            {
                if ((obj as TargetPosition).target == TargetPosition.Target.LocalVariable &&
                    (obj as TargetPosition).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as TargetPosition).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as TargetPosition).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(TargetCharacter))
            {
                if ((obj as TargetCharacter).target == TargetCharacter.Target.LocalVariable &&
                    (obj as TargetCharacter).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as TargetCharacter).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as TargetCharacter).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(BaseProperty<T>))
            {
                if ((obj as BaseProperty<T>).optionIndex == BaseProperty<T>.OPTION.UseLocalVariable &&
                    (obj as BaseProperty<T>).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as BaseProperty<T>).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as BaseProperty<T>).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
            else if (field.FieldType == typeof(HelperLocalVariable))
            {
                if ((obj as HelperLocalVariable).targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as HelperLocalVariable).targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as HelperLocalVariable).targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
#if SM_RPG
            else if (field.FieldType == typeof(TargetRPGGameObject))
            {
                if ((obj as TargetRPGGameObject).target == TargetRPGGameObject.Target.LocalVariable &&
                    (obj as TargetRPGGameObject).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as TargetRPGGameObject).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as TargetRPGGameObject).local.targetObject = (target as MonoBehaviour).gameObject;
result = true;
                }
            }
            else if (field.FieldType == typeof(TargetSkillPosition))
            {
                if ((obj as TargetSkillPosition).target == TargetSkillPosition.Target.LocalVariable &&
                    (obj as TargetSkillPosition).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as TargetSkillPosition).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as TargetSkillPosition).local.targetObject = (target as MonoBehaviour).gameObject;
                }
            }
            else if (field.FieldType == typeof(TargetActor))
            {
                if ((obj as TargetActor).target == TargetActor.Target.LocalVariable &&
                    (obj as TargetActor).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as TargetActor).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as TargetActor).local.targetObject = (target as MonoBehaviour).gameObject;
                }
            }
            else if (field.FieldType == typeof(TargetActorTrigger))
            {
                if ((obj as TargetActorTrigger).target == TargetActorTrigger.Target.LocalVariable &&
                    (obj as TargetActorTrigger).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as TargetActorTrigger).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as TargetActorTrigger).local.targetObject = (target as MonoBehaviour).gameObject;
                }
            }
            else if (field.FieldType == typeof(TargetSpawnPoint))
            {
                if ((obj as TargetSpawnPoint).target == TargetSpawnPoint.Target.LocalVariable &&
                    (obj as TargetSpawnPoint).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as TargetSpawnPoint).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as TargetSpawnPoint).local.targetObject = (target as MonoBehaviour).gameObject;
                }
            }
            else if (field.FieldType == typeof(TargetActor))
            {
                if ((obj as TargetActor).target == TargetActor.Target.LocalVariable &&
                    (obj as TargetActor).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as TargetActor).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as TargetActor).local.targetObject = (target as MonoBehaviour).gameObject;
                }
            }
            else if (field.FieldType == typeof(NJG.RPG.IntProperty))
            {
                if ((obj as NJG.RPG.IntProperty).optionIndex == BaseProperty<int>.OPTION.UseLocalVariable &&
                    (obj as NJG.RPG.IntProperty).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as NJG.RPG.IntProperty).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as NJG.RPG.IntProperty).local.targetObject = (target as MonoBehaviour).gameObject;
                }
            }
#endif
#if PHOTON_RPG
            else if (field.FieldType == typeof(GameCreator.Variables.IntProperty))
            {
                if ((obj as GameCreator.Variables.IntProperty).optionIndex == BaseProperty<int>.OPTION.UseLocalVariable &&
                    (obj as GameCreator.Variables.IntProperty).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as GameCreator.Variables.IntProperty).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as GameCreator.Variables.IntProperty).local.targetObject =
 (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }
#endif
#if PHOTON_MODULE
            else if (field.FieldType == typeof(TargetPhotonView))
            {
                if (((TargetPhotonView) obj).target == TargetPhotonView.Target.LocalVariable &&
                    ((TargetPhotonView) obj).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as TargetPhotonView)?.local.targetObject, (target as MonoBehaviour)?.gameObject))
                {
                    ((TargetPhotonView) obj).local.targetObject = (target as MonoBehaviour)?.gameObject;
                    result = true;
                }
            }
            /*else if (field.FieldType == typeof(TargetPrefabId))
            {
                if (((TargetPrefabId) obj).target == TargetPrefabId.Target.GameObject &&
                    ((TargetPrefabId) obj).gameObject.local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as TargetPrefabId)?.gameObject.local.targetObject, (target as MonoBehaviour)?.gameObject))
                {
                    ((TargetPrefabId) obj).gameObject.local.targetObject = (target as MonoBehaviour)?.gameObject;
                    result = true;
                }
            }*/
            /*else if (field.FieldType == typeof(TargetPhotonPlayer))
            {
                if ((obj as TargetPhotonPlayer).target == TargetPhotonPlayer.Target.LocalVariable &&
                    (obj as TargetPhotonPlayer).local.targetType == HelperLocalVariable.Target.GameObject &&
                    CheckTarget((obj as TargetPhotonPlayer).local.targetObject, (target as MonoBehaviour).gameObject))
                {
                    (obj as TargetPhotonPlayer).local.targetObject = (target as MonoBehaviour).gameObject;
                    result = true;
                }
            }*/

#endif

            /*else if (field.FieldType == typeof(TargetPhotonVariable))
            {
                if ((obj as TargetSkillPosition).target == TargetSkillPosition.Target.LocalVariable &&
                    (obj as TargetSkillPosition).local.targetType == HelperLocalVariable.Target.GameObject &&
                    (obj as TargetSkillPosition).local.targetObject == null)
                {
                    (obj as TargetSkillPosition).local.targetObject = target.gameObject;
                }
            }*/
            return result;
        }

        private static bool CheckTarget(GameObject go, GameObject target)
        {
            if (!go) return true;
            if (go.GetComponent<GraphReactions>() && !go.Equals(target)) return true;
            return false;
        }

        /// <summary>
        /// Finds a node within a StateMachine.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Node FindNode(StateMachine root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            Node[] nodes = root.NodesRecursive;
            for (int i = 0; i < nodes.Length; i++)
            {
                Node node = nodes[i];
                if (node.name == name)
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if a node exists within a StateMachine.
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool NodeExists(StateMachine stateMachine, string name)
        {
            StateMachine root = stateMachine.Root;
            if (FindNode(root, name) == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Re-parent newly created ScriptableObjects to where they should belong.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static bool ParentChilds(ScriptableObject root)
        {
            if (!EditorUtility.IsPersistent(root))
            {
                return false;
            }

            bool shouldSave = false;
            FieldInfo[] fields = root.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (field == null || !CanBeCopied(field)) continue;

                if (field.FieldType.IsSubclassOf(typeof(ScriptableObject)))
                {
                    ScriptableObject value = (ScriptableObject) field.GetValue(root);

                    if (value != null)
                    {
                        value.hideFlags = HideFlags.HideInHierarchy;
                        if (!AssetDatabase.IsSubAsset(value) && !EditorUtility.IsPersistent(value))
                        {
                            AssetDatabase.AddObjectToAsset(value, root);
                            shouldSave = true;
                        }

                        ParentChilds(value);
                    }
                }
                else if (field.FieldType.IsArray)
                {
                    var array = field.GetValue(root) as Array;
                    Type elementType = field.FieldType.GetElementType();
                    if (elementType.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        foreach (ScriptableObject value in array)
                        {
                            if (value != null)
                            {
                                value.hideFlags = HideFlags.HideInHierarchy;
                                if (!AssetDatabase.IsSubAsset(value) && !EditorUtility.IsPersistent(value))
                                {
                                    AssetDatabase.AddObjectToAsset(value, root);
                                    shouldSave = true;
                                }

                                ParentChilds(value);
                            }
                        }
                    }
                }
            }

            //if(shouldSave) AssetDatabase.SaveAssets();
            return shouldSave;
        }

        /*public static void UpdateInstances(GraphReactions source)
        {
            //Debug.LogWarning("UpdateInstances " + source.instances.Count);
            var buffer = new List<int>(source.instances.Keys);
            foreach (var key in buffer)
            {
                var v = source.instances[key];
                if (v == null) continue;
                GraphUtility.CopyReactions(source, v);
            }
        }*/

        public static string Truncate(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }
    }
}