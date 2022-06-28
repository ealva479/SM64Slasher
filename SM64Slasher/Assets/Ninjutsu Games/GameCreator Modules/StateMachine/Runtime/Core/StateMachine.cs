#if SM_RPG
using NJG.RPG;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using GameCreator.Core;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif


namespace NJG.Graph
{
    [Serializable]
    [CreateAssetMenu(fileName = "State Machine", menuName = "Game Creator/State Machine")]
    public class StateMachine : Node
    {
        private static readonly Node[] EMPTY = new Node[0];

#if UNITY_EDITOR
        public GraphReactions oldSource;
#endif

        public Node startNode;
        public Node upNode;
        public Node[] nodes = new Node[0];
        public GraphReactions sourceReactions;
        public Blackboard blackboard = new Blackboard();

        public List<Blackboard.Item> GetBlackboardItems()
        {
            List<Blackboard.Item> items = new List<Blackboard.Item>(blackboard.list);
            for (int i = 0; i < nodes.Length; ++i)
            {
                Node node = nodes[i];
                if (node && node != this && node is StateMachine)
                {
                    items.AddRange((node as StateMachine).GetBlackboardItems());
                }
            }

            return items;
        }

        public Node[] NodesRecursive
        {
            get
            {
                List<Node> nodes = new List<Node>();
                if (this.nodes.Length > 0)
                {
                    nodes.AddRange(this.nodes);
                }

                var states = StateMachines;
                if (states != null && states.Length > 0 )
                {
                    for (int i = 0, imax = states.Length; i < imax; i++)
                    {
                        StateMachine node = states[i];
                        nodes.AddRange(node.NodesRecursive);
                    }
                }
                return nodes.ToArray();
            }
        }

        
        public StateMachine[] StateMachines
        {
            get
            {
                if (nodes == null || nodes.Length == 0) return EMPTY as StateMachine[];
                List<StateMachine> list = new List<StateMachine>();
                for(int i = 0, imax = nodes.Length; i<imax; i++)
                {
                    if (nodes[i] is StateMachine) list.Add(nodes[i] as StateMachine);
                }
                return list.ToArray();
            }
        }

        public ActionsState[] ActionStates
        {
            get
            {
                if (nodes == null || nodes.Length == 0) return EMPTY as ActionsState[];
                List<ActionsState> list = new List<ActionsState>();
                for (int i = 0, imax = nodes.Length; i < imax; i++)
                {
                    if (nodes[i] is ActionsState) list.Add(nodes[i] as ActionsState);
                }
                return list.ToArray();
            }
        }

