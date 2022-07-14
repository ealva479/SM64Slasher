using System.Linq;
using GameCreator.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace NJG.Graph
{
    public class TransitionInspector
    {
        private Editor host;
        private Node node;
        private ReorderableList transitionList;
        private static Transition copy;
        private IConditionsListEditor conditionsEditor;
        private int lastCount;
        private bool useConditions;
        private const string ICONS_PATH = "Assets/Plugins/GameCreator/Extra/Icons/Trigger/{0}";

        public TransitionInspector(Node node, Editor host)
        {
            this.host = host;
            this.node = node;
            
        }

        public void OnEnable()
        {
            ResetTransitionList();
            if (node == null) return;

            int index = node.transitions.ToList().FindIndex(x => x == GraphEditor.SelectedTransition);
            if (index != transitionList.index && index > -1)
            {
                transitionList.GrabKeyboardFocus();
                transitionList.index = index;
                ResetConditionList();
            }
        }

        public void OnInspectorGUI()
        {
            int index = node.transitions.ToList().FindIndex(x => x == GraphEditor.SelectedTransition);

            if (index != transitionList.index && index != -1)
            {
                transitionList.GrabKeyboardFocus();
                transitionList.index = index;
                ResetConditionList();
            }
            transitionList.DoLayoutList();
            GUILayout.Space(10f);
            if (transitionList.index != -1 && node.transitions.Length > 0 && transitionList.index < node.transitions.Length)
            {
                Transition transition = node.transitions[transitionList.index];
                if (!transition || !transition.fromNode) return;
                EditorGUILayout.BeginHorizontal(GUILayout.Height(44)); //"IN GameObjectHeader", 

                GUILayout.Label(AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(ICONS_PATH, "conditions.png")), GUILayout.Width(32), GUILayout.Height(32));

                EditorGUILayout.BeginVertical();

                transition.name = EditorGUILayout.TextField(string.Empty, transition.name);
                EditorGUILayout.LabelField(transition.fromNode.name + " -> " + transition.toNode.name);

                EditorGUILayout.EndVertical(); 
                EditorGUILayout.EndHorizontal();

                //EditorGUILayout.Space();
                //EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                useConditions = EditorGUILayout.ToggleLeft(" Use Conditions", transition.useConditions);
                EditorGUI.BeginDisabledGroup(!useConditions);
                //EditorGUI.indentLevel++;
                transition.isNegative = EditorGUILayout.ToggleLeft(" Invert", transition.isNegative);
                //EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
                if(EditorGUI.EndChangeCheck() && useConditions != transition.useConditions)
                {
                    transition.useConditions = useConditions;
                    if (useConditions)
                    {
                        conditionsEditor = (IConditionsListEditor)Editor.CreateEditor(GraphEditor.Reactions.GetCondition<IConditionsList>(transition));
                        ResetConditionList();
                        if (GUI.changed) EditorUtility.SetDirty(transition);
                        if (GUI.changed) EditorUtility.SetDirty(node);
                        host.Repaint();
                    }
                    else if (GraphEditor.Reactions.HasConditions(transition))
                    {
                        //Debug.LogWarning("Removing conditions...");
                        GraphEditor.Reactions.RemoveCondition(transition);
                    }
                }
                //EditorGUI.indentLevel--;

                if (transition.useConditions)
                {
                    EditorGUILayout.Space();
                    
                    if(!conditionsEditor)
                    {
                        conditionsEditor =
                            (IConditionsListEditor) Editor.CreateEditor(
                                GraphEditor.Reactions.GetCondition<IConditionsList>(transition));
                        ResetConditionList(); 
                        if (GUI.changed) EditorUtility.SetDirty(transition);
                        if (GUI.changed) EditorUtility.SetDirty(node);
                        host.Repaint();
                    }
                    
                    if (GraphEditor.Reactions && GraphEditor.Reactions.GetCondition<IConditionsList>(transition).conditions.Length == 0)
                    {
                        var c = GraphEditor.Reactions.GetCondition<IConditionsList>(transition);
                        if (c == null || c.conditions == null || c.conditions.Length == 0)
                        {
                            EditorGUILayout.HelpBox("This transition has no conditions.", MessageType.Info);
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                      
                    conditionsEditor.OnInspectorGUI();

                    IConditionsList conditions = conditionsEditor.target as IConditionsList;

                    bool hasChanged = EditorGUI.EndChangeCheck() || lastCount != conditions.conditions.Length;
                    bool isInstance = GraphEditor.Controller != null && GraphEditor.Controller.stateMachine && GraphEditor.Controller.reactions;

                    if (hasChanged)
                    {
                        //Debug.Log("Saving Conditions Changes");
                        lastCount = conditions.conditions.Length;
                        GraphUtility.ReplaceLocalVarTargets(conditions.conditions);

                        if (isInstance)
                        {
                            GraphUtility.CopyIConditionList(GraphEditor.Controller.reactions.GetCondition<IConditionsList>(transition),
                                GraphEditor.Controller.stateMachine.sourceReactions.GetCondition<IConditionsList>(transition));
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Conditions disabled.", MessageType.Info);
                }
            }

            Event ev = Event.current;
            if (ev.rawType == EventType.KeyDown && ev.keyCode == KeyCode.Delete && GraphEditor.SelectedTransition != null &&
                EditorUtility.DisplayDialog("Delete selected transition?", GraphEditor.SelectedTransition.fromNode.name + " -> " + GraphEditor.SelectedTransition.toNode.name + "\r\n\r\nYou cannot undo this action.", "Delete", "Cancel"))
            {
                var transitions = node.transitions;
                ArrayUtility.Remove(ref transitions, GraphEditor.SelectedTransition);
                node.transitions = transitions;

                GraphEditor.Reactions.RemoveCondition(GraphEditor.SelectedTransition);
                GraphUtility.DestroyImmediate(GraphEditor.SelectedTransition);

                if (GUI.changed) EditorUtility.SetDirty(node);
            }
        }

        public void ResetTransitionList()
        {
            if (node == null) return;
            
            SerializedObject obj = new SerializedObject(node);
            SerializedProperty elements = obj.FindProperty("transitions");
            transitionList = new ReorderableList(obj, elements, true, true, false, true);
            transitionList.drawHeaderCallback = delegate (Rect rect) {
                EditorGUI.LabelField(rect, "Transitions");
                EditorGUI.LabelField(new Rect(rect.width - 20, rect.y, 50, 20), "Mute");
            };
            transitionList.onSelectCallback = list =>
            {
                if (node.transitions.Length > 0)
                {
                    if (list.index < node.transitions.Length && node.transitions[list.index])
                    {
                        GraphEditor.SelectTransition(node.transitions[list.index]);
                    }
                    ResetConditionList();
                }
            };

            transitionList.onRemoveCallback = list =>
            {
                Transition transition = node.transitions[list.index];
                list.index = Mathf.Clamp(list.index - 1, 0, list.count - 1);

                DeleteTransition(node, transition);

                if (conditionsEditor)
                {
                    IConditionsList conditions = conditionsEditor.target as IConditionsList;
                    lastCount = conditions.conditions.Length;

                    conditionsEditor = null;
                }

                ResetTransitionList();
                ResetConditionList();
                host.Repaint();
                if (GraphEditor.Instance != null)
                    GraphEditor.Instance.Repaint();
            };
            transitionList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (index >= node.transitions.Length) return;
                
                Transition transition = node.transitions[index];
                if (transition && transition.fromNode)
                {
                    if (Event.current.rawType == EventType.MouseDown && rect.Contains(Event.current.mousePosition) && Event.current.button == 1)
                    {
                        GenericMenu menu = new GenericMenu();
                        if (transition.useConditions)
                        {
                            menu.AddItem(new GUIContent("Copy Conditions"), false, delegate
                            {
                                Pasteboard.CopyConditions(transition, GraphEditor.Reactions);
                            });
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent("Copy Conditions"));
                        }
                        if (Pasteboard.copyTransition && Pasteboard.copyTransition != transition)
                        {
                            menu.AddItem(new GUIContent("Paste Conditions"), false, delegate
                            {
                                if (transition.useConditions &&
                                    transition.GetConditions(GraphEditor.Reactions).conditions.Length > 0)
                                {
                                    bool mOverride = EditorUtility.DisplayDialog("Override Conditions", "This transition already contains conditions. Do you want to override it?", "Yes", "No");
                                    if(mOverride)
                                    {
                                        Pasteboard.PasteConditions(transition, GraphEditor.Reactions);
                                        Reset(); 
                                    }
                                    else Pasteboard.ClearConditions();
                                }
                                else
                                {
                                    Pasteboard.PasteConditions(transition, GraphEditor.Reactions);
                                    if (conditionsEditor)
                                    {
                                        IConditionsList conditions = conditionsEditor.target as IConditionsList;
                                        lastCount = conditions.conditions.Length;

                                        conditionsEditor = null;
                                    }
                                    Reset();
                                }
                            });
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent("Paste Conditions"));
                        }
                        menu.AddItem(new GUIContent("Remove Transition"), false, delegate
                        {
                            DeleteTransition(node, transition);
                            
                            if (conditionsEditor)
                            {
                                IConditionsList conditions = conditionsEditor.target as IConditionsList;
                                lastCount = conditions.conditions.Length; 

                                conditionsEditor = null;
                            }

                            Reset();
                        });
                        menu.ShowAsContext();
                    }

                    rect.y += 2;

                    EditorGUI.LabelField(
                        new Rect(rect.x, rect.y, rect.width - 25, EditorGUIUtility.singleLineHeight),
                    !string.IsNullOrEmpty(transition.name) ? transition.name : transition.fromNode.name + " -> " + transition.toNode.name);

                    transition.mute = EditorGUI.Toggle(
                        new Rect(rect.x + rect.width - 25, rect.y - 2, 25, EditorGUIUtility.singleLineHeight),
                        transition.mute);
                }
            };

            transitionList.onReorderCallback = list =>
            {
                GraphEditor.SelectTransition(node.transitions[list.index]);
                ResetConditionList();
            };

            int index2 = node.transitions.ToList().FindIndex(x => x == GraphEditor.SelectedTransition);
            if (index2 != transitionList.index && index2 != -1)
            {
                transitionList.GrabKeyboardFocus();
                transitionList.index = index2;
            }

            ResetConditionList();
            host.Repaint();
            if (GraphEditor.Instance != null)
                GraphEditor.Instance.Repaint();
        }

        private void Reset()
        {
            ResetTransitionList();
            ResetConditionList();
            host.Repaint();
                            
            if (GraphEditor.Instance != null)
                GraphEditor.Instance.Repaint();
        }

        public static void DeleteTransition(Node node, Transition transition)
        {
            var transitions = node.transitions;
            ArrayUtility.Remove(ref transitions, transition);
            node.transitions = transitions;

            foreach (var c in transition.GetConditions(node.Root.sourceReactions).conditions)
            {
                if (c == null) continue;
                c.hideFlags = 0;
                UnityEngine.Object.DestroyImmediate(c, true);
            }

            if (Application.isPlaying)
            {
                if (transition.GetConditions(GraphEditor.Controller.reactions))
                {
                    foreach (var c in transition.GetConditions(GraphEditor.Controller.reactions).conditions)
                    {
                        if (c == null) continue;
                        c.hideFlags = 0;
                        UnityEngine.Object.Destroy(c);
                    }
                }
            }

            GraphUtility.DestroyImmediate(transition);
        }

        private void ResetConditionList()
        {
            if (node.transitions.Length == 0 || transitionList.index < 0)
            {
                return;
            }

            var tr = node.transitions[transitionList.index];
            if (tr && tr.useConditions && GraphEditor.Reactions.HasConditions(tr))
            {
                 conditionsEditor = (IConditionsListEditor)Editor.CreateEditor(GraphEditor.Reactions.GetCondition<IConditionsList>(tr));
            }
            /*if (!tr)
            {
                var transitions = node.transitions;
                ArrayUtility.RemoveAt(ref transitions, transitionList.index);
                node.transitions = transitions;
            }*/

            if (conditionsEditor != null)
            {
                IConditionsList conditions = conditionsEditor.target as IConditionsList;
                lastCount = conditions.conditions.Length;
                GraphUtility.ReplaceLocalVarTargets(conditions.conditions);
            }

            host.Repaint();
            if (GraphEditor.Instance != null)
                GraphEditor.Instance.Repaint();

        }

    }
}