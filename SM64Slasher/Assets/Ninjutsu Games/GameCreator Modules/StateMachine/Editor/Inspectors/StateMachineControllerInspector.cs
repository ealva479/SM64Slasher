using System;
using UnityEngine;
using UnityEditor;
using GameCreator.Core;
using System.Collections.Generic;
using System.Linq;
using GameCreator.Variables;
#if PHOTON_MODULE
using NJG.PUN;
using Photon.Pun;
#endif
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace NJG.Graph
{
    [CustomEditor(typeof(StateMachineController))]
    public class StateMachineControllerInspector : MultiSubEditor<MBVariableEditor, MBVariable>
    {
        private const string MSG_REPEATED_VARIABLE = "There are two variables with the same name";
        private const string INVOKER = "invoker";

        private static readonly GUIContent GC_PARAMS = new GUIContent("Parameters");

        private const string VAR_RUNTIME = "{0} (runtime)";

        private static Color COLOR_SECTION_BACKGR = new Color(0, 0, 0, 0.2f);
        private static Color COLOR_SECTION_BORDER = new Color(0, 0, 0, 0.4f);

        private const string ICONS_PATH = "Assets/Plugins/GameCreator/Extra/Icons/Actions/{0}";
        private const string ICONS_PATH2 = "Assets/Plugins/GameCreator/Extra/Icons/Trigger/{0}";
        private const string CUSTOM_ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/StateMachine/Icons/Core/{0}";

        private SerializedProperty spStateMachine;
        private SerializedProperty spRunMode;
        private SerializedProperty spOverrideCollider;
        private SerializedProperty spReferences;
        private static GUIContent GUI_BUTTON = new GUIContent("Open Editor Graph");

        private StateMachineController controller;
        private StateMachine sm;

        private static bool hiearchySubscribed;
        private int totalTriggers;

        private static GUIContent gcTrigger;
        private static GUIContent gcActions;
        private static GUIContent gcConditions;
        private static GUIContent gcStateMachines;
        private int totalActions;
        private int totalConditions;

        private GraphReactionsEditor reactionsEditor;
        private bool invokerChecked;
        public static StateMachineControllerInspector instance { private set; get; }

        #if PHOTON_MODULE
        private ActionsStateNetwork asn;
        #endif

        //private bool hasInvoker = false;

        public void OnEnable()
        {
            instance = this;
            controller = target as StateMachineController;
            spStateMachine = serializedObject.FindProperty("stateMachine");
            spRunMode = serializedObject.FindProperty("playMode");
            spOverrideCollider = serializedObject.FindProperty("overrideColliderValues");
            spReferences = serializedObject.FindProperty("references");
            if (!(controller is null))
            {
                sm = controller.stateMachine;
                if (gcTrigger == null || gcActions == null || gcConditions == null)
                {
                    Texture2D iconTexture =
                        AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(ICONS_PATH, "Trigger.png"));
                    gcTrigger = new GUIContent(string.Empty, iconTexture);

                    iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(ICONS_PATH2, "actions.png"));
                    iconTexture.hideFlags = HideFlags.DontSave;
                    gcActions = new GUIContent(string.Empty, iconTexture);

                    iconTexture =
                        AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(ICONS_PATH2, "conditions.png"));
                    iconTexture.hideFlags = HideFlags.DontSave;
                    gcConditions = new GUIContent(string.Empty, iconTexture);

                    iconTexture =
                        AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(CUSTOM_ICON_PATH, "StateMachine.png"));
                    iconTexture.hideFlags = HideFlags.DontSave;
                    gcStateMachines = new GUIContent(string.Empty, iconTexture);
                }

                if (controller && controller.stateMachine && controller.stateMachine.sourceReactions)
                {
                    foreach (var c in controller.stateMachine.sourceReactions.components)
                    {
                        if (c.Value == null) continue;
                        if (c.Value is GraphTrigger)
                        {
                            var trigger = c.Value as GraphTrigger;
                            if (!(trigger is null) && trigger.igniters.Count > 0)
                            {
                                foreach (var ig in trigger.igniters)
                                {
                                    if (ig.Value == null) continue;
                                    totalTriggers++;
                                }
                            }
                        }

                        if (c.Value is IActionsList)
                        {
                            var trigger = c.Value as IActionsList;
                            if (!(trigger is null) && trigger.actions.Length > 0)
                            {
                                foreach (var ig in trigger.actions)
                                {
                                    if (ig == null) continue;
                                    totalActions++;
                                }
                            }
                        }

                        if (c.Value is IConditionsList)
                        {
                            var trigger = c.Value as IConditionsList;
                            if (!(trigger is null) && trigger.conditions.Length > 0)
                            {
                                foreach (var ig in trigger.conditions)
                                {
                                    if (ig == null) continue;
                                    totalConditions++;
                                }
                            }
                        }
                    }
                }
            }
            
            controller.references = controller.references.Where(c => c != null).ToArray();
            
            EditorApplication.update += OnUpdate;
        }

        private void OnValidate()
        {
            controller = target as StateMachineController;
            controller.references = controller.references.Where(c => c != null).ToArray();
#if PHOTON_MODULE
            SyncNetworkStates();
#endif
        }

        private void OnUpdate()
        {
#if PHOTON_MODULE
            SyncNetworkStates();
#endif
            if(controller.overrideColliderValues && controller.stateMachine) controller.SyncCollider(true);
            
            // if (EditorGUI.EndChangeCheck())
            {
                if (!(controller is null) && sm != controller.stateMachine && controller.stateMachine)
                {
                    if (controller.stateMachineCollider)
                    {
                        if (!(controller.stateMachineCollider is CharacterController))
                        {
                            DestroyImmediate(controller.stateMachineCollider, true);
                        }
                    }

                    controller.overrideColliderValues = true;
                    CleanUpVariables();

                    if (controller.stateMachine && controller.stateMachine.sourceReactions)
                    {
                        CheckInvoker();
                        reactionsEditor = CreateEditor(controller.stateMachine.sourceReactions, typeof(GraphReactionsEditor)) as GraphReactionsEditor;
                        reactionsEditor.OnEnable();
                        controller.SyncCollider(false);
                        GraphEditor.SelectStateMachine(controller.stateMachine, true, true);
                        Selection.activeGameObject = controller.gameObject;
                    }
                    sm = controller.stateMachine;
#if PHOTON_MODULE
                    SyncNetworkStates();
#endif
                }
            }
            
            if (controller.stateMachine == null && controller.stateMachineCollider)
            {
                if (controller.stateMachineCollider is CharacterController)
                {
                    controller.stateMachineCollider = null;
                    return;
                }
                DestroyImmediate(controller.stateMachineCollider, true);
            }

        }

