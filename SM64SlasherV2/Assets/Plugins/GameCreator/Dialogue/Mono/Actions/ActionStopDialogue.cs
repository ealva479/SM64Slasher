namespace GameCreator.Dialogue
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;

	[AddComponentMenu("")]
	public class ActionStopDialogue : IAction
	{
        public Dialogue dialogue;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			if (this.dialogue != null) this.dialogue.Stop();
            return false;
        }

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

	    public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Dialogue/Icons/Actions/";

		public static new string NAME = "Messages/Stop Dialogue";
		private const string NODE_TITLE = "Stop Dialogue {0}";

		public override string GetNodeTitle()
		{
            string dialogueName = (this.dialogue == null ? "(none)" : this.dialogue.gameObject.name);
			return string.Format(NODE_TITLE, dialogueName);
		}

		#endif
	}
}
