namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using UnityEngine;

	[AddComponentMenu("")]
	public class ActionResetSkills : IAction
	{
		public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			Skills charSkill = this.target.GetComponent<Skills>(target);
			if (charSkill == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return false;
			}

			if (charSkill.skillTreeUIManager != null) charSkill.skillTreeUIManager.ResetAllSkills();
			return true;
		}

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public static new string NAME = "Skills/Reset All Skills";
		private const string NODE_TITLE = "{0} reset all skills";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.target != null) ? this.target.ToString() : "none"
				);

		#endif
	}
}