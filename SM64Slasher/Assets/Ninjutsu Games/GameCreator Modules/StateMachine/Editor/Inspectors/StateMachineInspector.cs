using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.Callbacks;

namespace NJG.Graph
{
    [CustomEditor(typeof(StateMachine), true)]
    public class StateMachineInspector : NodeInspector
    {
        private const string ICONS_PATH = "Assets/Plugins/GameCreator/Extra/Icons/Actions/{0}";
        private const string ICONS_PATH2 = "Assets/Plugins/GameCreator/Extra/Icons/Trigger/{0}";
        private const string CUSTOM_ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/StateMachine/Icons/Core/{0}";

        private StateMachine sm;
        private GraphReactions reactions;
        private  static GUIContent gcTrigger;
        private  static GUIContent gcActions;
        private  static GUIContent gcConditions;
        private  static GUIContent gcStateMachines;

        public SerializedProperty SpBlackboard { set; get; }
        public SerializedProperty SpBlackboardList { set; get; }

        public override void OnEnable()
        {
            if (target == null) return;

            sm = target as StateMachine;
            reactions = sm == null ? null : sm.sourceReactions;

            this.SpBlackboard = serializedObject.FindProperty("blackboard");
            this.SpBlackboardList = this.SpBlackboard.FindPropertyRelative("list");

            base.OnEnable();

            if(sm.sourceReactions == null)
            {
                string[] assets = AssetDatabase.FindAssets("graphReactions-" + sm.id);
                foreach(string s in assets)
                {
                    sm.sourceReactions = AssetDatabase.LoadAssetAtPath<GraphReactions>(AssetDatabase.GUIDToAssetPath(s));
                    break;
                }
            }

            if (gcTrigger == null || gcActions == null || gcConditions == null)
            {
                Texture2D iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(ICONS_PATH, "Trigger.png"));
                gcTrigger = new GUIContent(string.Empty, iconTexture);

                iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(ICONS_PATH2, "actions.png"));
                gcActions = new GUIContent(string.Empty, iconTexture);

                iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(ICONS_PATH2, "conditions.png"));
                gcConditions = new GUIContent(string.Empty, iconTexture);

                iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(CUSTOM_ICON_PATH, "StateMachine.png"));
                gcStateMachines = new GUIContent(string.Empty, iconTexture);
            }

            GraphEditor.RepaintAll();
        }

        public static bool IsTriggerInNodes(StateMachine sm, GraphReactions react, GraphTrigger a)
        {
            foreach (var n in sm.nodes)
            {
                if (n is TriggerState)
                {
                    var t = (n as TriggerState);
                    var gt = t.GetTrigger(react);
                    if (gt == a)
                    {
                        /*var igniters = t.GetTrigger(react).igniters;

                        foreach (var al in igniters)
                        {
                            if (al.Value == a) return true;
                        }*/
                        return true;
                    }
                }
            }
            return false;
        }

        public override void OnDisable()
        {
            sm = target as StateMachine;            
            base.OnDisable();

        }

        private void OnDestroy()
        {
            /*if(sm == null && reactions)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(reactions));
                AssetDatabase.SaveAssets();
            }*/

            GraphEditor.RepaintAll();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            sm = target as StateMachine;
            
            DrawDescription();
            serializedObject.ApplyModifiedProperties();

            if (sm && sm.oldSource)
            {
                if (!sm.sourceReactions) sm.sourceReactions = StateMachine.CreateReactions(sm);
                //Debug.LogWarning("This is a duplicate, lets copy oldSource: " + sm.oldSource + " to " + sm.sourceReactions);
                GraphUtility.CopyReactions(sm.oldSource, sm.sourceReactions);
                sm.oldSource = null;
                
                if(GraphEditor.Instance) GraphEditor.Instance.SetupBlackboard();
                BlackboardWindow.UpdateVariables();
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox); //GraphStyles.DescriptionBox
            //EditorGUILayout.LabelField(string.Format("Total Nodes: {0}", sm.nodes.Length));
            //EditorGUILayout.EndVertical();
            //EditorGUILayout.LabelField(string.Format("Total Components: {0}", sm.Reactions.components.Count));
            //EditorGUILayout.Space();

            //EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var nodes = sm.NodesRecursive;

            gcTrigger.text = $" Triggers: {nodes.Count(p => p is TriggerState)}";
            gcActions.text = $" Action Lists: {nodes.Count(p => p is ActionsState)}";

            int transitions = 0;
            foreach(var n in nodes)
            {
                if(!n) continue;
                if(n.transitions == null) continue;
                transitions += n.transitions.Length;
            }

            gcConditions.text = $" Transitions: {transitions}";
            gcStateMachines.text = $" Sub-State Machines: {nodes.Count(p => p is StateMachine)}";

            //EditorGUI.indentLevel++;
            //EditorGUILayout.Space();
            EditorGUILayout.LabelField(gcStateMachines);
            EditorGUILayout.LabelField(gcTrigger);
            EditorGUILayout.LabelField(gcActions);
            EditorGUILayout.LabelField(gcConditions);
            //EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            /*if (node.parent)
            {
                base.OnInspectorGUI();
            }*/

            if(node == node.Root)
            {
                if (GUILayout.Button("Export"))
                {
                    sm.Export();
                    EditorGUIUtility.ExitGUI();
                }
            }
            base.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Add the possibility to open the asset by just double clicking a my graph scriptable object
        /// </summary>
        /// <returns>True if the clicked item was of the type <see cref="StateMachine"/></returns>
        [OnOpenAsset(0)]
        public static bool OnOpenGraphAsset(int instanceId, int line)
        {
            UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj == null) return false;

            System.Type type = obj.GetType();
            if (type == typeof(StateMachine))
            {
                var window = EditorWindow.GetWindow<GraphEditor>("State Machine");
                StateMachine sm = obj as StateMachine;
                GraphEditor.SelectStateMachine(sm);
                window.Show();
                return true;
            }
            return false;
        }
    }
    
    public class CustomPostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// Handles when ANY asset is imported, deleted, or moved.  Each parameter is the full path of the asset, including filename and extension.
        /// </summary>
        /// <param name="importedAssets">The array of assets that were imported.</param>
        /// <param name="deletedAssets">The array of assets that were deleted.</param>
        /// <param name="movedAssets">The array of assets that were moved.  These are the new file paths.</param>
        /// <param name="movedFromPath">The array of assets that were moved.  These are the old file paths.</param>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
        {
            foreach(string asset in importedAssets)
            {
                // Debug.Log("Imported: " + asset);
                var sm = AssetDatabase.LoadAssetAtPath<StateMachine>(asset);
                if (sm)
                {
                    CheckIfDuplicate(sm);
                }
            }
 
            foreach (string asset in deletedAssets)
            {
                // Debug.Log("Deleted: " + asset);
            }
 
            for (int i = 0; i < movedAssets.Length; i++ )
            {
                // Debug.Log("Moved: from " + movedFromPath[i] + " to " + movedAssets[i]);
               
            }
            if (importedAssets.Length > 0)
            {
                // GraphReactions.CheckDirtyReactions();
            }

            if (deletedAssets.Length > 0)
            {
                // GraphReactions.CheckOrphanReactions();
            }
        }
        
        private static void CheckIfDuplicate(StateMachine stateMachine)
        {
            bool isDuplicate = false;
            var paths = AssetDatabase.FindAssets("t:StateMachine");
            foreach(var p in paths)
            {
                var sm = AssetDatabase.LoadAssetAtPath<StateMachine>(AssetDatabase.GUIDToAssetPath(p));
                if (sm == stateMachine) continue;

                if (sm.id == stateMachine.id)
                {
                    stateMachine.oldSource = sm.sourceReactions;
                    stateMachine.id = Mathf.Abs(Guid.NewGuid().GetHashCode());
                    stateMachine.sourceReactions = null;
                    isDuplicate = true;
                    break;
                }
            }

            if (stateMachine.oldSource) isDuplicate = true;
            
            // Debug.LogWarningFormat("SM: {0} isDuplicate: {1}", stateMachine, isDuplicate);
            if (isDuplicate)
            {
                string[] assets = AssetDatabase.FindAssets("graphReactions-" + stateMachine.id);
                // Debug.Log("Looking for reactions: " + assets.Length+" name: graphReactions-" + stateMachine.id);

                foreach(string s in assets)
                {
                    stateMachine.sourceReactions = AssetDatabase.LoadAssetAtPath<GraphReactions>(AssetDatabase.GUIDToAssetPath(s));
                    // Debug.Log("Found: " + AssetDatabase.GUIDToAssetPath(s));
                    break;
                }
                var reactions =  stateMachine.sourceReactions ? stateMachine.sourceReactions : StateMachine.CreateReactions(stateMachine);
                if (reactions)
                {
                    stateMachine.sourceReactions = reactions;
                    // Debug.LogWarning( "This is a duplicate, lets copy oldSource: " + stateMachine.oldSource + " to " + stateMachine.sourceReactions);
                    GraphUtility.CopyReactions(stateMachine.oldSource, stateMachine.sourceReactions);
                    stateMachine.sourceReactions.stateMachine = stateMachine;
                    stateMachine.oldSource = null;
                    EditorUtility.SetDirty(stateMachine);
                    AssetDatabase.SaveAssets();
                    // AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(stateMachine.sourceReactions));
                }
            }
            
            /*if(stateMachine.sourceReactions && stateMachine.oldSource && stateMachine.oldSource != stateMachine.sourceReactions)
            {
                stateMachine.oldSource = null;
                EditorUtility.SetDirty(stateMachine);
            }*/
        }
    }

    
    public class CustomAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        /*private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            Debug.Log("Source path: " + sourcePath + ". Destination path: " + destinationPath + ".");
            AssetMoveResult assetMoveResult = AssetMoveResult.DidMove;
            
            var stateMachine = AssetDatabase.LoadAssetAtPath<StateMachine>(destinationPath);
            if (stateMachine)
            {
                // CheckIfDuplicate(stateMachine);
            }
            
            return assetMoveResult;
        }
        private static void OnWillCreateAsset(string assetName)
        {
            Debug.Log("OnWillCreateAsset is being called with the following asset: " + assetName + ".");
            
            var stateMachine = AssetDatabase.LoadAssetAtPath<StateMachine>(assetName);
            if (stateMachine)
            {
                // CheckIfDuplicate(stateMachine);
            }
        }*/

        private static AssetDeleteResult OnWillDeleteAsset(string assetName, RemoveAssetOptions options)
        {
            StateMachine sm = AssetDatabase.LoadAssetAtPath<StateMachine>(assetName);

            if (sm == null || !sm.sourceReactions) return AssetDeleteResult.DidNotDelete;

            if(sm.parent != null) return AssetDeleteResult.DidNotDelete;
            
            //Debug.Log("Delete sourceReactions " + AssetDatabase.GetAssetPath(sm.sourceReactions.gameObject)+" sm.parent: "+sm.parent+" root: "+sm.Root);

            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(sm.sourceReactions.gameObject));
            // GraphReactions.CheckOrphanReactions();
            GraphReactions.CheckDirtyReactions();
            // AssetDatabase.SaveAssets();

            return AssetDeleteResult.DidNotDelete;
        }
    }
}