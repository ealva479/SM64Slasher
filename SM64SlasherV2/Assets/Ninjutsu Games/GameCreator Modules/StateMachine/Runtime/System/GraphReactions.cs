using System;
using System.Collections.Generic;
using GameCreator.Core;
using GameCreator.Variables;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NJG.Graph
{
    //[ExecuteInEditMode]
    public class GraphReactions : LocalVariables
    {
#if UNITY_EDITOR
        private const string VAR_INVOKER = "invoker";
        private const string REFERENCES = "references";
        private bool hasCheckedInvoker;

        //private SerializedObject serializedObject;
        //private SerializedProperty spReferences;
#endif
        public MBVariable invokerVariable;
        //private bool isExitingApplication = false;

        [Serializable]
        public class GraphReaction : SerializableDictionaryBase<int, Behaviour> { }

        [NonSerialized] public List<int> nodesSubscribed = new List<int>();

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool VariablesInitialized { get { return initalized; } }

        public GraphReaction components = new GraphReaction();
        public StateMachine stateMachine;
        //private bool destroying;

        /// <summary>
        /// Returns IConditionsList from source or live reactions object.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public TComponent GetCondition<TComponent>(Transition target) where TComponent : Behaviour
        {
            if (target.id <= 0) return null;

            if (!components.ContainsKey(target.id))
            {
//#if UNITY_EDITOR
//                Behaviour mono = Undo.AddComponent<TComponent>(gameObject);
//#else
                Behaviour mono = gameObject.AddComponent<TComponent>();
//#endif
                components.Add(target.id, mono);
#if UNITY_EDITOR
                EditorUtility.SetDirty(gameObject);
#endif
                return (TComponent)mono;
            }

            if (components[target.id] && components[target.id].GetType() == typeof(TComponent))
                return (TComponent) components[target.id];
            
            // Debug.LogWarningFormat("Error getting Condition for node: {0}", target.id);
            return null;

            //Debug.LogWarning("Conditions " + components[target.id]);

        }

        public void Reset()
        {
            RequireInit(true);
        }

        /// <summary>
        /// Returns true if this transition contains Conditions component.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool HasConditions(Transition target)
        {
            return components.ContainsKey(target.id);
        }

        public bool HasReaction(Node target)
        {
            return components.ContainsKey(target.id);
        }

        /// <summary>
        /// Returns IActionsList or Trigger from source or live reactions object.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public TComponent GetReaction<TComponent>(Node target) where TComponent : Behaviour
        {
            if (target.id <= 0) return null;

            if (!components.ContainsKey(target.id))
            {
//#if UNITY_EDITOR
//                Behaviour mono = Undo.AddComponent<TComponent>(gameObject);
//#else
                Behaviour mono = gameObject.AddComponent<TComponent>();
//#endif
                components.Add(target.id, mono);
#if UNITY_EDITOR
                EditorUtility.SetDirty(gameObject);
#endif
                return (TComponent)mono;
            }

            if (components[target.id] && components[target.id].GetType() == typeof(TComponent))
            {
                // Debug.Log("Reaction exists: "+target.id+" mono: "+components[target.id].gameObject+" Type: "+components[target.id].GetType(), components[target.id].gameObject);

                return (TComponent) components[target.id];
            }
            // Debug.LogWarningFormat(gameObject, "Error getting Reaction for node: {0} total components: {1} gameObject: {2}", target.id, components.Count, gameObject);
            return null;

            //Debug.LogWarning("Actions " + target.id+" / "+components[target.id]);

        }

        public void Initialize()
        {
            base.Initialize();
        }

#if UNITY_EDITOR

        /// <summary>
        /// Removes a IActionsList or Trigger from reactions object.
        /// </summary>
        /// <param name="target"></param>
        public void RemoveReaction(Node target)
        {
            if(!target) return;
            if(components == null) return;

            if (!components.ContainsKey(target.id))
            {
                //Debug.LogWarningFormat("Could not remove node: {0}", target.id);
                return;
            }
            var mono = components[target.id];
            components.Remove(target.id);
            //if (Application.isPlaying) Destroy(mono);
            //else
            if(mono) DestroyImmediate(mono, true);
        }

        /// <summary>
        /// Removes a IConditionsList from reactions object.
        /// </summary>
        /// <param name="target"></param>
        public void RemoveCondition(Transition target)
        {
            if(!target) return;
            if(components == null) return;
            
            if (!components.ContainsKey(target.id))
            {
                //Debug.LogWarningFormat("Could not remove condition: {0}", target.id);
                return;
            }
            var mono = components[target.id];
            components.Remove(target.id);
            //if (Application.isPlaying) Destroy(mono);
            //else
            if (mono) DestroyImmediate(mono, true);
        }

         protected override void OnValidate()
         {
             base.OnValidate();
             if (Application.isPlaying) return;

             /*if (stateMachine == null)
             {
                 //DestroyImmediate(gameObject);
                 Debug.LogWarningFormat(gameObject, "No StateMachine found. Attempt to destroy: {0}", gameObject);
                 return;
             }*/

             //HideStuff();
             /*if (stateMachine && name != stateMachine.name)
             {
                 AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(GetInstanceID()), stateMachine.name);
             }   */
             // if(stateMachine && stateMachine.id > 0) EditorApplication.delayCall += CleanUp;
         }

        [ContextMenu("Clean Up")]
        public void CleanUp()
        {
            if (this && !Application.isPlaying)
            {
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                // Debug.Log($"Found {count} missing scripts");
                count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                if(count > 0) Debug.Log($"Removed {count} missing scripts");

                EditorApplication.delayCall -= CleanUp;

                CleanUpRogueComponents(this);

                var keys2 = components.Keys;
                List<int> keysToRemove = new List<int>();

                foreach (var key in keys2)
                {
                    {
                        var comp = components[key];
                        bool emptyActions = comp == null ? false : (comp is IActionsList) && ((comp as IActionsList).actions == null || (comp as IActionsList).actions.Length == 0);
                        bool emptyConditions = comp == null ? false : (comp is IConditionsList) && ((comp as IConditionsList).conditions == null || (comp as IConditionsList).conditions.Length == 0);

                        if (comp == null) //  || emptyConditions|| emptyActions
                        {
                            keysToRemove.Add(key);
                        }
                    }
                }

                foreach (var key in keysToRemove)
                {
                    var mono = components[key];
                    if (mono)
                    {
                       // Debug.LogWarningFormat("Deleted mono: {0} on: {1} type: {2}", mono, this, mono.GetType());
                        DestroyImmediate(mono, true);
                    }
                    
                    components.Remove(key);
                }
                
                if (keysToRemove.Count > 0)
                {
                    // Debug.LogWarningFormat("Deleted unused: {0} components: {1} on: {2}", keysToRemove.Count, components.Count, this, this);
                }

                // Clean up emtpy references from variables list
                for(int i= 0; i< references.Length; i++)
                {
                    if(references[i] == null)
                    {
                        ArrayUtility.RemoveAt(ref references, i);
                    }
                }
            }
        }

        public static void CleanUpRogueComponents(GraphReactions react)
        {
            if (!react) return;
            if (Application.isPlaying) return;

            var actions = react.GetComponents<IAction>();
            int actionsCount = 0;

            foreach (var a in actions)
            {
                if (!IsActionInList(react, a))
                {
                    actionsCount++;
                    DestroyImmediate(a, true);
                }
            }
            
            var actions2 = react.GetComponents<IActionsList>();
            foreach (var a in actions2)
            {
                bool isInNodes = true;
                if (react.stateMachine != null) isInNodes = IsActionInNodes(react, a);
                if (!isInNodes)
                {
                    actionsCount++;
                    DestroyImmediate(a, true);
                }
//                Debug.LogWarningFormat("Action: {0} inNodes: {1} - {3} count: {2} SM: {3}", a, IsActionInNodes(react, a), actionsCount, isInNodes, react.stateMachine != null);
            }

            int conditionsCount = 0;
            /*var conditions = react.GetComponents<ICondition>();
            
            foreach (var a in conditions)
            {
                if (!IsConditionInList(react, a))
                {
                    conditionsCount++;
                    DestroyImmediate(a, true);
                }
            }*/

            var conditions2 = react.GetComponents<IConditionsList>();
            foreach (var a in conditions2)
            {
                bool isInNodes = true;
                if (react.stateMachine) isInNodes = IsConditionInNodes(react, a);
                if (!isInNodes)
                {
                    conditionsCount++;
                    DestroyImmediate(a, true);
                }
            }

            var igniters = react.GetComponents<Igniter>();
            int igniterCount = 0;
            foreach (var a in igniters)
            {
                if (!IsIgniterInList(react, a))
                {
                    igniterCount++;
                    DestroyImmediate(a, true);
                }
            }

            var triggers = react.GetComponents<GraphTrigger>();
            int triggersCount = 0;
            foreach (var a in triggers)
            {
                bool isInNodes = react.components.Values.Contains(a);
                if (react.stateMachine)
                {
                    isInNodes = react.components.Values.Contains(a) && IsTriggerInNodes(react, a);
                    //Debug.LogWarning("Trigger: " + a + " in nodes: " + isInNodes+ OnScreenEvent );
                }
                if (!isInNodes)
                {
                    triggersCount++;
                    DestroyImmediate(a, true);
                }
            }

            /*var nodes = react.stateMachine.NodesRecursive;
            foreach (var a in nodes)
            {
                if (!react.components.Values.Contains(a))
                {
                    triggersCount++;
                    DestroyImmediate(a, true);
                }
            }*/

            if (actionsCount > 0 || conditionsCount > 0 || igniterCount > 0 || triggersCount > 0)
            {
                // Debug.LogWarningFormat("Deleted rogue actions: {0} conditions: {1} igniters: {2} triggers: {3} from: {4}", actionsCount, conditionsCount, igniterCount, triggersCount, react, react);
            }
        }

        /*public static bool IsInVariables(LocalVariables list, MBVariable var)
        {
            foreach (var l in list.references)
            {
                if (l == var) return true;
            }

            return false;
        }*/
        
        public static bool IsActionInNodes(GraphReactions react, IActionsList a)
        {
            if (!react) return true;
            if (!react.stateMachine) return true;
            var nodes = react.stateMachine.NodesRecursive;
            foreach (var n in nodes)
            {
                if (n is ActionsState)
                {
                    var t = (n as ActionsState);
                    var gt = t.GetActions(react);
                    if (gt == a)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsTriggerInNodes(GraphReactions react, GraphTrigger a)
        {
            if (!react) return true;
            if (!react.stateMachine) return true;
            var nodes = react.stateMachine.NodesRecursive;
            foreach (var n in nodes)
            {
                if (n is TriggerState)
                {
                    var t = (n as TriggerState);
                    var gt = t.GetTrigger(react);
                    if (gt == a)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsConditionInNodes(GraphReactions react, IConditionsList a)
        {
            if (!react) return true;
            if (!react.stateMachine) return true;
            var nodes = react.stateMachine.NodesRecursive;

            foreach (var n in nodes)
            {
                if (n is ActionsState)
                {
                    var t = (n as ActionsState);
                    var gt = t.transitions;
                    foreach(var tn in gt)
                    {
                        if(tn.GetConditions(react) == a)
                        {
                            return true;
                        }
                    }
                }
                else if (n is TriggerState)
                {
                    var t = (n as TriggerState);
                    var gt = t.transitions;
                    foreach(var tn in gt)
                    {
                        if(tn.GetConditions(react) == a)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool IsActionInList(GraphReactions react, IAction a)
        {
            if (!react) return true;
            if (!react.stateMachine) return true;
            var actionLists = react.GetComponents<IActionsList>();
            foreach (var l in actionLists)
            {
                foreach (var al in l.actions)
                {
                    if (al == a) return true;
                }
            }

            return false;
        }

        public static bool IsConditionInList(GraphReactions react, ICondition a)
        {
            if (!react) return true;
            if (!react.stateMachine) return true;
            var actionLists = react.GetComponents<IConditionsList>();
            foreach (var l in actionLists)
            {
                foreach (var al in l.conditions)
                {
                    if (al == a) return true;
                }
            }

            return false;
        }

        public static bool IsIgniterInList(GraphReactions react, Igniter a)
        {
            if (!react) return true;
            if (!react.stateMachine) return true;
            var actionLists = react.GetComponents<Trigger>();
            foreach (var l in actionLists)
            {
                foreach (var al in l.igniters)
                {
                    if (al.Value == a) return true;
                }
            }

            return false;
        }

        /*private void Awake()
        {
            HideStuff();
            //CleanUp();
        }*/

        private void OnEnable()
        {
            //HideStuff();
            //gameObject.hideFlags = HideFlags.None;
            //CleanUp();
        }

        private void CheckInvoker()
        {
            //UnityEditor.EditorApplication.delayCall += CheckInvoker;
            if (!hasCheckedInvoker)
            {
                hasCheckedInvoker = true;
                bool hasInvoker = false;
                int imax = references.Length;
                for (int i = 0; i < imax; i++)
                {
                    MBVariable var = references[i];
                    if (var != null && var.variable != null && var.variable.name == VAR_INVOKER)
                    {
                        hasInvoker = true;
                        break;
                    }
                }
                Debug.Log("CheckInvoker " + hasInvoker+" / "+ this);
                if (!hasInvoker && this)
                {
                    //if (spReferences.arraySize == 0)
                    {
                        /*MBVariable variable = this.gameObject.AddComponent<MBVariable>();
                        variable.variable = new Variable(VAR_INVOKER, Variable.DataType.GameObject, this.gameObject, false);

                        spReferences.InsertArrayElementAtIndex(0);
                        spReferences.GetArrayElementAtIndex(0).objectReferenceValue = variable;

                        serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        serializedObject.Update();*/

                        MBVariable variable = gameObject.AddComponent<MBVariable>();
                        variable.variable.name = VAR_INVOKER;
                        variable.variable.type = (int)Variable.DataType.GameObject;

                        //spReferences.AddToObjectArray<MBVariable>(variable);

                        /*spReferences.InsertArrayElementAtIndex(spReferences.arraySize);
                        spReferences.GetArrayElementAtIndex(spReferences.arraySize - 1).objectReferenceValue = variable;

                        spReferences.serializedObject.ApplyModifiedProperties();
                        spReferences.serializedObject.Update();*/
                        invokerVariable = variable;
                        //this.AddSubEditorElement(variable, -1, true);
                    }
                }
            }
        }
        
        public static void CheckDirtyReactions()
        {
            if(Application.isPlaying) return;

            try
            {
                string[] assets = AssetDatabase.FindAssets(nameof(GraphReactions));
                if (assets != null)
                {
                    // Debug.LogWarningFormat("GraphReactions found: {0}", assets.Length);
                    foreach (string s in assets)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(s);
                        if (string.IsNullOrEmpty(path)) continue;
                        var r = AssetDatabase.LoadAssetAtPath<GraphReactions>(path);
                        if (!r) continue;
                        if (!r.stateMachine) continue;
                        // Debug.LogWarningFormat("GraphReaction asset: {0} reaction: {1}", s, r);
                        r.CleanUp();
                    }
                }
            }
            catch
            {
                //
            }
        }

        public static void CheckOrphanReactions()
        {
            if(Application.isPlaying) return;
            try
            {
                string[] assets = AssetDatabase.FindAssets(nameof(GraphReactions));
                string[] assets2 = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(StateMachine)));
                if (assets != null)
                {
                    // Debug.LogWarningFormat("GraphReactions found: {0}", assets.Length);
                    foreach (string s in assets)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(s);
                        if (string.IsNullOrEmpty(path)) continue;
                        var r = AssetDatabase.LoadAssetAtPath<GraphReactions>(path);
                        if (!r) continue;
                        if (r.stateMachine == null)
                        {
                            // bool found = false;
                            // Debug.LogWarningFormat("StateMachines found: {0}", assets2.Length);

                            foreach (string s2 in assets2)
                            {
                                string path2 = AssetDatabase.GUIDToAssetPath(s2);
                                var sm = AssetDatabase.LoadAssetAtPath<StateMachine>(path2);

                                if (!sm) continue;
                                // Debug.LogWarningFormat("2 Trying to find SM for r: {0} sm: {1}", r, sm);
                                if (r.name.Contains(sm.id.ToString()) || sm.sourceReactions == r)
                                {
                                    r.stateMachine = sm;
                                    // found = true;
                                    EditorUtility.SetDirty(r);
                                    break;
                                }
                            }
                            /*if(!found)
                            {
                                Debug.LogWarningFormat("Reaction r: {0} with empty SM delete: {1}", r, path);
                                if (PrefabUtility.IsPartOfRegularPrefab(r))
                                {
                                    AssetDatabase.DeleteAsset(path);
                                    // DestroyImmediate(r.gameObject, true);
                                }
                            }*/
                        }
                    }
                }
            }
            catch
            {
                //
            }
        }
#endif

        private void OnDisable()
        {
            nodesSubscribed.Clear();
            //CleanUp();
        }
        /*protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            isExitingApplication = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!Application.isPlaying && !isExitingApplication && gameObject != null && this && !destroying)
            {
                destroying = true;
                var buffer = new List<int>(components.Keys);

                foreach (var key in buffer)
                {
                    if (components.ContainsKey(key) && components[key] && components[key] != this)
                    {
                        //Debug.Log("Destroying " + key + " / " + components[key]);
                        DestroyImmediate(components[key], true);
                    }
                }

                nodesSubscribed.Clear();
                nodesSubscribed = null;
            }
        }*/
    }
}
