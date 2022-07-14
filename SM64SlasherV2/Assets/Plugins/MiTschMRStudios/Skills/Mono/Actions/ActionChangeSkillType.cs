namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using UnityEngine;

	[AddComponentMenu("")]
	public class ActionChangeSkillType : IAction
	{
		public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
		public SkillHolder skill = new SkillHolder();

		[SkillsSingleSkillType]
		public int newType = 0;

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			Skills charSkill = this.target.GetComponent<Skills>(target);
			if (charSkill == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return false;
			}

			charSkill.ChangeSkillType(skill, newType);
			charSkill.UpdateSkillsList();
			return true;
		}

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public static new string NAME = "Skills/Change Skill Type";
		private const string NODE_TITLE = "{0} change skill {1} type to {2}";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.target != null) ? this.target.ToString() : "none",
				(this.skill.skill != null) ? this.skill.skill.skillName.content : "none",
				(DatabaseSkills.Load().GetSkillTypesTypeByIndex(this.newType) != null) ? DatabaseSkills.Load().GetSkillTypesTypeByIndex(this.newType).id : "none"
				);

		#endif
	}
}