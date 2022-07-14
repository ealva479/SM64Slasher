namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using System.Collections;
	using UnityEngine;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

    [AddComponentMenu("")]
	public class ActionSkillDebug : IAction
	{
		public TargetGameObject targetCharacter = new TargetGameObject(TargetGameObject.Target.Player);
		public SkillHolder skill = new SkillHolder();

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			Skills charSkills = this.targetCharacter.GetComponent<Skills>(target);
			if (charSkills == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return false;
			}

			SkillAsset debugSkill = charSkills.GetSkill(skill.skill.uuid);
			if (debugSkill != null) Debug.Log($"{debugSkill.skillName.content} is {debugSkill.skillState}");
            return true;
        }

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index) => base.Execute(target, actions, index);

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public static new string NAME = "Skills/Debug Skill";
		private const string NODE_TITLE = "Debug skill {0} of {1}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spTargetCharacter;
		private SerializedProperty spSkill;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.skill.skill != null) ? this.skill.skill.skillName.content : "none",
				(this.targetCharacter != null) ? this.targetCharacter.ToString() : "none"
				);

		protected override void OnEnableEditorChild ()
		{
			this.spTargetCharacter = this.serializedObject.FindProperty("targetCharacter");
			this.spSkill = this.serializedObject.FindProperty("skill");
		}

		protected override void OnDisableEditorChild ()
		{
			this.spTargetCharacter = null;
			this.spSkill = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spTargetCharacter);
			EditorGUILayout.PropertyField(this.spSkill);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
