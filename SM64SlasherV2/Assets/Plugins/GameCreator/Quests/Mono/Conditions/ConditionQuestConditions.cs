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
	public class ConditionQuestConditions : ICondition
	{
		[IQuest] public IQuest quest;
		public bool result = true;

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
			if (this.quest == null) return false;
			IQuest questInstance = QuestsManager.Instance.GetQuest(this.quest.uniqueID);
			if (questInstance == null) return false;

			return (questInstance.CheckConditions() == this.result);
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

		public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Quests/Icons/Conditions/";

		public static new string NAME = "Quests/Quest Conditions";
		private const string NODE_TITLE = "Quest {0} conditions {1}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spQuest;
		private SerializedProperty spResult;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
			return string.Format(
                NODE_TITLE,
                (this.quest == null 
				    ? "none" 
				    : this.quest.ToString()
				),
				result
            );
		}

		protected override void OnEnableEditorChild ()
		{
			this.spQuest = this.serializedObject.FindProperty("quest");
			this.spResult = this.serializedObject.FindProperty("result");
		}

		protected override void OnDisableEditorChild ()
		{
			this.spQuest = null;
			this.spResult = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spQuest);
			EditorGUILayout.PropertyField(this.spResult);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
