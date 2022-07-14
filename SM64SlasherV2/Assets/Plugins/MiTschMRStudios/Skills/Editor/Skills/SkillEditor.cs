namespace MiTschMR.Skills
{
    using static GameCreator.Characters.CharacterEditor;
    using GameCreator.Core;
    using System.IO;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using UnityEngine;

    [CustomEditor(typeof(Skill))]
    public class SkillEditor : Editor
    {
        private const float ANIM_BOOL_SPEED = 3.0f;
        private const float BTN_HEIGHT = 24f;

        private const string PATH_PREFAB_SKILL_COND = "Assets/Plugins/MiTschMRStudiosData/Skills/Prefabs/Conditions/";
        private const string NAME_PREFAB_REQ_COND = "requirements.prefab";

        private const string PATH_PREFAB_SKILL = "Assets/Plugins/MiTschMRStudiosData/Skills/Prefabs/Actions/";
        private const string NAME_PREFAB_ACTIVATE = "activate.prefab";
        private const string NAME_PREFAB_CAST = "cast.prefab";
        private const string NAME_PREFAB_EXECUTE = "execute.prefab";
        private const string NAME_PREFAB_EXECUTEFAILED = "executeFailed.prefab";
        private const string NAME_PREFAB_FINISH = "finish.prefab";
        private const string NAME_PREFAB_RESET = "reset.prefab";

        private const string TEXTURE_TRIGGERS = "Assets/Plugins/GameCreator/Extra/Icons/Trigger/{0}.png";
        private const string TEXTURE_ACTIONS = "Assets/Plugins/GameCreator/Extra/Icons/Actions/{0}.png";

        private Section sectionSkillSettings;
        private Section sectionSkillEvents;

        private static readonly GUIContent[] EVENTS_OPTIONS = new GUIContent[]
        {
            new GUIContent("On Activate"),
            new GUIContent("On Cast"),
            new GUIContent("On Execute"),
            new GUIContent("On Finish")
        };

        private static DatabaseSkills DATABASE_SKILLS;

        private static readonly GUIContent LABEL_SKILLEXECUTIONSETTINGS = new GUIContent("Skill Execution Settings");
        private static readonly GUIContent LABEL_SKILLEXECUTIONCONDITIONSDESCRIPTION = new GUIContent("Conditions that need to be true to activate the skill");
        private static readonly GUIContent LABEL_SKILLEXECUTIONFAILEDDESCRIPTION = new GUIContent("Actions that are executed when the skill execution requirements are not met");
        private static readonly GUIContent LABEL_SKILLUNLOCKSETTINGS = new GUIContent("Skill Unlock Settings");
        private static readonly GUIContent LABEL_RELIESONSKILLSTITLE = new GUIContent("Relies on Skills");
        private static readonly GUIContent LABEL_SKILLREQUIREMENTSCONDITIONS = new GUIContent("Additional conditions that need to be true in order to unlock the skill");
        private static readonly GUIContent LABEL_CANCELRESETSKILLSSETTINGS = new GUIContent("Cancel / Reset Skills Settings");
        private static readonly GUIContent LABEL_CANCELRESETSKILLSSETTINGSDESCRIPTION = new GUIContent("Add here the actions that should be executed when this skill is being cancelled or reset");

        private static readonly GUILayoutOption miniButtonWidth = GUILayout.Width(80f);

        private static readonly string[] EVENTS_DESC = new string[]
        {
            "Actions executed when the skill is activated",
            "Actions executed when casting the skill",
            "Actions executed when executing the skill",
            "Actions executed when skill has finished executing"
        };

        private const string PROP_UUID = "uuid";
        private const string PROP_NAME = "skillName";
        private const string PROP_DESCRIPTION = "skillDescription";
        private const string PROP_ICON = "icon";

        private const string PROP_SKILLTYPE = "skillType";

        private const string PROP_SKILLEXECUTIONTYPE = "skillExecutionType";
        private const string PROP_CASTTIME = "castTime";
        private const string PROP_EXECUTIONTIME = "executionTime";
        private const string PROP_COOLDOWNTIME = "cooldownTime";
        private const string PROP_COOLDOWNTIMEBETWEEN = "cooldownTimeBetween";

        private const string PROP_USESKILLPOINTS = "useSkillPoints";
        private const string PROP_SKILLSTATE = "skillState";
        private const string PROP_SKILLPOINTSNEEDED = "skillPointsNeeded";

        private const string PROP_REQUIRESLEVEL = "requiresLevel";
        private const string PROP_LEVEL = "level";

        private const string PROP_RELIESONSKILLS = "reliesOnSkills";

        private const string PROP_ASSIGNABLETOSKILLLIST = "assignableToSkillBar";

        private const string PROP_MAXAMOUNT = "maxAmount";
        private const string PROP_STARTAMOUNT = "startAmount";

        private const string PROP_CONDITIONSEXECUTIONREQUIREMENTS = "conditionsExecutionRequirements";

        private const string PROP_ACTIONSEXECUTIONFAILED = "actionsExecutionFailed";

        private const string PROP_ACTIONSACTIVATE = "actionsOnActivate";
        private const string PROP_ACTIONSCAST = "actionsOnCast";
        private const string PROP_ACTIONSEXECUTE = "actionsOnExecute";
        private const string PROP_ACTIONSFINISH = "actionsOnFinish";

        private const string PROP_CONDITIONSREQUIREMENTS = "conditionsRequirements";

        private const string PROP_ACTIONSRESET = "actionsOnReset";

        public class SkillReturnOperation
        {
            public bool removeIndex = false;
            public bool duplicateIndex = false;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public SerializedProperty spUUID;
        private SerializedProperty spName;
        private SerializedProperty spDescription;
        
        private SerializedProperty spIcon;

        private SerializedProperty spSkillType;

        private SerializedProperty spSkillExecutionType;
        private SerializedProperty spCastTime;
        private SerializedProperty spExecutionTime;
        private SerializedProperty spCooldownTime;
        private SerializedProperty spCooldownTimeBetween;

        private SerializedProperty spUseSkillPoints;
        private SerializedProperty spSkillState;
        private SerializedProperty spSkillPointsNeeded;

        private SerializedProperty spRequiresLevel;
        private SerializedProperty spLevel;

        private SerializedProperty spReliesOnSkills;

        private int eventsToolbarIndex = 0;
        private SerializedProperty spAssignableToSkillList;

        private SerializedProperty spMaxAmount;
        private SerializedProperty spStartAmount;

        private SerializedProperty spConditionsExecutionRequirements;

        private SerializedProperty spActionsExecutionFailed;

        private SerializedProperty spActionsOnActivate;
        private SerializedProperty spActionsOnCast;
        private SerializedProperty spActionsOnExecute;
        private SerializedProperty spActionsOnFinish;

        private SerializedProperty spConditionsRequirements;

        private SerializedProperty spActionsOnReset;

        private IConditionsListEditor conditionsExecutionRequirementsEditor;

        private IActionsListEditor actionsExecutionFailedEditor;

        private IActionsListEditor actionsOnActivateEditor;
        private IActionsListEditor actionsOnCastEditor;
        private IActionsListEditor actionsOnExecuteEditor;
        private IActionsListEditor actionsOnFinishEditor;

        private IConditionsListEditor conditionsRequirementsEditor;

        private IActionsListEditor actionsOnResetEditor;

        private AnimBool animUnfold;

        // METHODS: ----------------------------------------------------------------------------------------------------

        public void OnEnable()
        {
            if (target == null || serializedObject == null) return;
            if (DATABASE_SKILLS == null) DATABASE_SKILLS = DatabaseSkills.LoadDatabase<DatabaseSkills>();

            this.sectionSkillSettings = new Section("Skill Settings", this.LoadIconFromTriggers("settings"), this.Repaint);
            this.sectionSkillEvents = new Section("Skill Events", this.LoadIconFromActions("Dispatch Event"), this.Repaint);

            this.spUUID = serializedObject.FindProperty(PROP_UUID);
            this.spName = serializedObject.FindProperty(PROP_NAME);
            this.spDescription = serializedObject.FindProperty(PROP_DESCRIPTION);

            this.spIcon = serializedObject.FindProperty(PROP_ICON);

            this.spSkillType = serializedObject.FindProperty(PROP_SKILLTYPE);

            this.spSkillExecutionType = serializedObject.FindProperty(PROP_SKILLEXECUTIONTYPE);
            this.spCastTime = serializedObject.FindProperty(PROP_CASTTIME);
            this.spExecutionTime = serializedObject.FindProperty(PROP_EXECUTIONTIME);
            this.spCooldownTime = serializedObject.FindProperty(PROP_COOLDOWNTIME);
            this.spCooldownTimeBetween = serializedObject.FindProperty(PROP_COOLDOWNTIMEBETWEEN);

            this.spConditionsExecutionRequirements = serializedObject.FindProperty(PROP_CONDITIONSEXECUTIONREQUIREMENTS);

            this.spActionsExecutionFailed = serializedObject.FindProperty(PROP_ACTIONSEXECUTIONFAILED);

            this.spUseSkillPoints = serializedObject.FindProperty(PROP_USESKILLPOINTS);
            this.spSkillState = serializedObject.FindProperty(PROP_SKILLSTATE);
            this.spSkillPointsNeeded = serializedObject.FindProperty(PROP_SKILLPOINTSNEEDED);

            this.spRequiresLevel = serializedObject.FindProperty(PROP_REQUIRESLEVEL);
            this.spLevel = serializedObject.FindProperty(PROP_LEVEL);

            this.spReliesOnSkills = serializedObject.FindProperty(PROP_RELIESONSKILLS);

            this.spAssignableToSkillList = serializedObject.FindProperty(PROP_ASSIGNABLETOSKILLLIST);

            this.spMaxAmount = serializedObject.FindProperty(PROP_MAXAMOUNT);
            this.spStartAmount = serializedObject.FindProperty(PROP_STARTAMOUNT);

            this.spActionsOnActivate = serializedObject.FindProperty(PROP_ACTIONSACTIVATE);
            this.spActionsOnCast = serializedObject.FindProperty(PROP_ACTIONSCAST);
            this.spActionsOnExecute = serializedObject.FindProperty(PROP_ACTIONSEXECUTE);
            this.spActionsOnFinish = serializedObject.FindProperty(PROP_ACTIONSFINISH);

            this.spConditionsRequirements = serializedObject.FindProperty(PROP_CONDITIONSREQUIREMENTS);

            this.spActionsOnReset = serializedObject.FindProperty(PROP_ACTIONSRESET);

            this.SetupConditionsList(
                ref this.spConditionsExecutionRequirements,
                ref this.conditionsExecutionRequirementsEditor,
                PATH_PREFAB_SKILL_COND,
                NAME_PREFAB_REQ_COND
                );

            this.SetupActionsList(
                ref this.spActionsExecutionFailed,
                ref this.actionsExecutionFailedEditor,
                PATH_PREFAB_SKILL,
                NAME_PREFAB_EXECUTEFAILED
                );

            this.SetupActionsList(
                ref this.spActionsOnActivate,
                ref this.actionsOnActivateEditor,
                PATH_PREFAB_SKILL,
                NAME_PREFAB_ACTIVATE
            );

            this.SetupActionsList(
                ref this.spActionsOnCast,
                ref this.actionsOnCastEditor,
                PATH_PREFAB_SKILL,
                NAME_PREFAB_CAST
            );

            this.SetupActionsList(
                ref this.spActionsOnExecute,
                ref this.actionsOnExecuteEditor,
                PATH_PREFAB_SKILL,
                NAME_PREFAB_EXECUTE
            );

            this.SetupActionsList(
                ref this.spActionsOnFinish,
                ref this.actionsOnFinishEditor,
                PATH_PREFAB_SKILL,
                NAME_PREFAB_FINISH
            );
            
            this.SetupConditionsList(
                ref this.spConditionsRequirements,
                ref this.conditionsRequirementsEditor,
                PATH_PREFAB_SKILL_COND,
                NAME_PREFAB_REQ_COND
                );

            this.SetupActionsList(
                ref this.spActionsOnReset,
                ref this.actionsOnResetEditor,
                PATH_PREFAB_SKILL,
                NAME_PREFAB_RESET
            );

            this.animUnfold = new AnimBool(false) { speed = ANIM_BOOL_SPEED };
            this.animUnfold.valueChanged.AddListener(this.Repaint);
        }

        private void SetupConditionsList(ref SerializedProperty sp, ref IConditionsListEditor editor,
                                      string prefabPath, string prefabName)
        {
            if (sp.objectReferenceValue == null)
            {
                GameCreatorUtilities.CreateFolderStructure(prefabPath);
                string conditionsPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(
                    prefabPath, prefabName
                ));

                GameObject sceneInstance = new GameObject("Conditions");
                sceneInstance.AddComponent<IConditionsList>();

                GameObject prefabInstance = PrefabUtility.SaveAsPrefabAsset(sceneInstance, conditionsPath);
                DestroyImmediate(sceneInstance);

                sp.objectReferenceValue = prefabInstance.GetComponent<IConditionsList>();
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            }

            editor = Editor.CreateEditor(
                sp.objectReferenceValue,
                typeof(IConditionsListEditor)
            ) as IConditionsListEditor;
        }

        private void SetupActionsList(ref SerializedProperty sp, ref IActionsListEditor editor,
                                      string prefabPath, string prefabName)
        {
            if (sp.objectReferenceValue == null)
            {
                GameCreatorUtilities.CreateFolderStructure(prefabPath);
                string actionsPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(
                    prefabPath, prefabName
                ));

                GameObject sceneInstance = new GameObject("Actions");
                sceneInstance.AddComponent<Actions>();

                GameObject prefabInstance = PrefabUtility.SaveAsPrefabAsset(sceneInstance, actionsPath);
                DestroyImmediate(sceneInstance);

                Actions prefabActions = prefabInstance.GetComponent<Actions>();
                prefabActions.destroyAfterFinishing = true;
                sp.objectReferenceValue = prefabActions.actionsList;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            }

            editor = Editor.CreateEditor(
                sp.objectReferenceValue,
                typeof(IActionsListEditor)
            ) as IActionsListEditor;
        }

        public void OnDestroySkill()
        {
            IConditionsList list = (IConditionsList)this.spConditionsExecutionRequirements.objectReferenceValue;
            IActionsList list1 = (IActionsList)this.spActionsExecutionFailed.objectReferenceValue;
            IActionsList list2 = (IActionsList)this.spActionsOnActivate.objectReferenceValue;
            IActionsList list3 = (IActionsList)this.spActionsOnCast.objectReferenceValue;
            IActionsList list4 = (IActionsList)this.spActionsOnExecute.objectReferenceValue;
            IActionsList list5 = (IActionsList)this.spActionsOnFinish.objectReferenceValue;
            IConditionsList list6 = (IConditionsList)this.spConditionsRequirements.objectReferenceValue;
            IActionsList list7 = (IActionsList)this.spActionsOnReset.objectReferenceValue;

            for (int i = 0; i < spReliesOnSkills.arraySize; i++)
            {
                Object skillToDelete = spReliesOnSkills.GetArrayElementAtIndex(i).FindPropertyRelative("skill").objectReferenceValue;
                if (skillToDelete != null) AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(skillToDelete));
            }

            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(list.gameObject));
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(list1.gameObject));
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(list2.gameObject));
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(list3.gameObject));
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(list4.gameObject));
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(list5.gameObject));
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(list6.gameObject));
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(list7.gameObject));
            AssetDatabase.SaveAssets();
        }

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;
            EditorGUILayout.HelpBox(
                "This Skill can only be edited in the Skills section of the Preferences window",
                MessageType.Info
            );

            if (GUILayout.Button("Open Preferences")) PreferencesWindow.OpenWindow();
        }

        public SkillReturnOperation OnPreferencesWindowGUI(DatabaseSkillsEditor skillsEditor, int index)
        {
            serializedObject.Update();
            skillsEditor.searchText = skillsEditor.searchText.ToLower();
            string spNameString = this.spName.FindPropertyRelative("content").stringValue;
            string spDescString = this.spDescription.FindPropertyRelative("content").stringValue;

            if (!string.IsNullOrEmpty(skillsEditor.searchText) &&
                !spNameString.ToLower().Contains(skillsEditor.searchText) &&
                !spDescString.ToLower().Contains(skillsEditor.searchText))
            {
                return new SkillReturnOperation();
            }

            SkillReturnOperation result = this.PaintHeader(skillsEditor, index);
            using (var group = new EditorGUILayout.FadeGroupScope(this.animUnfold.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());
                    this.PaintContent();
                    EditorGUILayout.EndVertical();
                }
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return result;
        }

        private SkillReturnOperation PaintHeader(DatabaseSkillsEditor skillsEditor, int index)
        {
            bool removeItem = false;
            bool duplicateIndex = false;

            EditorGUILayout.BeginHorizontal();

            bool forceSortRepaint = false;
            if (skillsEditor.skillsHandleRect.ContainsKey(index))
            {
                EditorGUIUtility.AddCursorRect(skillsEditor.skillsHandleRect[index], MouseCursor.Pan);
                forceSortRepaint = skillsEditor.editorSortableListSkills.CaptureSortEvents(
                    skillsEditor.skillsHandleRect[index], index
                );
            }

            if (forceSortRepaint) skillsEditor.Repaint();

            GUILayout.Label("=", CoreGUIStyles.GetButtonLeft(), GUILayout.Width(25f), GUILayout.Height(BTN_HEIGHT));
            if (Event.current.type == EventType.Repaint)
            {
                Rect dragRect = GUILayoutUtility.GetLastRect();

                if (skillsEditor.skillsHandleRect.ContainsKey(index)) skillsEditor.skillsHandleRect[index] = dragRect;
                else skillsEditor.skillsHandleRect.Add(index, dragRect);
            }

            if (skillsEditor.skillsHandleRectRow.ContainsKey(index))
            {
                skillsEditor.editorSortableListSkills.PaintDropPoints(
                    skillsEditor.skillsHandleRectRow[index],
                    index,
                    skillsEditor.spSkills.arraySize
                );
            }

            string name = (this.animUnfold.target ? "▾ " : "▸ ");
            string spNameString = this.spName.FindPropertyRelative("content").stringValue;
            name += (string.IsNullOrEmpty(spNameString) ? "No-name" : spNameString);

            GUIStyle style = (this.animUnfold.target
                ? CoreGUIStyles.GetToggleButtonMidOn()
                : CoreGUIStyles.GetToggleButtonMidOff()
            );

            Texture2D skillIcon = SkillEditorUtils.GetIcon();

            if (GUILayout.Button(new GUIContent(name, skillIcon), style, GUILayout.Height(BTN_HEIGHT))) this.animUnfold.target = !this.animUnfold.value;

            GUIContent gcDuplicate = ClausesUtilities.Get(ClausesUtilities.Icon.Duplicate);
            if (GUILayout.Button(gcDuplicate, CoreGUIStyles.GetButtonMid(), GUILayout.Width(25), GUILayout.Height(BTN_HEIGHT))) duplicateIndex = true;

            GUIContent gcDelete = ClausesUtilities.Get(ClausesUtilities.Icon.Delete);
            if (GUILayout.Button(gcDelete, CoreGUIStyles.GetButtonRight(), GUILayout.Width(25), GUILayout.Height(BTN_HEIGHT))) removeItem = true;

            EditorGUILayout.EndHorizontal();
            if (Event.current.type == EventType.Repaint)
            {
                Rect rect = GUILayoutUtility.GetLastRect();
                if (skillsEditor.skillsHandleRectRow.ContainsKey(index)) skillsEditor.skillsHandleRectRow[index] = rect;
                else skillsEditor.skillsHandleRectRow.Add(index, rect);
            }

            SkillReturnOperation result = new SkillReturnOperation();
            if (removeItem) result.removeIndex = true;
            if (duplicateIndex) result.duplicateIndex = true;

            return result;
        }

        private void PaintContent()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.spUUID);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(this.spName);
            EditorGUILayout.PropertyField(this.spDescription);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spIcon);

            EditorGUILayout.Space();

            PaintSectionSkillSettings();

            EditorGUILayout.Space();

            PaintSectionSkillEvents();
        }

        private void PaintSectionSkillSettings()
        {
            this.sectionSkillSettings.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionSkillSettings.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spSkillType);
                    
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(this.spAssignableToSkillList);

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField(LABEL_SKILLEXECUTIONSETTINGS, EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(this.spSkillExecutionType);
                    if (this.spSkillExecutionType.enumValueIndex == 1 || this.spSkillExecutionType.enumValueIndex == 3) EditorGUILayout.PropertyField(this.spCastTime);
                    if (this.spSkillExecutionType.enumValueIndex != 2) EditorGUILayout.PropertyField(this.spExecutionTime);
                    if (this.spSkillExecutionType.enumValueIndex != 2) EditorGUILayout.PropertyField(this.spCooldownTime);
                    if (this.spSkillExecutionType.enumValueIndex == 3) EditorGUILayout.PropertyField(this.spCooldownTimeBetween);


                    EditorGUILayout.Space();

                    if (this.spSkillExecutionType.enumValueIndex == 3) EditorGUILayout.PropertyField(this.spMaxAmount);
                    if (this.spSkillExecutionType.enumValueIndex == 3) EditorGUILayout.PropertyField(this.spStartAmount);

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField(LABEL_SKILLEXECUTIONCONDITIONSDESCRIPTION);
                    if (this.conditionsExecutionRequirementsEditor != null) this.conditionsExecutionRequirementsEditor.OnInspectorGUI();
                    
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField(LABEL_SKILLEXECUTIONFAILEDDESCRIPTION);
                    if (this.actionsExecutionFailedEditor != null) this.actionsExecutionFailedEditor.OnInspectorGUI();

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField(LABEL_SKILLUNLOCKSETTINGS, EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(this.spSkillState);

                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(this.spUseSkillPoints);
                    EditorGUI.BeginDisabledGroup(this.spUseSkillPoints.boolValue == false);
                    EditorGUILayout.PropertyField(this.spSkillPointsNeeded);
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(this.spRequiresLevel);
                    EditorGUI.BeginDisabledGroup(this.spRequiresLevel.boolValue == false);
                    EditorGUILayout.PropertyField(this.spLevel);
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField(LABEL_RELIESONSKILLSTITLE, EditorStyles.boldLabel);
                    
                    for (int i = 0; i < this.spReliesOnSkills.arraySize; i++)
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(this.spReliesOnSkills.GetArrayElementAtIndex(i));
                        if (GUILayout.Button("Add", EditorStyles.miniButtonLeft, miniButtonWidth))
                        {
                            SkillRelyOn skill = DATABASE_SKILLS.GetSkillListSkillByIndex(0).CopySkillToSkillRelyOn();
                            skill = Skill.CreateSkillRelyOnInstance(skill);
                            SerializedProperty arrayElementSkill = this.spReliesOnSkills.GetArrayElementAtIndex(i).FindPropertyRelative("skill");
                            this.spReliesOnSkills.InsertArrayElementAtIndex(i);
                            arrayElementSkill.objectReferenceValue = skill;
                        }
                            
                        if (GUILayout.Button("Remove", EditorStyles.miniButtonRight, miniButtonWidth))
                        {
                            SkillRelyOn skillToRemove = (SkillRelyOn)this.spReliesOnSkills.GetArrayElementAtIndex(i).FindPropertyRelative("skill").objectReferenceValue;
                            Skill.DeleteSkillRelyOnInstance(skillToRemove);
                            if (this.spReliesOnSkills.arraySize > 1) this.spReliesOnSkills.DeleteArrayElementAtIndex(i);
                        }
                            
                        EditorGUILayout.EndHorizontal();
                        
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField(LABEL_SKILLREQUIREMENTSCONDITIONS);
                    if (this.conditionsRequirementsEditor != null) this.conditionsRequirementsEditor.OnInspectorGUI();

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField(LABEL_CANCELRESETSKILLSSETTINGS, EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox(LABEL_CANCELRESETSKILLSSETTINGSDESCRIPTION.text, MessageType.Info);
                    if (this.actionsOnResetEditor != null) this.actionsOnResetEditor.OnInspectorGUI();

                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void PaintSectionSkillEvents()
        {
            this.sectionSkillEvents.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionSkillEvents.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    this.eventsToolbarIndex = GUILayout.Toolbar(this.eventsToolbarIndex, EVENTS_OPTIONS);
                    EditorGUILayout.HelpBox(EVENTS_DESC[this.eventsToolbarIndex], MessageType.Info);
                    switch (this.eventsToolbarIndex)
                    {
                        case 0: if (this.actionsOnActivateEditor != null) this.actionsOnActivateEditor.OnInspectorGUI(); break;
                        case 1: if (this.actionsOnCastEditor != null) this.actionsOnCastEditor.OnInspectorGUI(); break;
                        case 2: if (this.actionsOnExecuteEditor != null) this.actionsOnExecuteEditor.OnInspectorGUI(); break;
                        case 3: if (this.actionsOnFinishEditor != null) this.actionsOnFinishEditor.OnInspectorGUI(); break;
                    }

                    EditorGUILayout.EndVertical();
                }
            }
        }

        protected Texture2D LoadIconFromTriggers(string name) => AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(TEXTURE_TRIGGERS, name));
        protected Texture2D LoadIconFromActions(string name) => AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(TEXTURE_ACTIONS, name));
    }
}