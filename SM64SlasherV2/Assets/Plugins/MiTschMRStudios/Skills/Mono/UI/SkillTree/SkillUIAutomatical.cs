namespace MiTschMR.Skills
{
	using UnityEngine;

    [AddComponentMenu("MiTschMR Studios/UI/Skill UI Automatical")]
	public class SkillUIAutomatical : SkillUI
	{
		// PROPERTIES: ----------------------------------------------------------------------------

		protected string nameOfSkillTreeUIPrefab = "SkillTreeUIAutomatical-";
		protected GameObject skillTreeUIPrefab;

		// CONSTRUCTOR & UPDATER: -----------------------------------------------------------------

		public override void Setup(Skills characterSkills, SkillAsset skill, Skill.SkillState locked)
		{
			skillTreeUIPrefab = GameObject.Find(nameOfSkillTreeUIPrefab + characterSkills.name);

			this.skillTreeUIManager = skillTreeUIPrefab.GetComponent<SkillTreeUIManagerAutomatical>();

			this.skillTreeOpener = skillTreeUIPrefab.GetComponent<SkillTreeUIManagerAutomatical>().skillTreeOpener;

			base.Setup(characterSkills, skill, locked);
		}

		// PUBLIC METHODS: ------------------------------------------------------------------------

		public override void UpdateUI(SkillAsset skill, Skill.SkillState locked)
		{
			base.UpdateUI(skill, locked);

			if (locked == Skill.SkillState.Locked)
            {
				if (this.skillIconEnabled != null) this.skillIconEnabled.gameObject.SetActive(false);
				if (this.skillIconDisabled != null) this.skillIconDisabled.gameObject.SetActive(true);
				if (this.imageLocked != null) this.imageLocked.SetActive(true);
				if (this.imageUnlocked != null) this.imageUnlocked.SetActive(false);
			}
            else
            {
				if (this.skillIconEnabled != null) this.skillIconEnabled.gameObject.SetActive(true);
				if (this.skillIconDisabled != null) this.skillIconDisabled.gameObject.SetActive(false);
				if (this.imageLocked != null) this.imageLocked.SetActive(false);
				if (this.imageUnlocked != null) this.imageUnlocked.SetActive(true);
			}
		}
	}
}