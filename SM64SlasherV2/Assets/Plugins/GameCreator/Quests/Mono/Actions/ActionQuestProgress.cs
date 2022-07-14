namespace GameCreator.Quests
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
	using GameCreator.Variables;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
	public class ActionQuestProgress : IAction
	{
		public enum SetType
		{
            Add,
            Set
		}

		[IQuest] public IQuest quest;
		public SetType setType = SetType.Add;
		public NumberProperty amount = new NumberProperty(0.25f);

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (this.quest != null)
            {
                IQuest instance = QuestsManager.Instance.GetQuest(this.quest.uniqueID);
                switch (this.setType)
                {
                    case SetType.Add: instance.AddProgress(this.amount.GetValue(target)); break;
                    case SetType.Set: instance.SetProgress(this.amount.GetValue(target)); break;
                }
            }

            return true;
        }

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

		public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Quests/Icons/Actions/";

		public static new string NAME = "Quests/Quest Progress";
		private const string NODE_TITLE = "{0} progress to Quest {1}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spQuest;
		private SerializedProperty spSetType;
		private SerializedProperty spAmount;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
			return string.Format(
				NODE_TITLE,
				this.setType,
				(this.quest == null ? "(none)" : this.quest.internalName)
			);
		}

		protected override void OnEnableEditorChild ()
		{
			this.spQuest = this.serializedObject.FindProperty("quest");
			this.spSetType = this.serializedObject.FindProperty("setType");
			this.spAmount = this.serializedObject.FindProperty("amount");
		}

		protected override void OnDisableEditorChild ()
		{
			this.spQuest = null;
            this.spSetType = null;
			this.spAmount = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spQuest);
			EditorGUILayout.PropertyField(this.spSetType);
			EditorGUILayout.PropertyField(this.spAmount);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
