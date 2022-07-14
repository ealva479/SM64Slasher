namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using System.Collections;
	using UnityEngine;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

    [AddComponentMenu("")]
	public class ActionSetSkillState : IAction
	{
		public TargetGameObject targetCharacter = new TargetGameObject(TargetGameObject.Target.Player);
		public SkillHolder skill = new SkillHolder();
		public Skill.SkillState skillState = Skill.SkillState.Unlocked;

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			Skills charSkills = this.targetCharacter.GetComponent<Skills>(target);
			if (charSkills == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return false;
			}

			charSkills.UpdateSkillState(skill.skill.uuid, skillState);
			return true;
		}

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index) => base.Execute(target, actions, index);

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

		public static new string NAME = "Skills/Set Skill State";
		private const string NODE_TITLE = "{0} skill {1} of {2}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spTargetCharacter;
		private SerializedProperty spSkill;
		private SerializedProperty spSkillState;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() => 
			string.Format(NODE_TITLE,
				(this.skillState == Skill.SkillState.Locked) ? "Lock" : "Unlock",
				(this.skill.skill != null) ? this.skill.skill.skillName.content : "none",
				(this.targetCharacter != null) ? this.targetCharacter.ToString() : "none"
				);

		protected override void OnEnableEditorChild ()
		{
			this.spTargetCharacter = this.serializedObject.FindProperty("targetCharacter");
			this.spSkill = this.serializedObject.FindProperty("skill");
			this.spSkillState = this.serializedObject.FindProperty("skillState");
		}

		protected override void OnDisableEditorChild ()
		{
			this.spTargetCharacter = null;
			this.spSkill = null;
			this.spSkillState = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spTargetCharacter);
			EditorGUILayout.PropertyField(this.spSkill);
			EditorGUILayout.PropertyField(this.spSkillState, new GUIContent("Set State to"));

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
