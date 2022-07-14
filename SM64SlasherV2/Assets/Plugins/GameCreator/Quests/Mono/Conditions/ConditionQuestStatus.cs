namespace GameCreator.Quests
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
	public class ConditionQuestStatus : ICondition
	{
		[IQuest] public IQuest quest;
        public IQuest.Status status = IQuest.Status.Active;

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
			if (this.quest == null) return false;
			IQuest questInstance = QuestsManager.Instance.GetQuest(this.quest.name);
			if (questInstance == null) return false;

			return (questInstance.status == this.status);
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

		public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Quests/Icons/Conditions/";

		public static new string NAME = "Quests/Quest Status";
		private const string NODE_TITLE = "Quest {0} is {1}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spQuest;
        private SerializedProperty spStatus;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
			return string.Format(
                NODE_TITLE,
                this.quest == null ? "none" : this.quest.ToString(),
                this.status.ToString()
            );
		}

		protected override void OnEnableEditorChild ()
		{
			this.spQuest = this.serializedObject.FindProperty("quest");
            this.spStatus = this.serializedObject.FindProperty("status");
		}

		protected override void OnDisableEditorChild ()
		{
			this.spQuest = null;
			this.spStatus = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spQuest);
            EditorGUILayout.PropertyField(this.spStatus);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