#if PHOTON_MODULE
        private void SyncNetworkStates()
        {
            if (!sm.sourceReactions) return;
            var net = sm.sourceReactions.GetComponent<ActionsStateNetwork>();
            if (sm.sourceReactions && net)
            {
                EditorUtility.CopySerialized(net, controller.ASNetwork);
            }
        }
        #endif

        public void CheckInvoker()
        {
            if (Application.isPlaying) return;

            if (controller.invokerVariable || !controller.stateMachine) return;
            invokerChecked = true;

            //Debug.Log("CheckInvoker " + this.reactions.invokerVariable + " invokerChecked "+ invokerChecked);

            GameObject gameObject;
            MBVariable variable = (gameObject = controller.gameObject).AddComponent<MBVariable>();
            variable.variable.name = INVOKER;
            variable.variable.Set(Variable.DataType.GameObject, gameObject);

            //Debug.Log("variable " + variable);
            spReferences.AddToObjectArray<MBVariable>(variable);
            AddSubEditorElement(variable, -1, true);

            controller.invokerVariable = variable;

            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
            CleanSubEditors();
        }

        private void CleanUpVariables()
        {
            if (Application.isPlaying) return;
            
            #if PHOTON_MODULE
            var asn = controller.GetComponent<ActionsStateNetwork>();
            if(asn) DestroyImmediate(asn, true);
            #endif

            DestroyImmediate(controller.invokerVariable, true);

            LocalVariables[] localVars = controller.GetComponents<LocalVariables>();
            MBVariable[] vars = controller.GetComponents<MBVariable>();

            List<MBVariable> removeList = new List<MBVariable>();
            // Debug.LogWarning("CleanupVariables");

            foreach(var v in vars)
            {
                bool found = false;
                foreach(var lc in localVars)
                {
                    foreach(var mb in lc.references)
                    {
                        if (mb == v)
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    removeList.Add(v);
                }
            }

            for (int i = removeList.Count - 1; i >= 0; --i)
            {
                DestroyImmediate(removeList[i], true);
            }

            for (int i = 0; i < controller.references.Length; i++)
            {
                DeleteReferenceInstance(i);
            }
        }

        protected void DeleteReferenceInstance(int index)
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            MBVariable source = (MBVariable)spReferences
                .GetArrayElementAtIndex(index)
                .objectReferenceValue;

            if (controller.stateMachine && source == controller.invokerVariable) return;

            spReferences.RemoveFromObjectArrayAt(index);
            RemoveSubEditorsElement(index);
            DestroyImmediate(source, true);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.Update();

            //if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(this.instance.gameObject.scene);
        }

        // INSPECTOR PAINT METHOD: ----------------------------------------------------------------

        public override bool UseDefaultMargins()
        {
            return false;
        }

        public override void OnInspectorGUI()
        {
            controller = target as StateMachineController;

            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUI.BeginChangeCheck();

            //EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(spStateMachine);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(spRunMode);
            EditorGUI.BeginDisabledGroup(!controller.stateMachineCollider);
            EditorGUILayout.PropertyField(spOverrideCollider);
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            /*if (EditorGUI.EndChangeCheck())
            {
                if (!(controller is null) && sm != controller.stateMachine && controller.stateMachine)
                {
                    if(controller.stateMachineCollider) DestroyImmediate(controller.stateMachineCollider, true);

                    controller.overrideColliderValues = true;
                    CleanUpVariables();

                    if (controller.stateMachine && controller.stateMachine.sourceReactions)
                    {
                        CheckInvoker();
                        reactionsEditor = CreateEditor(controller.stateMachine.sourceReactions, typeof(GraphReactionsEditor)) as GraphReactionsEditor;
                        reactionsEditor.OnEnable();
                        // controller.SyncCollider(false);
                        GraphEditor.SelectStateMachine(controller.stateMachine, true, true);
                        Selection.activeGameObject = controller.gameObject;
                    }
                    sm = controller.stateMachine;
                }
            }
            
            if (controller.stateMachine == null && controller.stateMachineCollider)
            {
                if (controller.stateMachineCollider is CharacterController)
                {
                    controller.stateMachineCollider = null;
                    return;
                }
                Debug.LogFormat(controller.gameObject, "Destroyed SM collider on: {0}", controller.gameObject);
                DestroyImmediate(controller.stateMachineCollider, true);
            }
            */

            EditorGUI.EndDisabledGroup();

            if (!(controller is null) && controller.stateMachine)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                gcTrigger.text = $" Triggers: {totalTriggers}";
                gcActions.text = $" Actions: {totalActions}";
                gcConditions.text = $" Conditions: {totalConditions}";
                EditorGUILayout.LabelField(gcTrigger);
                EditorGUILayout.LabelField(gcActions);
                EditorGUILayout.LabelField(gcConditions);
                EditorGUILayout.EndVertical();
            }
            //EditorGUI.indentLevel--;

            /*EditorGUI.BeginDisabledGroup(controller.stateMachine == null);
            if (GUILayout.Button(GUI_BUTTON))
            {
                GraphEditor.ShowWindow();
                GraphEditor.SelectStateMachine(controller.stateMachine);
            }
            EditorGUI.EndDisabledGroup();*/

            PaintSection(GC_PARAMS);

            UpdateSubEditors(controller.references);

            if (!invokerChecked && reactionsEditor)
            {
                reactionsEditor.CheckInvoker();
                CheckInvoker();
                invokerChecked = true;
            }

            SyncBehaviorGraph();
            PaintLocalVariables();

            serializedObject.ApplyModifiedProperties();
            
            if (EditorApplication.isPlaying) Repaint();
        }

        // VARIABLES: ----------------------------------------------------------------

        private void PaintLocalVariables()
        {
            if (spStateMachine.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("No State Machine found", MessageType.Warning);
                return;
            }

            int referencesLength = spReferences.arraySize;
            if (referencesLength == 0)
            {
                EditorGUILayout.HelpBox("No Parameters", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

            HashSet<string> variables = new HashSet<string>();
            bool repeatedVariableName = false;

            for (int i = 0; i < referencesLength; ++i)
            {
                if(i >= subEditors.Length) break;
                if (subEditors[i] != null)
                {
                    string variableName = subEditors[i].spVariableName.stringValue;
                    if (variables.Contains(variableName)) repeatedVariableName = true;
                    else variables.Add(variableName);

                    subEditors[i].serializedObject.Update();

                    if (EditorApplication.isPlaying)
                    {
                        PaintVariableRuntime(subEditors[i]);
                    }
                    else
                    {
                        PaintVariableEditor(subEditors[i]);
                    }

                    if (subEditors[i] != null)
                    {
                        subEditors[i].serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            if (repeatedVariableName)
            {
                EditorGUILayout.HelpBox(MSG_REPEATED_VARIABLE, MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void PaintVariableRuntime(MBVariableEditor editor)
        {
            string label = string.Format(
                VAR_RUNTIME,
                editor.spVariableName.stringValue
            );

            var runtimeVar = editor.GetRuntimeVariable();
            if (runtimeVar == null) return;

            object variable = runtimeVar.Get();

            Rect rect = GUILayoutUtility.GetRect(
                EditorGUIUtility.fieldWidth + EditorGUIUtility.fieldWidth,
                EditorGUIUtility.singleLineHeight
            );

            EditorGUI.LabelField(
                rect,
                label,
                variable == null ? "(null)" : variable.ToString(),
                EditorStyles.boldLabel
            );
        }

        private void PaintVariableEditor(MBVariableEditor editor)
        {
            if (editor == null) return;

            editor.serializedObject.Update();

            GUIContent label = new GUIContent(editor.spVariableName.stringValue);
            switch ((Variable.DataType)editor.spVariableType.intValue)
            {
                case Variable.DataType.String: PaintProperty(editor.spVariableStr, label); break;
                case Variable.DataType.Number: PaintProperty(editor.spVariableNum, label); break;
                case Variable.DataType.Bool: PaintProperty(editor.spVariableBol, label); break;
                case Variable.DataType.Color: PaintProperty(editor.spVariableCol, label); break;
                case Variable.DataType.Vector2: PaintProperty(editor.spVariableVc2, label); break;
                case Variable.DataType.Vector3: PaintProperty(editor.spVariableVc3, label); break;
                case Variable.DataType.Texture2D: PaintProperty(editor.spVariableTxt, label); break;
                case Variable.DataType.Sprite: PaintProperty(editor.spVariableSpr, label); break;
                case Variable.DataType.GameObject: PaintProperty(editor.spVariableObj, label); break;
            }

            editor.serializedObject.ApplyModifiedProperties();
        }

        private void PaintProperty(SerializedProperty property, GUIContent label)
        {
            if (property == null) return;

            Rect rect = GUILayoutUtility.GetRect(
                EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth,
                EditorGUI.GetPropertyHeight(property)
            );

            Rect rectLabel = new Rect(
                rect.x,
                rect.y,
                EditorGUIUtility.labelWidth,
                rect.height
            );

            Rect rectField = new Rect(
                rectLabel.x + rectLabel.width,
                rect.y,
                rect.width - rectLabel.width,
                rect.height
            );

            if(label.text.Equals(INVOKER)) EditorGUI.PrefixLabel(rectLabel, label, EditorStyles.boldLabel);
            else EditorGUI.PrefixLabel(rectLabel, label);
            EditorGUI.PropertyField(rectField, property, GUIContent.none);
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void SyncBehaviorGraph()
        {
            if (spStateMachine.objectReferenceValue == null || !controller ||
                !controller.stateMachine || !controller.stateMachine.sourceReactions)
            {
                return;
            }

            

            /*if (!hasInvoker)
            {
                MBVariable variable = this.controller.gameObject.AddComponent<MBVariable>();
                variable.variable.name = INVOKER;
                variable.variable.type = (int)Variable.DataType.GameObject;
                variable.variable.Set(Variable.DataType.GameObject, controller.gameObject);
                variable.variable.save = false;

                if (variable != null)
                {
                    this.spReferences.AddToObjectArray<MBVariable>(variable);
                    this.AddSubEditorElement(variable, 0, true);
                }
            }*/

            int referencesSize = spReferences.arraySize;
            List<Blackboard.Item> addList = controller.stateMachine.GetBlackboardItems();
            List<Blackboard.Item> itemsList = controller.stateMachine.GetBlackboardItems();
            int[] blackboardIndex = new int[addList.Count];
            List<int> removeList = new List<int>();

            for (int i = 0; i < referencesSize; ++i)
            {
                if(subEditors[i] == null) continue;
                
                string refName = subEditors[i].spVariableName.stringValue;
                int refType = subEditors[i].spVariableType.intValue;
                // if(refName.ToLower() == INVOKER) continue;

                int addListIndex = addList.FindIndex(item => item.name == refName);
                if (addListIndex >= 0)
                {
                    if (refType != (int)addList[addListIndex].type)
                    {
                        int type = (int)addList[addListIndex].type;
                        subEditors[i].serializedObject.Update();
                        subEditors[i].spVariableType.intValue = type;
                        subEditors[i].serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    }

                    addList.RemoveAt(addListIndex);
                }
                else
                {
                    removeList.Add(i);
                }
            }

            for (int i = removeList.Count - 1; i >= 0; --i)
            {
                MBVariable source = (MBVariable)spReferences
                    .GetArrayElementAtIndex(removeList[i])
                    .objectReferenceValue;

                if (controller.stateMachine && controller.invokerVariable == source) continue;

                spReferences.RemoveFromObjectArrayAt(removeList[i]);
                RemoveSubEditorsElement(removeList[i]);
                DestroyImmediate(source, true);
            }

            if (!controller.stateMachine) addList.Clear();

            int addListCount = addList.Count;
            for (int i = 0; i < addListCount; ++i)
            {
                name = addList[i].name;
                if (string.IsNullOrEmpty(name)) continue;

                MBVariable variable = controller.gameObject.AddComponent<MBVariable>();
                variable.variable.name = addList[i].name;
                variable.variable.type = (int)addList[i].type;
                variable.variable.save = false;

                if (variable.variable.name == INVOKER)
                {
                    variable.variable.Set(Variable.DataType.GameObject, controller.gameObject);
                }

                if (variable)
                {
                    spReferences.AddToObjectArray<MBVariable>(variable);
                    // spReferences.AddToObjectArray<MBVariable>(variable);
                    AddSubEditorElement(variable, -1, true);
                }
            }

            if (controller.stateMachine && !reactionsEditor)
            {
                reactionsEditor = CreateEditor(controller.stateMachine.sourceReactions, typeof(GraphReactionsEditor)) as GraphReactionsEditor;
                Debug.Assert(reactionsEditor != null, nameof(reactionsEditor) + " != null");
                reactionsEditor.OnEnable();
            }
            reactionsEditor.SyncBehaviorGraph();
            
            for (int i = 0, imax = spReferences.arraySize; i < imax; ++i)
            {
                if (i >= subEditors.Length || !subEditors[i]) continue;
                string refName = subEditors[i].spVariableName.stringValue;
                if (refName.Equals(INVOKER)) continue;
                
                int targetIndex = itemsList.FindIndex(item => item.name == refName) + 1;
                spReferences.MoveArrayElement(i, targetIndex);
                MoveSubEditorsElement(i, targetIndex);
            }
        }

        private void PaintSection(GUIContent content)
        {
            Rect rect = GUILayoutUtility.GetRect(content, GraphStyles.GetInspectorSection());
            rect.xMin -= 4f;
            rect.xMax += 4f;

            GUI.DrawTexture(
                rect, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                false, 1f, COLOR_SECTION_BACKGR, 0f, 0f
            );

            GUI.DrawTexture(
                rect, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                false, 1f, COLOR_SECTION_BORDER, new Vector4(0, 1, 0, 1), 0f
            );

            EditorGUI.LabelField(rect, content, GraphStyles.GetInspectorSection());
        }
        
        // DRAG AND DROP -----------------------------------
        
        [InitializeOnLoadMethod]
        static void DragToHierarchyCheck()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // Adds a callback for when the hierarchy window processes GUI events
                // for every GameObject in the heirarchy.
                if (!hiearchySubscribed)
                {
                    EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
                    hiearchySubscribed = true;
                }
                
                GraphReactions.CheckDirtyReactions();
                GraphReactions.CheckOrphanReactions();
            }
        }

        private static void OnHierarchyGUI(int instanceID, Rect r)
        {
            //Debug.Log("DragAndDrop.activeControlID " + Event.current.type + " / " + r.Contains(Event.current.mousePosition));
            if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragExited || Event.current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                //Debug.Log("DragAndDrop.activeControlID " + Event.current.type + " / " + r.Contains(Event.current.mousePosition));
                GameObject gameObject = null;
                if(r.Contains(Event.current.mousePosition)) gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

                Object[] draggedObjects = DragAndDrop.objectReferences;
                if (draggedObjects != null && draggedObjects.Length == 1)
                {
                    if (draggedObjects[0] is StateMachine)
                    {
                        if (Event.current.type == EventType.DragUpdated)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        }

                        if ((Event.current.type == EventType.DragPerform) && r.Contains(Event.current.mousePosition) && gameObject) //Event.current.type == EventType.DragExited || 
                        {
                            StateMachine data = draggedObjects[0] as StateMachine;
                            StateMachineController controller = gameObject.GetComponent<StateMachineController>();
                            if (!controller)
                            {
                                controller = gameObject.AddComponent<StateMachineController>();
                            }
                            controller.stateMachine = data;
                            controller.SyncCollider(false);

                            Selection.activeGameObject = gameObject;

                            //DragAndDrop.objectReferences = null;
                            Event.current.Use();
                        }


                        if (Event.current.type == EventType.MouseUp)
                        {
                            // Clean up, in case MouseDrag never occurred:
                            DragAndDrop.PrepareStartDrag();
                        }
                    }
                }
            }
        }

        // HIERARCHY CONTEXT MENU: ----------------------------------------------------------------

        [MenuItem("GameObject/Game Creator/Other/StateMachine Controller", false, 0)]
        public static void CreateSMController()
        {
            GameObject controller = CreateSceneObject.Create("StateMachine Controller");
            controller.AddComponent<StateMachineController>();
        }
    }
}