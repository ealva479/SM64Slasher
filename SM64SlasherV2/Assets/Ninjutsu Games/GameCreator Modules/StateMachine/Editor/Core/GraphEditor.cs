using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GameCreator.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if SM_RPG
using NJG.RPG;
#endif

namespace NJG.Graph
{
    public class GraphEditor : BaseGraphEditor
    {
        private List<StateMachine> breadcrumbs;

        public static GraphEditor Instance;// { get; private set; }

        public enum SelectionMode
        {
            None,
            Pick,
            Rect,
        }

        private const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

        private Node[] Nodes => Active == null ? new Node[0] : Active.nodes;

        [SerializeField]
        private List<Node> selection = new List<Node>();

        public static int SelectionCount => Instance != null ? Instance.selection.Count : 0;

        public static List<Node> SelectedNodes => Instance == null ? null : Instance.selection;

        private bool centerView;
        private Vector2 selectionStartPosition;
        private Vector2 lastClickPos;
        private SelectionMode selectionMode;
        public Node fromNode;
        private Rect toolbarRect;
        private Rect bottomToolbarRect;

        #if SM_RPG
        private Rect skillToolbarRect;
#endif

        [SerializeField]
        private ShortcutEditor shortcutEditor;

        [SerializeField]
        private Transition selectedTransition;

        public static Transition SelectedTransition => Instance == null ? null : Instance.selectedTransition;

        [SerializeField]
        private StateMachine active;
        public static StateMachine Active => Instance == null ? null : Instance.active;

        public static GameObject ActiveGameObject => Instance == null ? null : Instance.activeGameObject;
        public static StateMachine Root => Active == null ? null : Active.Root;


        public static StateMachineController Controller
        {
            get
            {
                if (ActiveGameObject)
                {
                    var controller = ActiveGameObject.GetComponent<StateMachineController>();
                    if (controller)
                    {
                        return controller;
                    }
                }
                return null;
            }
        }

        public static GraphReactions Reactions
        {
            get
            {
                // Active.sourceReactions ?? Active.Root.sourceReactions;
                GraphReactions newReactions = null;

                if (Application.isPlaying && Controller && 
                    Controller.reactions && Active.Root == Controller.stateMachine)
                {
                    newReactions = Controller.reactions;
                    //Debug.LogWarningFormat("1 Reactions Controller {0} Active:{1}", reactions, Active);
                }
                
                if (!newReactions && Active && Active.sourceReactions)
                {
                    newReactions = Active.sourceReactions;
                    // Debug.LogWarningFormat("2 Reactions Source {0} Active:{1}", reactions, Active);
                }
                
                if (!newReactions && Active && Active.Root && Active.Root.sourceReactions)
                {
                    newReactions = Active.Root.sourceReactions;
                    // Debug.LogWarningFormat("3 Reactions Root {0} Active:{1}", reactions, Active);
                }
                
                if (!newReactions && Active && Active.parent && Active.parent.sourceReactions)
                {
                    newReactions = Active.parent.sourceReactions;
                    // Debug.LogWarningFormat("4 Reactions Parent {0} Active:{1}", reactions, Active);
                }

                if (reactions != newReactions)
                {
                    //Selection.activeObject != Active || 
                    //Debug.LogWarningFormat("reactions error active: {0} go: {1} lastActive: {2} last: {3}", 
                    //    Active, ActiveGameObject, Instance.lastActive, StateMachine.LastReactions);
                    //Debug.LogWarningFormat("reactions error active: {0} source: {1} root: {2} rootSource: {3}, parent: {4}, reactions {5}",
                    //    Active, Active.sourceReactions, Active.Root, Active.Root.sourceReactions, Active.parent, reactions);
                    //!reactions && 
                    if (Active && Active.isSelected) Active.isSelected = false;
                    SelectedNodes?.Clear();
                    // Selection.activeObject = Active;

                    if (Instance)
                    {
                        // Debug.LogWarningFormat("Reactions Cleanup");

                        Instance.CleanUpEditors();
                        if (Instance.active) Instance.active.isSelected = false;
                        Instance.active = Active;
                        Instance.selection.Clear();
                        Editors.Clear();
                    }
                    reactions = newReactions;
                    SelectStateMachine(Active);
                    if (Active) Active.isSelected = true;
                    //Instance.SetupBlackboard();
                }

                //Debug.LogWarningFormat("5 Reactions {0} Active:{1} root: {2} selection: {3}", reactions, Active, Active.Root.sourceReactions, Selection.activeObject);

                return reactions;
            }
        }

        public BlackboardWindow windowBlackboard;// { private set; get; }

        private static StateMachine lastActive;

        public StateMachineInspector StateMachineEditor
        {
            get
            {
                if (Editors == null || !Root) return null;
                //stateMachineEditor = null;
                if (!Editors.ContainsKey(Root))
                {
                    stateMachineEditor = Editor.CreateEditor(Root, typeof(StateMachineInspector)) as StateMachineInspector;;
                    Editors.Add(Root, stateMachineEditor);
                }
                else
                {
                    stateMachineEditor = (StateMachineInspector)Editors[Root];
                }

                if (stateMachineEditor.serializedObject == null || stateMachineEditor.serializedObject.targetObject == null)
                {
                    stateMachineEditor = Editor.CreateEditor(Root, typeof(StateMachineInspector)) as StateMachineInspector;
                    Editors[Root] = stateMachineEditor;
                }
                
                if (stateMachineEditor)
                {
                    stateMachineEditor.SpBlackboard = stateMachineEditor.serializedObject.FindProperty("blackboard");
                    stateMachineEditor.SpBlackboardList = stateMachineEditor.SpBlackboard.FindPropertyRelative("list");
                }

                return stateMachineEditor;
            }
        }

        public GraphReactionsEditor ReactionsEditor
        {
            get
            {
                if (Editors == null || !Reactions) return null;

                if (!Editors.ContainsKey(Reactions))
                {
                    reactionsEditor = Editor.CreateEditor(Reactions, typeof(GraphReactionsEditor)) as GraphReactionsEditor;;
                    Editors.Add(Reactions, reactionsEditor);
                }
                else
                {
                    reactionsEditor = (GraphReactionsEditor)Editors[Reactions];
                }
                if (reactionsEditor)
                {
                    //reactionsEditor.spBlackboard = reactionsEditor.serializedObject.FindProperty("blackboard");
                    //reactionsEditor.spBlackboardList = stateMreactionsEditorachineEditor.spBlackboard.FindPropertyRelative("list");
                }

                return reactionsEditor;
            }
        }

        private StateMachineInspector stateMachineEditor;// { private set; get; }
        private GraphReactionsEditor reactionsEditor;// { private set; get; }

        [SerializeField]
        private GameObject activeGameObject;
        private bool lastDocked;
        private StateMachine overStateMachine;
        public static bool shouldUpdate;
        private static Dictionary<System.Type, GUIContent> IconGUI = new Dictionary<System.Type, GUIContent>();
        private static Dictionary<System.Type, string> NodeNames = new Dictionary<System.Type, string>();
        private static Dictionary<UnityEngine.Object, Editor> Editors = new Dictionary<UnityEngine.Object, Editor>();
        private static GraphReactions reactions = null;

        [MenuItem("Window/State Machine")]
        public static GraphEditor ShowWindow()
        {
            GraphEditor window = EditorWindow.GetWindow<GraphEditor>("State Machine");
            window.autoRepaintOnSceneChange = true;
            return window;
        }

        //[InitializeOnLoadMethod]
        /*private static void UpdateInstances()
        {
            //Dictionary<int, GraphReactions.Instances> toDelete = new Dictionary<int, GraphReactions.Instances>();
            List<StateMachine> sms = GameCreator.Core.GameCreatorUtilities.FindAssetsByType<StateMachine>();
            foreach (var sm in sms)
            {
                if (sm == null || sm.sourceReactions == null || sm.sourceReactions.instances == null) continue;

                var keys = sm.sourceReactions.instances.Keys;
                try
                {
                    foreach (var k in keys)
                    {
                        var val = sm.sourceReactions.instances[k];
                        var re2 = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(val);
                        GraphReactions re = AssetDatabase.LoadAssetAtPath<GraphReactions>(val);
                        var obj = EditorUtility.InstanceIDToObject(k);

                        //Debug.Log("1 Re " + re+" / "+r.Value+" obj2: "+ re2+ " / obj: "+ obj + " / status: " + PrefabUtility.GetPrefabInstanceStatus(re));

                        if (re2)
                        {
                            if (re2 is GameObject)
                            {
                                var go = (re2 as GameObject);
                                GraphReactions rc = go.GetComponent<GraphReactions>();
                                //Debug.Log("rc " + rc+" / "+ (re2 as GameObject).scene.path);

                                if (rc)
                                {
                                    //if(sm.sourceReactions.components.Count != rc.components.co)
                                    GraphUtility.CopyReactions(sm.sourceReactions, rc);

                                    //EditorUtility.SetDirty(go);
                                    EditorUtility.SetDirty(rc);
                                    //EditorUtility.SetDirty(sm);
                                    // EditorUtility.SetDirty(sm.sourceReactions);

                                    //EditorSceneManager.MarkSceneDirty(go.scene);
                                    //EditorSceneManager.SaveScene(go.scene);
                                }
                            }
                        }
                        else if (re)
                        {
                            //Debug.Log("2 Re " + re + " / " + PrefabUtility.GetPrefabAssetType(re) + " / " + PrefabUtility.GetPrefabInstanceStatus(re));
                            //PrefabUtility.SavePrefabAsset(re.gameObject);
                            GraphUtility.CopyReactions(sm.sourceReactions, re);
                            EditorUtility.SetDirty(re);
                            //EditorUtility.SetDirty(sm);
                            //EditorUtility.SetDirty(sm.sourceReactions);
                        }
                        else
                        {
                            //toDelete.Add(k, sm.sourceReactions.instances);
                            //EditorUtility.SetDirty(sm);
                            //EditorUtility.SetDirty(sm.sourceReactions);
                            //sm.sourceReactions.instances.Remove(r.Key);
                        }
                    }
                }
                catch { }
            }

            /*foreach(var t in toDelete)
            {
                t.Value.Remove(t.Key);
            }*
            //GraphUtility.CopyReactions(controller.stateMachine.sourceReactions, controller.reactions);
        }*/

        public bool IsValidNode(Node node)
        {
#if SM_RPG
            return !(node is UpState) && !(node is ExitState) && !(node is SkillStateMachine);
#else
            return !(node is UpState) && !(node is ExitState);
#endif
        }

