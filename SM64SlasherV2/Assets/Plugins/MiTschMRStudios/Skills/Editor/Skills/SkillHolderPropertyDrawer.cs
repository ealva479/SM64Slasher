namespace MiTschMR.Skills
{
	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(SkillHolder), true)]
	public class SkillHolderPropertyDrawer : PropertyDrawer
	{
		public const string PROP_SKILL = "skill";

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty spSkill = property.FindPropertyRelative(PROP_SKILL);

			Rect activatorRect = EditorGUI.PrefixLabel(position, label);

			string skillName = "(none)";
			if (spSkill.objectReferenceValue != null)
			{
				skillName = ((Skill)spSkill.objectReferenceValue).skillName.content;
				if (string.IsNullOrEmpty(skillName)) skillName = "No-name";
			}

			GUIContent variableContent = new GUIContent(skillName);
			if (EditorGUI.DropdownButton(activatorRect, variableContent, FocusType.Keyboard))
			{
				PopupWindow.Show(
					activatorRect,
					new SkillHolderPropertyDrawerWindow(activatorRect, property)
				);
			}
		}
	}
}