using System.IO;
using GameCreator.Core;
#if PHOTON_MODULE
using Photon.Pun;
using NJG.PUN;
#endif
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;

namespace NJG.Graph
{
    [CustomEditor(typeof(ActionsState), true)]
    public class ActionsStateInspector : NodeInspector
    {
        #if PHOTON_MODULE
        private GUIContent GUI_SYNC = new GUIContent("Sync Actions", "Adds a PhotonNetwork component to sync these actions.");
        private GUIContent GUI_TARGET = new GUIContent("Targets", "Adds a PhotonNetwork component to sync these actions.");
        private GUIContent GUI_TARGET_TYPE = new GUIContent("Target Type", "The target used to send this Actions RPC.");

        private const string All = "Sends the RPC to everyone else and executes it immediately on this client. Player who join later will not execute this RPC.";
        private const string Others = "Sends the RPC to everyone else. This client does not execute the RPC. Player who join later will not execute this RPC.";
        private const string MasterClient = "Sends the RPC to MasterClient only. Careful: The MasterClient might disconnect before it executes the RPC and that might cause dropped RPCs.";
        private const string AllBuffered = "Sends the RPC to everyone else and executes it immediately on this client. New players get the RPC when they join as it's buffered (until this client leaves).";
        private const string OthersBuffered = "Sends the RPC to everyone. This client does not execute the RPC. New players get the RPC when they join as it's buffered (until this client leaves).";
        private const string AllViaServer = "Sends the RPC to everyone (including this client) through the server.\nThe server's order of sending the RPCs is the same on all clients.";
        private const string AllBufferedViaServer = "Sends the RPC to everyone (including this client) through the server and buffers it for players joining later.\nThe server's order of sending the RPCs is the same on all clients.";
        private const string SpecificPlayer = "Sends the RPC to an specific Player.";
        
        private bool hasComponent = false;
        private ActionsStateNetwork.ActionRPC actionsRPC;
        private int index = -1;
        private bool initialized = false;
        private ActionsStateNetwork actionsNetwork;
        private SerializedObject serializedNetwork;
        // private Actions actions;
        private static Section section;
        private SerializedProperty spActions;
        private SerializedObject actionSerializedObject;
        
        #endif
        
        private IActionsListEditor actionEditor;
        private ActionsState state;
        private int lastCount;
        private int lastHashCode;
        private SerializedProperty spCanReset;

        public override void OnEnable()
        {
            base.OnEnable();

            state = node as ActionsState;
            spCanReset = serializedObject.FindProperty("resetState");

            actionEditor = (IActionsListEditor) CreateEditor(state.GetActions(GraphEditor.Reactions));

            if (actionEditor)
            {
                IActionsList actions = actionEditor.target as IActionsList;
                lastCount = actions.actions.Length;
                lastHashCode = actions.actions.GetHashCode();

                GraphUtility.ReplaceLocalVarTargets(actions.actions);
            }
        }

        public override void OnInspectorGUI()
        {
            if(!actionEditor) return;
            if(actionEditor.serializedObject.targetObject == null) return;
            
            state = node as ActionsState;
            serializedObject.Update();
            
            DrawDescription();

            //EditorGUI.BeginChangeCheck();
            //EditorGUILayout.PropertyField(spCanReset);
            //if(EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();

            EditorGUI.BeginChangeCheck();
            actionEditor.serializedObject.Update();
            actionEditor.OnInspectorGUI();
            IActionsList actions = actionEditor.target as IActionsList;
            bool hasChanged = lastHashCode != actions.actions.GetHashCode() ||
                              lastCount != actions.actions.Length || EditorGUI.EndChangeCheck();
            bool isInstance = GraphEditor.Controller && GraphEditor.Controller.stateMachine &&
                              GraphEditor.Controller.reactions;
            if (hasChanged)
            {
                lastCount = actions.actions.Length;
                lastHashCode = actions.actions.GetHashCode();

                BlackboardWindow.UpdateVariables();
                GraphUtility.ReplaceLocalVarTargets(actions.actions);

                //Debug.Log("Actions Changed instance: " + isInstance);

                if (isInstance)
                {
                    GraphUtility.CopyIActionList(GraphEditor.Controller.reactions.GetReaction<IActionsList>(state),
                        GraphEditor.Controller.stateMachine.sourceReactions.GetReaction<IActionsList>(state));
                }
            }
            
            if (state.IsStartNode)
            {
                EditorGUILayout.HelpBox("This node is set as Default Node, this runs as soon as the State Machine is executed.\nThere is no need to use On Start Trigger", MessageType.Info);
                // EditorGUILayout.Space();
            }

            actionEditor.serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            base.OnInspectorGUI();

            #if PHOTON_MODULE
            // PaintNetwork(hasChanged);
            #endif
            
            serializedObject.ApplyModifiedProperties();
        }
        
