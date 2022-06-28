namespace MiTschMR.Skills
{
	using UnityEngine;
	using GameCreator.Core;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
	public class ActionCancelAllSkills : IAction
	{
		public TargetGameObject skillCanceller = new TargetGameObject(TargetGameObject.Target.Player);
		protected bool cancelAction = true;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			Skills charSkillCanceller = this.skillCanceller.GetComponent<Skills>(target);
			if (charSkillCanceller == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return false;
			}

			charSkillCanceller.CancelExecutingSkills(this.cancelAction);
			charSkillCanceller.ResetPhaseProperties();
			return true;
		}

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public static new string NAME = "Skills/Cancel All Skills";
		private const string NODE_TITLE = "{0} cancel all skills";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.skillCanceller != null) ? this.skillCanceller.ToString() : "none"
				);

		#endif
	}
}