        /// <summary>
        /// Returns an Actions State node by name.
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public ActionsState GetActionsState(string stateName)
        {
            var states = ActionStates;
            for (int i = 0, imax = states.Length; i < imax; i++)
            {
                ActionsState state = states[i];
                if (state.name.Equals(stateName))
                {
                    return state;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a node by name.
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public Node GetNode(string nodeName)
        {
            Node[] nodes = NodesRecursive;
            for (int i = 0, imax = nodes.Length; i < imax; i++)
            {
                Node node = this.nodes[i];
                if (node.name.Equals(nodeName))
                {
                    return node;
                }
            }
            return null;
        }


        public virtual GameObject Init(StateMachineController controller)
        {
            //this.owner = component;
            GameObject instance = null;
//            Debug.LogWarning("SM Init Application.isPlaying "+Application.isPlaying);

            if (Application.isPlaying)
            {
                StateMachine sm = this;

                /*if (!controller.reactions)
                {
                    instance = Instantiate(
                        sm.sourceReactions.gameObject,
                        controller.transform.position,
                        controller.transform.rotation,
                        controller.transform
                    );
                    //instance.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
                    //instance.SetActive(false);
                    GraphReactions reactions = instance.GetComponent<GraphReactions>();
                    controller.reactions = reactions;
                    reactions.Initialize();
                    //controller.Initialize();
                    for (int i = 0, imax = controller.references.Length; i < imax; i++)
                    {
                        MBVariable controllerVar = controller.references[i];
                        if (i >= reactions.references.Length) break;

                        MBVariable reactionVar = reactions.references[i];
                        
                        if (reactionVar.variable.name == controllerVar.variable.name)
                        {
                            //reactionVar = controllerVar;
                            //reactionVar.variable = new Variable(controllerVar.variable);
                            reactionVar.variable.Update(controllerVar.variable.Get());
                            Debug.Log("Controller ref " + i + " name: " + controllerVar.variable.name + " = " + reactionVar.variable.name+" val1: "+ controllerVar.variable.Get()+" val2: "+reactionVar.variable.Get()+
                                " init1: "+ reactions.VariablesInitialized+" init2: "+ controller.VariablesInitialized);
                        }
                    }
                    
                    //instance.SetActive(true);
                }*/
                sm.OnEnter(controller.reactions, controller.gameObject);
            }

            return instance;
        }

        /// <summary>
        /// Returns the current Reactions object wheter the application is running or not.
        /// If application is running source reactions are instantiated on the scene.
        /// </summary>
        /*public GraphReactions Reactions
        {
            get
            {
                
                if(parent != null && parent.Root != null && parent.Root.Reactions != null)
                {
                    return parent.Root.Reactions;
                }

                if (Owner != null && Owner.instance.sourceReactions != null)
                {
                    return this.Owner.instance.sourceReactions;
                }
#if UNITY_EDITOR
                else if (sourceReactions == null)
                {
                    sourceReactions = CreateReactions(id);
                }
#endif
                return sourceReactions;
            }
        }*/

#if UNITY_EDITOR

        private void CheckDuplicates()
        {
            if (sourceReactions == null) return;

            /*bool isDuplicate = false;
            var paths = AssetDatabase.FindAssets("t:StateMachine");
            foreach(var p in paths)
            {
                var sm = AssetDatabase.LoadAssetAtPath<StateMachine>(AssetDatabase.GUIDToAssetPath(p));
                if (sm == this || sm.sourceReactions == null) continue;

                if (sm.sourceReactions.GetInstanceID() == sourceReactions.GetInstanceID())
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (isDuplicate)
            {
                oldSource = sourceReactions;
                id = Mathf.Abs(Guid.NewGuid().GetHashCode());
                sourceReactions = null;
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
            }*/
            
            
                
            // if(GraphEditor.Instance) GraphEditor.Instance.SetupBlackboard();
            // BlackboardWindow.UpdateVariables();
        }

        protected virtual void Awake()
        {
            CheckDuplicates();
            
            if (this && id <= 0 && EditorUtility.IsPersistent(this) && sourceReactions == null)
            {
                id = Mathf.Abs(Guid.NewGuid().GetHashCode());

                Node node = (Node)CreateInstance(typeof(ActionsState));
                node.hideFlags = HideFlags.HideInHierarchy;
                node.id = Mathf.Abs(Guid.NewGuid().GetHashCode());
                node.name = "Start";
                node.parent = this;
                node.color = 5;

                AssetDatabase.AddObjectToAsset(node, this);

                var nodes = this.nodes;
                ArrayUtility.Add(ref nodes, node);
                this.nodes = nodes;
                node.IsStartNode = true;

                node.position = new Rect(25000, 25000, 150, 30);
            }
        }

        private void OnValidate()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                CheckDuplicates();
                if (startNode && !startNode.IsStartNode) startNode.IsStartNode = true;
                if (sourceReactions && !sourceReactions.stateMachine)
                {
                    //Debug.LogWarning("Set StateMachine: " + this, this);
                    sourceReactions.stateMachine = this;
                    /*int count = (sourceReactions.instances == null ? 0 : sourceReactions.instances.Count);
                    if (count > 0 && !(this is SkillStateMachine))
                    {
                        Debug.LogWarning("StateMachine update " + count+" on "+this, this);
                    }*/
                }
            }
        }
#endif
        /// <summary>
        /// Initialize Start Node and prepare Trigger States.
        /// </summary>
        public override void OnEnter(GraphReactions reactions, GameObject invoker)
        {
            base.OnEnter(reactions, invoker);

            if(startNode) startNode.OnEnter(reactions, invoker);
            //if(entryNode) entryNode.OnEnter();

            for (int i = 0, imax = nodes.Length; i < imax; i++)
            {
                if (nodes[i] is TriggerState)
                {
                    ((TriggerState) nodes[i]).OnEnter(reactions, invoker);
                }
            }
        }

        public override void OnExit(GraphReactions reactions, GameObject invoker)
        {
            base.OnExit(reactions, invoker);

            //if (exitNode) exitNode.OnEnter();

            for (int i = 0, imax = nodes.Length; i < imax; i++)
            {
                if (nodes[i] is TriggerState)
                {
                    ((TriggerState) nodes[i]).OnExit(reactions, invoker);
                }
            }
        }        

        private void OnDestroy()
        {
#if UNITY_EDITOR
//            Debug.LogFormat("sourceReactions {0} this: {1} hasParent: {2}", sourceReactions, this, parent != null);
            if (sourceReactions != null && parent == null)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(sourceReactions));
                Debug.Log("Delete sourceReactions " + AssetDatabase.GetAssetPath(sourceReactions), this);

                AssetDatabase.SaveAssets();
            }
#endif
        }

#if UNITY_EDITOR
        private const string NAME_PREFAB_REACTIONS = "graphReactions-{0}.prefab";
        private const string PATH_PREFAB_REACTIONS = "Assets/Plugins/GameCreatorData/StateMachine/";

