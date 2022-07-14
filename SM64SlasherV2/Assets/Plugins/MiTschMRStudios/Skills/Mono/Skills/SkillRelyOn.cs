namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using GameCreator.Localization;
    using System;
	using UnityEngine;

	[Serializable]
    public class SkillRelyOn : ScriptableObject
    {
		// PROPERTIES: -------------------------------------------------------------------------------------------------

		public int uuid = -1;
		[LocStringNoPostProcess] public LocString skillName = new LocString("");
		[LocStringNoPostProcess] public LocString skillDescription = new LocString();

		public Sprite icon;

		[SkillsSingleSkillType]
		public int skillType = 0;

		public Skill.SkillExecutionType skillExecutionType = Skill.SkillExecutionType.InstantExecute;
		public float castTime = 1;
		public float executionTime = 1;
		public float cooldownTime = 1;
		public float cooldownTimeBetween = .5f;

		public Skill.SkillState skillState = Skill.SkillState.Locked;
		public bool useSkillPoints = true;
		public float skillPointsNeeded = 1;

		public bool requiresLevel = false;
		public float level = 0;

		public bool assignableToSkillBar = false;

		public float maxAmount = 3;
		public float startAmount = 0;
	}
}