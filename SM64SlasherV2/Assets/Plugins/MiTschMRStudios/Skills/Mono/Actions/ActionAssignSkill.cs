namespace MiTschMR.Skills
{
	using UnityEngine;
	using GameCreator.Core;
    using static MiTschMR.Skills.DatabaseSkills.SkillSettings;

    [AddComponentMenu("")]
	public class ActionAssignSkill : IAction
	{
		public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
		public SkillHolder skill = new SkillHolder();
		public int skillBarIndex = 0;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			Skills charSkill = this.target.GetComponent<Skills>(target);
			if (charSkill == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return false;
			}

			SkillAsset skillAsset = charSkill.GetSkill(skill.skill.uuid);

			if (charSkill.skillTreeUIManager == null)
            {
				SkillTreeSelection skillTreeSelection = DatabaseSkills.Load().GetSkillSettings().GetSkillTreeSelection();

				if (skillTreeSelection == SkillTreeSelection.Automatic) SkillTreeUIManagerAutomatical.OpenSkillTree(charSkill);
				else SkillTreeUIManagerManual.OpenSkillTree(charSkill);

				if (skillTreeSelection == SkillTreeSelection.Automatic) SkillTreeUIManagerAutomatical.CloseSkillTree(charSkill);
				else SkillTreeUIManagerManual.CloseSkillTree(charSkill);
			}
			
			charSkill.skillTreeUIManager.skillBarElements.skillBarElements[this.skillBarIndex].BindSkill(skillAsset);
			charSkill.SyncSkillBarSkills(
				charSkill.skillTreeUIManager.skillBarElements.skillBarElements[this.skillBarIndex],
				skillAsset
				);

			return true;
		}

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public static new string NAME = "Skills/Assign Skill";
		private const string NODE_TITLE = "{0} assign skill {1} to index {2}";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.target != null) ? this.target.ToString() : "none",
				(this.skill.skill != null) ? this.skill.skill.skillName.content : "none",
				this.skillBarIndex
				);

		#endif
	}
}