namespace MiTschMR.Skills
{
	using System.Collections;
	using UnityEngine;
	using GameCreator.Core;

    [AddComponentMenu("")]
	public class ActionChangeSkillIcon : IAction
	{
		public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
		public SkillHolder skill = new SkillHolder();
		public Sprite newIcon;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			Skills charSkill = this.target.GetComponent<Skills>(target);
			if (charSkill == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return false;
			}

			charSkill.GetSkill(skill.skill.uuid).icon = this.newIcon;
			if (charSkill.skillTreeUIManager != null) charSkill.skillTreeUIManager.skillsContent.UpdateSkills(charSkill);
			if (charSkill.useSkillBar && charSkill.skillTreeUIManager != null) charSkill.skillTreeUIManager.UpdateSkillBar(skill);
			charSkill.UpdateSkillsList();

			return true;
        }

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index) => base.Execute(target, actions, index);

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public static new string NAME = "Skills/Change Skill Icon";
		private const string NODE_TITLE = "{0} change icon of {1}";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.target != null) ? this.target.ToString() : "none",
				(this.skill.skill != null) ? this.skill.skill.skillName.content : "none"
				);

		#endif
	}
}
