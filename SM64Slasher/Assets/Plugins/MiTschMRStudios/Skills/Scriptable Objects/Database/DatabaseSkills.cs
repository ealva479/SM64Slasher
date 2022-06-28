namespace MiTschMR.Skills
{
    using GameCreator.Core;
    using GameCreator.Stats;
    using GameCreator.Variables;
    using System.IO;
    using System.Collections.Generic;
    using UnityEngine;

    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    public class DatabaseSkills : IDatabase
    {
        [System.Serializable]
        public class SkillsList
        {
            public List<Skill> skills = new List<Skill>();
            public SkillType[] skillTypes = new SkillType[SkillType.MAX]
            {
                new SkillType(), new SkillType(), new SkillType(), new SkillType(),
                new SkillType(), new SkillType(), new SkillType(), new SkillType(),
                new SkillType(), new SkillType(), new SkillType(), new SkillType(),
                new SkillType(), new SkillType(), new SkillType(), new SkillType(),
                new SkillType(), new SkillType(), new SkillType(), new SkillType(),
                new SkillType(), new SkillType(), new SkillType(), new SkillType(),
                new SkillType(), new SkillType(), new SkillType(), new SkillType(),
                new SkillType(), new SkillType(), new SkillType(), new SkillType(),
            };
        }

        [System.Serializable]
        public class SkillSettings
        {
            [SerializeField] protected SkillTreeSelection skillTreeSelection;
            [SerializeField] protected GameObject skillTreeUIPrefabAutomatic;
            [SerializeField] protected GameObject skillTreeUIPrefabManual;

            [Tooltip("Check if you want to pause the game when opening the Skills menu")]
            [SerializeField] protected bool pauseTimeOnUI = false;

            [SerializeField] protected bool dragDropSkills = false;
            [SerializeField] protected Vector2 dragDropHotspot;

            [SerializeField] protected StatSelection levelSelection;
            [VariableFilter(Variable.DataType.Number)]
            [SerializeField] protected VariableProperty levelVariable = new VariableProperty(Variable.VarType.GlobalVariable);
            [StatSelector]
            [SerializeField] protected StatAsset levelStat;

            [SerializeField] protected StatSelection skillPointsSelection;
            [VariableFilter(Variable.DataType.Number)]
            [SerializeField] protected VariableProperty skillPointsVariable = new VariableProperty(Variable.VarType.GlobalVariable);
            [StatSelector]
            [SerializeField] protected StatAsset skillPointsStat;

            public enum SkillTreeSelection
            {
                Automatic,
                Manual
            }

            public enum StatSelection
            {
                GlobalVariable,
                Stat
            }

            public virtual bool GetPauseTimeOnUI() => this.pauseTimeOnUI;

            public virtual bool GetDragDropSkills() => this.dragDropSkills;

            public virtual Vector2 GetDragDropHotspot() => this.dragDropHotspot;

            public virtual SkillTreeSelection GetSkillTreeSelection() => this.skillTreeSelection;

            public virtual GameObject GetSkillTreeUIPrefabAutomatic() => this.skillTreeUIPrefabAutomatic;

            public virtual GameObject GetSkillTreeUIPrefabManual() => this.skillTreeUIPrefabManual;

            public virtual StatSelection GetLevelSelection() => this.levelSelection;

            public virtual VariableProperty GetLevelVariable() => this.levelVariable;

            public virtual StatAsset GetLevelStat() => this.levelStat;

            public virtual StatSelection GetSkillPointsSelection() => this.skillPointsSelection;

            public virtual VariableProperty GetSkillPointsVariable() => this.skillPointsVariable;

            public virtual StatAsset GetSkillPointsStat() => this.skillPointsStat;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        [SerializeField] protected SkillsList skillList;
        [SerializeField] protected SkillSettings skillsSettings;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public virtual List<Skill> GetSkillList() => this.skillList.skills;

        public virtual Skill GetSkillListSkillByIndex(int index) => this.skillList.skills[index];

        public virtual SkillType[] GetSkillTypes() => this.skillList.skillTypes;

        public virtual SkillType GetSkillTypesTypeByIndex(int index) => this.skillList.skillTypes[index];

        public virtual SkillSettings GetSkillSettings() => this.skillsSettings;

        public virtual List<int> GetSkillSuggestions(string hint)
        {
            hint = hint.ToLower();
            List<int> suggestions = new List<int>();
            for (int i = 0; i < this.skillList.skills.Count; ++i)
            {
                if (this.skillList.skills[i] == null) continue;
                if (this.skillList.skills[i].skillName.content.ToLower().Contains(hint) ||
                    this.skillList.skills[i].skillDescription.content.ToLower().Contains(hint))
                {
                    suggestions.Add(i);
                }
            }
            return suggestions;
        }

        public virtual string[] GetSkillTypesNames()
        {
            SkillType[] skillTypes = this.skillList.skillTypes;
            if (skillTypes.Length == 0) return new string[0];

            string[] names = new string[skillTypes.Length];
            for (int i = 0; i < skillTypes.Length; ++i)
            {
                if (Application.isPlaying) names[i] = skillTypes[i].name.GetText();
                else names[i] = skillTypes[i].name.content;
            }
            return names;
        }

        public virtual string[] GetSkillTypesIDs()
        {
            SkillType[] skillTypes = this.skillList.skillTypes;
            if (skillTypes.Length == 0) return new string[0];

            string[] ids = new string[skillTypes.Length];
            for (int i = 0; i < skillTypes.Length; ++i)
            {
                ids[i] = Path.Combine(skillTypes[i].id, skillTypes[i].name.content);
            }
            return ids;
        }

        // PUBLIC STATIC METHODS: -----------------------------------------------------------------

        public static DatabaseSkills Load() => IDatabase.LoadDatabase<DatabaseSkills>();

        // OVERRIDE METHODS: ----------------------------------------------------------------------

#if UNITY_EDITOR

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad() => IDatabase.Setup<DatabaseSkills>();

        protected override string GetProjectPath() => "Assets/Plugins/MiTschMRStudiosData/Skills/Resources";

#endif
    }
}