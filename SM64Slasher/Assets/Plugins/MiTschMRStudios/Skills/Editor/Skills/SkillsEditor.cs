namespace MiTschMR.Skills
{
    using System.Collections.Generic;
    using GameCreator.Core;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using UnityEngine;

    [CustomEditor(typeof(Skills))]
    public class SkillsEditor : Editor
    {
        private const string PROP_USESKILLBAR = "useSkillBar";
        private const string PROP_SKILLBARELEMENTS = "skillBarElements";
        private const string PROP_SAVESKILLS = "saveSkills";
        public class EditorData
        {
            public AnimBool isExpanded;

            public EditorData(SkillsEditor skillsEditor)
            {
                this.isExpanded = new AnimBool(skillsEditor.Repaint) { speed = 3.0f };
            }
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        protected Skills instance;
        protected SerializedProperty spUseSkillBar;
        protected SerializedProperty spSkillBarElements;
        protected SerializedProperty spSaveSkills;

        protected List<SkillAsset> skillsList = new List<SkillAsset>();
        protected List<EditorData> skillsEditorData;

        protected virtual void OnEnable()
        {
            if (target == null || serializedObject == null) return;
            if (this.instance == null) this.instance = (Skills)this.target;

            this.spUseSkillBar = serializedObject.FindProperty(PROP_USESKILLBAR);
            this.spSkillBarElements = serializedObject.FindProperty(PROP_SKILLBARELEMENTS);
            this.spSaveSkills = serializedObject.FindProperty(PROP_SAVESKILLS);

            this.skillsList = this.instance.skillsList;

            this.skillsEditorData = new List<EditorData>();

            int skillsSize = this.skillsList.Count;
            for (int i = 0; i < skillsSize; ++i)
            {
                this.skillsEditorData.Add(new EditorData(this));
            }
        }

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;

            if (this.skillsList == null) this.OnEnable();
            else this.skillsList = this.instance.skillsList;

            this.serializedObject.Update();

            int skillsListSize = this.skillsList.Count;
            for (int i = 0; i < skillsListSize; ++i)
            {
                SkillAsset skill = this.skillsList[i];

                this.PaintSkillHeader(skill, i);

                using (var group = new EditorGUILayout.FadeGroupScope(this.skillsEditorData[i].isExpanded.faded))
                {
                    if (group.visible)
                    {
                        EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());
                        EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
                        EditorGUILayout.LabelField(new GUIContent($"Is Executing: {skill.isExecuting}"));
                        EditorGUILayout.LabelField(new GUIContent($"Cooldown Time: {skill.cooldownTime}"));
                        if (skill.skillExecutionType == Skill.SkillExecutionType.Stored) EditorGUILayout.LabelField(new GUIContent($"Executions Left: {skill.currentAmount}"));
                        EditorGUILayout.LabelField(new GUIContent($"Cooldown Time Left: {skill.cooldownTimeLeft}"));

                        if (skill.reliesOnSkills != null)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.LabelField(new GUIContent("Relies on:"), EditorStyles.boldLabel);
                            for (int j = 0; j < skill.reliesOnSkills.Length; j++)
                            {
                                EditorGUILayout.LabelField(new GUIContent(skill.reliesOnSkills[j].skill.skillName.GetText() + ": " + skill.reliesOnSkills[j].skill.skillState));
                            }
                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndVertical();
                    }
                }
            }

            if (this.spUseSkillBar.boolValue = EditorGUILayout.ToggleLeft(
                this.spUseSkillBar.displayName,
                this.spUseSkillBar.boolValue
                ))
            {
                EditorGUILayout.PropertyField(this.spSkillBarElements);
            }

            EditorGUILayout.Space();

            this.spSaveSkills.boolValue = EditorGUILayout.ToggleLeft(
                this.spSaveSkills.displayName,
                this.spSaveSkills.boolValue
            );

            GlobalEditorID.Paint(this.instance);
            this.serializedObject.ApplyModifiedProperties();
        }

        // STATS PAINTER: -------------------------------------------------------------------------

        protected virtual void PaintSkillHeader(SkillAsset skill, int index)
        {
            EditorData skillEditorData = this.skillsEditorData[index];

            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, CoreGUIStyles.GetToggleButtonOff());
            Rect rectIcon = new Rect(rect.x, rect.y, 25f, rect.height);
            Rect rectButton = new Rect(rect.x + rectIcon.width, rect.y, rect.width - rectIcon.width, rect.height);

            if (skill.icon != null) GUI.DrawTexture(rectIcon, skill.icon.texture);

            string title = $"{skill.skillName.content} ({DatabaseSkills.Load().GetSkillTypesTypeByIndex(skill.skillType).id})";
            if (Application.isPlaying)
            {
                title = string.Format(
                    "<b>{0}</b>: {1}",
                    title,
                    skill.skillState
                );
            }

            GUIStyle style = (skillEditorData.isExpanded.target
                ? CoreGUIStyles.GetToggleButtonRightOn()
                : CoreGUIStyles.GetToggleButtonRightOff()
            );

            if (GUI.Button(rectButton, title, style)) skillEditorData.isExpanded.target = !skillEditorData.isExpanded.target;
        }
    }
}