        #if PHOTON_MODULE
        private void PaintNetwork(bool changed)
        {
            if(section == null)
            {
                section = new Section("Network Settings", "ActionNetwork.png", Repaint);
            }

            if (!initialized)
            {
                actionsNetwork = state.Root.sourceReactions.gameObject.GetComponent<ActionsStateNetwork>();
                
                //if(actionsNetwork != null) actionsRPC = ArrayUtility.Find(actionsNetwork.actions, p => p.actions == actions);
                if (actionsNetwork != null)
                {
                    serializedNetwork = new SerializedObject(actionsNetwork);
                    actionsRPC = actionsNetwork.actions.Find(p => p.actions == state);
                    index = actionsNetwork.actions.IndexOf(actionsRPC);
                }
                hasComponent = actionsRPC != null;
                initialized = true;
            }

            bool hasChanged = false;

            section.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(section.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUI.BeginChangeCheck();
                    hasComponent = EditorGUILayout.Toggle(GUI_SYNC, hasComponent);
                    hasChanged = EditorGUI.EndChangeCheck();

                    if (actionsRPC != null && actionsNetwork && initialized)
                    {
                        actionsRPC.targetType = (ActionsStateNetwork.ActionRPC.TargetType)EditorGUILayout.EnumPopup(GUI_TARGET_TYPE, actionsRPC.targetType);

                        if (actionsRPC.targetType == ActionsStateNetwork.ActionRPC.TargetType.PhotonPlayer)
                        {
                            if (serializedNetwork != null)
                            {
                                if (index < 0) index = 0;
                                EditorGUILayout.PropertyField(serializedNetwork.FindProperty("actions").GetArrayElementAtIndex(index).FindPropertyRelative("targetPlayer"));
                            }
                            EditorGUILayout.HelpBox(SpecificPlayer, MessageType.Info, false);
                        }
                        else
                        {
                            actionsRPC.targets = (RpcTarget)EditorGUILayout.EnumPopup(GUI_TARGET, actionsRPC.targets);

                            RpcTarget targets = actionsRPC.targets;
                            if (targets == RpcTarget.All)
                            {
                                EditorGUILayout.HelpBox(All, MessageType.Info, false);
                            }
                            else if (targets == RpcTarget.AllBuffered)
                            {
                                EditorGUILayout.HelpBox(AllBuffered, MessageType.Info, false);
                            }
                            else if (targets == RpcTarget.AllBufferedViaServer)
                            {
                                EditorGUILayout.HelpBox(AllBufferedViaServer, MessageType.Info, false);
                            }
                            else if (targets == RpcTarget.AllViaServer)
                            {
                                EditorGUILayout.HelpBox(AllViaServer, MessageType.Info, false);
                            }
                            else if (targets == RpcTarget.MasterClient)
                            {
                                EditorGUILayout.HelpBox(MasterClient, MessageType.Info, false);
                            }
                            else if (targets == RpcTarget.Others)
                            {
                                EditorGUILayout.HelpBox(Others, MessageType.Info, false);
                            }
                            else if (targets == RpcTarget.OthersBuffered)
                            {
                                EditorGUILayout.HelpBox(OthersBuffered, MessageType.Info, false);
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            if (hasChanged)
            {
                if (actionsRPC != null)
                {
                    Debug.Log("Removed actions.");
                    actionsNetwork.actions.Remove(actionsRPC);
                    if(actionsNetwork.actions.Count == 0)
                    {
                        DestroyImmediate(actionsNetwork, true);
                    }

                    actionsRPC = null;
                    actionsNetwork = null;

                    initialized = false;
                }
                else
                {
                    actionsNetwork = state.Root.sourceReactions.gameObject.GetComponent<ActionsStateNetwork>() ?? state.Root.sourceReactions.gameObject.AddComponent<ActionsStateNetwork>();

                    SerializedObject serializedObject = new SerializedObject(actionsNetwork);

                    actionsRPC = new ActionsStateNetwork.ActionRPC() { actions = state };
                    actionsNetwork.actions.Add(actionsRPC);

                    Debug.LogFormat("Actions sync added: {0}", actionsRPC.actions.name);

                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();

                    hasComponent = true;
                }
                hasChanged = false;
            }
            EditorGUILayout.Space();

            if (changed && GraphEditor.Controller && hasComponent)
            {
                EditorUtility.CopySerialized(state.Root.sourceReactions.GetComponent<ActionsStateNetwork>(), GraphEditor.Controller.ASNetwork);
            }
        }
        #endif
    }
    
    #if PHOTON_MODULE
        public class Section
        {
            private const string ICONS_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/";
            private const string KEY_STATE = "network-action-section-{0}";

            private const float ANIM_BOOL_SPEED = 3.0f;

            // PROPERTIES: ------------------------------------------------------------------------

            public GUIContent name;
            public AnimBool state;

            // INITIALIZERS: ----------------------------------------------------------------------

            public Section(string name, string icon, UnityAction repaint, string overridePath = "")
            {
                this.name = new GUIContent(
                    string.Format(" {0}", name),
                    GetTexture(icon, overridePath)
                );

                state = new AnimBool(GetState());
                state.speed = ANIM_BOOL_SPEED;
                state.valueChanged.AddListener(repaint);
            }

            // PUBLIC METHODS: --------------------------------------------------------------------

            public void PaintSection()
            {
                GUIStyle buttonStyle = (state.target
                    ? CoreGUIStyles.GetToggleButtonNormalOn()
                    : CoreGUIStyles.GetToggleButtonNormalOff()
                );

                if (GUILayout.Button(name, buttonStyle))
                {
                    state.target = !state.target;
                    string key = string.Format(KEY_STATE, name.text.GetHashCode());
                    EditorPrefs.SetBool(key, state.target);
                }
            }

            // PRIVATE METHODS: -------------------------------------------------------------------

            private bool GetState()
            {
                string key = string.Format(KEY_STATE, name.text.GetHashCode());
                return EditorPrefs.GetBool(key, false);
            }

            private Texture2D GetTexture(string icon, string overridePath = "")
            {
                string path = Path.Combine(string.IsNullOrEmpty(overridePath) ? ICONS_PATH : overridePath, icon);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
        }
    #endif
}