        public static GraphReactions CreateReactions(StateMachine sm)
        {
            GraphReactions reactions = null;
#if SM_RPG
            if (sm is SkillStateMachine)
            {
                SkillStateMachine ssm = sm as SkillStateMachine;
                if (ssm.skillAsset.skillReactions)
                {
                    reactions = ssm.skillAsset.skillReactions.gameObject.GetComponent<GraphReactions>();
                    if (!reactions) reactions = ssm.skillAsset.skillReactions.gameObject.AddComponent<GraphReactions>();

                    reactions.stateMachine = ssm;
                    if (GUI.changed) EditorUtility.SetDirty(ssm.skillAsset.skillReactions);
                }
            }
            else
#endif
            {
                GameCreatorUtilities.CreateFolderStructure(PATH_PREFAB_REACTIONS);

                string pathname = PATH_PREFAB_REACTIONS;
                string filename = string.Format(NAME_PREFAB_REACTIONS, sm.id);

                GameObject sceneInstance = new GameObject("graphReactions");
                sceneInstance.AddComponent<GraphReactions>();

                string path = Path.Combine(pathname, filename);
                path = AssetDatabase.GenerateUniqueAssetPath(path);

                GameObject prefabInstance = PrefabUtility.SaveAsPrefabAsset(sceneInstance, path);
                // Debug.LogWarningFormat("Create reactions: {0} prefabInstance: {1}", path, prefabInstance);

                if (prefabInstance)
                {
                    reactions = prefabInstance.GetComponent<GraphReactions>();
                    reactions.stateMachine = sm;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(prefabInstance));
                }
                else
                {
                    AssetDatabase.SaveAssets();
                    // AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }
                DestroyImmediate(sceneInstance);
            }
            
            // AssetDatabase.SaveAssets();
            return reactions;
        }


        public void Export()
        {
            string packageFilename = $"{name}.unitypackage";
            string relativePackagePath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
            relativePackagePath = Path.Combine(relativePackagePath, packageFilename);
            string absolutePackagePath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length), relativePackagePath);

            Object otherPackage = AssetDatabase.LoadMainAssetAtPath(relativePackagePath);
            if (otherPackage != null) AssetDatabase.DeleteAsset(relativePackagePath);

            List<string> paths = new List<string>();
            paths.Add(AssetDatabase.GetAssetPath(this));
            paths.Add(AssetDatabase.GetAssetPath(sourceReactions));

            AssetDatabase.ExportPackage(
                paths.ToArray(),
                absolutePackagePath,
                ExportPackageOptions.Recurse
            );

            AssetDatabase.Refresh();
            
            Debug.LogFormat("State Machine exported to: {0}", relativePackagePath);
            EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(relativePackagePath));
            //Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(relativePackagePath);
        }
#endif
        }
    }