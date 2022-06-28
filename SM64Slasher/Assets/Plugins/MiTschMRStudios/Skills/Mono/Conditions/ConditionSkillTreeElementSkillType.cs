namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using UnityEngine;

	[AddComponentMenu("")]
	public class ConditionSkillTreeElementSkillType : ICondition
	{
		public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
		[SkillsSingleSkillType]
		public int skillType = 0;

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
			SkillUI skillTreeItem = this.target.GetComponent<SkillUI>(target);
			if (skillTreeItem == null)
			{
				Debug.LogError("Target Game Object does not have a SkillUI component");
				return false;
			}

			return skillTreeItem.SkillAsset.skillType == this.skillType;
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

	    public static new string NAME = "Skills/Skill Tree Element Skill Type";
		private const string NODE_TITLE = "Is {0} skill of type {1}?";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.target != null) ? this.target.ToString() : "none",
				(DatabaseSkills.Load().GetSkillTypesTypeByIndex(this.skillType) != null) ? DatabaseSkills.Load().GetSkillTypesTypeByIndex(this.skillType).id : "none"
				);

		#endif
	}
}
