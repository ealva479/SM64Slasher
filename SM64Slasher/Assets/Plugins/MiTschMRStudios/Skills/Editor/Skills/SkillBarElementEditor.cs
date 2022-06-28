namespace MiTschMR.Skills
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SkillBarElement))]
    public class SkillBarElementEditor : Editor
    {
        private const string PROP_TARGET = "target";
        private const string PROP_KEYCODE = "keyCode";
        private const string PROP_SKILLICON = "skillIcon";
        private const string PROP_SKILLNOTASSIGNED = "skillNotAssigned";
        private const string PROP_SKILLCOOLDOWNICON = "skillCooldownIcon";
        private const string PROP_SHOWSKILLEXECUTINGICON = "showSkillExecutingIcon";
        private const string PROP_SKILLEXECUTINGICON = "skillExecutingIcon";
        private const string PROP_SHOWKEYCODETEXT = "showKeyCodeText";
        private const string PROP_KEYCODETEXT = "keyCodeText";
        private const string PROP_SHOWCOOLDOWNCOUNTER = "showCooldownCounter";
        private const string PROP_COOLDOWNCOUNTER = "cooldownCounter";
        private const string PROP_SHOWEXECUTINGTIMECOUNTER = "showExecutingTimeCounter";
        private const string PROP_EXECUTINGTIMECOUNTER = "executingTimeCounter";
        private const string PROP_SHOWAMOUNTCOUNTER = "showAmountCounter";
        private const string PROP_AMOUNTCOUNTER = "amountCounter";

        // PROPERTIES: ----------------------------------------------------------------------------

        protected SerializedProperty spTarget;
        protected SerializedProperty spKeyCode;
        protected SerializedProperty spSkillIcon;
        protected SerializedProperty spSkillNotAssigned;
        protected SerializedProperty spSkillCooldownIcon;
        protected SerializedProperty spShowSkillExecutingIcon;
        protected SerializedProperty spSkillExecutingIcon;
        protected SerializedProperty spShowKeyCodeText;
        protected SerializedProperty spKeyCodeText;
        protected SerializedProperty spShowCooldownCounter;
        protected SerializedProperty spCooldownCounter;
        protected SerializedProperty spShowExecutingTimeCounter;
        protected SerializedProperty spExecutingTimeCounter;
        protected SerializedProperty spShowAmountCounter;
        protected SerializedProperty spAmountCounter;

        protected virtual void OnEnable()
        {
            this.spTarget = serializedObject.FindProperty(PROP_TARGET);
            this.spKeyCode = serializedObject.FindProperty(PROP_KEYCODE);
            this.spSkillIcon = serializedObject.FindProperty(PROP_SKILLICON);
            this.spSkillNotAssigned = serializedObject.FindProperty(PROP_SKILLNOTASSIGNED);
            this.spSkillCooldownIcon = serializedObject.FindProperty(PROP_SKILLCOOLDOWNICON);
            this.spShowSkillExecutingIcon = serializedObject.FindProperty(PROP_SHOWSKILLEXECUTINGICON);
            this.spSkillExecutingIcon = serializedObject.FindProperty(PROP_SKILLEXECUTINGICON);
            this.spShowKeyCodeText = serializedObject.FindProperty(PROP_SHOWKEYCODETEXT);
            this.spKeyCodeText = serializedObject.FindProperty(PROP_KEYCODETEXT);
            this.spShowCooldownCounter = serializedObject.FindProperty(PROP_SHOWCOOLDOWNCOUNTER);
            this.spCooldownCounter = serializedObject.FindProperty(PROP_COOLDOWNCOUNTER);
            this.spShowExecutingTimeCounter = serializedObject.FindProperty(PROP_SHOWEXECUTINGTIMECOUNTER);
            this.spExecutingTimeCounter = serializedObject.FindProperty(PROP_EXECUTINGTIMECOUNTER);
            this.spShowAmountCounter = serializedObject.FindProperty(PROP_SHOWAMOUNTCOUNTER);
            this.spAmountCounter = serializedObject.FindProperty(PROP_AMOUNTCOUNTER);
        }

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;

            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spTarget);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spKeyCode);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spSkillIcon);
            EditorGUILayout.PropertyField(this.spSkillNotAssigned);
            EditorGUILayout.PropertyField(this.spSkillCooldownIcon);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(this.spShowSkillExecutingIcon);
            EditorGUI.BeginDisabledGroup(this.spShowSkillExecutingIcon.boolValue == false);
            EditorGUILayout.PropertyField(this.spSkillExecutingIcon, GUIContent.none);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(this.spShowKeyCodeText);
            EditorGUI.BeginDisabledGroup(this.spShowKeyCodeText.boolValue == false);
            EditorGUILayout.PropertyField(this.spKeyCodeText, GUIContent.none);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(this.spShowCooldownCounter);
            EditorGUI.BeginDisabledGroup(this.spShowCooldownCounter.boolValue == false);
            EditorGUILayout.PropertyField(this.spCooldownCounter, GUIContent.none);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(this.spShowExecutingTimeCounter);
            EditorGUI.BeginDisabledGroup(this.spShowExecutingTimeCounter.boolValue == false);
            EditorGUILayout.PropertyField(this.spExecutingTimeCounter, GUIContent.none);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(this.spShowAmountCounter);
            EditorGUI.BeginDisabledGroup(this.spShowAmountCounter.boolValue == false);
            EditorGUILayout.PropertyField(this.spAmountCounter, GUIContent.none);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
