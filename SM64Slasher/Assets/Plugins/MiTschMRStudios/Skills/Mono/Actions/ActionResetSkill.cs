﻿namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using UnityEngine;

	[AddComponentMenu("")]
	public class ActionResetSkill : IAction
	{
		public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
		public SkillHolder skill = new SkillHolder();

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			Skills charSkill = this.target.GetComponent<Skills>(target);
			if (charSkill == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return false;
			}

			if (charSkill.skillTreeUIManager != null) charSkill.skillTreeUIManager.ResetSkill(this.skill);
			return true;
		}

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public static new string NAME = "Skills/Reset Skill";
		private const string NODE_TITLE = "{0} reset skill {1}";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.target != null) ? this.target.ToString() : "none",
				(this.skill.skill != null) ? this.skill.skill.skillName.content : "none"
				);

		#endif
	}
}