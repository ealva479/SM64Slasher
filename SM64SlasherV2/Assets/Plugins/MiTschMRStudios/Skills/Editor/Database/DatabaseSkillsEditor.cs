namespace MiTschMR.Skills
{
	using GameCreator.Core;
	using GameCreator.ModuleManager;
	using System;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(DatabaseSkills))]
	public class DatabaseSkillsEditor : IDatabaseEditor
	{
		private const string PROP_SKILLS_LIST = "skillList";
		private const string PROP_SKILLS = "skills";
		private const string PROP_SKILL_TYPES = "skillTypes";

		private const string PROP_SKILLS_SETTINGS = "skillsSettings";

		private const string PROP_SKILLTREESELECTION = "skillTreeSelection";
		private const string PROP_SKILLTREE_UI_PREFAB_AUTOMATIC = "skillTreeUIPrefabAutomatic";
		private const string PROP_SKILLTREE_UI_PREFAB_MANUAL = "skillTreeUIPrefabManual";

		private const string PROP_PAUSETIMEONUI = "pauseTimeOnUI";

		private const string PROP_DRAGDROPSKILLS = "dragDropSkills";
		private const string PROP_DRAGDROPHOTSPOT = "dragDropHotspot";

		private const string PROP_LEVELSELECTION = "levelSelection";
		private const string PROP_LEVELVARIABLE = "levelVariable";
		private const string PROP_LEVELSTAT = "levelStat";

		private const string PROP_SKILLPOINTSSELECTION = "skillPointsSelection";
		private const string PROP_SKILLPOINTSVARIABLE = "skillPointsVariable";
		private const string PROP_SKILLPOINTSSTAT = "skillPointsStat";

		private const string MSG_EMPTY_SKILLLIST = "There are no skills. Add one by clicking the 'Add Skill' button";
		private const string MSG_EMPTY_SKILLTREEPREFAB = "If no skill tree prefab is selected, then the default one for the selected type will be chosen.";
		private const string MSG_EMPTY_LEVELSELECTION = "Please set the target variable or stat for the level.";
		private const string MSG_EMPTY_SKILLPOINTSSELECTION = "Please set the target variable or stat for the skill points.";

		private const string SEARCHBOX_NAME = "searchbox";

		private class SkillsData
		{
			public SkillEditor cachedEditor;
			public SerializedProperty spSkill;

			public SkillsData(SerializedProperty skill)
			{
				this.spSkill = skill;

				Editor cache = this.cachedEditor;
				Editor.CreateCachedEditor(skill.objectReferenceValue, typeof(SkillEditor), ref cache);
				this.cachedEditor = (SkillEditor)cache;
			}
		}

		protected static readonly GUIContent[] TAB_NAMES = new GUIContent[]
		{
			new GUIContent("Skills"),
			new GUIContent("Types"),
			new GUIContent("Settings")
		};

		// PROPERTIES: -------------------------------------------------------------------------------------------------

		protected int tabIndex = 0;

		public SerializedProperty spSkills;
		public SerializedProperty spSkillTypes;

		private SerializedProperty spSkillTreeSelection;
		private SerializedProperty spSkillTreeUIPrefabAutomatic;
		private SerializedProperty spSkillTreeUIPrefabManual;

		private SerializedProperty spPauseTimeOnUI;

		private SerializedProperty spDragDropSkills;
		private SerializedProperty spDragDropHotspot;

		private SerializedProperty spLevelSelection;
		private SerializedProperty spLevelVariable;
		private SerializedProperty spLevelStat;

		private SerializedProperty spSkillPointsSelection;
		private SerializedProperty spSkillPointsVariable;
		private SerializedProperty spSkillPointsStat;

		private List<SkillsData> skillsData;

		private GUIStyle searchFieldStyle;
		private GUIStyle searchCloseOnStyle;
		private GUIStyle searchCloseOffStyle;

		public string searchText = "";
		public bool searchFocus = true;

		public EditorSortableList editorSortableListSkills;

		public Dictionary<int, Rect> skillsHandleRect = new Dictionary<int, Rect>();
		public Dictionary<int, Rect> recipesHandleRect = new Dictionary<int, Rect>();

		public Dictionary<int, Rect> skillsHandleRectRow = new Dictionary<int, Rect>();
		public Dictionary<int, Rect> recipesHandleRectRow = new Dictionary<int, Rect>();

		protected string versionSkills = "1.1.7";
		protected string versionSkillsExamples = "1.0.4";

		// INITIALIZE: -------------------------------------------------------------------------------------------------

		protected virtual void OnEnable()
		{
			if (target == null || serializedObject == null) return;

			SerializedProperty spSkillsList = serializedObject.FindProperty(PROP_SKILLS_LIST);
			this.spSkills = spSkillsList.FindPropertyRelative(PROP_SKILLS);
			this.spSkillTypes = spSkillsList.FindPropertyRelative(PROP_SKILL_TYPES);

			SerializedProperty spSkillsSettings = serializedObject.FindProperty(PROP_SKILLS_SETTINGS);
			this.spSkillTreeSelection = spSkillsSettings.FindPropertyRelative(PROP_SKILLTREESELECTION);
			this.spSkillTreeUIPrefabAutomatic = spSkillsSettings.FindPropertyRelative(PROP_SKILLTREE_UI_PREFAB_AUTOMATIC);
			this.spSkillTreeUIPrefabManual = spSkillsSettings.FindPropertyRelative(PROP_SKILLTREE_UI_PREFAB_MANUAL);

			this.spPauseTimeOnUI = spSkillsSettings.FindPropertyRelative(PROP_PAUSETIMEONUI);

			this.spDragDropSkills = spSkillsSettings.FindPropertyRelative(PROP_DRAGDROPSKILLS);
			this.spDragDropHotspot = spSkillsSettings.FindPropertyRelative(PROP_DRAGDROPHOTSPOT);

			this.spLevelSelection = spSkillsSettings.FindPropertyRelative(PROP_LEVELSELECTION);
			this.spLevelVariable = spSkillsSettings.FindPropertyRelative(PROP_LEVELVARIABLE);
			this.spLevelStat = spSkillsSettings.FindPropertyRelative(PROP_LEVELSTAT);

			this.spSkillPointsSelection = spSkillsSettings.FindPropertyRelative(PROP_SKILLPOINTSSELECTION);
			this.spSkillPointsVariable = spSkillsSettings.FindPropertyRelative(PROP_SKILLPOINTSVARIABLE);
			this.spSkillPointsStat = spSkillsSettings.FindPropertyRelative(PROP_SKILLPOINTSSTAT);

			int skillsSize = this.spSkills.arraySize;
			this.skillsData = new List<SkillsData>();
			for (int i = 0; i < skillsSize; ++i)
			{
				this.skillsData.Add(new SkillsData(this.spSkills.GetArrayElementAtIndex(i)));
			}

			this.editorSortableListSkills = new EditorSortableList();

			this.versionSkills = (ModuleManager.GetAssetModule("com.mitschmr-studios.module.skills") != null) ? ModuleManager.GetAssetModule("com.mitschmr-studios.module.skills").module.version.ToString() : "Module manifest not found";
			this.versionSkillsExamples = (ModuleManager.GetAssetModule("com.mitschmr-studios.examples.skills") != null) ? ModuleManager.GetAssetModule("com.mitschmr-studios.examples.skills").module.version.ToString() : "Module manifest not found";
		}

		// OVERRIDE GC METHODS: -------------------------------------------------------------------------------------------

		public override string GetDocumentationURL() => "https://docs.mitschmr-studios.io/skills/skills";

		public override string GetName() => "Skills";

		public override bool CanBeDecoupled() => true;

		// GUI METHODS: ------------------------------------------------------------------------------------------------

		[MenuItem("Window/MiTschMR Studios/Skills %&s")]
		public static void OpenWindow() => PreferencesWindow.OpenWindowTab("Skills");

		public override void OnInspectorGUI() => this.OnPreferencesWindowGUI();

		public override void OnPreferencesWindowGUI()
		{
			this.serializedObject.Update();

			int prevTabIndex = this.tabIndex;
			this.tabIndex = GUILayout.Toolbar(this.tabIndex, TAB_NAMES);
			if (prevTabIndex != this.tabIndex) this.ResetSearch();

			EditorGUILayout.Space();

			switch (this.tabIndex)
			{
				case 0: this.PaintSkillList(); break;
				case 1: this.PaintTypes(); break;
				case 2: this.PaintSettings(); break;
			}

			this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
		}

		protected virtual void PaintSkillList()
		{
			SkillEditor.SkillReturnOperation returnOp = new SkillEditor.SkillReturnOperation();
			int removeIndex = -1;
			int duplicateIndex = -1;

			this.PaintSearch();

			int skillsListSize = this.spSkills.arraySize;
			if (skillsListSize == 0) EditorGUILayout.HelpBox(MSG_EMPTY_SKILLLIST, MessageType.Info);

			for (int i = 0; i < skillsListSize; ++i)
			{
				if (this.skillsData[i].cachedEditor == null) continue;
				returnOp = this.skillsData[i].cachedEditor.OnPreferencesWindowGUI(this, i);
				if (returnOp.removeIndex) removeIndex = i;
				if (returnOp.duplicateIndex) duplicateIndex = i;
			}

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Add Skill", GUILayout.MaxWidth(200)))
			{
				this.ResetSearch();

				int insertIndex = skillsListSize;
				this.spSkills.InsertArrayElementAtIndex(insertIndex);

				Skill skill = Skill.CreateSkillInstance();
				this.spSkills.GetArrayElementAtIndex(insertIndex).objectReferenceValue = skill;
				this.skillsData.Insert(insertIndex, new SkillsData(this.spSkills.GetArrayElementAtIndex(insertIndex)));
			}

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			if (removeIndex != -1)
			{
				this.skillsData[removeIndex].cachedEditor.OnDestroySkill();
				UnityEngine.Object deleteItem = this.skillsData[removeIndex].cachedEditor.target;
				this.spSkills.RemoveFromObjectArrayAt(removeIndex);
				this.skillsData.RemoveAt(removeIndex);

				string path = AssetDatabase.GetAssetPath(deleteItem);
				DestroyImmediate(deleteItem, true);
				AssetDatabase.ImportAsset(path);
			}
			else if (duplicateIndex != -1)
			{
				this.ResetSearch();

				int srcIndex = duplicateIndex;
				int insertIndex = duplicateIndex + 1;

				this.spSkills.InsertArrayElementAtIndex(insertIndex);

				Skill skill = Skill.CreateSkillInstance();
				EditorUtility.CopySerialized(
					this.spSkills.GetArrayElementAtIndex(srcIndex).objectReferenceValue,
					skill
				);

				SerializedProperty newSkill = this.spSkills.GetArrayElementAtIndex(insertIndex);
				newSkill.objectReferenceValue = skill;

				newSkill.serializedObject.ApplyModifiedPropertiesWithoutUndo();
				newSkill.serializedObject.Update();

				SkillsData newItemData = new SkillsData(newSkill);

				for (int i = 0; i < skill.reliesOnSkills.Length; i++)
				{
					newItemData.cachedEditor.serializedObject
					.FindProperty("reliesOnSkills")
					.GetArrayElementAtIndex(i).FindPropertyRelative("skill").objectReferenceValue = this.MakeCopyOfSkill(skill.reliesOnSkills[i].skill);
				}

				newItemData.cachedEditor.serializedObject
					.FindProperty("conditionsExecutionRequirements")
					.objectReferenceValue = this.MakeCopyOf(skill.conditionsExecutionRequirements);

				newItemData.cachedEditor.serializedObject
					.FindProperty("actionsExecutionFailed")
					.objectReferenceValue = this.MakeCopyOf(skill.actionsExecutionFailed);

				newItemData.cachedEditor.serializedObject
					.FindProperty("actionsOnActivate")
					.objectReferenceValue = this.MakeCopyOf(skill.actionsOnActivate);

				newItemData.cachedEditor.serializedObject
					.FindProperty("actionsOnCast")
					.objectReferenceValue = this.MakeCopyOf(skill.actionsOnCast);

				newItemData.cachedEditor.serializedObject
					.FindProperty("actionsOnExecute")
					.objectReferenceValue = this.MakeCopyOf(skill.actionsOnExecute);

				newItemData.cachedEditor.serializedObject
					.FindProperty("actionsOnFinish")
					.objectReferenceValue = this.MakeCopyOf(skill.actionsOnFinish);

				newItemData.cachedEditor.serializedObject
					.FindProperty("conditionsRequirements")
					.objectReferenceValue = this.MakeCopyOf(skill.conditionsRequirements);

				newItemData.cachedEditor.serializedObject
					.FindProperty("actionsOnReset")
					.objectReferenceValue = this.MakeCopyOf(skill.actionsOnReset);

				int uuid = Mathf.Abs(Guid.NewGuid().GetHashCode());
				newItemData.cachedEditor.spUUID.intValue = uuid;

				newItemData.cachedEditor.serializedObject.ApplyModifiedPropertiesWithoutUndo();
				newItemData.cachedEditor.serializedObject.Update();
				newItemData.cachedEditor.OnEnable();

				this.skillsData.Insert(insertIndex, newItemData);
			}

			EditorSortableList.SwapIndexes swapIndexes = this.editorSortableListSkills.GetSortIndexes();
			if (swapIndexes != null)
			{
				this.spSkills.MoveArrayElement(swapIndexes.src, swapIndexes.dst);

				SkillsData tempItem = this.skillsData[swapIndexes.src];
				this.skillsData[swapIndexes.src] = this.skillsData[swapIndexes.dst];
				this.skillsData[swapIndexes.dst] = tempItem;
			}
		}

		protected virtual GameObject MakeCopyOf(UnityEngine.Object original)
		{
			string originalPath = AssetDatabase.GetAssetPath(original);
			string targetPath = AssetDatabase.GenerateUniqueAssetPath(originalPath);

			AssetDatabase.CopyAsset(originalPath, targetPath);
			AssetDatabase.Refresh();

			return AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
		}

		protected virtual SkillRelyOn MakeCopyOfSkill(UnityEngine.Object original)
		{
			string originalPath = AssetDatabase.GetAssetPath(original);
			string targetPath = AssetDatabase.GenerateUniqueAssetPath(originalPath);

			AssetDatabase.CopyAsset(originalPath, targetPath);
			AssetDatabase.Refresh();

			return AssetDatabase.LoadAssetAtPath<SkillRelyOn>(targetPath);
		}

		protected virtual void PaintTypes()
		{
			this.spSkillTypes.arraySize = SkillType.MAX;
			for (int i = 0; i < SkillType.MAX; ++i)
			{
				SerializedProperty spSkillType = this.spSkillTypes.GetArrayElementAtIndex(i);
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.PropertyField(
					spSkillType.FindPropertyRelative("id"),
					new GUIContent(string.Format("ID: {0}", i + 1))
				);

				EditorGUILayout.EndHorizontal();
			}
		}

		protected virtual void PaintSettings()
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);

			EditorGUILayout.LabelField("Versions", EditorStyles.boldLabel);

			EditorGUILayout.LabelField($"Skills version {this.versionSkills}");
			EditorGUILayout.LabelField($"Skills Examples version {this.versionSkillsExamples}");

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);

			EditorGUILayout.LabelField("User Interface", EditorStyles.boldLabel);

			if (this.spSkillTreeSelection.intValue == 0 && this.spSkillTreeUIPrefabAutomatic.objectReferenceValue == null)
			{
				EditorGUILayout.HelpBox(MSG_EMPTY_SKILLTREEPREFAB, MessageType.Info);
			}
			else if (this.spSkillTreeSelection.intValue == 1 && this.spSkillTreeUIPrefabManual.objectReferenceValue == null)
			{
				EditorGUILayout.HelpBox(MSG_EMPTY_SKILLTREEPREFAB, MessageType.Info);
			}
			EditorGUILayout.PropertyField(this.spSkillTreeSelection);
			if (this.spSkillTreeSelection.intValue == 0) EditorGUILayout.PropertyField(this.spSkillTreeUIPrefabAutomatic);
			else EditorGUILayout.PropertyField(this.spSkillTreeUIPrefabManual);

			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField("Behavior Configuration", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(this.spPauseTimeOnUI);

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(this.spDragDropSkills);
			EditorGUILayout.PropertyField(this.spDragDropHotspot);

			EditorGUILayout.Space();

			if (this.spLevelSelection.intValue == 0 && this.spLevelVariable.FindPropertyRelative("global").FindPropertyRelative("name").stringValue == "")
			{
				EditorGUILayout.HelpBox(MSG_EMPTY_LEVELSELECTION, MessageType.Error);
			}
			else if (this.spLevelSelection.intValue == 1 && this.spLevelStat.objectReferenceValue == null)
			{
				EditorGUILayout.HelpBox(MSG_EMPTY_LEVELSELECTION, MessageType.Error);
			}

			EditorGUILayout.PropertyField(this.spLevelSelection);

			if (this.spLevelSelection.intValue == 0) EditorGUILayout.PropertyField(this.spLevelVariable);
			else EditorGUILayout.PropertyField(this.spLevelStat);

			EditorGUILayout.Space();

			if (this.spSkillPointsSelection.intValue == 0 && this.spSkillPointsVariable.FindPropertyRelative("global").FindPropertyRelative("name").stringValue == "")
			{
				EditorGUILayout.HelpBox(MSG_EMPTY_SKILLPOINTSSELECTION, MessageType.Error);
			}
			else if (this.spSkillPointsSelection.intValue == 1 && this.spSkillPointsStat.objectReferenceValue == null)
			{
				EditorGUILayout.HelpBox(MSG_EMPTY_SKILLPOINTSSELECTION, MessageType.Error);
			}

			EditorGUILayout.PropertyField(this.spSkillPointsSelection);

			if (this.spSkillPointsSelection.intValue == 0) EditorGUILayout.PropertyField(this.spSkillPointsVariable);
			else EditorGUILayout.PropertyField(this.spSkillPointsStat);

			EditorGUILayout.EndVertical();
		}

		// PRIVATE METHODS: --------------------------------------------------------------------------------------------

		protected virtual void PaintSearch()
		{
			if (this.searchFieldStyle == null) this.searchFieldStyle = new GUIStyle(GUI.skin.FindStyle("SearchTextField"));
			if (this.searchCloseOnStyle == null) this.searchCloseOnStyle = new GUIStyle(GUI.skin.FindStyle("SearchCancelButton"));
			if (this.searchCloseOffStyle == null) this.searchCloseOffStyle = new GUIStyle(GUI.skin.FindStyle("SearchCancelButtonEmpty"));

			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(5f);

			GUI.SetNextControlName(SEARCHBOX_NAME);
			this.searchText = EditorGUILayout.TextField(this.searchText, this.searchFieldStyle);

			if (this.searchFocus)
			{
				EditorGUI.FocusTextInControl(SEARCHBOX_NAME);
				this.searchFocus = false;
			}

			GUIStyle style = (string.IsNullOrEmpty(this.searchText)
				? this.searchCloseOffStyle
				: this.searchCloseOnStyle
			);

			if (GUILayout.Button("", style)) this.ResetSearch();

			GUILayout.Space(5f);
			EditorGUILayout.EndHorizontal();
		}

		protected virtual void ResetSearch()
		{
			this.searchText = "";
			GUIUtility.keyboardControl = 0;
			EditorGUIUtility.keyboardControl = 0;
			this.searchFocus = true;
		}
	}
}