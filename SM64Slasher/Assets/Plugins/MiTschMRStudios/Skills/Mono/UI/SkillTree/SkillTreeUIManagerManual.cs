namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using UnityEngine;

	public class SkillTreeUIManagerManual : SkillTreeUIManager
	{
		protected const string DEFAULT_UI_PATH = "SkillTree/SkillTreeUIManual";

		// PUBLIC METHODS: ------------------------------------------------------------------------

		public override void Open(Skills characterSkills)
		{
			base.Open(characterSkills);

			this.skillTreeOpener.skillTreeWindowOpen = true;
			this.skillsContent.UpdateSkills(this.skillTreeOpener);
		}

		// STATIC METHODS: ------------------------------------------------------------------------

		public static void OpenSkillTree(Skills characterSkills)
		{
			if (IsSkillTreeOpen(characterSkills)) return;

			SkillTreeUIManagerManual skillTree = SkillTreeUIManagerManual.RequireInstance(characterSkills).GetComponent<SkillTreeUIManagerManual>();
			skillTree.Open(characterSkills);
		}

		public static void CloseSkillTree(Skills characterSkills)
		{
			if (!IsSkillTreeOpen(characterSkills)) return;

			SkillTreeUIManagerManual skillTree = SkillTreeUIManagerManual.RequireInstance(characterSkills).GetComponent<SkillTreeUIManagerManual>();
			skillTree.Close();
		}

		public static bool IsSkillTreeOpen(Skills characterSkills)
		{
			GameObject skillTree = GameObject.Find($"SkillTreeUIManual-{characterSkills.name}");
			if (skillTree == null) return false;
			return skillTree.GetComponent<SkillTreeUIManagerManual>().IsOpen();
		}

		protected static GameObject RequireInstance(Skills characterSkills)
		{
			if (DATABASE_SKILLS == null) DATABASE_SKILLS = DatabaseSkills.Load();

			GameObject prefab = DATABASE_SKILLS.GetSkillSettings().GetSkillTreeUIPrefabManual();
			GameObject skillTree = GameObject.Find($"SkillTreeUIManual-{characterSkills.name}");

			if (skillTree == null)
			{
				EventSystemManager.Instance.Wakeup();
				if (DATABASE_SKILLS.GetSkillSettings() == null)
				{
					Debug.LogError("No skills database found");
					return null;
				}

				if (prefab == null) prefab = Resources.Load<GameObject>(DEFAULT_UI_PATH);

				Instantiate(prefab, Vector3.zero, Quaternion.identity);
				skillTree = GameObject.Find($"{prefab.name}(Clone)");
				skillTree.name = $"SkillTreeUIManual-{characterSkills.name}";
				return skillTree;
			}
			return skillTree;
		}
	}
}