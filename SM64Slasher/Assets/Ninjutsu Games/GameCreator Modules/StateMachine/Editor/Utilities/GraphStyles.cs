using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
#if SM_RPG
using NJG.RPG;
#endif

namespace NJG.Graph
{
    public static class GraphStyles
    {
        public const float StateWidth = 180f;
        public const float StateHeight = 32f;
        public const float StateMachineWidth = 230f;
        public const float StateMachineHeight = 35f;

        public static readonly Color COLOR_BG_PRO = new Color(0.22f, 0.22f, 0.22f);
        public static readonly Color COLOR_BG_PERSONAL = new Color(0.76f, 0.76f, 0.76f);

        public static GUIStyle selectionRect;
        public static GUIStyle breadcrumbLeft;
        public static GUIStyle breadcrumbLeftBg;
        public static GUIStyle breadcrumbMiddle;
        public static GUIStyle breadcrumbLeftFocused;
        public static GUIStyle breadcrumbMiddleFocused;
        public static GUIStyle WrappedLabel;
        public static GUIStyle DescriptionBox;
        public static GUIStyle CommentBox;
        public static GUIStyle DescriptionLabel;
        public static GUIStyle DescriptionTextArea;
        public static GUIStyle miniLabelRight;
        public static GUIStyle instructionLabel;
        public static GUIStyle bottomToolbar;

        private static GUIStyle BLACKBOARD = null;
        private static GUIStyle BLACKBOARD_HEAD = null;
        private static GUIStyle BLACKBOARD_BODY = null;

        private static GUIStyle SECTION = null;

        public static Color CommentBoxColor;
        public static Color gridMinorColor;
        public static Color gridMajorColor;
        public static Color ConnectionColor;
        public static Color ConditionColor;

        public static int StateMachineColor;
        public static int startNodeColor;
        public static int anyStateColor;
        public static int defaultNodeColor;
        public static int TriggerNodeColor;
        public static int EntryNodeColor;
        public static int ExitNodeColor;

        static GraphStyles()
        {
            nodeStyleCache = new Dictionary<string, GUIStyle>();
            gridMinorColor = EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.18f) : new Color(0f, 0f, 0f, 0.1f);
            gridMajorColor = EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.28f) : new Color(0f, 0f, 0f, 0.15f);
            ConnectionColor = new Color(107f / 255f, 178f / 255f, 1, 1);
            ConditionColor = new Color(220f / 255f, 119f / 255f, 52f / 255f);

            Color c = Color.white;
            // c.a = 0.7f;z
            CommentBoxColor = c;
            
            selectionRect = "SelectionRect";
            breadcrumbLeft = "GUIEditor.BreadcrumbLeft";
            breadcrumbLeftBg = "GUIEditor.BreadcrumbLeftBackground";
            breadcrumbMiddle = "GUIEditor.BreadcrumbMid";

            breadcrumbLeft.alignment = TextAnchor.MiddleCenter;

            breadcrumbLeftFocused = new GUIStyle("GUIEditor.BreadcrumbLeftBackground");
            breadcrumbLeftFocused.normal = breadcrumbLeftFocused.onNormal;
            breadcrumbLeftFocused.focused = breadcrumbLeftFocused.onFocused;
            breadcrumbLeftFocused.hover = breadcrumbLeftFocused.onHover;
            breadcrumbLeftFocused.active = breadcrumbLeftFocused.onActive;

            breadcrumbMiddleFocused = new GUIStyle("GUIEditor.BreadcrumbMid");
            // breadcrumbMiddleFocused.normal = breadcrumbMiddleFocused.onNormal;
            // breadcrumbMiddleFocused.focused = breadcrumbMiddleFocused.onFocused;
            // breadcrumbMiddleFocused.hover = breadcrumbMiddleFocused.onHover;
            // breadcrumbMiddleFocused.active = breadcrumbMiddleFocused.onActive;

            bottomToolbar = new GUIStyle("OL Title TextRight");

            WrappedLabel = new GUIStyle("label")
            {
                fixedHeight = 0,
                wordWrap = true
            };
            miniLabelRight = new GUIStyle("RightLabel")
            {
                padding = new RectOffset(0, 0, 3, 0),
                fixedHeight = 0,
                wordWrap = true,
                fontSize = 9,
                alignment = TextAnchor.MiddleRight
            };
            instructionLabel = new GUIStyle("LargeLabel") //TL Selection H3
            {
                padding = new RectOffset(3, 3, 3, 3),
                contentOffset = WrappedLabel.contentOffset,
                alignment = TextAnchor.UpperLeft,
                fixedHeight = 0,
                wordWrap = true,
                fontSize = 15,
                fontStyle = FontStyle.Normal
            };
            instructionLabel.normal.textColor = Color.white;