        private void OnSelectionChange()
        {
            if (Instance == null)
            {
                var window = EditorWindow.GetWindow<GraphEditor>("State Machine");
                Instance = window;
                //GraphEditor.SelectStateMachine(sm);
                window.Show();
            }
            
            // Debug.Log("1 OnSelectionChange go: "+Selection.activeGameObject+" Active: "+Active+" last: "+lastActive+" activeObject: "+Selection.activeObject+
            //           " type: "+(Selection.activeObject != null ? Selection.activeObject.GetType().ToString() : "")+
            //           " meetConditions: "+(Selection.activeObject != null && (Selection.activeObject != null && Selection.activeObject is StateMachine)));
            //if(Selection.activeGameObject == null && Active != null) SelectStateMachine(Active, true, true);

            if (Selection.activeGameObject) SelectGameObject(Selection.activeGameObject);
            if (Selection.activeObject != null && Selection.activeObject is StateMachine)
            {
                var sm = Selection.activeObject as StateMachine;
                // Debug.LogWarningFormat("TryChange sm: {0} parent: {1} active: {2}", sm, sm.parent, Active);
                if (sm.parent == null || Active == null)
                {
                    
                    //Instance.CleanUpEditors();
                    if (Instance.active) Instance.active.isSelected = false;
                    Instance.selection.Clear();
                    Editors.Clear();
                    
                    Instance.active = sm;
                    CenterView();
                    Repaint();
                    SetupBlackboard();
                    // Debug.LogWarningFormat("2 OnSelectionChange sm: {0} parent: {1} active: {2} Instance: {3}", sm, sm.parent, Active, Instance);

                    
                    // if(Active == null) SelectStateMachine(sm, true, true);
                }
            }
        }

        private void OnDestroy()
        {
            //Selection.activeObject = null;
            //CleanUpEditors();
            Editors.Clear();
            if (Active && Active.isSelected) Active.isSelected = false;
            if(SelectedNodes != null) SelectedNodes.Clear();
            selection.Clear();
            UpdateSelection();
            if(Instance) Instance.selection.Clear();
            //System.GC.Collect();
            // stateMachineEditor = null;
            // reactionsEditor = null;
           // windowBlackboard = null;
            //Instance = null;
            //AssetDatabase.Refresh();
            //Debug.Log("Graph on destroy");
        }

