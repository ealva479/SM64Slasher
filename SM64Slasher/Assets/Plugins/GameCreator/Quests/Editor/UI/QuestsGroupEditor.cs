namespace GameCreator.Quests.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    [CustomEditor(typeof(QuestsGroup))]
    public class QuestsGroupEditor : Editor
    {
        private static readonly GUIContent GUICONTENT_INIT = new GUIContent("Initialization");
        private static readonly GUIContent GUICONTENT_FILTER = new GUIContent("Filter");

        private string[] OPTIONS_FILTER = new string[]
        {
            "Inactive",
            "Active",
            "Complete",
            "Failed",
            "Abandoned"
        };

        // PROPERTIES: ----------------------------------------------------------------------------

        public SerializedProperty spInitType;
        public SerializedProperty spStatusFilter;
        public SerializedProperty spOnlyTracked;

        public QuestUIUtilities.Section sectionList;
        public SerializedProperty spToggleGroup;
        public SerializedProperty spContainer;
        public SerializedProperty spPrefab;

        public QuestUIUtilities.Section sectionQuestUI;
        public SerializedProperty spQuestUI;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            this.spInitType = serializedObject.FindProperty("initType");
            this.spStatusFilter = serializedObject.FindProperty("statusFilter");
            this.spOnlyTracked = serializedObject.FindProperty("onlyTracked");

            this.sectionList = new QuestUIUtilities.Section(
                "List",
                "List.png",
                this.Repaint
            );

            this.spToggleGroup = serializedObject.FindProperty("toggleGroup");
            this.spContainer = serializedObject.FindProperty("container");
            this.spPrefab = serializedObject.FindProperty("prefab");

            this.sectionQuestUI = new QuestUIUtilities.Section(
                "Quest UI",
                "QuestUI.png",
                this.Repaint
            );

            this.spQuestUI = serializedObject.FindProperty("questUI");
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spInitType, GUICONTENT_INIT);

            this.spStatusFilter.intValue = EditorGUILayout.MaskField(
                GUICONTENT_FILTER,
                this.spStatusFilter.intValue,
                OPTIONS_FILTER
            );

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(this.spOnlyTracked);
            EditorGUI.indentLevel--;

            this.PaintList();
            this.PaintQuestUI();

            EditorGUILayout.Space();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void PaintList()
        {
            this.sectionList.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionList.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spToggleGroup);
                    EditorGUILayout.PropertyField(this.spContainer);
                    EditorGUILayout.PropertyField(this.spPrefab);

                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void PaintQuestUI()
        {
            this.sectionQuestUI.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionQuestUI.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spQuestUI);

                    EditorGUILayout.EndVertical();
                }
            }
        }
    }
}