            DescriptionLabel = new GUIStyle("wordWrappedLabel")
            {
                // wordWrap = true,
                // font = WrappedLabel.font,
                fontSize = 11,
                // fontStyle = WrappedLabel.fontStyle,
                // contentOffset = WrappedLabel.contentOffset,
                // stretchHeight = false,
                // padding = new RectOffset(10, 10, 10, 10),
            };
            DescriptionLabel.richText = true;

            c.a = 0.7f;
            DescriptionLabel.normal.textColor = c;

            DescriptionBox = new GUIStyle("ShurikenEffectBg")//TE NodeBackground
            {
                wordWrap = true,
                font = WrappedLabel.font,
                fontSize = 10,
                fontStyle = WrappedLabel.fontStyle,
                contentOffset = WrappedLabel.contentOffset,
                stretchHeight = false,
                padding = new RectOffset(10, 10, 10, 10),
            };
            
            CommentBox = new GUIStyle("ShurikenEffectBg")
            {
                wordWrap = true,
                font = WrappedLabel.font,
                fontSize = 10,
                fontStyle = WrappedLabel.fontStyle,
                contentOffset = WrappedLabel.contentOffset,
                stretchHeight = false,
                padding = new RectOffset(10, 10, 0, 5),
            };
            CommentBox.onNormal.textColor = c;
            CommentBox.normal.textColor = c;

            DescriptionTextArea = new GUIStyle(EditorStyles.textArea);
            DescriptionTextArea.wordWrap = true;

            StateMachineColor = (int)UnityEditor.Graphs.Styles.Color.Grey;
            startNodeColor = (int)UnityEditor.Graphs.Styles.Color.Orange;
            anyStateColor = (int)UnityEditor.Graphs.Styles.Color.Aqua;
            defaultNodeColor = (int)UnityEditor.Graphs.Styles.Color.Grey;
            TriggerNodeColor = (int)UnityEditor.Graphs.Styles.Color.Aqua;
            EntryNodeColor = (int)UnityEditor.Graphs.Styles.Color.Green;
            ExitNodeColor = (int)UnityEditor.Graphs.Styles.Color.Red;
        }

        private static Dictionary<string, GUIStyle> nodeStyleCache;

        private static string[] styleCache =
        {
            "flow node 0",
            "flow node 1",
            "flow node 2",
            "flow node 3",
            "flow node 4",
            "flow node 5",
            "flow node 6"
        };

        private static string[] styleCacheHex =
        {
            "flow node hex 0",
            "flow node hex 1",
            "flow node hex 2",
            "flow node hex 3",
            "flow node hex 4",
            "flow node hex 5",
            "flow node hex 6"
        };

        public static GUIStyle GetNodeStyle(Node node, bool isActive, float offset)
        {
            bool hex = node is StateMachine || node is TriggerState || node is UpState;
            string styleName = hex ? styleCacheHex[node.color] : styleCache[node.color];
            if (EditorApplication.isPlaying)
            {
                if (node is ActionsState && GraphEditor.InProgress(node))
                {
                    styleName = styleCache[(int)UnityEditor.Graphs.Styles.Color.Blue];
                }
            }

            string newStyle = isActive ? string.Concat(styleName, " on") : styleName;
#if SM_RPG
            if (node is SkillStateMachine && isActive) newStyle = styleCacheHex[1];
#endif
            if (!nodeStyleCache.ContainsKey(newStyle))
            {
                GUIStyle style = new GUIStyle(newStyle);
                style.contentOffset = new Vector2(0, style.contentOffset.y - offset);
                nodeStyleCache[newStyle] = style;
            }
            return nodeStyleCache[newStyle];
        }

        public static GUIStyle GetBlackboard()
        {
            if (BLACKBOARD == null)
            {
                BLACKBOARD = new GUIStyle();
            }

            return BLACKBOARD;
        }

        public static GUIStyle GetBlackboardHeader()
        {
            if (BLACKBOARD_HEAD == null)
            {
                BLACKBOARD_HEAD = new GUIStyle(EditorStyles.label);
                BLACKBOARD_HEAD.alignment = TextAnchor.MiddleCenter;
                BLACKBOARD_HEAD.fontStyle = FontStyle.Bold;

                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, new Color(256, 256, 256, 0.1f));
                texture.Apply();

                BLACKBOARD_HEAD.normal.background = texture;
            }

            return BLACKBOARD_HEAD;
        }

        public static GUIStyle GetBlackboardBody()
        {
            if (BLACKBOARD_BODY == null)
            {
                BLACKBOARD_BODY = new GUIStyle();
                BLACKBOARD_BODY.padding = new RectOffset(4, 4, 4, 4);
            }

            return BLACKBOARD_BODY;
        }

        public static GUIStyle GetInspectorSection()
        {
            if (SECTION == null)
            {
                SECTION = new GUIStyle(EditorStyles.boldLabel);
                SECTION.alignment = TextAnchor.MiddleLeft;
                SECTION.padding = new RectOffset(16, 4, 4, 4);
            }

            return SECTION;
        }
    }
}