        private void CleanUpEditors()
        {
            // Debug.LogWarningFormat("CleanUpEditors");
            /*foreach (var ed in Editors)
            {
                if(ed.Value) DestroyImmediate(ed.Value);
            }*/
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (breadcrumbs == null) breadcrumbs = new List<StateMachine>();
            Instance = this;
            if (shortcutEditor == null) shortcutEditor = new ShortcutEditor();
            centerView = true;


            //EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange playMode)
        {
            if (ActiveGameObject != null)
            {
                StateMachineController behaviour = ActiveGameObject.GetComponent<StateMachineController>();
                if (behaviour != null)
                {
                    SelectStateMachine(behaviour.stateMachine);
                }
            }
        }

        public void SetupBlackboard()
        {
            if (Root && Reactions)
            {
                stateMachineEditor = null;
                reactionsEditor = null;

                // if (!Editors.ContainsKey(Root))
                // {
                //     stateMachineEditor = Editor.CreateEditor(Root, typeof(StateMachineInspector)) as StateMachineInspector;;
                //     Editors.Add(Root, stateMachineEditor);
                // }
                // else
                // {
                //     stateMachineEditor = (StateMachineInspector)Editors[Root];
                // }
                //
                // if (stateMachineEditor)
                // {
                //     stateMachineEditor.SpBlackboard = stateMachineEditor.serializedObject.FindProperty("blackboard");
                //     stateMachineEditor.SpBlackboardList = stateMachineEditor.SpBlackboard.FindPropertyRelative("list");
                // }
                /*if (!Editors.ContainsKey(Reactions))
                {
                    reactionsEditor = Editor.CreateEditor(Reactions, typeof(GraphReactionsEditor)) as GraphReactionsEditor;;
                    Editors.Add(Reactions, reactionsEditor);
                }
                else
                {
                    reactionsEditor = (GraphReactionsEditor)Editors[Reactions];
                }
                if (reactionsEditor)
                {
                    //reactionsEditor.spBlackboard = reactionsEditor.serializedObject.FindProperty("blackboard");
                    //reactionsEditor.spBlackboardList = stateMreactionsEditorachineEditor.spBlackboard.FindPropertyRelative("list");
                }*/
            }
            if(Instance.ReactionsEditor) Instance.ReactionsEditor.CheckInvoker();
            windowBlackboard = new BlackboardWindow(this);

        }

        private void Update()
        {
            // if (lastDocked != IsDocked)
            {
                lastDocked = IsDocked;
                //Repaint();
            }
            
            if (Active != null && Active.sourceReactions && !Active.sourceReactions.stateMachine)
            {
                Active.sourceReactions.stateMachine = Active.Root;
            }

            if (Active != null && (stateMachineEditor == null || reactionsEditor == null) && lastActive != Active)
            {
                SetupBlackboard();
                lastActive = Active;
            }

            if (Root != null && Root.id <= 0 && EditorUtility.IsPersistent(Root) && !Root.sourceReactions)
            {
#if SM_RPG
                if (Root is SkillStateMachine)
                {
                    InitSkillSM(Root as SkillStateMachine);
                }
                else
#endif
                {
                    Root.id = Mathf.Abs(Guid.NewGuid().GetHashCode());
                    Root.sourceReactions = StateMachine.CreateReactions(Root);
                    if(!Root.startNode)
                    {
                        var startNode = GraphUtility.AddNode<ActionsState>(Center, Root);
                        startNode.name = "Start";
                    }
                    SetupBlackboard();
                    BlackboardWindow.UpdateVariables();
                }

                CenterView();
            }

            /*if (Root != null && Root.oldSource != null)
            {
                if (Root.sourceReactions == null) Root.sourceReactions = StateMachine.CreateReactions(Root);
                Debug.LogWarningFormat("Copying: {0} to {1}...", Root.oldSource, Root.sourceReactions);
                GraphUtility.CopyReactions(Root.oldSource, Root.sourceReactions);
                SetupBlackboard();
                BlackboardWindow.UpdateVariables();
                Root.oldSource = null;
            }*/

            if (Active != null)
            {
                if (EditorApplication.isPlaying)
                {
                    debugProgress += Time.deltaTime * 120;

                    for (int i = 0; i < transitionsProgress.Count; i++)
                    {
                        Transition transition = transitionsProgress[i];
                        if (transition != null)
                        {
                            transition.progress += Time.deltaTime * 280;
                            if (transition.progress > 100)
                            {
                                transition.entered = false;
                                transitionsProgress.RemoveAt(i);
                            }
                        }
                    }

                    for (int i = 0; i < nodesProgress.Count; i++)
                    {
                        Node node = nodesProgress[i];
                        if (node != null)
                        {
                            node.progress += Time.deltaTime * (node is TriggerState ? 400 : 240);
                            if (node.progress > 142)
                            {
                                node.progress = 0;
                                node.internalExecuting = false;
                                nodesProgress.RemoveAt(i);
                            }
                        }
                    }
                    /*transitionProgress += Time.deltaTime * 60;
                    if (transitionProgress > 100)
                    {
                        transitionProgress = 0;

                        for (int i = 0; i < Nodes.Length; i++)
                        {
                            Node node = Nodes[i];
                            if (node == null) continue;
                            ClearTransition(node);
                        }
                    }*/
                    if (debugProgress > 142)
                    {
                        debugProgress = 0;
                    }
                    //RepaintAll();
                }
            }
            
            /*Debug.LogWarningFormat("Selection {0} count: {1} Controller: {2}", Selection.activeObject ? Selection.activeObject.GetType() : null, Selection.objects.Length, Controller);

            if(Selection.activeObject != null && Selection.activeObject.GetType() == typeof(TriggerState))
            {
                if (Controller && Controller.stateMachineCollider && !Controller.enableDebugDrawCollider)
                {
                    Controller.enableDebugDrawCollider = true;
                }
            }

            if (Controller && !Controller.enableDebugDrawCollider &&
                (Selection.activeObject == null || Selection.objects.Length == 0))
            {
                Controller.enableDebugDrawCollider = false;
            }*/

            //RepaintAll();
        }

#if SM_RPG
        public static void InitSkillSM(SkillStateMachine skillStateMachine)
        {
            skillStateMachine.id = Mathf.Abs(System.Guid.NewGuid().GetHashCode());
            if (!skillStateMachine.sourceReactions)
            {
                skillStateMachine.sourceReactions = StateMachine.CreateReactions(skillStateMachine);
                Debug.LogWarning("Initializing Skill StateMachine...");
            }

            SkillStateMachine node = GraphUtility.AddNode<SkillStateMachine>(GraphEditor.Center, skillStateMachine);
            node.name = "On Activate";
            //node.position = new Rect(24500, 25000, 150, 30);

            skillStateMachine.onActivateNode = node;

            ////

            node = GraphUtility.AddNode<SkillStateMachine>(GraphEditor.Center, skillStateMachine);
            node.name = "On Cast";
            //node.position = new Rect(24500, 25100, 150, 30);

            skillStateMachine.onCastNode = node;

            ////

            node = GraphUtility.AddNode<SkillStateMachine>(GraphEditor.Center, skillStateMachine);
            node.name = "On Hit";
            //node.position = new Rect(24500, 25200, 150, 30);

            skillStateMachine.onHitNode = node;

            node = GraphUtility.AddNode<SkillStateMachine>(GraphEditor.Center, skillStateMachine);
            node.name = "On End";
            //node.position = new Rect(24500, 25300, 150, 30);

            skillStateMachine.onEndNode = node;

            AssetDatabase.SaveAssets();
        }
#endif

        protected override void OnGUI()
        {

            Event ev = currentEvent;
            
            
            EventType eventType = GraphUtility.ReserveEvent(toolbarRect, bottomToolbarRect,
#if SM_RPG
                skillToolbarRect, 
#endif
                windowBlackboard?.windowRect ?? Rect.zero);

            // EditorGUI.BeginChangeCheck();
            ZoomableArea.Begin(new Rect(0f, 0f, ScaledCanvasSize.width + 2, ScaledCanvasSize.height + 21), scale, IsDocked);
            Begin();
            
            if (Active) DrawNodes();
            else ZoomableArea.End();        

            AcceptDragAndDrop();
            End();            

            GraphUtility.ReleaseEvent(eventType);  
            
            if(eventType == EventType.ContextClick) CanvasContextMenu();         

            DrawToolbar();
            DrawBottomToolbar();
            
            DrawDescriptionAndTitle();
            
            if (centerView)
            {
                CenterView();
                centerView = false;
            }

#if SM_RPG
            if (Active != null && Active is SkillStateMachine)
            {
                var sk = Active as SkillStateMachine;
                SkillAsset asst = sk.skillAsset;
                if (asst == null) asst = (sk.parent as SkillStateMachine).skillAsset;
                SkillStateMachine prnt = sk.skillAsset ? sk : (sk.parent as SkillStateMachine);
                //string description = sk.skillAsset.definition.description.content;
                //if (string.IsNullOrEmpty(description)) description = string.Format("{0}'s state machine.", sk.skillAsset.definition.shortName);
                if (asst)
                {
                    GUI.backgroundColor = Color.white;
                    GUILayout.BeginArea(skillToolbarRect);
                    EditorGUILayout.BeginVertical();
                    int padding = 5;

                    string skName = asst.definition.shortName;
                    if (string.IsNullOrEmpty(skName)) skName = "Skill";

                    EditorGUILayout.Space();

                    if (GUILayout.Button("On Skill Activate", GraphStyles.GetNodeStyle(prnt, sk == prnt.onActivateNode, 0), GUILayout.ExpandWidth(true), GUILayout.Height(35f)))
                    {
                        SelectStateMachine(prnt.onActivateNode);
                    }
                    GUILayout.Space(padding);
                    if (GUILayout.Button("On Skill Effect", GraphStyles.GetNodeStyle(prnt, sk == prnt.onCastNode, 0), GUILayout.ExpandWidth(true), GUILayout.Height(35f)))
                    {
                        SelectStateMachine(prnt.onCastNode);
                    }
                    GUILayout.Space(padding);
                    if (GUILayout.Button("On Skill Hit", GraphStyles.GetNodeStyle(prnt, sk == prnt.onHitNode, 0), GUILayout.ExpandWidth(true), GUILayout.Height(35f)))
                    {
                        SelectStateMachine(prnt.onHitNode);
                    }
                    GUILayout.Space(padding);
                    if (GUILayout.Button("On Skill Ends", GraphStyles.GetNodeStyle(prnt, sk == prnt.onEndNode, 0), GUILayout.ExpandWidth(true), GUILayout.Height(35f)))
                    {
                        SelectStateMachine(prnt.onEndNode);
                    }

                    EditorGUILayout.EndVertical();
                    GUILayout.EndArea();
                    GUI.backgroundColor = bg;
                }
            }
#endif
           if(windowBlackboard == null || (!windowBlackboard.IsOpen())) shortcutEditor.HandleKeyEvents();

            if(ev != null && ev.rawType != EventType.Repaint)
            {
                RepaintAll();
            }
            /*if (ev != null)
            {
                Debug.LogWarningFormat("ev.commandName {0} type: {1} isMouse: {2}", ev.commandName, ev.rawType, ev.isMouse);
            }*/
            
            if (ev != null && ev.rawType == EventType.ValidateCommand)
            {
                bool isDeleteCommand = (ev.commandName == "SoftDelete");
                isDeleteCommand |= (
                    SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX &&
                    ev.commandName == "Delete"
                ); 
                
                if (SelectedNodes.Count == 1 && isDeleteCommand && SelectedTransition) //ev.keyCode == KeyCode.Delete 
                {
                    Node node = SelectedTransition.fromNode;

                    var transitions = node.transitions;
                    ArrayUtility.Remove(ref transitions, SelectedTransition);
                    node.transitions = transitions;

                    foreach (var c in SelectedTransition.GetConditions(node.Root.sourceReactions).conditions)
                    {
                        if (c == null) continue;
                        c.hideFlags = 0;
                        DestroyImmediate(c, true);
                    }

                    if (Application.isPlaying)
                    {
                        if (SelectedTransition.GetConditions(Controller.reactions))
                        {
                            foreach (var c in SelectedTransition.GetConditions(Controller.reactions).conditions)
                            {
                                if (c == null) continue;
                                c.hideFlags = 0;
                                Destroy(c);
                            }
                        }
                    }

                    GraphUtility.DestroyImmediate(SelectedTransition);
                    if (GUI.changed) EditorUtility.SetDirty(node);

                    selection.Clear();
                    UpdateSelection();
                }
                else if (isDeleteCommand && selection.Count > 0) //ev.keyCode == KeyCode.Delete
                {
                    try
                    {
                        foreach (Node mNode in selection)
                        {
#if SM_RPG
                        if (!(mNode is UpState) && !(mNode is SkillStateMachine))
#else
                            if (!(mNode is UpState))
#endif
                            {
                                GraphUtility.DeleteNode(mNode, mNode.Root.sourceReactions);
                                if (Controller && Controller.reactions)
                                    GraphUtility.DeleteNode(mNode, Controller.reactions);
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    selection.Clear();
                    UpdateSelection();
                    Repaint();
                    if (GUI.changed) EditorUtility.SetDirty(Active);
                }
            }

            windowBlackboard?.Update(this);
        }

        private void DrawDescriptionAndTitle()
        {
            GUI.contentColor = Color.white;
            Color bg = Color.black;
            bg.a = 0.25f;
            GUI.backgroundColor = bg;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(-2);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(24);
            // EditorGUILayout.Space();
            // EditorGUILayout.Space();
            // EditorGUILayout.Space();
            // EditorGUILayout.Space();
            // EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(GraphStyles.DescriptionBox, GUILayout.MinWidth(300), GUILayout.ExpandWidth(true));

            if (Active)
            {
                if (!Active.isSelected) Active.isSelected = true;

#if SM_RPG
                if (Active is SkillStateMachine)
                {
                    var sk = Active as SkillStateMachine;
                    SkillStateMachine prnt = sk.skillAsset ? sk : (sk.parent as SkillStateMachine);
                    SkillAsset asst = prnt.skillAsset;
                    string description = string.Empty;

                    if (sk == prnt.onActivateNode) description = SkillReactionsEditor.MSG_ONACT;
                    else if (sk == prnt.onCastNode) description = SkillReactionsEditor.MSG_ONEFFECT;
                    else if (sk == prnt.onHitNode) description = SkillReactionsEditor.MSG_ONHIT;
                    else if (sk == prnt.onEndNode) description = SkillReactionsEditor.MSG_ONEND;
                     
                    //if (string.IsNullOrEmpty(description)) description = string.Format("{0}'s state machine.", sk.skillAsset.definition.shortName);
                    if (asst)
                    {
                        string skName = asst.definition.shortName;
                        if (string.IsNullOrEmpty(skName)) skName = "Skill"; 

                        EditorGUILayout.LabelField(new GUIContent(string.Format("{0}'s state machine.", skName)), GraphStyles.instructionLabel);
                    }
                    var content = new GUIContent(description);
                    var style = GraphStyles.DescriptionLabel;
                    Vector2 size = style.CalcSize(content);
                    //EditorGUILayout.LabelField(new GUIContent(sk.skillAsset.definition.shortName), GraphStyles.instructionLabel); //+" Comments"
                    if (!string.IsNullOrEmpty(description))
                    {
                        //EditorGUILayout.LabelField(new GUIContent(description), GraphStyles.DescriptionLabel, GUILayout.Width(size.x));
                    }
                    SkillReactionsEditor.PaintDocumentation(SkillReactionsEditor.VARIABLES_SHARED, description);
                    
                    if (sk == prnt.onCastNode) 
                        SkillReactionsEditor.PaintDocumentation(SkillReactionsEditor.VARIABLES_ONEFFECT, string.Empty, false);
                    else if (sk == prnt.onHitNode) 
                        SkillReactionsEditor.PaintDocumentation(SkillReactionsEditor.VARIABLES_ONHIT, string.Empty, false);
                    else if (sk == prnt.onEndNode) 
                        SkillReactionsEditor.PaintDocumentation(SkillReactionsEditor.VARIABLES_ONHIT, string.Empty, false);

                    EditorGUILayout.Space();
                }
                else
#endif
                {
                    // EditorGUILayout.BeginVertical(GraphStyles.DescriptionBox, GUILayout.Width(node.position.width));
                    // GUI.backgroundColor = Color.clear;
                    // GUILayout.BeginArea(new Rect(node.position.x, node.position.y + node.position.height + 4, node.position.width, 22f), GraphStyles.DescriptionBox);
                    
                    // EditorGUILayout.EndVertical();
                    
                    /*GUIContent title = new GUIContent(Active.name);
                    GUIContent description = new GUIContent(Active.description);
                    Rect r = GUILayoutUtility.GetRect(title, GraphStyles.instructionLabel);
                    Rect r2 = string.IsNullOrEmpty(Active.description) ? Rect.zero : GUILayoutUtility.GetRect(description, GraphStyles.instructionLabel);

                    Rect rect = new Rect(20, 10, Mathf.Max(300, r.width), Mathf.Max(22f, r.height));
                    Rect rect2 = new Rect(rect.x, rect.y + rect.height, Mathf.Max(300, r2.width), Mathf.Max(22f, r2.height));
                    Rect rect3 = new Rect(-10, 40, rect2.width, rect.height + rect2.height + 50);
                    
                    Debug.LogWarningFormat("rect {0} r2: {1} r3: {2} type: {3}", rect, rect2, rect3, Event.current.type);

                    GUILayout.BeginArea(rect3, GraphStyles.DescriptionBox);
                    GUI.color = Color.white;
                    EditorGUI.LabelField(rect, title, GraphStyles.instructionLabel);
                    EditorGUI.LabelField(rect2, description, GraphStyles.DescriptionLabel);
                    GUILayout.EndArea();*/
                    
                    EditorGUILayout.LabelField(new GUIContent(Active.name), GraphStyles.instructionLabel); //+" Comments"
                    if (!string.IsNullOrEmpty(Active.description))
                    {
                        EditorGUILayout.LabelField(new GUIContent(Active.description), GraphStyles.DescriptionLabel);
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField(new GUIContent("Select a State Machine."), GraphStyles.instructionLabel);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        private void OnAddedAsTab()
        {
            CenterView();
        }

        private void OnTabDetached()
        {
            CenterView();
        }

        private void DrawBottomToolbar()
        {
            GUILayout.BeginArea(bottomToolbarRect, GraphStyles.bottomToolbar);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(Root == null ? GUIContent.none : new GUIContent(AssetDatabase.GetAssetPath(Root)), GraphStyles.miniLabelRight);

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawToolbar()
        {
            GUILayout.BeginArea(toolbarRect, EditorStyles.toolbar);
            EditorGUILayout.BeginHorizontal();

            StateMachine parent = Active;
            breadcrumbs.Clear();

            while (parent)
            {
                breadcrumbs.Add(parent);
                parent = parent.parent;
            }
            breadcrumbs.Reverse();
            //GUI.depth = -1000; 
            for (int i = 0; i < breadcrumbs.Count; i++)
            {
                // GUIStyle style = i == 0 ? GraphStyles.breadcrumbLeft : GraphStyles.breadcrumbMiddle;
                // GUIStyle styleFocused = i == 0 ? GraphStyles.breadcrumbLeftFocused : GraphStyles.breadcrumbMiddleFocused;
                GUIStyle style = GraphStyles.breadcrumbLeft;
                //GUIStyle styleFocused = GraphStyle//s.breadcrumbLeftFocused;
                StateMachine sm = breadcrumbs[i];
                string nName = sm.name;

                GUI.depth = breadcrumbs.Count - i;

                GUIContent content = new GUIContent(string.IsNullOrEmpty(nName) || sm == Root ? "Base" : nName);
                float width = style.CalcSize(content).x;
                width = Mathf.Clamp(width, 20f, width) + 10;
                
                //Debug.LogWarningFormat("Breadcrumb name: {0} depth: {1}", nName, GUI.depth);

#if SM_RPG
                if (sm != null && sm == Root && sm is SkillStateMachine)
                {
                    string skName = (sm == Root && sm is SkillStateMachine) ? (sm as SkillStateMachine).skillAsset.definition.shortName : string.Empty;
                    if (string.IsNullOrEmpty(skName)) skName = "Skill";
                    content.text = skName;
                    GUILayout.Label(content, sm.Equals(Active) ? styleFocused : style, GUILayout.Width(width));
                    continue;
                }
#endif
                Rect rect = GetBreadcrumbLayoutRect(content, style);//GUILayoutUtility.GetRect(content, style);
                // rect.width = width;
                // rect.height = 20;
                
                if (Event.current.type == EventType.Repaint)
                {
                    GraphStyles.breadcrumbLeftBg.Draw(rect, GUIContent.none, 0);
                }

                //rect.x += 5;

                //sm.Equals(Active) ? styleFocused : style
                if (GUI.Button(rect, content, style))//, GUILayout.Width(width), GUILayout.Height(20))
                //if (GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(width)))
                {
                    
                    SelectStateMachine(breadcrumbs[i]);
                }

            }
            GUILayout.FlexibleSpace();

            if (windowBlackboard != null)
            {
                bool show = GUILayout.Toggle(
                    windowBlackboard.IsOpen(),
                    "Blackboard",
                    EditorStyles.toolbarButton
                );

                windowBlackboard.Show(show);

                //window.Repaint();
            }

            if (GUILayout.Button("Center", EditorStyles.toolbarButton))
            {
                CenterView();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        
        static Rect GetBreadcrumbLayoutRect(GUIContent content, GUIStyle style)
        {
            // the image makes the button far too big compared to non-image versions
            var image = content.image;
            content.image = null;
            var size = style.CalcSize(content);
            //var size = style.CalcSizeWithConstraints(content, Vector2.zero);
            content.image = image;
            if (image != null)
                size.x += size.y; // assumes square image, constrained by height

            return GUILayoutUtility.GetRect(content, style, GUILayout.MaxWidth(size.x));
        }


        protected override Rect GetCanvasSize()
        {
            toolbarRect = new Rect(0, 0, canvasSize.width, 22f);
            bottomToolbarRect = new Rect(0, canvasSize.height - 41f, canvasSize.width, 22f);
#if SM_RPG
            if (Active != null && Active is SkillStateMachine) skillToolbarRect = new Rect(-20, (canvasSize.height - 200f), 200f, 200f);
#endif
            return new Rect(0, 17f, position.width + 2f, position.height + 22f);
        }

        private void AcceptDragAndDrop()
        {
            EventType eventType = Event.current.type;
            bool isAccepted = false;
            if ((eventType == EventType.DragUpdated || eventType == EventType.DragPerform))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;

                if (eventType == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    isAccepted = true;
                }
                Event.current.Use();
            }
            if (isAccepted && DragAndDrop.objectReferences[0] is StateMachine)
            {
                StateMachine source = DragAndDrop.objectReferences[0] as StateMachine;
                StateMachine sm = GraphUtility.AddNode<StateMachine>(Event.current.mousePosition + scrollPosition, Active, false);
                if (source != null)
                {
                    sm.name = GraphUtility.GenerateUniqueNodeName(source.name);
                    sm.nodes = GraphUtility.CopyNodes(source.nodes, sm, source.sourceReactions, Reactions);
                }
                GraphUtility.AddNode<UpState>(GraphEditor.Center + new Vector2(0, -(GraphStyles.StateMachineHeight + 5)), sm);
                GraphUtility.ParentChilds(Active);
                shouldUpdate = true; 
            }
            else if (isAccepted)
            {
                float padding = 10;
                var pos = mousePosition;
                List<MonoBehaviour> ignoreActions = new List<MonoBehaviour>();
                List<Node> newNodes = new List<Node>();

                foreach (Object reference in DragAndDrop.objectReferences)
                {
                    if (reference is GameObject)
                    {
                        var go = reference as GameObject;
                        var actions = go.GetComponents<Actions>();
                        var triggers = go.GetComponents<Trigger>();
                        var conditions = go.GetComponents<Conditions>();
                        
                        Debug.LogWarningFormat("Dropped actions: {0} triggers: {1} conditions: {2}", actions.Length, triggers.Length, conditions.Length);
                        
                        foreach (Trigger trigger in triggers)
                        {
                            TriggerState state = GraphUtility.AddNode<TriggerState>(pos, Active);
                            newNodes.Add(state);
                            GraphUtility.UpdateNodeColor(state);
                            var list = GraphUtility.CopyTrigger(trigger, state.GetTrigger(Reactions), pos, state, newNodes);
                            ignoreActions.AddRange(list);
                            state.hasCustomName = true;
                            state.name = GraphUtility.GenerateUniqueNodeName(trigger.gameObject.name);
                            pos.y += GraphStyles.StateMachineHeight + padding;

                            bool conditionCheck = false;
                            bool actionCheck = false;
                            foreach (var item in trigger.items)
                            {
                                if (actionCheck && conditionCheck) break;
                                
                                if (item.conditions && !conditionCheck)
                                {
                                    pos.y += GraphStyles.StateHeight + padding;
                                    conditionCheck = true;
                                }
                                if(item.conditions && !actionCheck)
                                {
                                    pos.y += GraphStyles.StateHeight + padding;
                                    actionCheck = true;
                                }
                            }
                            if(list.Count > 0) pos.y += GraphStyles.StateHeight + padding;
                        }
                        
                        foreach (var condition in conditions)
                        {
                            if(ignoreActions.Contains(condition)) continue;

                            ActionsState state = GraphUtility.AddNode<ActionsState>(pos, Active);
                            newNodes.Add(state);
                            state.hasCustomName = true;
                            
                            // state.name = $"Conditions {conditionIndex}";
                            GraphUtility.UpdateNodeColor(state);
                            state.name = GraphUtility.GenerateUniqueNodeName(condition.gameObject.name);
                            state.transitionMode = Node.TransitionMode.Selective;
                            // GraphUtility.AddTransition(triggerState, state, false);
                            pos.y += GraphStyles.StateHeight + padding;

                            float totalWidth2 = 0;
                            int count2 = 0;
                            var pos2 = pos;
                            pos2.y += GraphStyles.StateHeight;
                            
                            for (int j = 0; j < condition.clauses.Length; j++)
                            {
                                var clause = condition.clauses[j];
                                if (clause.actions)
                                {
                                    totalWidth2 += GraphStyles.StateWidth + padding;
                                    count2++;
                                }
                            }

                            if (condition.defaultActions)
                            {
                                totalWidth2 += GraphStyles.StateWidth + padding;
                                count2++;
                            }
                            
                            if(count2 > 1) pos2.x -= (totalWidth2 / 2) - (GraphStyles.StateWidth / 2) - 5;
                            
                            foreach (Clause clause in condition.clauses)
                            {
                                if(!clause.actions) continue;
                                
                                ActionsState subState = GraphUtility.AddNode<ActionsState>(pos2, GraphEditor.Active);
                                
                                newNodes.Add(subState);
                                GraphUtility.UpdateNodeColor(subState);
                                ignoreActions.Add(clause.actions);
                                GraphUtility.CopyIActionList(clause.actions.actionsList, subState.GetActions(GraphEditor.Reactions));
                                subState.hasCustomName = true;
                                subState.name = GraphUtility.GenerateUniqueNodeName(clause.actions.gameObject.name);
                                Transition t = GraphUtility.AddTransition(state, subState, false);
                                t.useConditions = true;
                                Editor.CreateEditor(GraphEditor.Reactions.GetCondition<IConditionsList>(t));
                                GraphUtility.CopyIConditionList(clause.conditionsList, t.GetConditions(GraphEditor.Reactions));
                                pos2.x += GraphStyles.StateWidth + padding;
                            }

                            if (condition.defaultActions)
                            {
                                ActionsState subState = GraphUtility.AddNode<ActionsState>(pos2, GraphEditor.Active);
                                
                                newNodes.Add(subState);
                                ignoreActions.Add(condition.defaultActions);
                                GraphUtility.UpdateNodeColor(subState);
                                GraphUtility.CopyIActionList(condition.defaultActions.actionsList, subState.GetActions(GraphEditor.Reactions));
                                subState.hasCustomName = true;
                                subState.name = GraphUtility.GenerateUniqueNodeName(condition.defaultActions.gameObject.name);
                                GraphUtility.AddTransition(state, subState, false);
                                pos2.x += GraphStyles.StateWidth + padding;
                            }
                            
                            pos.x += GraphStyles.StateWidth + padding;
                        }
                        
                        foreach (var action in actions)
                        {
                            if(ignoreActions.Contains(action)) continue;
                            var t = action.GetComponentInParent<Trigger>();
                            if(t && triggers.Contains(t)) continue;
                            
                            ActionsState state = GraphUtility.AddNode<ActionsState>(pos, Active);
                            newNodes.Add(state);
                            GraphUtility.UpdateNodeColor(state);
                            GraphUtility.CopyIActionList(action.actionsList, state.GetActions(Reactions));
                            state.hasCustomName = true;
                            state.name = GraphUtility.GenerateUniqueNodeName(action.gameObject.name);
                            pos.y += GraphStyles.StateHeight + padding;
                        }
                    }
                    // Debug.LogWarningFormat("Dropped: {0}", DragAndDrop.objectReferences[0]);
                }

                selection.Clear();
                selection.AddRange(newNodes);
                newNodes.Clear();
            }
        }

        private GUIContent GetIconGUI(Node node)
        {
            GUIContent result = GUIContent.none;

            if (node == null) return result;

            if (!IconGUI.TryGetValue(node.GetType(), out result))
            {
                IconGUI.Add(node.GetType(), new GUIContent(AssetPreview.GetMiniThumbnail(node)));
            }

            if (Reactions != null)
            {

                if (node is TriggerState)
                {
                    TriggerState trigger = node as TriggerState;

                    var trig = trigger.GetTrigger(Reactions);

                    if (trig != null && trig.igniters.Count > 0 && trig.igniters.ContainsKey(-1) && trig.igniters[-1] != null)
                    {
                        bool hasIcon = IconGUI.TryGetValue(trig.igniters[-1].GetType(), out result);

                        if (!hasIcon)
                        {
                            Texture2D igniterIcon = null;
                            UnityEngine.Object reference = trig.igniters[-1];

                            string igniterName = (string)reference.GetType().GetField("NAME", BINDING_FLAGS).GetValue(null);
                            string iconPath = (string)reference.GetType().GetField("ICON_PATH", BINDING_FLAGS).GetValue(null);

                            if (!string.IsNullOrEmpty(igniterName))
                            {
                                string[] igniterNameSplit = igniterName.Split(new char[] { '/' });
                                igniterName = igniterNameSplit[igniterNameSplit.Length - 1];
                            }

                            igniterIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(iconPath, igniterName + ".png"));
                            if (igniterIcon == null) igniterIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath + "Default.png");
                            if (igniterIcon != null)
                            {
                                IconGUI.Add(trig.igniters[-1].GetType(), new GUIContent(igniterIcon));
                                NodeNames.Add(trig.igniters[-1].GetType(), igniterName);
                            }
                        }
                    }
                }
                else if (node is ActionsState)
                {
                    ActionsState actions = node as ActionsState;

                    var act = actions.GetActions(Reactions);
                    

                    if (act != null && act.actions != null 
                        && act.actions.Length > 0 && act.actions[0] != null && 
                        !IconGUI.TryGetValue(act.actions[0].GetType(), out result))
                    {
                        System.Type actionType = act.actions[0].GetType();
                        string actionName = (string)actionType.GetField("NAME", BINDING_FLAGS).GetValue(null);
                        actionName = actionName.Substring(actionName.LastIndexOf("/") + 1);

                        IActionsListEditor ed = (IActionsListEditor)Editor.CreateEditor(act);                        

                        IconGUI.Add(act.actions[0].GetType(), new GUIContent(ed.subEditors[0].GetIcon()));
                        NodeNames.Add(act.actions[0].GetType(), actionName);
                    }
                }
            }

            return result == null ? GUIContent.none : result;
        }

        private void HandleContextSelect()
        {
            if (currentEvent.button == 1)
            {
                int controlId = GUIUtility.GetControlID(FocusType.Passive);
                switch (currentEvent.type)
                {
                    case EventType.MouseDown:
                        GUIUtility.hotControl = controlId;
                        selectionStartPosition = mousePosition;
                        Node node = MouseOverNode();

                        bool withinArea = (lastClickPos - currentEvent.mousePosition).sqrMagnitude <= 5 * 5;

                        if (node)
                        {
                            GUIUtility.hotControl = controlId;
                            SelectTransition(null);
                            if (node is UpState && currentEvent.clickCount == 2 && withinArea)
                            {
                                SelectStateMachine((node as UpState).parent.parent);
                            }
                            else if (node is StateMachine && currentEvent.clickCount == 2 && withinArea)
                            {
                                SelectStateMachine((StateMachine)node);
                            }
                            else
                            {
                                if (EditorGUI.actionKey || currentEvent.shift)
                                {
                                    if (!selection.Contains(node)) selection.Add(node);
                                    else selection.Remove(node);
                                }
                                else if (!selection.Contains(node))
                                {
                                    selection.Clear();
                                    selection.Add(node);
                                }
                            }
                            lastClickPos = currentEvent.mousePosition;

                            GUIUtility.hotControl = 0;
                            GUIUtility.keyboardControl = 0;
                            UpdateSelection();
                        }
                        break;
                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == controlId)
                        {
                            GUIUtility.hotControl = 0;
                            //currentEvent.Use();
                        }
                        //currentEvent.Use();
                        break;
                    case EventType.ContextClick:
                        NodeContextMenu();
                        break;
                }
            }
        }

        private void DoNodeEvents()
        {
            if (currentEvent.button != 0) return; 

            SelectNodes();
            DragNodes();
        }

        private void SelectNodes()
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            switch (currentEvent.rawType)
            {
                case EventType.MouseDown:
                    GUIUtility.hotControl = controlId;
                    selectionStartPosition = mousePosition;
                    Node node = MouseOverNode();
                    Transition transition = MouseOverTransition();

                    if (transition && !node)
                    {
                        selection.Clear();
                        Selection.activeObject = null;
                        selection.Add(transition.fromNode);

                        SelectTransition(transition);

                        GUIUtility.hotControl = 0;
                        GUIUtility.keyboardControl = 0;
                        UpdateSelection();
                        Event.current.Use();
                        Repaint();
                        shouldUpdate = true;
                        return;
                    }
                    
                    if (fromNode && !node && scrollView.Contains(mousePosition))
                    {
                        // Debug.LogWarningFormat("Add action node mouse Pos valid: {0} test2: {1}", scrollView.Contains(mousePosition), ScaledCanvasSize.Contains((mousePosition)));
                        node = GraphUtility.AddNode<ActionsState>(mousePosition, Active);
                        GraphUtility.UpdateNodeColor(node);
                        shouldUpdate = true;
                    }

                    if (node)
                    {
                        SelectTransition(null);

                        if (fromNode)
                        {
                            if (node is StateMachine || node is UpState)
                            {
                                GenericMenu transitionMenu = new GenericMenu();

                                StateMachine sm = node is UpState ? node.parent.parent : node as StateMachine;
                                /*if (node is UpState)
                                {
                                    Debug.LogWarningFormat("Up Node context parent: {0}", node.parent);
                                }*/
                                foreach(var n in sm.nodes)
                                {
                                    Node subnode = n;
                                    if (n is StateMachine)
                                    {
                                        if(n == Active) continue;
                                        transitionMenu.AddItem(new GUIContent("StateMachine/" + n.name), false, () =>
                                        {
                                            Transition tr = GraphUtility.AddTransition(fromNode, subnode);
                                            if (tr)
                                            {
                                                fromNode = null;
                                                GUIUtility.hotControl = 0;
                                                GUIUtility.keyboardControl = 0;
                                                shouldUpdate = true;
                                            }
                                        });
                                    }
                                    else if (!(n is TriggerState) && !(n is EntryState) && !(n is ExitState) && !(n is UpState))
                                    {
                                        transitionMenu.AddItem(new GUIContent("States/" + n.name), false, () =>
                                        {
                                            Transition tr = GraphUtility.AddTransition(fromNode, subnode);
                                            if (tr)
                                            {
                                                fromNode = null;
                                                GUIUtility.hotControl = 0;
                                                GUIUtility.keyboardControl = 0;
                                                shouldUpdate = true;
                                            }
                                        });
                                    }
                                }

                                if(sm != Active.Root)
                                {
                                    transitionMenu.AddItem(new GUIContent("StateMachine/" + sm.name), false, () =>
                                    {
                                        Transition tr = GraphUtility.AddTransition(fromNode, sm);
                                        if (tr)
                                        {
                                            fromNode = null;
                                            GUIUtility.hotControl = 0;
                                            GUIUtility.keyboardControl = 0;
                                            shouldUpdate = true;
                                        }
                                    });
                                }

                                // var pos = new Vector2(mousePosition.x - GraphStyles.StateWidth / 2, mousePosition.y - GraphStyles.StateHeight / 2);
                                // transitionMenu.DropDown(new Rect(scrollPosition, Vector2.zero));
                                // cache the original matrix(we assume this is scaled)
                                Matrix4x4 m4 = GUI.matrix;
                                //reset to non-scaled
                                GUI.matrix = Matrix4x4.identity;
                                transitionMenu.ShowAsContext();
                                GUI.matrix = m4;

                                currentEvent.Use();
                            }
                            else if(!fromNode.Equals(node))
                            {
                                
                                Transition tr = GraphUtility.AddTransition(fromNode, node);
                                if (tr)
                                {
                                    fromNode = null;
                                    GUIUtility.hotControl = 0;
                                    GUIUtility.keyboardControl = 0;
                                    shouldUpdate = true;
                                }
                            }
                            return;
                        }
                        if (node is UpState && currentEvent.clickCount == 2)
                        {
                            SelectStateMachine((node as UpState).parent.parent, false);
                        }
                        else if (node is StateMachine && currentEvent.clickCount == 2)
                        {
                            SelectStateMachine((StateMachine)node);
                        }
                        else
                        {
                            if (EditorGUI.actionKey || currentEvent.shift)
                            {
                                if (!selection.Contains(node)) selection.Add(node);
                                else selection.Remove(node);
                            }
                            else if (!selection.Contains(node))
                            {
                                selection.Clear();
                                selection.Add(node);
                            }
                        }

                        GUIUtility.hotControl = 0;
                        GUIUtility.keyboardControl = 0;
                        UpdateSelection();
                        return;
                    }
                    fromNode = null;
                    selectionMode = SelectionMode.Pick;
                    if (!EditorGUI.actionKey && !currentEvent.shift)
                    {
                        selection.Clear();
                        SelectTransition(null);
                        UpdateSelection();
                    }
                    currentEvent.Use();
                    break;
                case EventType.MouseUp:
                    if (fromNode)
                    {
                        fromNode = null;
                        selection.Clear();
                        SelectTransition(null);
                        UpdateSelection();
                    }
                    if (GUIUtility.hotControl == controlId)
                    {
                        selectionMode = SelectionMode.None;
                        GUIUtility.hotControl = 0;
                        currentEvent.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId && !EditorGUI.actionKey && !currentEvent.shift && !currentEvent.alt && (selectionMode == SelectionMode.Pick || selectionMode == SelectionMode.Rect))
                    {
                        selectionMode = SelectionMode.Rect;
                        SelectNodesInRect(FromToRect(selectionStartPosition, mousePosition));
                        currentEvent.Use();
                    }
                    break;
                case EventType.Repaint:
                    if (GUIUtility.hotControl == controlId && selectionMode == SelectionMode.Rect && !currentEvent.alt)
                    {
                        GraphStyles.selectionRect.Draw(FromToRect(selectionStartPosition, mousePosition), false, false, false, false);
                        //UnityEditor.Graphs.Styles.selectionRect.Draw(FromToRect(selectionStartPosition, mousePosition), false, false, false, false);
                    }
                    break;
            }
        }

        private void DragNodes()
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            
            switch (currentEvent.rawType)
            {
                case EventType.MouseDown:
                    GUIUtility.hotControl = controlID;
                    currentEvent.Use();
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        if (overStateMachine)
                        {
                            Pasteboard.Copy(selection, Reactions);
                            Pasteboard.Paste(mousePosition, overStateMachine, Reactions, true);
                            foreach (Node mNode in selection)
                            {
#if SM_RPG
                                if (!(mNode is UpState) && !(mNode is SkillStateMachine))
#else
                                if (!(mNode is UpState))
#endif
                                {
                                    GraphUtility.DeleteNode(mNode, mNode.Root.sourceReactions);
                                    if(Controller) GraphUtility.DeleteNode(mNode, Controller.reactions);
                                }
                            }
                            selection.Clear();
                            UpdateSelection();
                            overStateMachine = null;
                            if (GUI.changed) EditorUtility.SetDirty(Active);                            
                        }
                        GUIUtility.hotControl = 0;
                        currentEvent.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        //Debug.Log("Drag");
                        for (int i = 0; i < selection.Count; i++)
                        {
                            Node node = selection[i];
#if SM_RPG
                            if (node is SkillStateMachine) continue;
#endif
                            //node.position.position = WindowToGridPosition(currentEvent.mousePosition);
                            node.position.position += currentEvent.delta;

                            var pos = node.position.position;
                            pos.x = Mathf.Round(node.position.position.x);
                            pos.y = Mathf.Round(node.position.position.y);
                            node.position.position = pos;
                            // node.position.position = GetSharedSnapPosition(node.position.position + currentEvent.delta, 2f);
                            //node.position.position = SnapToGrid(node.position.position);
                        }

                        Node overNode = MouseOverNode(true);

                        if(overNode is UpState)
                        {
                            overStateMachine = (overNode as UpState).parent.parent;
                        }
                        else if(overNode is StateMachine)
                        {
                            overStateMachine = overNode as StateMachine;                            
                        }
                        else
                        {
                            overStateMachine = null;
                        }

                        currentEvent.Use();
                    }
                    break;
                case EventType.Repaint:
                    if (GUIUtility.hotControl == controlID)
                    {                        
                        AutoPanNodes(1.5f);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Accepts a position, and sets each axis-value of the position to be snapped according to the value of snap
        /// </summary>
        public static Vector3 GetSharedSnapPosition(Vector3 originalPosition, float snap = 0.01f)
        {
            return new Vector3(GetSnapValue(originalPosition.x, snap), GetSnapValue(originalPosition.y, snap), GetSnapValue(originalPosition.z, snap));
        }
 
        /// <summary>
        /// Accepts a value, and snaps it according to the value of snap
        /// </summary>
        public static float GetSnapValue(float value, float snap = 0.01f)
        {
            return (!Mathf.Approximately(snap, 0f)) ? Mathf.RoundToInt(value / snap) * snap : value;
        }

        private void AutoPanNodes(float speed)
        {
            Vector2 delta = Vector2.zero;
            if (mousePosition.x > ScaledCanvasSize.width + scrollPosition.x - 50f)
            {
                delta.x += speed;
            }

            if ((mousePosition.x < scrollPosition.x + 50f) && scrollPosition.x > 0f)
            {
                delta.x -= speed;
            }

            if (mousePosition.y > ScaledCanvasSize.height + scrollPosition.y - 50f)
            {
                delta.y += speed;
            }

            if ((mousePosition.y < scrollPosition.y + 50f) && scrollPosition.y > 0f)
            {
                delta.y -= speed;
            }

            if (delta != Vector2.zero)
            {
                for (int i = 0; i < selection.Count; i++)
                {
                    Node node = selection[i];
#if SM_RPG
                    if (node is SkillStateMachine) continue;
#endif
                    node.position.position += delta;
                }
                UpdateScrollPosition(scrollPosition + delta);
                Repaint();
            }
        }

        private void DrawNodes()
        {
            if (fromNode)
            {
                Node overNode = MouseOverNode();
                Vector2 position = mousePosition;

                if (overNode && !(overNode is TriggerState)) position = overNode.position.center;

                DrawConnection(fromNode.position.center, position, Color.white, 1, false, null);
                Repaint();
            }

            for (int i = 0; i < Nodes.Length; i++)
            {
                Node node = Nodes[i];
                if (!node)
                {
                    var nodes = Nodes;
                    ArrayUtility.RemoveAt(ref nodes, i);
                    Active.nodes = nodes;
                    continue;
                }
#if SM_RPG
                if (node is SkillStateMachine) continue;
#endif
                if (node is StateMachine)
                {
                    var subNodes = (node as StateMachine).nodes;
                    for (int j = 0; j < subNodes.Length; j++)
                    {
                        var sn = subNodes[j];
                        if (sn.transitions.Any(n => n.toNode.parent == Active && n.fromNode.parent != Active))
                        {
                            DrawTransitions(sn);
                        }
                    }
                }
                DrawTransitions(node);
            }

            if(Active.parent)
            {
                for (int i = 0; i < Active.parent.nodes.Length; i++)
                {
                    Node node = Active.parent.nodes[i];
                    if (!node)
                    {
                        Debug.LogWarningFormat("[Error] Invalid node: {0}", node);
                        continue;
                    }
#if SM_RPG
                    if (node is SkillStateMachine) continue;
#endif
                    DrawTransitions(node, Active.parent);
                }
            }

            for (int i = 0; i < Nodes.Length; i++)
            {
                Node node = Nodes[i];
                if (!node)
                {
                    Debug.LogWarningFormat("[Error] Invalid node: {0}", node);
                    continue;
                }
#if SM_RPG
                if (node is SkillStateMachine) continue;
#endif
                
                if (!string.IsNullOrEmpty(node.description) && !node.Root.disableComments)
                {
                    Color c2 = Color.white;
                    c2.a = 0.4f;
                
                    GUI.color = c2;
                    // GUI.contentColor = c2;
                    // EditorGUILayout.BeginVertical(GraphStyles.DescriptionBox, GUILayout.Width(node.position.width));
                    Color c = Color.gray;// new Color(38f / 255f, 38f / 255f, 38f / 255f, 255f);
                    c.a = 1.7f;
                    GUI.backgroundColor = c;
                    
                    GUIContent content = new GUIContent(node.description); 
                    // Rect r2 = GUILayoutUtility.GetRect(content, GraphStyles.DescriptionLabel);
                    var height = GraphStyles.DescriptionLabel.CalcHeight(content, GraphStyles.StateWidth);
                    var x = node.position.x;
                    var y = node.position.y + node.position.height;
                    if (node is StateMachine || node is TriggerState)
                    {
                        x = node.position.x + (GraphStyles.StateMachineWidth - GraphStyles.StateWidth) / 2;
                        // y = node.position.y + node.position.height;
                    }
                    // GUILayout.BeginArea(new Rect(x, y, GraphStyles.StateWidth, height + 4), GraphStyles.DescriptionBox);
                    GUI.Label(new Rect(x, y, GraphStyles.StateWidth, height + 4), node.description, GraphStyles.CommentBox);//GraphStyles.DescriptionLabel
                    // GUILayout.EndArea();
                    
                    // EditorGUILayout.EndVertical();
                    GUI.backgroundColor = Color.white;
                    GUI.color = Color.white;
                    GUI.contentColor = Color.white;
                }

                if (!selection.Contains(node))
                {
                    DoNode(node);
                }
            }

            for (int i = 0; i < selection.Count; i++)
            {
                Node node = selection[i];
                if (!node) continue;
#if SM_RPG
                if (node is SkillStateMachine) continue;
#endif
                DoNode(node);
            }

            DoNodeEvents();

            if (overStateMachine)
            {
                EditorGUIUtility.AddCursorRect(new Rect(mousePosition, new Vector2(32, 32)), MouseCursor.ArrowPlus);
            }

            ZoomableArea.End();
            
            HandleContextSelect();
        }

        private void CleanupNode(Node node)
        {
            var nodes = Active.nodes;
            ArrayUtility.Remove(ref nodes, node);
            Active.nodes = nodes;
            
            Debug.LogFormat("Node removed: {0}", node);

            DestroyImmediate(node, true);
        }

        private void DoNode(Node node)
        {
            if (!node) return;
            if (!node.parent)
            {
                CleanupNode(node);
                return;
            }
            if (!node.parent.Equals(Active)) return;

#if SM_RPG
            if (node is SkillStateMachine && node.parent.parent.Equals(Root)) return;
#endif

            if (EditorApplication.isPlaying)
            {

                if (node is TriggerState && node.internalExecuting)
                {
                    if (!nodesProgress.Contains(node))
                    {
                        node.progress = 0;
                        nodesProgress.Add(node);
                    }
                }
                else if (node is ActionsState && (node.isEntered || node.internalExecuting))
                {
                    if (!nodesProgress.Contains(node))
                    {
                        node.progress = 0;
                        nodesProgress.Add(node);
                    }
                }
            }
            
            float offset = 0;
            //if (node is StateMachine || node is TriggerState || node is UpState) offset = 5f;

            //node.position.position = Round(node.position.position);

            GUIStyle style = GraphStyles.GetNodeStyle(node, selection.Contains(node) && SelectedTransition == null, offset);

            string nodeName = node.name;
            if (node is UpState && node.parent != null && node.parent.parent != null)
            {
                nodeName = node.parent.parent == Root ? "(Up) Base" : "(Up) " + node.parent.parent.name;
            }
            else if (node is TriggerState)
            {
                if ((node.name == "Trigger" || string.IsNullOrEmpty(node.name)) 
                    || ((node.name == "Trigger" || !node.hasCustomName) || string.IsNullOrEmpty(node.name) && GraphTriggerInspector.HasChanged))
                {
                    var trigger = node as TriggerState;
                    var trig = trigger.GetTrigger(Reactions);
                    string resultName = node.name;
                    if (trig && trig.igniters != null && trig.igniters.ContainsKey(-1) && trig.igniters[-1])
                    {
                        NodeNames.TryGetValue(trig.igniters[-1].GetType(), out resultName);
                        node.name = resultName;
                    }
                }
                if (string.IsNullOrEmpty(node.name)) node.name = nodeName = "Trigger";
            }
            else if (node is ActionsState)
            {
                if (node.name == "Actions" || !node.hasCustomName || string.IsNullOrEmpty(node.name))
                {
                    var actions = node as ActionsState;
                    var trig = actions.GetActions(Reactions);
                    string resultName = node.name;
                    if (trig && trig.actions != null && trig.actions.Length > 0 && trig.actions[0])
                    {
                        NodeNames.TryGetValue(trig.actions[0].GetType(), out resultName);
                        node.name = resultName;
                    }
                }
                if (string.IsNullOrEmpty(node.name)) node.name = nodeName = "Actions";
            }
            bool isSpecialNode = node is StateMachine || node is TriggerState || node is UpState;
            var r = node.position;
            r.height = isSpecialNode ? GraphStyles.StateMachineHeight : GraphStyles.StateHeight;
            r.width = isSpecialNode ? GraphStyles.StateMachineWidth : GraphStyles.StateWidth;
            node.position = r;

            GUIContent icon = GetIconGUI(node);
            GUI.Box(node.position, isSpecialNode ? nodeName : nodeName.Truncate(17), style);

            if (isSpecialNode)
            {
                Rect rect = node.position;
                rect.width = 24;
                rect.height = 24;
                rect.x += 18;
                rect.y += 6;

                GUI.Label(rect, icon);
            }
            else if (node is ActionsState)
            {
                Rect rect = node.position;
                rect.width = 24;
                rect.height = 24;
                rect.x += 4;
                rect.y += 4;

                GUI.Label(rect, icon);
            }

            if (InProgress(node))
            {
                Rect rectBack = new Rect(node.position.x + (GraphStyles.StateWidth / 2 - 60), node.position.y + 23, 120, 5);
                if (node is TriggerState)
                {
                    rectBack = new Rect(node.position.x + (GraphStyles.StateMachineWidth / 2 - 70), node.position.y + 27, 140, 5);
                }
                    //Rect rectFront = new Rect(node.position.x + 4, node.position.y + 20, node.progress, 2);

                    /*if (node == Active.Owner.ActiveNode.Parent)
                    {
                        rectFront.y += 5;
                        rectFront.x += 15;
                        rectFront.width *= 0.8f;
                    }*/
                    /*rectBack.width *= 0.71f;
                    if (node is TriggerState)
                    {
                        rectBack.y += 8;
                        rectBack.x += 25;

                    }*/

                    EditorGUI.ProgressBar(rectBack, node.progress / 142f, string.Empty);
                /*GUI.Box(rectBack, "", (GUIStyle)"ProgressBarBack");

                GUIStyle st = (GUIStyle)"ProgressBarBar";
                //st.
                GUI.backgroundColor = Color.white;
                GUI.Box(rectFront, "", st);*/

                //GUI.Box(rectBack, string.Empty, GUI.skin.FindStyle("progress back"));
                //GUI.Box(rectFront, string.Empty, GUI.skin.FindStyle("progress front"));
            }

        }

        /// <summary>
        /// Returns true if given node is currently being executed.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool InProgress(Node node)
        {
            return EditorApplication.isPlaying && ((node is ActionsState && (node as ActionsState).internalExecuting) || (Instance.nodesProgress.Contains(node) && node.progress > 0));
        }

        /// <summary>
        /// Draws node transitions.
        /// </summary>
        /// <param name="node"></param>
        private void DrawTransitions(Node node, StateMachine onlyFrom = null)
        {
            if (!node || node.transitions == null || node.transitions.Length == 0) return;
 
            var groups = node.transitions.GroupBy(c => c.toNode).ToList();
            var groups2 = node.transitions.GroupBy(c => c.toNode.parent).ToList();
            int upNodeCount = 0;
            foreach (var group in groups)
            {
                Transition transition = group.First();
                if (!transition) continue;
                Node toNode = transition.toNode;
                Node fromNode = transition.fromNode;
                if (!toNode || !fromNode)
                {
                    if(fromNode)
                    {
                        fromNode.transitions = CleanupTransitions(transition, fromNode.transitions);
                    }
                    continue;
                }
                
                if(!fromNode) return;
                if(!toNode)
                {                        
                    fromNode.transitions = CleanupTransitions(transition, fromNode.transitions);
                    return;
                }

                if (!toNode.parent)
                {
                    fromNode.transitions = CleanupTransitions(transition, fromNode.transitions);
                    return;
                }

                if (toNode.parent != Active && toNode.parent == fromNode.parent)// && toNode != Active
                {
                    // Debug.LogWarningFormat("Ignoring transition toNode: {0} fromNode: {1} toParent: {2} fromParent: {3}", toNode.name, fromNode.name, toNode.parent.name, fromNode.parent.name); 
                    continue;
                }

                // var l2 = groups2.Find(t => (t as Transition).toNode.parent == transition.toNode.parent);
                // Debug.LogWarningFormat("groups2 count: {0} isMatch: {1} find: {2}", groups2.Count(), groups2.Exists(t => t == transition), groups2.Find(t2  == transition));
                int arrowCount = group.Count() > 1 ? 3 : 1;
                foreach (var g in groups2)
                {
                    Transition t = g.First();
                    if(!t) continue;
                    if (t.Equals(transition) && groups2.Count() > 1)
                    {
                        arrowCount = 3;
                    }
                }
                bool offset = toNode.transitions.Any(x => x.toNode == fromNode);
                Color selected = GraphStyles.ConnectionColor;// GUI.skin.settings.selectionColor;
                selected.a = 1;
                bool sel = transition == SelectedTransition || 
                    ((selection.Contains(fromNode) || selection.Contains(fromNode.parent)) && (selection.Contains(toNode) || selection.Contains(toNode.parent))) 
                     || (SelectedTransition && (SelectedTransition.toNode == transition.toNode && SelectedTransition.fromNode == transition.fromNode)); // ||
                                                // SelectedTransition.fromNode == transition.fromNode
                Color color = sel ? selected : Color.white;
                color.a = 1;

                if (transition.useConditions && !sel)
                {
                    color = GraphStyles.ConditionColor;
                }
                // Debug.LogFormat("DrawConnection from: {0} to: {1} selected: {2} selTr: {3}", fromNode.name, toNode.name, sel, SelectedTransition);

                if (onlyFrom)
                {
                    upNodeCount++;
                    if(upNodeCount > 0)
                    {
                        arrowCount = upNodeCount > 1 ? 3 : 1;
                        // offset = upNodeCount > 1;
                    }
        
                    //Debug.LogFormat("OnlyFrom state: {0} from: {1} to: {2} fromParent: {3} toParent: {4} offset: {5} arrowCount: {6}", onlyFrom.name, fromNode.name, toNode.name, fromNode.parent.name, toNode.parent.name, offset, arrowCount);

                    if(fromNode.parent == onlyFrom && toNode.parent != onlyFrom)
                    {
                        // Debug.LogFormat("1 Draw this from Up state: {0} to: {1}", toNode.parent.upNode, toNode.name);

                        if(toNode.parent.upNode) DrawConnection(toNode.parent.upNode.position.center, toNode.position.center, color, arrowCount, offset, transition);
                    }
                    /*else if(fromNode.parent != toNode.parent)
                    {
                        // Debug.LogFormat("2 Draw this from Up state: {0} to: {1}", toNode.parent.upNode, toNode.name);

                        if(toNode.parent.upNode) DrawConnection(toNode.parent.upNode.position.center, toNode.position.center, color, arrowCount, offset, transition);
                    }*/
                }
                else
                {

                    var end = toNode.position.center;
                    var start = fromNode.position.center;
                    if (toNode.parent != fromNode.parent && toNode.parent != Active)// && fromNode.parent == Active
                    {
                        bool sel2 = SelectedTransition && SelectedTransition.fromNode == transition.fromNode && SelectedTransition.toNode.parent == transition.toNode.parent;
                        if(sel2) color = selected;
                        upNodeCount++;
                        
                        if (fromNode.parent.upNode && toNode.parent != Active)
                        {
                            end = fromNode.parent.upNode.position.center;
                        }
                        else
                        {
                            end = toNode.parent.position.center;
                        }
                        
                        if(upNodeCount > 0)
                        {
                            arrowCount = upNodeCount > 1 ? 3 : 1;
                            // offset = upNodeCount > 1;
                        }
                    }

                    if (fromNode.parent != Active)
                    {
                        start = fromNode.parent.position.center;
                    }
                    // Debug.LogFormat("DrawConnection from: {0} to: {1} start: {2} end: {3} offset: {4}", fromNode.name, toNode.name, start, end, offset);
                    //Debug.LogFormat("DrawConnection this from: {0} to: {1} fromParent: {2} toParent: {3} offset: {4} arrows: {5}", fromNode.name, toNode.name, fromNode.parent.name, toNode.parent.name, offset, arrowCount);

                    DrawConnection(start, end, color, arrowCount, offset, transition);
                    // DrawConnection(fromNode.position.center, toNode.parent != fromNode.parent ? toNode.parent.position.center : toNode.position.center, color, arrowCount, offset, transition);
                }
            }
        }

        private Transition[] CleanupTransitions(Transition transition, Transition[] list)
        {
            var transitions = list;
            ArrayUtility.Remove(ref transitions, transition);
            list = transitions;
            
            Debug.LogFormat("Transition removed: {0}", transition);

            DestroyImmediate(transition, true);
            return list;
        }

        protected override void CanvasContextMenu()
        {
            // if (currentEvent.type != EventType.MouseDown || currentEvent.button != 1 || currentEvent.clickCount != 1)
            // {
            //     Debug.LogWarning("Not doing the context");
            //     return;
            // }
            if (currentEvent.type != EventType.ContextClick)
            {
                return;
            }
            Node node = MouseOverNode();
            if (node) return;

            GenericMenu canvasMenu = new GenericMenu();

            if (!Active)
            {
                /*canvasMenu.AddItem(GraphContent.CreateStateMachine, false, () =>
                {
                    ShowWindow();
                    StateMachine stateMachine = CreateAsset<StateMachine>(true);
                    if (stateMachine == null)
                    {
                        return;
                    }
                    stateMachine.color = GraphStyles.StateMachineColor;
                    stateMachine.name = stateMachine.name;
                    System.Guid guid = System.Guid.NewGuid();
                    stateMachine.id = Mathf.Abs(guid.GetHashCode());

                    //GraphUtility.AddNode<EntryState>(GraphEditor.Center - new Vector2(GraphStyles.StateWidth, 0), stateMachine);
                    //GraphUtility.AddNode<ExitState>(GraphEditor.Center + new Vector2(GraphStyles.StateWidth, 0), stateMachine);

                    SelectStateMachine(stateMachine);
                });
                canvasMenu.ShowAsContext();*/
                return;
            }

            canvasMenu.AddItem(GraphContent.CreateActions, false, () =>
            {
                ActionsState state = GraphUtility.AddNode<ActionsState>(mousePosition, Active);
                GraphUtility.UpdateNodeColor(state);
                shouldUpdate = true;
            });
            canvasMenu.AddItem(GraphContent.CreateTrigger, false, () =>
            {
                TriggerState state = GraphUtility.AddNode<TriggerState>(mousePosition, Active);
                GraphUtility.UpdateNodeColor(state);
                shouldUpdate = true;
            });
            canvasMenu.AddItem(GraphContent.CreateSubFsm, false, () =>
            {
                StateMachine stateMachine = GraphUtility.AddNode<StateMachine>(mousePosition, Active);
                GraphUtility.UpdateNodeColor(stateMachine);
                shouldUpdate = true;
            });

            if (Pasteboard.CanPaste())
            {
                canvasMenu.AddItem(GraphContent.Paste, false, () =>
                {
                    Pasteboard.Paste(mousePosition, Active, Reactions, false);
                    shouldUpdate = true;
                });
            }
            else
            {
                canvasMenu.AddDisabledItem(GraphContent.Paste);
            }

            canvasMenu.AddItem(GraphContent.CopyCurrent, false, () =>
            {
                Pasteboard.Copy(new List<Node> { Active }, Reactions);
            });
            
            canvasMenu.ShowAsContext();
        }

        private void NodeContextMenu()
        {
            /*if (currentEvent.type != EventType.MouseDown || currentEvent.button != 1 || currentEvent.clickCount != 1)
            {
                return;
            }
            */
            Node node = MouseOverNode();

            if (!node)
            {
                return;
            }
            GenericMenu nodeMenu = new GenericMenu();
            if (Application.isPlaying && Controller)
            {
                nodeMenu.AddItem(new GUIContent("Execute"), false, delegate {
                    node.OnEnter(Reactions, Controller.gameObject);
                });
            }

            if (IsValidNode(node))
            {
                nodeMenu.AddItem(GraphContent.MakeTransition, false, () =>
                {
                    fromNode = node;
                    shouldUpdate = true;
                });
            }

            if (!node.IsStartNode && IsValidNode(node) && !(node is TriggerState) && !(node is EntryState))
            {
                nodeMenu.AddItem(GraphContent.SetAsDefault, false, () =>
                {
                    GraphUtility.SetDefaultNode(node, Active);
                    shouldUpdate = true;
                });
            }

            if (IsValidNode(node) && !(node is EntryState))
            {
                /*nodeMenu.AddItem(GraphContent.moveToSubStateMachine, false, () =>
                {
                    StateMachine stateMachine = GraphUtility.AddNode<StateMachine>(mousePosition, Active);
                    Pasteboard.Copy(selection);
                    Pasteboard.Paste(mousePosition, stateMachine);
                    foreach (Node mNode in selection)
                    {
                        if (!(mNode is UpState))
                        {
                            GraphUtility.DeleteNode(mNode);
                        }
                    }
                    selection.Clear();
                    UpdateSelection();
                    EditorUtility.SetDirty(Active);
                });

                if (Active.parent != null)
                {
                    nodeMenu.AddItem(GraphContent.moveToParentStateMachine, false, () =>
                    {
                        Pasteboard.Copy(selection);
                        Pasteboard.Paste(mousePosition, Active.parent);
                        foreach (Node mNode in selection)
                        {
                            if (!(mNode is UpState))
                            {
                                GraphUtility.DeleteNode(mNode);
                            }
                        }
                        selection.Clear();
                        UpdateSelection();
                        EditorUtility.SetDirty(Active);
                    });
                }
                else
                {
                    nodeMenu.AddDisabledItem(GraphContent.moveToParentStateMachine);
                }*/

                nodeMenu.AddItem(GraphContent.Copy, false, () =>
                {
                    Pasteboard.Copy(selection, Reactions);
                    shouldUpdate = true;
                });

                nodeMenu.AddItem(GraphContent.Delete, false, () =>
                {
                    if (selection.Contains(node))
                    {
                        foreach (Node mNode in selection)
                        {
#if SM_RPG
                            if (!(mNode is UpState) && !(mNode is SkillStateMachine))
#else
                            if (!(mNode is UpState))
#endif
                            {
                                GraphUtility.DeleteNode(mNode, mNode.Root.sourceReactions);
                                if (Controller) GraphUtility.DeleteNode(mNode, Controller.reactions);
                            }
                        }
                        selection.Clear();
                        UpdateSelection();
                    }
                    else
                    {
                        GraphUtility.DeleteNode(node, node.Root.sourceReactions);
                        if (Controller && Controller.reactions) GraphUtility.DeleteNode(node, Controller.reactions);
                    }
                    if (GUI.changed) EditorUtility.SetDirty(Active);
                    shouldUpdate = true;
                });
            }
            /*else
            {
                nodeMenu.AddDisabledItem(GraphContent.Copy);
                nodeMenu.AddDisabledItem(GraphContent.Delete);
            }*/
            nodeMenu.ShowAsContext();
            Event.current.Use();
        }

        public Node MouseOverNode(bool ignoreSelection = false)
        {
            for (int i = 0; i < Nodes.Length; i++)
            {
                Node node = Nodes[i];
                if (!node || (ignoreSelection && selection.Contains(node))) continue;
                if (node.position.Contains(mousePosition))
                {
                    return node;
                }
            }
            return null;
        }

        private Transition MouseOverTransition()
        {
            if (Active.parent)
            {
                for (int i = 0; i < Active.parent.nodes.Length; i++)
                {
                    Node node = Active.parent.nodes[i];
                    if (!node) continue;
                    for (int j = 0; j < node.transitions.Length; j++)
                    {
                        Transition transition = node.transitions[j];
                        if (!transition || !transition.toNode || !transition.fromNode) continue;

                        if (transition.fromNode.parent == Active.parent && transition.toNode.parent != Active.parent)
                        {
                            bool offset = transition.toNode.transitions.Any(x => x.toNode == transition.fromNode);
                            Vector3 start = transition.toNode.position.center;
                            Vector3 end = transition.toNode.parent.upNode.position.center;
                            Vector3 cross = Vector3.Cross((start - end).normalized, Vector3.forward);

                            if (offset)
                            {
                                start = start + cross * 6;
                                end = end + cross * 6;
                            }
                            
                            // Debug.LogWarningFormat("1 OverTransition: '{2}' distance: {0} offset: {1}", HandleUtility.DistanceToLine(start, end), offset, transition.fromNode.name);

                            if (HandleUtility.DistanceToLine(start, end) < 4f)
                            {
                                return transition;
                            }
                        }
                    }
                }
            }
            
            

            for (int i = 0; i < Nodes.Length; i++)
            {
                Node node = Nodes[i];
                if (!node) continue;
                // Debug.LogWarningFormat("Node: '{0}' transitions: {1}", node.name, node.transitions.Length);
                
                if (node is StateMachine)
                {
                    var subNodes = (node as StateMachine).nodes;
                    for (int j = 0; j < subNodes.Length; j++)
                    {
                        var sn = subNodes[j];
                        var list = sn.transitions.Where(n => n.toNode.parent == Active);
                        foreach (Transition transition in list)
                        {
                            if (!transition || !transition.toNode || !transition.fromNode) continue;

                            bool offset = transition.toNode.transitions.Any(x => x.toNode == transition.fromNode);
                            Vector3 start = transition.fromNode.parent.position.center;
                            Vector3 end = transition.toNode.parent != Root && transition.fromNode.parent == Root ? transition.toNode.parent.position.center : transition.toNode.position.center;

                            Vector3 cross = Vector3.Cross((start - end).normalized, Vector3.forward);
                            if (offset)
                            {
                                start = start + cross * 6;
                                end = end + cross * 6;
                            }
                    
                            // Debug.LogWarningFormat("3 OverTransition: '{2}' distance: {0} offset: {1}", HandleUtility.DistanceToLine(start, end), offset, transition.fromNode.name);

                            if (HandleUtility.DistanceToLine(start, end) < 3f)
                            {
                                return transition;
                            }
                        }
                    }
                }

                for (int j = 0; j < node.transitions.Length; j++)
                {
                    Transition transition = node.transitions[j];
                    if (!transition || !transition.toNode || !transition.fromNode) continue;

                    bool offset = transition.toNode.transitions.Any(x => x.toNode == transition.fromNode);
                    Vector3 start = transition.fromNode.position.center;
                    Vector3 end = transition.toNode.parent != Root && transition.fromNode.parent == Root ? transition.toNode.parent.position.center : transition.toNode.position.center;
                    if (transition.toNode.parent != Active && transition.fromNode && transition.fromNode.parent && transition.fromNode.parent.upNode)
                    {
                        end = transition.fromNode.parent.upNode.position.center;
                    }

                    Vector3 cross = Vector3.Cross((start - end).normalized, Vector3.forward);
                    if (offset)
                    {
                        start = start + cross * 6;
                        end = end + cross * 6;
                    }
                    
                    // Debug.LogWarningFormat("2 OverTransition: '{2}' distance: {0} offset: {1}", HandleUtility.DistanceToLine(start, end), offset, transition.fromNode.name);

                    if (HandleUtility.DistanceToLine(start, end) < 3f)
                    {
                        return transition;
                    }
                }
            }
            return null;
        }

        private void SelectNodesInRect(Rect r)
        {
            for (int i = 0; i < Nodes.Length; i++)
            {
                Node node = Nodes[i];
                if (!node) continue;
                Rect rect = node.position;
                if (rect.xMax < r.x || rect.x > r.xMax || rect.yMax < r.y || rect.y > r.yMax)
                {
                    selection.Remove(node);
                    continue;
                }
                if (!selection.Contains(node))
                {
                    selection.Add(node);
                }
            }
            UpdateSelection();
        }

        private Rect FromToRect(Vector2 start, Vector2 end)
        {
            Rect rect = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);
            if (rect.width < 0f)
            {
                rect.x = rect.x + rect.width;
                rect.width = -rect.width;
            }
            if (rect.height < 0f)
            {
                rect.y = rect.y + rect.height;
                rect.height = -rect.height;
            }
            return rect;
        }

        private void UpdateSelection()
        {
            Selection.objects = selection.ToArray();

            if (NodeInspector.Instance) NodeInspector.Instance.Refresh();

            /*if(Selection.objects.Length == 0)
            {
                StateMachineEditor.OnInspectorGUI();
                // Selection.objects = new Object[1] { Active };
            }*/
        }

        public void ToggleSelection()
        {
            if (!Active)
            {
                return;
            }
            if (selection.Count == Active.nodes.Length)
            {
                selection.Clear();
            }
            else
            {
                selection.Clear();
                selection.AddRange(Active.nodes);
            }
            /*if (selectionTransition.Count == Active.Nodes.Length)
            {
                selection.Clear();
            }
            else
            {
                selection.Clear();
                selection.AddRange(Active.Nodes);
            }*/
            UpdateSelection();
        }

        public static void SelectStateMachine(StateMachine stateMachine, bool centerView = true, bool forceSelect = false)
        {
            if (!Instance) return;

            if (Instance.active == stateMachine && !forceSelect)
            {
                if(centerView) Instance.CenterView();
                return;
            }
            // Debug.Log("SelectStateMachine "+stateMachine);

            if (Instance.active) Instance.active.isSelected = false;
            Instance.active = stateMachine;
            Instance.selection.Clear();
            if(stateMachine) stateMachine.isSelected = true;
            //Selection.activeObject = Instance.active;
            //Instance.UpdateUnitySelection();
            if (centerView) Instance.CenterView();
            Instance.SetupBlackboard();
        }

        public static void SelectGameObject(GameObject gameObject)
        {
            if (Instance == null)
            { //|| ActiveGameObject == gameObject) {
                return;
            }
            //!PreferencesEditor.GetBool(Preference.LockSelection) && 
            if (gameObject != null)
            {
                StateMachineController behaviour = gameObject.GetComponent<StateMachineController>();

                bool isSame = false;
                if (behaviour != null && Instance.active != null && Instance.activeGameObject != null)
                {
                    isSame = behaviour.stateMachine == Instance.active || Instance.activeGameObject == behaviour.gameObject;
                }

                //Debug.Log("SelectGO activeGameObject " + Instance.activeGameObject+" / active: "+ Instance.active+" same? "+ isSame);

                if (behaviour != null && behaviour.stateMachine != null)// && !isSame
                {
                    Instance.activeGameObject = behaviour.gameObject;
                    SelectStateMachine(behaviour.stateMachine);
                }
            }
        }

        public static void SelectTransition(Transition transition)
        {
            if (!Instance || SelectedTransition == transition)
            {
                return;
            }
            
            Instance.selectedTransition = transition;
            Instance.Repaint();
        }

        public void CenterView()
        {
            Vector2 center = Vector2.zero;
            if (Nodes.Length > 0)
            {
                for (int i = 0; i < Nodes.Length; i++)
                {
                    Node node = Nodes[i];
                    if (!node) continue;
                    center += new Vector2(node.position.center.x - ScaledCanvasSize.width * 0.5f, node.position.center.y - ScaledCanvasSize.height * 0.5f);
                }
                center /= Nodes.Length;
            }
            else
            {
                center = Center;
            }

            UpdateScrollPosition(center);
            Repaint();
        }

        public static void RepaintAll()
        {
            if (Instance)
            {
                Instance.Repaint();
            }
        }
        /*[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        static void DrawGameObjectName(Transform transform, GizmoType gizmoType)
        {
            StateMachineController behaviour = transform.GetComponent<StateMachineController>();
            if (behaviour == null)
            {
                return;
            }
            //if (behaviour.showSceneIcon)
            //{
            Handles.Label(transform.position, AssetPreview.GetMiniThumbnail(behaviour.stateMachine));
            //}
            *if (behaviour.showStateGizmos && behaviour.stateMachine != null)
            {
                Node activeNode = behaviour.ActiveNode;
                if (activeNode != null)
                {
                    Handles.Label(transform.position, activeNode.Name, GraphEditorStyles.stateLabelGizmo);
                }
            }*
        }*/

        private bool IsDocked
        {
            get
            {
                BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
                MethodInfo isDockedMethod = typeof(EditorWindow).GetProperty("docked", fullBinding)?.GetGetMethod(true);
                return !(isDockedMethod is null) && (bool)isDockedMethod.Invoke(this, null);
            }
        }

    }
}

