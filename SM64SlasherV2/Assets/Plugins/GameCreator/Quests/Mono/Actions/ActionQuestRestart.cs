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
	public class ActionQuestRestart : IAction
	{
		[IQuest] public IQuest quest;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            QuestsManager.Instance.RestartQuest(this.quest);
            return true;
        }

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

	    public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Quests/Icons/Actions/";

		public static new string NAME = "Quests/Quest Restart";
		private const string NODE_TITLE = "Restart Quest {0}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spQuest;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
			return string.Format(
				NODE_TITLE,
                (this.quest == null ? "none" : this.quest.ToString())
			);
		}

		protected override void OnEnableEditorChild ()
		{
			this.spQuest = this.serializedObject.FindProperty("quest");
		}

		protected override void OnDisableEditorChild ()
		{
			this.spQuest = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spQuest);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
