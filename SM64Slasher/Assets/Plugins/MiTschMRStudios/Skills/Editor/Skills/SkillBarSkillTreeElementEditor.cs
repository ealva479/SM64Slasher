namespace MiTschMR.Skills
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SkillBarSkillTreeElement))]
    public class SkillBarSkillTreeElementEditor : Editor
    {
        private const string PROP_KEYCODE = "keyCode";
        private const string PROP_ACCEPTSKILLTYPES = "acceptSkillTypes";
        private const string PROP_SKILLICON = "skillIcon";
        private const string PROP_SKILLNOTASSIGNED = "skillNotAssigned";
        private const string PROP_ICONSKILLBARCONFIGURATIONMODEOFF = "iconSkillBarConfigurationModeOff";
        private const string PROP_ICONSKILLBARCONFIGURATIONMODEON = "iconSkillBarConfigurationModeOn";
        private const string PROP_KEYCODETEXT = "keyCodeText";
        private const string PROP_SHOWKEYCODETEXT = "showKeyCodeText";
        private const string PROP_ALLOWREPLACEMENT = "allowReplacement";
        private const string PROP_ALLOWQUICKREMOVAL = "allowQuickRemoval";
        private const string PROP_EVENTONHOVERENTER = "eventOnHoverEnter";
        private const string PROP_EVENTONHOVEREXIT = "eventOnHoverExit";

        // PROPERTIES: ----------------------------------------------------------------------------

        protected SerializedProperty spKeyCode;
        protected SerializedProperty spAcceptSkillTypes;
        protected SerializedProperty spSkillIcon;
        protected SerializedProperty spSkillNotAssigned;
        protected SerializedProperty spIconSkillBarConfigurationModeOff;
        protected SerializedProperty spIconSkillBarConfigurationModeOn;
        protected SerializedProperty spKeyCodeText;
        protected SerializedProperty spShowKeyCodeText;
        protected SerializedProperty spAllowReplacement;
        protected SerializedProperty spAllowQuickRemoval;
        protected SerializedProperty spEventOnHoverEnter;
        protected SerializedProperty spEventOnHoverExit;

        protected virtual void OnEnable()
        {
            this.spKeyCode = serializedObject.FindProperty(PROP_KEYCODE);
            this.spAcceptSkillTypes = serializedObject.FindProperty(PROP_ACCEPTSKILLTYPES);
            this.spSkillIcon = serializedObject.FindProperty(PROP_SKILLICON);
            this.spSkillNotAssigned = serializedObject.FindProperty(PROP_SKILLNOTASSIGNED);
            this.spIconSkillBarConfigurationModeOff = serializedObject.FindProperty(PROP_ICONSKILLBARCONFIGURATIONMODEOFF);
            this.spIconSkillBarConfigurationModeOn = serializedObject.FindProperty(PROP_ICONSKILLBARCONFIGURATIONMODEON);
            this.spKeyCodeText = serializedObject.FindProperty(PROP_KEYCODETEXT);
            this.spShowKeyCodeText = serializedObject.FindProperty(PROP_SHOWKEYCODETEXT);
            this.spAllowReplacement = serializedObject.FindProperty(PROP_ALLOWREPLACEMENT);
            this.spAllowQuickRemoval = serializedObject.FindProperty(PROP_ALLOWQUICKREMOVAL);
            this.spEventOnHoverEnter = serializedObject.FindProperty(PROP_EVENTONHOVERENTER);
            this.spEventOnHoverExit = serializedObject.FindProperty(PROP_EVENTONHOVEREXIT);
        }

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;

            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spKeyCode);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(this.spAcceptSkillTypes);
            if (GUILayout.Button(new GUIContent("Reset"))) this.spAcceptSkillTypes.intValue = -1;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spSkillIcon);
            EditorGUILayout.PropertyField(this.spSkillNotAssigned);
            EditorGUILayout.PropertyField(this.spIconSkillBarConfigurationModeOff);
            EditorGUILayout.PropertyField(this.spIconSkillBarConfigurationModeOn);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(this.spShowKeyCodeText);
            EditorGUI.BeginDisabledGroup(this.spShowKeyCodeText.boolValue == false);
            EditorGUILayout.PropertyField(this.spKeyCodeText, GUIContent.none);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(this.spAllowReplacement);
            EditorGUILayout.PropertyField(this.spAllowQuickRemoval);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spEventOnHoverEnter);
            EditorGUILayout.PropertyField(this.spEventOnHoverExit);

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
