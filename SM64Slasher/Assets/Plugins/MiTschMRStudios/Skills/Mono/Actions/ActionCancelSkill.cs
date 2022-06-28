namespace MiTschMR.Skills
{
	using UnityEngine;
	using GameCreator.Core;

	[AddComponentMenu("")]
	public class ActionCancelSkill : IAction
	{
		public TargetGameObject skillCanceller = new TargetGameObject(TargetGameObject.Target.Player);
		public SkillHolder skill = new SkillHolder();
		protected readonly bool cancelAction = true;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			Skills charSkillCanceller = this.skillCanceller.GetComponent<Skills>(target);
			if (charSkillCanceller == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return false;
			}

			charSkillCanceller.CancelExecutingSkill(this.skill, this.cancelAction);
			charSkillCanceller.ResetPhaseProperties();
			return true;
		}

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public static new string NAME = "Skills/Cancel Skill";
		private const string NODE_TITLE = "{0} cancel skill {1}";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.skillCanceller != null) ? this.skillCanceller.ToString() : "none",
				(this.skill.skill != null) ? this.skill.skill.skillName.content : "none"
				);

		#endif
	}
}