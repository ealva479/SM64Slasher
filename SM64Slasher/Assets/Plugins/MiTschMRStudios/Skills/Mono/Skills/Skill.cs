namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using GameCreator.Localization;
	using System;
	using System.IO;
	using UnityEngine;
	using UnityEngine.Events;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[Serializable]
	public class Skill : ScriptableObject
	{
		// PROPERTIES: -------------------------------------------------------------------------------------------------

		public int uuid = -1;
		[LocStringNoPostProcess] public LocString skillName = new LocString("");
		[LocStringNoPostProcess] public LocString skillDescription = new LocString();

		public Sprite icon;

		[SkillsSingleSkillType]
		public int skillType = 0;

		public SkillExecutionType skillExecutionType = SkillExecutionType.InstantExecute;
		public float castTime = 1;
		public float executionTime = 1;
		public float cooldownTime = 1;
		public float cooldownTimeBetween = .5f;

		public SkillState skillState = SkillState.Locked;
		public bool useSkillPoints = true;
		public float skillPointsNeeded = 1;

		public bool requiresLevel = false;
		public float level = 0;

		public SkillHolderSerialized[] reliesOnSkills = new SkillHolderSerialized[1];

		public bool assignableToSkillBar = false;

		public float maxAmount = 3;
		public float startAmount = 0;

		public IConditionsList conditionsExecutionRequirements;

		public IActionsList actionsExecutionFailed;

		public IActionsList actionsOnActivate;
		public IActionsList actionsOnCast;
		public IActionsList actionsOnExecute;
		public IActionsList actionsOnFinish;

		public IConditionsList conditionsRequirements;

		public IActionsList actionsOnReset;

		public enum SkillExecutionType
		{
			InstantExecute,
			CastExecute,
			PermanentExecute,
			Stored
		};

		public enum SkillState
		{
			Locked,
			Unlocked
		}

		// CONSTRUCTOR: ------------------------------------------------------------------------------------------------

		public virtual SkillAsset CopySkill()
		{
			SkillAsset skill = new SkillAsset();

			skill.uuid = this.uuid;
			skill.skillName = new LocString(this.skillName.content);
			skill.skillDescription = new LocString(this.skillDescription.content);

			if (this.icon != null) skill.icon = Sprite.Instantiate<Sprite>(this.icon);

			skill.skillType = this.skillType;

			skill.skillExecutionType = this.skillExecutionType;
			skill.castTime = this.castTime;
			skill.executionTime = this.executionTime;
			skill.cooldownTime = this.cooldownTime;
			skill.cooldownTimeBetween = this.cooldownTimeBetween;

			skill.skillState = this.skillState;
			skill.useSkillPoints = this.useSkillPoints;
			skill.skillPointsNeeded = this.skillPointsNeeded;

			skill.requiresLevel = this.requiresLevel;

			skill.level = this.level;

			if (this.reliesOnSkills.Length > 0 && this.reliesOnSkills[0].skill != null)
			{
				skill.reliesOnSkills = new SkillHolderSerialized[this.reliesOnSkills.Length];
				for (int i = 0; i < this.reliesOnSkills.Length; i++)
				{
                    skill.reliesOnSkills[i] = new SkillHolderSerialized { skill = Instantiate(this.reliesOnSkills[i].skill) };
                }
			}

			skill.assignableToSkillBar = this.assignableToSkillBar;

			skill.maxAmount = this.maxAmount;
			skill.startAmount = this.startAmount;
			skill.currentAmount = this.startAmount;

			skill.conditionsExecutionRequirements = this.conditionsExecutionRequirements;

			skill.actionsExecutionFailed = this.actionsExecutionFailed;

			skill.actionsOnActivate = this.actionsOnActivate;
			skill.actionsOnCast = this.actionsOnCast;
			skill.actionsOnExecute = this.actionsOnExecute;
			skill.actionsOnFinish = this.actionsOnFinish;

			skill.conditionsRequirements = this.conditionsRequirements;

			skill.actionsOnReset = this.actionsOnReset;

			skill.onChange = new UnityEvent();

			return skill;
		}

		public virtual SkillRelyOn CopySkillToSkillRelyOn()
		{
			SkillRelyOn skill = ScriptableObject.CreateInstance<SkillRelyOn>();

			skill.uuid = this.uuid;
			skill.skillName = this.skillName;
			skill.skillDescription = this.skillDescription;

			skill.icon = this.icon;

			skill.skillType = this.skillType;

			skill.skillExecutionType = this.skillExecutionType;
			skill.castTime = this.castTime;
			skill.executionTime = this.executionTime;
			skill.cooldownTime = this.cooldownTime;
			skill.cooldownTimeBetween = this.cooldownTimeBetween;

			skill.skillState = this.skillState;
			skill.useSkillPoints = this.useSkillPoints;
			skill.skillPointsNeeded = this.skillPointsNeeded;

			skill.requiresLevel = this.requiresLevel;

			skill.level = this.level;

			skill.assignableToSkillBar = this.assignableToSkillBar;

			skill.maxAmount = this.maxAmount;
			skill.startAmount = this.startAmount;

			return skill;
		}

		#if UNITY_EDITOR

		public static SkillRelyOn CreateSkillRelyOnInstance(SkillRelyOn relyOnSkill)
		{
			string pathFolder = "Assets/Plugins/MiTschMRStudiosData/Skills/ReliesOnSkills";
			string fileName = $"skill.{relyOnSkill.uuid}.asset";
			string pathFile = Path.Combine(pathFolder, fileName);
			string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(pathFile);

			if (!AssetDatabase.IsValidFolder("Assets/Plugins/MiTschMRStudiosData/Skills/ReliesOnSkills")) AssetDatabase.CreateFolder("Assets/Plugins/MiTschMRStudiosData/Skills", "ReliesOnSkills");

			AssetDatabase.CreateAsset(relyOnSkill, uniqueFileName);

			return relyOnSkill;
		}

		public static SkillRelyOn UpdateSkillRelyOnInstance(SkillRelyOn target, SkillRelyOn source)
        {
			target.uuid = source.uuid;
			target.skillName = source.skillName;
			target.icon = source.icon;
			target.skillType = source.skillType;
			target.skillExecutionType = source.skillExecutionType;
			target.castTime = source.castTime;
			target.executionTime = source.executionTime;
			target.cooldownTime = source.cooldownTime;
			target.cooldownTimeBetween = source.cooldownTime;
			target.skillState = source.skillState;
			target.useSkillPoints = source.useSkillPoints;
			target.skillPointsNeeded = source.skillPointsNeeded;
			target.requiresLevel = source.requiresLevel;
			target.level = source.level;
			target.assignableToSkillBar = source.assignableToSkillBar;
			target.maxAmount = source.maxAmount;
			target.startAmount = source.startAmount;

			return target;
        }

		public static void DeleteSkillRelyOnInstance(UnityEngine.Object relyOnSkillOld)
        {
			string pathFile = AssetDatabase.GetAssetPath(relyOnSkillOld);
			if (pathFile != "") AssetDatabase.DeleteAsset(pathFile);
        }

		public static Skill CreateSkillInstance()
		{
			Skill skill = ScriptableObject.CreateInstance<Skill>();
			Guid guid = Guid.NewGuid();

			skill.name = "skill." + Mathf.Abs(guid.GetHashCode());
			skill.uuid = Mathf.Abs(guid.GetHashCode());

			skill.skillName = new LocString();
			skill.skillDescription = new LocString();
			skill.hideFlags = HideFlags.HideInHierarchy;

			DatabaseSkills databaseSkill = DatabaseSkills.Load();
			AssetDatabase.AddObjectToAsset(skill, databaseSkill);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(skill));
			return skill;
		}

        protected virtual void OnDestroy()
		{
			this.DestroyAsset(this.actionsOnActivate);
			this.DestroyAsset(this.actionsOnCast);
			this.DestroyAsset(this.actionsOnExecute);
			this.DestroyAsset(this.actionsOnFinish);
			this.DestroyAsset(this.conditionsRequirements);
			this.DestroyAsset(this.actionsExecutionFailed);
			this.DestroyAsset(this.actionsOnReset);
		}

		protected virtual void DestroyAsset(MonoBehaviour reference)
		{
			if (reference == null) return;
			if (reference.gameObject == null) return;
			DestroyImmediate(reference.gameObject, true);
		}
		#endif
	}
}