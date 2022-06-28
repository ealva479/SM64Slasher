namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using UnityEngine;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
	public class ActionExecuteSkill : IAction
	{
		public TargetGameObject skillExecuter = new TargetGameObject(TargetGameObject.Target.Player);
		public SkillHolder skill = new SkillHolder();

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			Skills charSkillExecuter = this.skillExecuter.GetComponent<Skills>(target);
			if (charSkillExecuter == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return false;
			}

			charSkillExecuter.StartCoroutine(charSkillExecuter.ExecuteSkill(skill));
			return true;
		}

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public static new string NAME = "Skills/Execute Skill";
		private const string NODE_TITLE = "{0} execute skill {1}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spSkillExecuter;
		private SerializedProperty spSkill;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.skillExecuter != null) ? this.skillExecuter.ToString() : "none",
				(this.skill.skill != null) ? this.skill.skill.skillName.content : "none"
				);

		protected override void OnEnableEditorChild ()
		{
			this.spSkillExecuter = this.serializedObject.FindProperty("skillExecuter");
			this.spSkill = this.serializedObject.FindProperty("skill");
		}

		protected override void OnDisableEditorChild ()
		{
			this.spSkillExecuter = null;
			this.spSkill = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spSkillExecuter);
			EditorGUILayout.PropertyField(this.spSkill);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}