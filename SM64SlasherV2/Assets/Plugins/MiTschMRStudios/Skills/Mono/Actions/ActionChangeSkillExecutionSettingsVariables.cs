namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using GameCreator.Variables;
	using UnityEditor;
	using UnityEngine;

	[AddComponentMenu("")]
	public class ActionChangeSkillExecutionSettingsVariables : IAction
	{
		public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
		public SkillHolder skill = new SkillHolder();

		public NumberProperty castTime = new NumberProperty(-1f);
		public NumberProperty executionTime = new NumberProperty(-1f);
		public NumberProperty cooldownTime = new NumberProperty(-1f);
		public NumberProperty cooldownTimeBetween = new NumberProperty(-1f);
		public NumberProperty maxAmount = new NumberProperty(-1f);
		public NumberProperty startAmount = new NumberProperty(-1f);

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool InstantExecute(GameObject target, IAction[] actions, int index)
		{
			Skills charSkill = this.target.GetComponent<Skills>(target);
			if (charSkill == null)
			{
				Debug.LogError("Target Game Object does not have a Skills component");
				return false;
			}

			charSkill.ChangeSkillExecutionSettings(this.skill, this.castTime.GetValue(target), this.executionTime.GetValue(target), this.cooldownTime.GetValue(target), this.cooldownTimeBetween.GetValue(target), this.maxAmount.GetValue(target), this.startAmount.GetValue(target));
			charSkill.UpdateSkillsList();
			return true;
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

		public static new string NAME = "Skills/Change Skill Execution Settings From Variables";
		private const string NODE_TITLE = "{0} change skill {1} execution settings";

		// PROPERTIES: -------------------------------------------------------------------------------------------------

		private SerializedProperty spTarget;
		private SerializedProperty spSkill;
		private SerializedProperty spCastTime;
		private SerializedProperty spExecutionTime;
		private SerializedProperty spCooldownTime;
		private SerializedProperty spCooldownTimeBetween;
		private SerializedProperty spMaxAmount;
		private SerializedProperty spStartAmount;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle() =>
			string.Format(NODE_TITLE,
				(this.target != null) ? this.target.ToString() : "none",
				(this.skill.skill != null) ? this.skill.skill.skillName.content : "none"
				);

		protected override void OnEnableEditorChild()
		{
			this.spTarget = this.serializedObject.FindProperty("target");
			this.spSkill = this.serializedObject.FindProperty("skill");
			this.spCastTime = this.serializedObject.FindProperty("castTime");
			this.spExecutionTime = this.serializedObject.FindProperty("executionTime");
			this.spCooldownTime = this.serializedObject.FindProperty("cooldownTime");
			this.spCooldownTimeBetween = this.serializedObject.FindProperty("cooldownTimeBetween");
			this.spMaxAmount = this.serializedObject.FindProperty("maxAmount");
			this.spStartAmount = this.serializedObject.FindProperty("startAmount");
		}

		protected override void OnDisableEditorChild()
		{
			this.spTarget = null;
			this.spSkill = null;
			this.spCastTime = null;
			this.spExecutionTime = null;
			this.spCooldownTime = null;
			this.spCooldownTimeBetween = null;
			this.spMaxAmount = null;
			this.spStartAmount = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spTarget);
			EditorGUILayout.PropertyField(this.spSkill);

			if (this.spSkill.FindPropertyRelative("skill").objectReferenceValue != null)
			{
				if (this.castTime.value == -1f) this.castTime.value = this.skill.skill.castTime;
				if (this.executionTime.value == -1f) this.executionTime.value = this.skill.skill.executionTime;
				if (this.cooldownTime.value == -1f) this.cooldownTime.value = this.skill.skill.cooldownTime;
				if (this.cooldownTimeBetween.value == -1f) this.cooldownTimeBetween.value = this.skill.skill.cooldownTimeBetween;
				if (this.maxAmount.value == -1f) this.maxAmount.value = this.skill.skill.maxAmount;
				if (this.startAmount.value == -1f) this.startAmount.value = this.skill.skill.startAmount;

				switch (this.skill.skill.skillExecutionType)
				{
					case Skill.SkillExecutionType.CastExecute:
						EditorGUILayout.PropertyField(this.spCastTime);
						EditorGUILayout.PropertyField(this.spExecutionTime);
						EditorGUILayout.PropertyField(this.spCooldownTime);
						break;

					case Skill.SkillExecutionType.InstantExecute:
						EditorGUILayout.PropertyField(this.spExecutionTime);
						EditorGUILayout.PropertyField(this.spCooldownTime);
						break;

					case Skill.SkillExecutionType.PermanentExecute:
						EditorGUILayout.HelpBox("No fields to change", MessageType.Info);
						break;

					case Skill.SkillExecutionType.Stored:
						EditorGUILayout.PropertyField(this.spCastTime);
						EditorGUILayout.PropertyField(this.spExecutionTime);
						EditorGUILayout.PropertyField(this.spCooldownTime);
						EditorGUILayout.PropertyField(this.spCooldownTimeBetween);

						EditorGUILayout.Space();

						EditorGUILayout.PropertyField(this.spMaxAmount);
						EditorGUILayout.PropertyField(this.spStartAmount);
						break;
				}
			}

			EditorGUILayout.Space();

			EditorGUILayout.HelpBox("Hint: Type '-1' to reset the value.", MessageType.Info);

			this.serializedObject.ApplyModifiedProperties();
		}

#endif
	}
}