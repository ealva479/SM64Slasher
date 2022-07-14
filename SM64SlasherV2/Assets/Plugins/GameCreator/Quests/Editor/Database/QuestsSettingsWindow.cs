namespace GameCreator.Quests
{
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

	public class QuestsSettingsWindow : PopupWindowContent
    {
		private const string TITLE = "Quest Settings";

		private const int WINDOW_W = 300;
		private const int WINDOW_H = 80;

		// PUBLIC METHODS: ------------------------------------------------------------------------

		private SerializedObject serializedObject;
		private SerializedProperty spJournalSkin;
        private SerializedProperty spQuestsHUDSkin;

		// INITIALIZERS: --------------------------------------------------------------------------

		public QuestsSettingsWindow(SerializedProperty spSettings) : base()
		{
			this.serializedObject = spSettings.serializedObject;
			this.spJournalSkin = spSettings.FindPropertyRelative("journalSkin");
            this.spQuestsHUDSkin = spSettings.FindPropertyRelative("questsHUDSkin");
		}

        public override Vector2 GetWindowSize()
        {
			return new Vector2(WINDOW_W, WINDOW_H);
        }

        public override void OnGUI(Rect rect)
        {
			this.serializedObject.Update();

			EditorGUILayout.BeginVertical(EditorStyles.toolbar);
			EditorGUILayout.LabelField(TITLE, EditorStyles.centeredGreyMiniLabel);         
            EditorGUILayout.EndVertical();
            
			EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(this.spJournalSkin);
            EditorGUILayout.PropertyField(this.spQuestsHUDSkin);

            EditorGUILayout.Space();
			EditorGUILayout.EndVertical();

			this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
			this.editorWindow.Repaint();
        }
    }
}