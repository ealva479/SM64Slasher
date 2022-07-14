namespace NJG.Graph
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;
    using GameCreator.Variables;

    [CustomEditor(typeof(GraphReactions))]
    public class GraphReactionsEditor : MultiSubEditor<MBVariableEditor, MBVariable>
    {
        private const string VAR_INVOKER = "invoker";
        private const string REFERENCES = "references";

        private const string MSG_REPEATED_VARIABLE = "There are two or more variables with the same name";
        private static GUIContent GUI_BUTTON = new GUIContent("Open Editor Graph");

        private static readonly GUIContent GC_PARAMS = new GUIContent("Parameters");

        private const string VAR_RUNTIME = "{0} (runtime)";

        private static Color COLOR_SECTION_BACKGR = new Color(0, 0, 0, 0.2f);
        private static Color COLOR_SECTION_BORDER = new Color(0, 0, 0, 0.4f);

        // PROPERTIES: ----------------------------------------------------------------------------

        private GraphReactions reactions;

        private SerializedProperty spStateMachine;
        private SerializedProperty spComponents;
        private SerializedProperty spReferences;
        private bool invokerChecked;

        // INITIALIZERS: --------------------------------------------------------------------------

        public void OnEnable()
        {
            if (target == null) return;
            reactions = (GraphReactions)target;

            spStateMachine = serializedObject.FindProperty("stateMachine");
            spComponents = serializedObject.FindProperty("components");
            spReferences = serializedObject.FindProperty("references");
            
            UpdateSubEditors(reactions.references);
        }

        public void CheckInvoker()
        {
            if (Application.isPlaying) return;
            if (reactions.invokerVariable) return;
            invokerChecked = true;

            //Debug.Log("CheckInvoker " + this.reactions.invokerVariable + " invokerChecked "+ invokerChecked);

            GameObject gameObject;
            MBVariable variable = (gameObject = reactions.gameObject).AddComponent<MBVariable>();
            variable.variable.name = VAR_INVOKER;
            variable.variable.Set(Variable.DataType.GameObject, gameObject);

            //Debug.Log("variable " + variable);
            spReferences.AddToObjectArray<MBVariable>(variable);
            //this.AddSubEditorElement(variable, -1, true);

            reactions.invokerVariable = variable;

            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable()
        {
            CleanSubEditors();
        }

        // INSPECTOR PAINT METHOD: ----------------------------------------------------------------

        public override bool UseDefaultMargins()
        {
            return false;
        }

        public override void OnInspectorGUI()
        {
            if (target == null) return;
            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            //this.PaintBehaviorGraph();
            EditorGUI.BeginDisabledGroup(spStateMachine.objectReferenceValue == null);
            if (GUILayout.Button(GUI_BUTTON))
            {
                GraphEditor.ShowWindow();
                GraphEditor.SelectStateMachine(spStateMachine.objectReferenceValue as StateMachine);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(spComponents);

            PaintSection(GC_PARAMS);

            UpdateSubEditors(reactions.references);

            if (!invokerChecked && !Application.isPlaying)
            {
                CheckInvoker();
            }

            if (!Application.isPlaying) SyncBehaviorGraph();
            PaintLocalVariables();

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        public void ExternalUpdate()
        {
            if (target == null) return;
            serializedObject.Update();
            UpdateSubEditors(reactions.references);
            if (!invokerChecked && !Application.isPlaying)
            {
                CheckInvoker();
            }

            if (!Application.isPlaying) SyncBehaviorGraph();
            // PaintLocalVariables();
            serializedObject.ApplyModifiedProperties();
        }

        // PAINT METHODS: -------------------------------------------------------------------------

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
        
        public void UpdateLocalVariables()
        {
            if (spStateMachine == null) return;
            
            if (spStateMachine.objectReferenceValue == null)
            {
                return;
            }

            int referencesLength = spReferences.arraySize;
            if (referencesLength == 0)
            {
                EditorGUILayout.HelpBox("No Parameters", MessageType.Info);
                return;
            }

            // EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

            HashSet<string> variables = new HashSet<string>();
            bool repeatedVariableName = false;

            for (int i = 0; i < referencesLength; ++i)
            {
                if (i < subEditors.Length && subEditors[i] != null)
                {
                    string variableName = subEditors[i].spVariableName.stringValue;
                    if (variables.Contains(variableName)) repeatedVariableName = true;
                    else variables.Add(variableName);
                    
                    if(!subEditors[i]) continue;
                    if(!subEditors[i].serializedObject.targetObject) continue;

                    subEditors[i].serializedObject.Update();

                    /*if (EditorApplication.isPlaying)
                    {
                        PaintVariableRuntime(subEditors[i]);
                    }
                    else
                    {
                        PaintVariableEditor(subEditors[i]);
                    }*/

                    subEditors[i].serializedObject.ApplyModifiedProperties();
                }
            }

            if (repeatedVariableName)
            {
                EditorGUILayout.HelpBox(MSG_REPEATED_VARIABLE, MessageType.Warning);
            }

            // EditorGUILayout.EndVertical();
        }

        public void PaintLocalVariables()
        {
            if (spStateMachine.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("No StateMachine found", MessageType.Warning);
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
                if (i < subEditors.Length && subEditors[i] != null)
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

                    subEditors[i].serializedObject.ApplyModifiedProperties();
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
            if(!editor) return;
            
            string label = string.Format(
                VAR_RUNTIME,
                editor.spVariableName.stringValue
            );

            var mvar = editor.GetRuntimeVariable();
            if(mvar != null)
            {
                object variable = mvar.Get();

                Rect rect = GUILayoutUtility.GetRect(
                    EditorGUIUtility.fieldWidth + EditorGUIUtility.fieldWidth,
                    EditorGUIUtility.singleLineHeight
                );

                EditorGUI.LabelField(
                    rect,
                    label,
                    variable == null ? "(null)" : variable.ToString()
                );
            }
        }

        private void PaintVariableEditor(MBVariableEditor editor)
        {
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

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PaintProperty(SerializedProperty property, GUIContent label)
        {
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

            EditorGUI.PrefixLabel(rectLabel, label);
            EditorGUI.PropertyField(rectField, property, GUIContent.none);
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        public void SyncBehaviorGraph(StateMachine sm = null)
        {
            if (Application.isPlaying) return;
            
            if (spStateMachine == null && !sm)
            {
                //Debug.Log("[objectReferenceValue] Cannot update GraphReactions state machine. "+spStateMachine.objectReferenceValue);
                return;
            }
            
            if (spStateMachine.objectReferenceValue == null && !sm)
            {
                //Debug.Log("[objectReferenceValue] Cannot update GraphReactions state machine. "+spStateMachine.objectReferenceValue);
                return;
            }
            StateMachine stateMachine = spStateMachine == null && sm ? sm : (StateMachine)spStateMachine.objectReferenceValue;
            
            if (!stateMachine)
            {
                //Debug.Log("Cannot sync GraphReactions state machine not found.");
                return;
            }
            
            spReferences.serializedObject.Update();

            int referencesSize = spReferences.arraySize;
            List<Blackboard.Item> addList = stateMachine.GetBlackboardItems();
            List<int> removeList = new List<int>();

            for (int i = 0; i < referencesSize; ++i)
            {
                if (i >= subEditors.Length || !subEditors[i]) continue;

                string refName = subEditors[i].spVariableName.stringValue;
                int refType = subEditors[i].spVariableType.intValue;
                if(refName == VAR_INVOKER) continue;

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
                MBVariable source = (MBVariable) spReferences
                    .GetArrayElementAtIndex(removeList[i])
                    .objectReferenceValue;

                if (source == reactions.invokerVariable) continue;
                if(i < spReferences.arraySize - 1)
                {
                    spReferences.RemoveFromObjectArrayAt(removeList[i]);
                }
                RemoveSubEditorsElement(removeList[i]);

                DestroyImmediate(source, true);

            }
            
            int addListCount = addList.Count;
            for (int i = 0; i < addListCount; ++i)
            {
                name = addList[i].name;
                if (string.IsNullOrEmpty(name)) continue;

                MBVariable variable = reactions.gameObject.AddComponent<MBVariable>();
                variable.variable.name = addList[i].name;
                variable.variable.type = (int)addList[i].type;
                variable.variable.save = false;
                
                spReferences.AddToObjectArray<MBVariable>(variable);
                AddSubEditorElement(variable, -1, true);
            }
            
            spReferences.serializedObject.ApplyModifiedProperties();
            spReferences.serializedObject.Update();
        }
        
        /*public static void RemoveFromObjectArrayAt (SerializedProperty spArray, int index)
        {
            // if(index < 0) throw new UnityException(string.Format(ERR_NEG_ARRAY, spArray.name));
            // if (!spArray.isArray) throw new UnityException(string.Format(ERR_NOT_ARRAY, spArray.name));
            // if(index > spArray.arraySize - 1) 
            // {
            //     throw new UnityException(string.Format(ERR_OUT_BOUND, spArray.name, index, spArray.arraySize));
            // }

            try
            {
                if (spArray.GetArrayElementAtIndex(index).objectReferenceValue) spArray.DeleteArrayElementAtIndex(index);

                spArray.serializedObject.ApplyModifiedProperties();
                spArray.serializedObject.Update();
            }
            catch
            {
                //
            }
        }*/

        /*public static void AddToObjectArray<T> (SerializedProperty spArray, T element) where T : Object
        {
            // spArray.serializedObject.Update();
            // spArray.serializedObject.ApplyModifiedProperties();
            
            spArray.InsertArrayElementAtIndex(spArray.arraySize);
            spArray.GetArrayElementAtIndex(spArray.arraySize - 1).objectReferenceValue = element;
        
            spArray.serializedObject.ApplyModifiedProperties();
            spArray.serializedObject.Update();
        }*/
    }
}