namespace MiTschMR.Skills
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SkillUIManual))]
    public class SkillUIManualEditor : Editor
    {
        private const string PROP_SKILLTREEUIMANAGER = "skillTreeUIManager";
        private const string PROP_SKILL = "skill";
        private const string PROP_SKILLCONNECTORENABLED = "skillConnectorEnabled";
        private const string PROP_SKILLCONNECTORDISABLED = "skillConnectorDisabled";
        private const string PROP_SKILLICONENABLED = "skillIconEnabled";
        private const string PROP_SKILLICONDISABLED = "skillIconDisabled";
        private const string PROP_TOOLTIPICON = "tooltipIcon";
        private const string PROP_TOOLTIPNAME = "tooltipName";
        private const string PROP_TOOLTIPDESCRIPTION = "tooltipDescription";
        private const string PROP_TOOLTIPLEVELREQUIREDVALUE = "tooltipLevelRequiredValue";
        private const string PROP_TOOLTIPSKILLPOINTSREQUIREDVALUE = "tooltipSkillPointsRequiredValue";
        private const string PROP_TOOLTIPSKILLSTATEVALUE = "tooltipSkillStateValue";
        private const string PROP_TOOLTIPSKILLSTATEVALUECOLORLOCKED = "tooltipSkillStateValueColorLocked";
        private const string PROP_TOOLTIPSKILLSTATEVALUECOLORUNLOCKED = "tooltipSkillStateValueColorUnlocked";
        private const string PROP_IMAGELOCKED = "imageLocked";
        private const string PROP_IMAGEUNLOCKED = "imageUnlocked";
        private const string PROP_SKILLTRANSFORM = "skillTransform";
        private const string PROP_PREFABCONTEXTMENU = "prefabContextMenu";
        private const string PROP_EVENTONHOVERENTER = "eventOnHoverEnter";
        private const string PROP_EVENTONHOVEREXIT = "eventOnHoverExit";

        private const string MSG_NOTE_SKILLTREEUIMANAGER_SKILL = "Only define these two variables when you are in the manual skill tree prefab!";
        private const string MSG_NOTE_SKILLCONNECTORS = "Only define these two variables when this skill relies on another!";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spSkillTreeUIManager;
        private SerializedProperty spSkill;
        private SerializedProperty spSkillConnectorEnabled;
        private SerializedProperty spSkillConnectorDisabled;
        private SerializedProperty spSkillIconEnabled;
        private SerializedProperty spSkillIconDisabled;
        private SerializedProperty spTooltipIcon;
        private SerializedProperty spTooltipName;
        private SerializedProperty spTooltipDescription;
        private SerializedProperty spTooltipLevelRequiredValue;
        private SerializedProperty spTooltipSkillPointsRequiredValue;
        private SerializedProperty spTooltipSkillStateValue;
        private SerializedProperty spTooltipSkillStateValueColorLocked;
        private SerializedProperty spTooltipSkillStateValueColorUnlocked;
        private SerializedProperty spImageLocked;
        private SerializedProperty spImageUnlocked;
        private SerializedProperty spSkillTransform;
        private SerializedProperty spPrefabContextMenu;
        private SerializedProperty spEventOnHoverEnter;
        private SerializedProperty spEventOnHoverExit;

        protected virtual void OnEnable()
        {
            this.spSkillTreeUIManager = serializedObject.FindProperty(PROP_SKILLTREEUIMANAGER);
            this.spSkill = serializedObject.FindProperty(PROP_SKILL);
            this.spSkillConnectorEnabled = serializedObject.FindProperty(PROP_SKILLCONNECTORENABLED);
            this.spSkillConnectorDisabled = serializedObject.FindProperty(PROP_SKILLCONNECTORDISABLED);
            this.spSkillIconEnabled = serializedObject.FindProperty(PROP_SKILLICONENABLED);
            this.spSkillIconDisabled = serializedObject.FindProperty(PROP_SKILLICONDISABLED);
            this.spTooltipIcon = serializedObject.FindProperty(PROP_TOOLTIPICON);
            this.spTooltipName = serializedObject.FindProperty(PROP_TOOLTIPNAME);
            this.spTooltipDescription = serializedObject.FindProperty(PROP_TOOLTIPDESCRIPTION);
            this.spTooltipLevelRequiredValue = serializedObject.FindProperty(PROP_TOOLTIPLEVELREQUIREDVALUE);
            this.spTooltipSkillPointsRequiredValue = serializedObject.FindProperty(PROP_TOOLTIPSKILLPOINTSREQUIREDVALUE);
            this.spTooltipSkillStateValue = serializedObject.FindProperty(PROP_TOOLTIPSKILLSTATEVALUE);
            this.spTooltipSkillStateValueColorLocked = serializedObject.FindProperty(PROP_TOOLTIPSKILLSTATEVALUECOLORLOCKED);
            this.spTooltipSkillStateValueColorUnlocked = serializedObject.FindProperty(PROP_TOOLTIPSKILLSTATEVALUECOLORUNLOCKED);
            this.spImageLocked = serializedObject.FindProperty(PROP_IMAGELOCKED);
            this.spImageUnlocked = serializedObject.FindProperty(PROP_IMAGEUNLOCKED);
            this.spSkillTransform = serializedObject.FindProperty(PROP_SKILLTRANSFORM);
            this.spPrefabContextMenu = serializedObject.FindProperty(PROP_PREFABCONTEXTMENU);
            this.spEventOnHoverEnter = serializedObject.FindProperty(PROP_EVENTONHOVERENTER);
            this.spEventOnHoverExit = serializedObject.FindProperty(PROP_EVENTONHOVEREXIT);
        }

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;

            this.serializedObject.Update();

            EditorGUILayout.HelpBox(MSG_NOTE_SKILLTREEUIMANAGER_SKILL, MessageType.Info);
            EditorGUILayout.PropertyField(this.spSkillTreeUIManager);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(this.spSkill);
            if (GUILayout.Button(new GUIContent("Reset")) && this.spSkill.FindPropertyRelative("skill").objectReferenceValue != null)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(spSkill.FindPropertyRelative("skill").objectReferenceValue));
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(MSG_NOTE_SKILLCONNECTORS, MessageType.Info);
            EditorGUILayout.PropertyField(this.spSkillConnectorEnabled);
            EditorGUILayout.PropertyField(this.spSkillConnectorDisabled);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spSkillIconEnabled);
            EditorGUILayout.PropertyField(this.spSkillIconDisabled);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spTooltipIcon);
            EditorGUILayout.PropertyField(this.spTooltipName);
            EditorGUILayout.PropertyField(this.spTooltipDescription);
            EditorGUILayout.PropertyField(spTooltipLevelRequiredValue);
            EditorGUILayout.PropertyField(spTooltipSkillPointsRequiredValue);
            EditorGUILayout.PropertyField(spTooltipSkillStateValue);
            EditorGUILayout.PropertyField(spTooltipSkillStateValueColorLocked);
            EditorGUILayout.PropertyField(spTooltipSkillStateValueColorUnlocked);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spImageLocked);
            EditorGUILayout.PropertyField(this.spImageUnlocked);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spSkillTransform);
            EditorGUILayout.PropertyField(this.spPrefabContextMenu);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spEventOnHoverEnter);
            EditorGUILayout.PropertyField(this.spEventOnHoverExit);

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
