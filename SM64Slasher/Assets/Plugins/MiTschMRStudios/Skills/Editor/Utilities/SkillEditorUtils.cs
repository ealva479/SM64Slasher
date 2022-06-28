namespace MiTschMR.Skills
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public static class SkillEditorUtils
    {
        private const string SKILL_ICON_PATH = "Assets/Plugins/MiTschMRStudios/Skills/Extra/Icons/";
        private static readonly string SKILL_ICON_NAME = "Skill.png";

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static Texture2D GetIcon()
        {
            string name = SKILL_ICON_NAME;

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    Path.Combine(SKILL_ICON_PATH, name)
                );

            return icon;
        }
    }
}