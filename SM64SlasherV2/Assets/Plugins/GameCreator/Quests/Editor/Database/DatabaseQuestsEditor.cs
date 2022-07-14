namespace GameCreator.Quests
{
    using System;
	using System.Text.RegularExpressions;
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using UnityEditor.SceneManagement;
    using UnityEditor.IMGUI.Controls;
    using UnityEditorInternal;
    using System.Linq;
    using System.Reflection;
    using GameCreator.Core;

    [CustomEditor(typeof(DatabaseQuests))]
    public class DatabaseQuestsEditor : IDatabaseEditor
    {
        private const string PROP_QUESTS = "quests";
		private const string PROP_LIST = "list";
		private const string PROP_SETTINGS = "settings";

        private const float TREE_HEIGHT = 540f;      
		private static readonly Regex REGEX_NAME = new Regex("[^a-zA-Z0-9 -]");
		private static readonly GUIContent SETTINGS = new GUIContent("Settings");

        // PROPERTIES: ----------------------------------------------------------------------------

        public DatabaseQuests databaseQuests;
        private SerializedProperty spQuests;
		private SerializedProperty spList;

		private SerializedProperty spSettings;
		private Rect settingsRect = Rect.zero;

        private Vector2 scrollTree = Vector2.zero;
        private SearchField searchField;

        public IQuestEditor editorRoot = null;
        public QuestsTreeView questsTree = null;

        public Dictionary<int, IQuestEditor> questsEditors;
        private Dictionary<int, IQuest> questsInstances;
              
		private bool stylesInitialized = false;
		private GUIStyle styleBtnLeft;
		private GUIStyle styleBtnMid;
		private GUIStyle styleBtnRight;
		private GUIStyle styleBtn;

        // INITIALIZE: ----------------------------------------------------------------------------

        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;

            this.databaseQuests = (DatabaseQuests)this.target;
            this.spQuests = serializedObject.FindProperty(PROP_QUESTS);
			this.spList = serializedObject.FindProperty(PROP_LIST);
			this.spSettings = serializedObject.FindProperty(PROP_SETTINGS);

            if (this.spQuests.objectReferenceValue == null)
            {
                this.spQuests.objectReferenceValue = QuestUtilities.GetQuestsRoot();
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            }

            Quests quests = (Quests)this.spQuests.objectReferenceValue;
            this.questsInstances = new Dictionary<int, IQuest>();
            this.questsInstances.Add(QuestsTreeView.ROOT_ID, quests);

            this.editorRoot = IQuestEditor.CreateEditor(quests);
            this.questsEditors = new Dictionary<int, IQuestEditor>();
            this.questsEditors.Add(quests.GetInstanceID(), this.editorRoot);

			for (int i = 0; i < this.databaseQuests.list.Count; ++i)
            {
				IQuest item = this.databaseQuests.list[i];
				if (!this.questsInstances.ContainsKey(item.GetInstanceID()))
                {
					this.questsInstances.Add(item.GetInstanceID(), item);
                }

				IQuestEditor editor = IQuestEditor.CreateEditor(item);
				this.questsEditors.Add(item.GetInstanceID(), editor);
            }

            this.questsTree = new QuestsTreeView(this.databaseQuests.questsTreeState, this);

            this.searchField = new SearchField();
            this.searchField.downOrUpArrowKeyPressed += this.questsTree.SetFocusAndEnsureSelectedItem;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IQuest InstanceIDToObject(int instanceID)
        {
            if (this.questsInstances.ContainsKey(instanceID))
            {
                return (IQuest)this.questsInstances[instanceID];
            }

            return null;
        }

        public T CreateItem<T>() where T : IQuest
        {
			T instance = QuestUtilities.CreateIQuest<T>();         
			instance.children = new List<IQuest>();
            this.questsInstances.Add(instance.GetInstanceID(), instance);         
                     
			int index = this.spList.arraySize;
			this.spList.InsertArrayElementAtIndex(index);
            this.spList.GetArrayElementAtIndex(index).objectReferenceValue = instance;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.Update();

            return instance;
        }

        private void DeleteItems(List<int> items)
        {
			if (items == null || items.Count == 0) return;

            for (int i = 0; i < items.Count; ++i)
            {
                int selectionID = items[i];

				IQuest instance = this.InstanceIDToObject(selectionID);
                if (instance == null) continue;

				List<int> childrenIDs = new List<int>();
				for (int j = 0; j < instance.children.Count; ++j)
				{
					childrenIDs.Add(instance.children[j].GetInstanceID());
				}

				this.DeleteItems(childrenIDs);
				instance.parent.children.Remove(instance);

				this.questsInstances.Remove(instance.GetInstanceID());
				int spListCount = this.spList.arraySize;
                for (int j = spListCount - 1; j >= 0; --j)
                {
					SerializedProperty property = this.spList.GetArrayElementAtIndex(j);
                    if (property.objectReferenceValue == null ||
                        property.objectReferenceValue.GetInstanceID() == instance.GetInstanceID())
                    {
						this.spList.RemoveFromObjectArrayAt(j);
                    }
                }

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();

				if (instance.reactions != null)
				{
                    string path = AssetDatabase.GetAssetPath(instance.reactions.gameObject);
                    AssetDatabase.DeleteAsset(path);
                }

                DestroyImmediate(instance, true);
            }
        }
        
        public void MoveItemTo(TreeViewItem draggedItem, TreeViewItem parent, int insertIndex)
        {
            IQuest draggedQuestItem = this.InstanceIDToObject(draggedItem.id);
            if (draggedQuestItem != null && draggedQuestItem.parent != null)
            {
                int draggedItemParentID = draggedItem.parent.id;
                IQuest draggedItemParent = this.InstanceIDToObject(draggedItemParentID);

                if (draggedItemParent != null)
                {
                    int rmIndex = -1;
					for (int i = 0; i < draggedQuestItem.parent.children.Count; ++i)
                    {
                        if (draggedQuestItem.parent.children[i] == draggedQuestItem)
                        {
							rmIndex = i;
                            break;
                        }
                    }

                    SerializedObject spDraggedItemParent = new SerializedObject(draggedItemParent);
                    spDraggedItemParent.FindProperty(IQuestEditor.PROP_CHILDREN).RemoveFromObjectArrayAt(rmIndex);
                }
            }

            IQuest parentQuestItem = this.InstanceIDToObject(parent.id);
            if (parentQuestItem != null)
            {
                SerializedObject spParentQuestItem = new SerializedObject(parentQuestItem);
                SerializedProperty spParentQuestItemChildren = spParentQuestItem.FindProperty(IQuestEditor.PROP_CHILDREN);

                insertIndex = Mathf.Min(spParentQuestItemChildren.arraySize, insertIndex);

                spParentQuestItemChildren.InsertArrayElementAtIndex(insertIndex);
                spParentQuestItemChildren.GetArrayElementAtIndex(insertIndex).objectReferenceValue = draggedQuestItem;

                spParentQuestItemChildren.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                spParentQuestItemChildren.serializedObject.Update();

                SerializedObject spDraggedQuestItem = new SerializedObject(draggedQuestItem);
                spDraggedQuestItem.FindProperty(IQuestEditor.PROP_PARENT).objectReferenceValue = parentQuestItem;

                spDraggedQuestItem.ApplyModifiedPropertiesWithoutUndo();
                spDraggedQuestItem.Update();
            }
        }      

		public string RenameItem(int itemID, string name)
		{
			if (!this.questsEditors.ContainsKey(itemID)) return name;
            
			name = REGEX_NAME.Replace(name, "");
			name = name.Trim().ToLower().Replace(' ', '-');

			this.questsEditors[itemID].spInternalName.stringValue = name;         
			this.questsEditors[itemID].serializedObject.ApplyModifiedPropertiesWithoutUndo();
			this.questsEditors[itemID].serializedObject.Update();
			return name;
		}

		public override bool RequiresConstantRepaint()
		{
			return EditorApplication.isPlaying || base.RequiresConstantRepaint();
		}

		// OVERRIDE METHODS: ----------------------------------------------------------------------

		public override string GetDocumentationURL ()
        {
            return "https://docs.gamecreator.io/quests";
        }

        public override string GetName ()
        {
            return "Quests";
        }

        public override bool CanBeDecoupled()
        {
            return true;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // GUI METHODS: ---------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

			this.InitializeStyles();
            this.PaintQuestToolbar();
            this.PaintQuestTree();

			EditorGUILayout.EndVertical();         
            this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void PaintQuestToolbar()
        {
			bool hasSelection = this.questsTree.HasSelection();
            int selectionCount = (hasSelection ? this.questsTree.GetSelection().Count : 0);
			bool selectionRoot = (
				selectionCount > 0 &&
				this.questsTree.GetSelection().Contains(QuestsTreeView.ROOT_ID)
			);

            System.Type selectionType = null;
            if (selectionCount == 1)
            {
                int instanceID = this.questsTree.GetSelection()[0];
                IQuest instance = this.InstanceIDToObject(instanceID);
                selectionType = (instance != null ? instance.GetType() : null);
            }

			QuestToolbarUtils.ContentStyle contentStyle = QuestToolbarUtils.ContentStyle.IconOnly;
            GUIContent gcQuest = QuestToolbarUtils.GetContent(QuestToolbarUtils.ContentType.Quest, contentStyle);
            GUIContent gcTask = QuestToolbarUtils.GetContent(QuestToolbarUtils.ContentType.Task, contentStyle);
            GUIContent gcDelete = QuestToolbarUtils.GetContent(QuestToolbarUtils.ContentType.Delete, contentStyle);

            EditorGUILayout.BeginHorizontal();
			GUILayoutOption height = GUILayout.Height(18f);
            
            EditorGUI.BeginDisabledGroup(!QuestEditor.CanAddElement(selectionCount, selectionType));
			if (GUILayout.Button(gcQuest, this.styleBtnLeft, height)) QuestEditor.AddElement(this);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!TaskEditor.CanAddElement(selectionCount, selectionType));
			if (GUILayout.Button(gcTask, this.styleBtnMid, height)) TaskEditor.AddElement(this);
            EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(!hasSelection || selectionRoot);
			if (GUILayout.Button(gcDelete, this.styleBtnRight, height) && this.questsTree.HasSelection())
            {
                List<int> items = new List<int>(this.questsTree.GetSelection());
                this.DeleteItems(items);
				AssetDatabase.ImportAsset(QuestUtilities.GetQuestsRootPath());
                this.questsTree.Reload();
            }
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Space();
			this.questsTree.searchString = this.searchField.OnGUI(this.questsTree.searchString);
			EditorGUILayout.Space();         

			if (GUILayout.Button(SETTINGS, this.styleBtn))
			{
				QuestsSettingsWindow settings = new QuestsSettingsWindow(this.spSettings);
				PopupWindow.Show(this.settingsRect, settings);
			}

			if (UnityEngine.Event.current.type == EventType.Repaint)
            {
				this.settingsRect = GUILayoutUtility.GetLastRect();
            }

            EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
        }

        private void PaintQuestTree()
        {
            this.scrollTree = EditorGUILayout.BeginScrollView(
                    this.scrollTree,
                    GUILayout.MinHeight(TREE_HEIGHT),
                    GUILayout.MaxHeight(TREE_HEIGHT),
                    GUILayout.ExpandHeight(false)
                );

            Rect treeViewRect = GUILayoutUtility.GetRect(
                0f, 10000f,
                TREE_HEIGHT,
                TREE_HEIGHT
            );

            this.questsTree.OnGUI(treeViewRect);

            EditorGUILayout.EndScrollView();
        }

		private void InitializeStyles()
		{
			if (this.stylesInitialized) return;

			this.styleBtnLeft = new GUIStyle(GUI.skin.GetStyle("ButtonLeft"));
			this.styleBtnMid = new GUIStyle(GUI.skin.GetStyle("ButtonMid"));
			this.styleBtnRight = new GUIStyle(GUI.skin.GetStyle("ButtonRight"));
			this.styleBtn = new GUIStyle(GUI.skin.GetStyle("Button"));

			RectOffset rect = new RectOffset(0, 0, 2, 2);
			this.styleBtnLeft.margin = rect;
			this.styleBtnMid.margin = rect;
			this.styleBtnRight.margin = rect;
			this.styleBtn.margin = rect;

			this.styleBtnLeft.alignment = TextAnchor.MiddleCenter;
			this.styleBtnMid.alignment = TextAnchor.MiddleCenter;
			this.styleBtnRight.alignment = TextAnchor.MiddleCenter;
			this.styleBtn.alignment = TextAnchor.MiddleCenter;
		}
    }
}