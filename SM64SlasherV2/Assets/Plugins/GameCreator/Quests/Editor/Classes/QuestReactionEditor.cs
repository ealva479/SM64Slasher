namespace GameCreator.Quests
{
	using UnityEngine;
	using UnityEditor;
	using GameCreator.Core;

	[CustomEditor(typeof(QuestReaction))]
	public class QuestReactionEditor : Editor
	{
		private static readonly string[] OPTIONS = new string[]
		{
            "Conditions",
            "On Complete",
            "On Fail"
		};

		private const string MSG_CONDIT = "These Conditions must check before activating the Quest/Task";
		private const string MSG_ONCOMP = "These Actions will start after the Quest/Task is complete";
		private const string MSG_ONFAIL = "These Actions will start after the Quest/Task is failed or abandoned";

		// PROPERTIES: ----------------------------------------------------------------------------

		private QuestReaction reaction;

		private IConditionsListEditor editorConditions;
		private IActionsListEditor editorOnComplete;
		private IActionsListEditor editorOnFail;

		private int optionsIndex = 0;

		// INITIALIZERS: --------------------------------------------------------------------------

		private void OnEnable()
		{
            if (target == null || serializedObject == null) return;
			this.reaction = (QuestReaction)this.target;
		}

		// PAINT METHODS: -------------------------------------------------------------------------

		public override void OnInspectorGUI()
		{
			this.optionsIndex = GUILayout.Toolbar(this.optionsIndex, OPTIONS);
			EditorGUILayout.Space();

			switch (this.optionsIndex)
			{
				case 0 : this.PaintConditions(); break;
				case 1 : this.PaintOnComplete(); break;
				case 2 : this.PaintOnFail(); break;
			}

			EditorGUILayout.Space();
		}

        public void PaintConditions()
		{
			if (this.editorConditions == null)
			{
				if (this.reaction.conditions == null)
                {
                    SerializedProperty spConditions = this.serializedObject.FindProperty("conditions");
                    spConditions.objectReferenceValue = this.reaction.gameObject.AddComponent<IConditionsList>();
					serializedObject.ApplyModifiedPropertiesWithoutUndo();
					serializedObject.Update();
                }

				this.editorConditions = (IConditionsListEditor)Editor.CreateEditor(this.reaction.conditions);
			}


			EditorGUILayout.HelpBox(MSG_CONDIT, MessageType.Info);
			this.editorConditions.OnInspectorGUI();
		}

		public void PaintOnComplete()
		{
			if (this.editorOnComplete == null)
			{
				if (this.reaction.onComplete == null)
                {
                    SerializedProperty spOnComplete = this.serializedObject.FindProperty("onComplete");
                    spOnComplete.objectReferenceValue = this.reaction.gameObject.AddComponent<IActionsList>();
					serializedObject.ApplyModifiedPropertiesWithoutUndo();
					serializedObject.Update();
                }

				this.editorOnComplete = (IActionsListEditor)Editor.CreateEditor(this.reaction.onComplete);
			}

			EditorGUILayout.HelpBox(MSG_ONCOMP, MessageType.Info);
			this.editorOnComplete.OnInspectorGUI();
		}

		public void PaintOnFail()
		{
			if (this.editorOnFail == null)
            {
				if (this.reaction.onFail == null)
                {
					SerializedProperty spOnFail = this.serializedObject.FindProperty("onFail");
                    spOnFail.objectReferenceValue = this.reaction.gameObject.AddComponent<IActionsList>();
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    serializedObject.Update();
                }

				this.editorOnFail = (IActionsListEditor)Editor.CreateEditor(this.reaction.onFail);
            }

			EditorGUILayout.HelpBox(MSG_ONFAIL, MessageType.Info);
			this.editorOnFail.OnInspectorGUI();
		}
	}
}