using GameCreator.Variables;

namespace NJG.Graph
{
    using System;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using UnityEditorInternal;

    [Serializable]
    public class BlackboardWindow
    {
        private static Color COLOR_SEPARATOR = new Color(0, 0, 0, 0.3f);

        private const float WINDOW_WIDTH = 250f;
        private const float WINDOW_BOTTOM = 46f;
        private const float WNDOW_MARGIN_TOP = 10f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Rect windowRect { private set; get; }

        private AnimBool isOpen;
        private Vector2 scroll;

        private ReorderableList list;
        private bool initialized;
        private bool imported;

        // INITIALIZERS: --------------------------------------------------------------------------

        public BlackboardWindow(GraphEditor window)
        {
            // if (window.stateMachineEditor == null ||
            //     window.stateMachineEditor.target == null)
            // {
            //     return;
            // }

            // Debug.LogWarning("Blackboard initialized: "+initialized);

            Init(window);
        }

        private void Init(GraphEditor window)
        {
            if (!window)
            {
                Debug.LogWarningFormat("There was an error setting up Blackboard {0}", window);
                return;
            }

            if (window.StateMachineEditor == null || window.StateMachineEditor.serializedObject == null)
            {
                initialized = false;
                return;
            }
            
            //if(!window.StateMachineEditor) window.SetupBlackboard();
            /*if(window.StateMachineEditor.target != null && !Application.isPlaying) 
            {
                var path = AssetDatabase.GetAssetPath(window.StateMachineEditor.target);
                if(!string.IsNullOrEmpty(path)) AssetDatabase.ImportAsset(path);
            }*/

            imported = false;
            initialized = true;
            isOpen = new AnimBool(false);
            isOpen.valueChanged.AddListener(window.Repaint);
            isOpen.speed = 3f;

            scroll = Vector2.zero;
            list = new ReorderableList(
                window.StateMachineEditor.serializedObject,
                window.StateMachineEditor.SpBlackboardList,
                true, true, true, true
            );

            list.drawHeaderCallback = DrawHeaderCallback;
            list.drawElementCallback = DrawElementCallback;
            list.elementHeight = EditorGUIUtility.singleLineHeight + 10f;
        }

        // LIST METHODS: --------------------------------------------------------------------------

        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Parameters", EditorStyles.miniLabel);
        }

        private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            SerializedProperty property = list.serializedProperty.GetArrayElementAtIndex(index);

            Rect rectName = new Rect(
                rect.x,
                rect.y + (rect.height / 2.0f - EditorGUIUtility.singleLineHeight / 2.0f),
                rect.width / 2.0f,
                EditorGUIUtility.singleLineHeight
            );

            Rect rectType = new Rect(
                rectName.x + rectName.width + EditorGUIUtility.standardVerticalSpacing,
                rectName.y,
                rect.width / 2.0f - EditorGUIUtility.standardVerticalSpacing,
                rectName.height
            );

            string name = property.FindPropertyRelative("name").stringValue;
            // name = EditorGUI.DelayedTextField(
            name = EditorGUI.TextField(
                rectName,
                GUIContent.none,
                property.FindPropertyRelative("name").stringValue
            );

            if (name != property.FindPropertyRelative("name").stringValue)
            {
                property.FindPropertyRelative("name").stringValue =
                    VariableEditor.ProcessName(name);

                UpdateVariables();
            }

            EditorGUI.PropertyField(rectType, property.FindPropertyRelative("type"), GUIContent.none);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Show(bool show)
        {
            if (isOpen != null)
            {
                isOpen.target = show;
            }
        }

        public bool IsOpen()
        {
            return isOpen?.target ?? false;
        }

        public void Update(GraphEditor window)
        {
            if(!window.StateMachineEditor) return;
            
            if (!initialized || isOpen == null || list == null) Init(window);

            if (isOpen.faded < 0.0001) return;

            windowRect = new Rect(
                (isOpen.faded * WINDOW_WIDTH) - WINDOW_WIDTH,
                EditorStyles.toolbar.fixedHeight + WNDOW_MARGIN_TOP,
                WINDOW_WIDTH,
                window.position.height - (WNDOW_MARGIN_TOP + WINDOW_BOTTOM)
            );

            /*if (Event.current.type != EventType.Layout)
            {
                BehaviorWindowEvents.HOVER_IS_BLACKBOARD = windowRect.Contains(currentEvent.mousePosition);
            }*/

            Color color = (EditorGUIUtility.isProSkin
                    ? GraphStyles.COLOR_BG_PRO
                    : GraphStyles.COLOR_BG_PERSONAL
                );

            // EditorStyles.helpBox.Draw(windowRect, false, true, false, false);
            GUI.DrawTexture(
                windowRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false,
                1.0f, color, 0f, 4f
            );

            GUILayout.BeginArea(windowRect, GraphStyles.GetBlackboard());
            //if(!window.stateMachineEditor) window.SetupBlackboard();
            window.StateMachineEditor.serializedObject.Update();

            PaintHeader();
            
            //EditorGUI.BeginChangeCheck();
            PaintWindow();

            window.StateMachineEditor.serializedObject.ApplyModifiedProperties();
            
            // if (Event.current.type != EventType.Layout)
            {
                UpdateVariables();
            }
            
            window.StateMachineEditor.serializedObject.ApplyModifiedProperties();

            
            GUILayout.EndArea();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PaintHeader()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 4000f, 30f, 30f);

            GUI.DrawTexture(
                new Rect(rect.x, rect.y + rect.height - 1f, rect.width, 1f),
                Texture2D.whiteTexture, ScaleMode.StretchToFill, true,
                1.0f, COLOR_SEPARATOR, 0f, 4f
            );

            GUI.Box(rect, "Blackboard", GraphStyles.GetBlackboardHeader());
        }

        private void PaintWindow()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll, GraphStyles.GetBlackboardBody());
            if (list == null && GraphEditor.Active) GraphEditor.Instance.SetupBlackboard();
            if (list != null)
            {
                list.onChangedCallback = reorderableList =>
                {
                    if(!imported && list.serializedProperty.arraySize > 0)
                    {
                        if (GraphEditor.Instance.StateMachineEditor.target != null)
                        {
                            var path = AssetDatabase.GetAssetPath(GraphEditor.Instance.StateMachineEditor.target);
                            if (!string.IsNullOrEmpty(path))
                            {
                                AssetDatabase.ImportAsset(path);
                                imported = true;
                            }
                        }
                    }
                    UpdateVariables();
                };
                list.DoLayoutList();
                /*if (GraphEditor.Instance && GraphEditor.Reactions &&
                    GraphEditor.Reactions.references != null)
                {
                    GraphEditor.Instance.ReactionsEditor.UpdateLocalVariables();
                }*/
            }

            EditorGUILayout.EndScrollView();
        }

        public static void UpdateVariables()
        {
            if (StateMachineControllerInspector.instance)
            {
                StateMachineControllerInspector.instance.UpdateSubEditors(
                    (StateMachineControllerInspector.instance.target as StateMachineController)?.references);
                StateMachineControllerInspector.instance.SyncBehaviorGraph();
                StateMachineControllerInspector.instance.Repaint();
            }
            
            if (GraphEditor.Instance && GraphEditor.Reactions &&
                GraphEditor.Reactions.references != null)
            {
                EditorUtility.SetDirty(GraphEditor.Reactions);
                EditorUtility.SetDirty(GraphEditor.Active);
                GraphEditor.Instance.ReactionsEditor.UpdateLocalVariables();
                GraphEditor.Instance.ReactionsEditor.ExternalUpdate();
            }
        }
    }
}