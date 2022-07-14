namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using UnityEngine;

	[AddComponentMenu("")]
	public class ConditionIsSkillInCooldown : ICondition
	{
		public TargetGameObject targetCharacter = new TargetGameObject(TargetGameObject.Target.Player);
		public SkillHolder skill = new SkillHolder();
		public bool inCooldown = false;

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
			Skills charSkills = this.targetCharacter.GetComponent<Skills>(target);
			if (charSkills == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return false;
			}

			SkillAsset skillAsset = charSkills.GetSkill(skill.skill.uuid);
			return skillAsset.isInCooldown == inCooldown;
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

		public static new string NAME = "Skills/Is Skill In Cooldown";
		private const string NODE_TITLE = "Is skill {0} of {1} in cooldown ({2})?";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.skill.skill != null) ? this.skill.skill.skillName.content : "none",
				(this.targetCharacter != null) ? this.targetCharacter.ToString() : "none",
				this.inCooldown
				);

		#endif
	}
}
