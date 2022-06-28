namespace GameCreator.Quests
{
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class QuestConfigWindow : EditorWindow
    {
		private const int WINDOW_W = 500;
		private const int WINDOW_H = 400;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static QuestConfigWindow Instance { get; private set; }

		private IQuestEditor questEditor;
        private Vector2 scroll = Vector2.zero;

        private bool stylesInitialized = false;
        private GUIStyle styleMargins;

        // INITIALIZERS: --------------------------------------------------------------------------

        public static void Open(IQuestEditor questEditor)
        {
            if (QuestConfigWindow.Instance != null) QuestConfigWindow.Instance.Close();

            Rect windowRect = new Rect(0, 0, WINDOW_W, WINDOW_H);
            QuestConfigWindow.Instance = EditorWindow.GetWindowWithRect<QuestConfigWindow>(
                windowRect, true, questEditor.GetInternalName(), true
            );

            QuestConfigWindow.Instance.questEditor = questEditor;
            QuestConfigWindow.Instance.Show();
        }

        public void OnGUI()
        {
            if (this.questEditor == null)
            {
                Close();
                return;
            }

            if (!this.stylesInitialized)
            {
                this.stylesInitialized = true;
                this.styleMargins = new GUIStyle(EditorStyles.inspectorFullWidthMargins);
                this.styleMargins.margin = new RectOffset(10, 10, 10, 0);
            }

            this.scroll = EditorGUILayout.BeginScrollView(
                this.scroll,
				this.styleMargins
            );

			this.questEditor.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();         

			this.Repaint();
        }
    }
}