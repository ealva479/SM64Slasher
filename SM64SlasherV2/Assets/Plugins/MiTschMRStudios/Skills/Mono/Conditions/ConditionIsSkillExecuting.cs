namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using UnityEngine;

	[AddComponentMenu("")]
	public class ConditionIsSkillExecuting : ICondition
	{
		public TargetGameObject targetCharacter = new TargetGameObject(TargetGameObject.Target.Player);
		public SkillHolder skill = new SkillHolder();
		public bool isExecuting = false;

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
			return skillAsset.isExecuting == isExecuting; 
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

	    public static new string NAME = "Skills/Is Skill Executing";
		private const string NODE_TITLE = "Is skill {0} of {1} executing ({2})?";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.skill.skill != null) ? this.skill.skill.skillName.content : "none", 
				(this.targetCharacter != null) ? this.targetCharacter.ToString() : "none",
				this.isExecuting
				);

		#endif
	}
}
