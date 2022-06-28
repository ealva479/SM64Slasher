namespace MiTschMR.Skills
{
	[System.Serializable]
	public class SkillHolderSerialized
	{
		public SkillRelyOn skill;

		// PUBLIC METHODS: ------------------------------------------------------------------------

		public override string ToString()
		{
			if (this.skill == null) return "(none)";
			return this.skill.skillName.content;
		}
	}
}