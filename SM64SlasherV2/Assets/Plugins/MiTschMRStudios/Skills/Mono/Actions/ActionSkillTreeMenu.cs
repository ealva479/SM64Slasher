namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using static MiTschMR.Skills.DatabaseSkills.SkillSettings;
	using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
	#endif

    [AddComponentMenu("")]
	public class ActionSkillTreeMenu : IAction
	{
		public TargetGameObject character = new TargetGameObject(TargetGameObject.Target.Player);
		public enum MENU_TYPE
		{
			SkillTree
		}

		public enum ACTION_TYPE
		{
			Open,
			Close
		}

		public MENU_TYPE menuType = MENU_TYPE.SkillTree;
		public ACTION_TYPE actionType = ACTION_TYPE.Open;

		// EXECUTABLE: -------------------------------------------------------------------------------------------------

		public override bool InstantExecute(GameObject target, IAction[] actions, int index)
		{
			Skills characterSkills = this.character.GetComponent<Skills>(target);
			if (characterSkills == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return false;
			}

			SkillTreeSelection skillTreeSelection = DatabaseSkills.Load().GetSkillSettings().GetSkillTreeSelection();

			switch (this.menuType)
			{
				case MENU_TYPE.SkillTree:
					if (this.actionType == ACTION_TYPE.Open)
                    {
						if (skillTreeSelection == SkillTreeSelection.Automatic) SkillTreeUIManagerAutomatical.OpenSkillTree(characterSkills);
						else SkillTreeUIManagerManual.OpenSkillTree(characterSkills);
					}
							
					if (this.actionType == ACTION_TYPE.Close)
                    {
						if (skillTreeSelection == SkillTreeSelection.Automatic) SkillTreeUIManagerAutomatical.CloseSkillTree(characterSkills);
						else SkillTreeUIManagerManual.CloseSkillTree(characterSkills);
					}
						
					break;
			}

			return true;
		}

		// +-----------------------------------------------------------------------------------------------------------+
		// | EDITOR                                                                                                    |
		// +-----------------------------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

		public static new string NAME = "Skills/Skill Tree UI";
		private const string NODE_TITLE = "{0} {1} menu";

		// PROPERTIES: -------------------------------------------------------------------------------------------------

		private SerializedProperty spCharacter;
		private SerializedProperty spMenuType;
		private SerializedProperty spActionType;

		// INSPECTOR METHODS: ------------------------------------------------------------------------------------------

		public override string GetNodeTitle()
		{
			return string.Format(
				NODE_TITLE,
				this.actionType.ToString(),
				this.menuType.ToString(),
				this.character.target
			);
		}

		protected override void OnEnableEditorChild()
		{
			this.spCharacter = this.serializedObject.FindProperty("character");
			this.spMenuType = this.serializedObject.FindProperty("menuType");
			this.spActionType = this.serializedObject.FindProperty("actionType");
		}

		protected override void OnDisableEditorChild()
		{
			this.spCharacter = null;
			this.spMenuType = null;
			this.spActionType = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spMenuType);
			EditorGUILayout.PropertyField(this.spActionType);
			EditorGUILayout.PropertyField(this.spCharacter);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}