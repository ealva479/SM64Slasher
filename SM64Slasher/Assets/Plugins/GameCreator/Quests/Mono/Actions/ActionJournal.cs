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
	public class ActionJournal : IAction
	{
        public enum State
        {
            Open,
            Close,
            Toggle
        }

        public State action = State.Open;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            switch (this.action)
            {
                case State.Open: QuestsManager.Instance.OpenJournal(); break;
                case State.Close: QuestsManager.Instance.CloseJournal(); break;
                case State.Toggle: QuestsManager.Instance.ToggleJournal(); break;
            }

            return true;
        }

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

	    public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Quests/Icons/Actions/";

		public static new string NAME = "Quests/Quests Journal";
        private const string NODE_TITLE = "{0} Quests Journal";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spAction;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
			return string.Format(
				NODE_TITLE, 
                this.action.ToString()
			);
		}

		protected override void OnEnableEditorChild ()
		{
            this.spAction = this.serializedObject.FindProperty("action");
		}

		protected override void OnDisableEditorChild ()
		{
            this.spAction = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spAction);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
