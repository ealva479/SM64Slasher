namespace MiTschMR.Skills
{
	using GameCreator.Core;
    using static MiTschMR.Skills.DatabaseSkills.SkillSettings;
	using UnityEngine;

	[AddComponentMenu("")]
	public class ActionUnassignSkill : IAction
	{
		public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
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

			if (charSkill.skillTreeUIManager == null)
            {
				SkillTreeSelection skillTreeSelection = DatabaseSkills.Load().GetSkillSettings().GetSkillTreeSelection();

				if (skillTreeSelection == SkillTreeSelection.Automatic) SkillTreeUIManagerAutomatical.OpenSkillTree(charSkill);
				else SkillTreeUIManagerManual.OpenSkillTree(charSkill);

				if (skillTreeSelection == SkillTreeSelection.Automatic) SkillTreeUIManagerAutomatical.CloseSkillTree(charSkill);
				else SkillTreeUIManagerManual.CloseSkillTree(charSkill);
			}
			

			charSkill.skillTreeUIManager.skillBarElements.skillBarElements[this.skillBarIndex].UnbindSkill();
			charSkill.SyncSkillBarSkills(
				charSkill.skillTreeUIManager.skillBarElements.skillBarElements[this.skillBarIndex],
				null
				);

			return true;
		}

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public static new string NAME = "Skills/Unassign Skill";
		private const string NODE_TITLE = "{0} unassign skill of skill bar index {1}";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.target != null) ? this.target.ToString() : "none",
				this.skillBarIndex
				);

		#endif
	}
}