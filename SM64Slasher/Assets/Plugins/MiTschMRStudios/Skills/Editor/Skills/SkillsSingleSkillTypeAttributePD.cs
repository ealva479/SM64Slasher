namespace MiTschMR.Skills
{
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(SkillsSingleSkillTypeAttribute))]
    public class SkillsSingleSkillTypeAttributePD : PropertyDrawer
    {
        protected static DatabaseSkills SKILLS;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (SKILLS == null) SKILLS = DatabaseSkills.Load();

            string[] ids = SKILLS.GetSkillTypesIDs();
            int[] values = new int[ids.Length];
            for (int i = 0; i < values.Length; ++i) values[i] = i;

            property.intValue = EditorGUI.IntPopup(
                position,
                label.text,
                property.intValue,
                ids,
                values
            );
        }
    }
}