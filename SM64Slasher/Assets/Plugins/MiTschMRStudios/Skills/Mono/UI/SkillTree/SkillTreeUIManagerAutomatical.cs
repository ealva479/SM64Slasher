namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using System.Collections.Generic;
	using UnityEngine;

    public class SkillTreeUIManagerAutomatical : SkillTreeUIManager
	{
		protected const string DEFAULT_UI_PATH = "SkillTree/SkillTreeUIAutomatical";

		// PROPERTIES: ----------------------------------------------------------------------------

		[SerializeField] protected float skillDistanceFromTop = 80f;
		[SerializeField] protected float distanceBetweenSkillTypes = 50f;
		[SerializeField] protected float skillTypeSize = 100f;

		[SerializeField] protected GameObject skillTypePrefab = null;
		protected int skillTypes;

		// PUBLIC METHODS: ------------------------------------------------------------------------

		public override void Open(Skills characterSkills)
		{
			base.Open(characterSkills);

			if (this.skillTypes == 0)
			{
				int previousSkillCount = 0;
				bool firstSkillType = true;
				Skills skills = this.skillTreeOpener.GetComponent<Skills>();
				for (int i = 0; i < DATABASE_SKILLS.GetSkillTypes().Length; i++)
				{
					if (DATABASE_SKILLS.GetSkillTypesTypeByIndex(i).id != "") this.skillTypes++;
				}
				for (int i = 0; i < skillTypes; i++)
                {
					int skillsCountInType = 0;
					foreach (KeyValuePair<int, SkillAsset> entry in skills.GetSkillsDictionary())
                    {
						// Skills that don't rely on other skills
						if (entry.Value.skillType == i && entry.Value.reliesOnSkills == null) skillsCountInType++;
                    }

					if (skillsCountInType == 0) continue;

					Vector3 position;
					if (firstSkillType)
                    {
						position = new Vector3(0, -skillDistanceFromTop - ((skillTypeSize / 2) * (skillsCountInType - 1)), 0);
						firstSkillType = false;
					}
					else
                    {
						position = new Vector3(0, -skillDistanceFromTop - ((distanceBetweenSkillTypes * (i - 1)) + (skillTypeSize / 2) * (skillsCountInType - 1) + (skillTypeSize * previousSkillCount)), 0);
					}

					GameObject newType = Instantiate(skillTypePrefab, Vector3.zero, this.skillsContent.transform.rotation, this.skillsContent.transform);
					newType.GetComponent<RectTransform>().anchoredPosition = position;
					newType.GetComponent<RectTransform>().sizeDelta = new Vector2(skillTypeSize, skillTypeSize * skillsCountInType);
					newType.name = $"Type{i}";
					previousSkillCount += skillsCountInType;
				}
			}

			this.skillTreeOpener.skillTreeWindowOpen = true;
			this.skillsContent.UpdateSkills(characterSkills);
		}

		// STATIC METHODS: ------------------------------------------------------------------------

		public static void OpenSkillTree(Skills characterSkills)
		{
			if (IsSkillTreeOpen(characterSkills)) return;

			SkillTreeUIManagerAutomatical skillTree = SkillTreeUIManagerAutomatical.RequireInstance(characterSkills).GetComponent<SkillTreeUIManagerAutomatical>();
			skillTree.Open(characterSkills);
		}

		public static void CloseSkillTree(Skills characterSkills)
		{
			if (!IsSkillTreeOpen(characterSkills)) return;

			SkillTreeUIManagerAutomatical skillTree = SkillTreeUIManagerAutomatical.RequireInstance(characterSkills).GetComponent<SkillTreeUIManagerAutomatical>();
			skillTree.Close();
		}

		public static bool IsSkillTreeOpen(Skills characterSkills)
		{
			GameObject skillTree = GameObject.Find($"SkillTreeUIAutomatical-{characterSkills.name}");
			if (skillTree == null) return false;
			return skillTree.GetComponent<SkillTreeUIManagerAutomatical>().IsOpen();
		}

		protected static GameObject RequireInstance(Skills characterSkills)
		{
			if (DATABASE_SKILLS == null) DATABASE_SKILLS = DatabaseSkills.Load();

			GameObject prefab = DATABASE_SKILLS.GetSkillSettings().GetSkillTreeUIPrefabAutomatic();
			GameObject skillTree = GameObject.Find($"SkillTreeUIAutomatical-{characterSkills.name}");

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
				skillTree.name = $"SkillTreeUIAutomatical-{characterSkills.name}";
				return skillTree;
			}
			return skillTree;
		}
	}
}