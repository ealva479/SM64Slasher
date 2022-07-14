namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using UnityEngine;

	[AddComponentMenu("")]
	public class ConditionSkillState : ICondition
	{
		public TargetGameObject targetCharacter = new TargetGameObject(TargetGameObject.Target.Player);
		public SkillHolder skill = new SkillHolder();
		public Skill.SkillState skillState = Skill.SkillState.Unlocked;

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
			Skills charSkills = this.targetCharacter.GetComponent<Skills>(target);
			if (charSkills == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return true;
			}

			return charSkills.GetSkillLockedState(this.skill.skill.uuid, this.skillState);
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

	    public static new string NAME = "Skills/Skill State";
		private const string NODE_TITLE = "Is {0} skill {1} {2}?";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.targetCharacter != null) ? this.targetCharacter.ToString() : "none",
				(this.skill.skill != null) ? this.skill.skill.skillName.content : "none",
				(this.skillState == Skill.SkillState.Locked) ? "Locked" : "Unlocked"
				);

		#endif
	}
}
