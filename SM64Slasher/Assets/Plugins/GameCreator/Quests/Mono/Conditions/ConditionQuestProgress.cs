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
	public class ConditionQuestProgress : ICondition
	{
		public enum Operation
		{
            Greater,
			GreaterEqual,
            Less,
            LessEqual,
            Different,
            Equal
		}

		[IQuest] public IQuest quest;
		public Operation operation = Operation.GreaterEqual;
		public float value = 1f;

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
			if (this.quest != null)
			{
				IQuest questInstance = QuestsManager.Instance.GetQuest(this.quest.uniqueID);
                if (questInstance == null) return false;

				switch (this.operation)
				{
					case Operation.Greater : return questInstance.progress > this.value;
					case Operation.GreaterEqual : return questInstance.progress >= this.value;
					case Operation.Less: return questInstance.progress < this.value;
					case Operation.LessEqual: return questInstance.progress <= this.value;
					case Operation.Different: return !Mathf.Approximately(questInstance.progress, this.value);
					case Operation.Equal: return Mathf.Approximately(questInstance.progress, this.value);
				}
			}

			return false;
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

		public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Quests/Icons/Conditions/";

		public static new string NAME = "Quests/Quest Progress";
		private const string NODE_TITLE = "Task {0} progress {1} than {2}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spQuest;
		private SerializedProperty spOperation;
		private SerializedProperty spValue;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
			string op = "?";
			switch (this.operation)
            {
				case Operation.Greater: op = ">"; break;
				case Operation.GreaterEqual: op = ">="; break;
				case Operation.Less: op = "<"; break;
				case Operation.LessEqual: op = "<="; break;
				case Operation.Different: op = "=/="; break;
				case Operation.Equal: op = "="; break;
            }

			return string.Format(
				NODE_TITLE,
				(this.quest == null ? "(none)" : this.quest.internalName),
                op,
				this.value
			);
		}

		protected override void OnEnableEditorChild ()
		{
			this.spQuest = this.serializedObject.FindProperty("quest");
			this.spOperation = this.serializedObject.FindProperty("operation");
			this.spValue = this.serializedObject.FindProperty("value");
		}

		protected override void OnDisableEditorChild ()
		{
			this.spQuest = null;
            this.spOperation = null;
            this.spValue = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spQuest);
			EditorGUILayout.PropertyField(this.spOperation);
			EditorGUILayout.PropertyField(this.spValue);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
