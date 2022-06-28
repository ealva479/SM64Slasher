namespace MiTschMR.Skills
{
    using GameCreator.Core;
    using GameCreator.Localization;
    using System;
    using UnityEngine;

    [Serializable]
    public class SkillType
    {
        public const int MAX = 32;

        // PROPERTIES: ----------------------------------------------------------------------------

        public string id;

        [LocStringNoPostProcess]
        public LocString name;

        // INITIALIZERS: --------------------------------------------------------------------------

        public SkillType()
        {
            this.id = "";
            this.name = new LocString();
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // SKILL ATTRIBUTE: ----------------------------------------------------------------------------

    public class SkillsMultiSkillTypeAttribute : PropertyAttribute
    {
        public SkillsMultiSkillTypeAttribute() { }
    }

    public class SkillsSingleSkillTypeAttribute : PropertyAttribute
    {
        public SkillsSingleSkillTypeAttribute() { }
    }
}