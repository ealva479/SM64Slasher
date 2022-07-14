namespace MiTschMR.Skills
{
    using UnityEditor;

    [CustomEditor(typeof(SkillTreeItemsUIAutomatical))]
    public class SkillTreeItemsUIAutomaticalEditor : Editor
    {
        private const string PROP_SKILLSCONTAINER = "skillsContainer";
        private const string PROP_PREFABSKILL = "prefabSkill";
        private const string PROP_PREFABSKILLCONNECTORENABLED = "prefabSkillConnectorEnabled";
        private const string PROP_PREFABSKILLCONNECTORDISABLED = "prefabSkillConnectorDisabled";
        private const string PROP_DISTANCEBETWEENSKILLSINLINE = "distanceBetweenSkillsInLine";
        private const string PROP_PREFABSKILLHOLDER = "prefabSkillHolder";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spSkillsContainer;
        private SerializedProperty spPrefabSkill;
        private SerializedProperty spPrefabSkillConnectorEnabled;
        private SerializedProperty spPrefabSkillConnectorDisabled;
        private SerializedProperty spDistanceBetweenSkillsInLine;
        private SerializedProperty spPrefabSkillHolder;

        protected virtual void OnEnable()
        {
            this.spSkillsContainer = serializedObject.FindProperty(PROP_SKILLSCONTAINER);
            this.spPrefabSkill = serializedObject.FindProperty(PROP_PREFABSKILL);
            this.spPrefabSkillConnectorEnabled = serializedObject.FindProperty(PROP_PREFABSKILLCONNECTORENABLED);
            this.spPrefabSkillConnectorDisabled = serializedObject.FindProperty(PROP_PREFABSKILLCONNECTORDISABLED);
            this.spDistanceBetweenSkillsInLine = serializedObject.FindProperty(PROP_DISTANCEBETWEENSKILLSINLINE);
            this.spPrefabSkillHolder = serializedObject.FindProperty(PROP_PREFABSKILLHOLDER);
        }

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;

            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spSkillsContainer);
            EditorGUILayout.PropertyField(this.spPrefabSkill);
            EditorGUILayout.PropertyField(this.spPrefabSkillConnectorEnabled);
            EditorGUILayout.PropertyField(this.spPrefabSkillConnectorDisabled);
            EditorGUILayout.PropertyField(this.spDistanceBetweenSkillsInLine);
            EditorGUILayout.PropertyField(spPrefabSkillHolder);

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
