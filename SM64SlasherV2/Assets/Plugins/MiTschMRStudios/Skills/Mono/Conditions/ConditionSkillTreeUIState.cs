namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using UnityEngine;
    using static MiTschMR.Skills.DatabaseSkills.SkillSettings;

    [AddComponentMenu("")]
	public class ConditionSkillTreeUIState : ICondition
	{
		public TargetGameObject targetCharacter = new TargetGameObject(TargetGameObject.Target.Player);
		public State state = State.Opened;
		public enum State
        {
			Opened,
			Closed
        }

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
			Skills charSkills = this.targetCharacter.GetComponent<Skills>(target);
			if (charSkills == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return true;
			}

			SkillTreeSelection skillTreeSelection = DatabaseSkills.Load().GetSkillSettings().GetSkillTreeSelection();

            switch (this.state)
            {
				case State.Closed:
					return skillTreeSelection == SkillTreeSelection.Automatic
						? !SkillTreeUIManagerAutomatical.IsSkillTreeOpen(charSkills)
						: !SkillTreeUIManagerManual.IsSkillTreeOpen(charSkills);

				case State.Opened:
					return skillTreeSelection == SkillTreeSelection.Automatic
						? SkillTreeUIManagerAutomatical.IsSkillTreeOpen(charSkills)
						: SkillTreeUIManagerManual.IsSkillTreeOpen(charSkills);
			}

			return false;
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

	    public static new string NAME = "Skills/Skill Tree UI State";
		private const string NODE_TITLE = "Is {0} skill tree UI {1}?";

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.targetCharacter != null) ? this.targetCharacter.ToString() : "none",
				this.state
				);

		#endif
	}
}
