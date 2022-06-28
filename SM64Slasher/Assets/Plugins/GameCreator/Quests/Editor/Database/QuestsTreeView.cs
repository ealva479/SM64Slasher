namespace GameCreator.Quests
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;

	public class QuestsTreeView : TreeView
	{
		public const int ROOT_ID = 0;
		public const string ROOT_NAME = "quests";

		private static readonly GUIContent GUICONTENT_EDIT = new GUIContent("Edit", "[Space]");
		private const string KEY_DRAG = "gamecreator-quests-drag";

        private const float ROW_ICON_WIDTH = 20f;
		private const float ICON_INFO_WIDTH = 20f;
		private const float SETTINGS_BTN_WIDTH = 50f;

		private static readonly Color PROG_BCK_E = new Color(0f, 0f, 0f, 0.2f);
		private static readonly Color PROG_BAR_E = new Color(0f, 0f, 0f, 0.7f);

		private static readonly Color PROG_BCK_R = new Color( 0f,  0f,  0f, 0.5f);
		private static readonly Color PROG_BAR_R = new Color(.3f, .9f, .3f, 1.0f);

		private const float PROG_PADDING = 2.0f;
		private const float PROG_HEIGHT = 4.0f;

        // PROPERTIES: -----------------------------------------------------------------------------

        private DatabaseQuestsEditor databaseQuestsEditor;
        public Dictionary<int, TreeViewItem> treeItems;

        // CONSTRUCTORS: ---------------------------------------------------------------------------

        public QuestsTreeView(TreeViewState state, DatabaseQuestsEditor questsEditor) : base(state)
        {
            this.databaseQuestsEditor = questsEditor;
            this.showAlternatingRowBackgrounds = true;
            this.showBorder = true;
            this.Reload();
        }

        private void BuildTree(ref TreeViewItem parentTree, IQuest parentAsset)
        {
            this.treeItems.Add(parentAsset.GetInstanceID(), parentTree);

            IQuestEditor editor = this.databaseQuestsEditor.questsEditors[parentAsset.GetInstanceID()];
            parentTree.displayName = editor.GetInternalName();
            parentTree.icon = editor.GetIcon();

			IQuest[] childrenAssets = parentAsset.children.ToArray();
            for (int i = 0; i < childrenAssets.Length; ++i)
            {
                IQuest childAsset = childrenAssets[i];
                if (childAsset == null) continue;

                int childAssetID = childAsset.GetInstanceID();
                int depth = parentTree.depth + 1;
                TreeViewItem childTree = new TreeViewItem(childAssetID, depth, "Loading...");

                if (!this.databaseQuestsEditor.questsEditors.ContainsKey(childAssetID))
                {
                    Debug.LogError("No IQuest Editor found with instanceID: " + childAssetID);
                    continue;
                }

                this.BuildTree(ref childTree, childAsset);
                parentTree.AddChild(childTree);
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            this.treeItems = new Dictionary<int, TreeViewItem>();
            TreeViewItem root = new TreeViewItem(ROOT_ID, -1, ROOT_NAME);

            this.BuildTree(ref root, this.databaseQuestsEditor.databaseQuests.quests);

            if (root.hasChildren) SetupDepthsFromParentsAndChildren(root);
            else SetupParentsAndChildrenFromDepths(root, new List<TreeViewItem>());

            return root;
        }

		protected override void RowGUI(TreeView.RowGUIArgs args)
        {
			base.RowGUI(args);
            if (args.isRenaming) return;

            if (this.CanRename(args.item) && args.selected)
            {
                if (Event.current.type == EventType.ContextClick)
                {
                    this.BeginRename(args.item);
                    Event.current.Use();
                    return;
                }
            }

			Rect rectActionsFail = new Rect(
                args.rowRect.x + args.rowRect.width - ICON_INFO_WIDTH,
                args.rowRect.y,
				ICON_INFO_WIDTH,
                args.rowRect.height
            );

			Rect rectActionsComplete = new Rect(
				rectActionsFail.x - ICON_INFO_WIDTH,
                args.rowRect.y,
				ICON_INFO_WIDTH,
                args.rowRect.height
            );

			Rect rectConditions = new Rect(
				rectActionsComplete.x - ICON_INFO_WIDTH,
                args.rowRect.y,
                SETTINGS_BTN_WIDTH,
                args.rowRect.height
            );

			if (!this.databaseQuestsEditor.questsEditors.ContainsKey(args.item.id)) return;
			IQuestEditor iquestEditor = this.databaseQuestsEditor.questsEditors[args.item.id];

			QuestReaction reaction = iquestEditor.iquest.reactions;
			if (reaction == null) return;

			GUIContent contentFail = QuestTreeUtils.GetIcon((
                reaction.onFail != null && reaction.onFail.actions.Length > 0
                ? QuestTreeUtils.Icon.OnFail
                : QuestTreeUtils.Icon.OffFail
            ));

			GUIContent contentComplete = QuestTreeUtils.GetIcon((
				reaction.onComplete != null && reaction.onComplete.actions.Length > 0
				? QuestTreeUtils.Icon.OnComplete
				: QuestTreeUtils.Icon.OffComplete
			));

			GUIContent contentConditions = QuestTreeUtils.GetIcon((
				reaction.conditions != null && reaction.conditions.conditions.Length > 0
				? QuestTreeUtils.Icon.OnConditions
				: QuestTreeUtils.Icon.OffConditions
            ));

			EditorGUI.LabelField(rectActionsFail, contentFail);
			EditorGUI.LabelField(rectActionsComplete, contentComplete);
			EditorGUI.LabelField(rectConditions, contentConditions);

			if (EditorApplication.isPlaying)
			{
				args.item.icon = iquestEditor.GetIcon();
			}

			Rect rectSettings = new Rect(
                rectConditions.x - SETTINGS_BTN_WIDTH - 5f,
                args.rowRect.y,
                SETTINGS_BTN_WIDTH,
                args.rowRect.height
            );

			if (!EditorApplication.isPlaying && this.IsSelected(args.item.id))
			{
				if (GUI.Button(rectSettings, GUICONTENT_EDIT, EditorStyles.miniButton))
				{
                    QuestConfigWindow.Open(iquestEditor);
				}
			}
			else if (iquestEditor.ShowProgress())
			{
				float progress = iquestEditor.GetProgress();
				Rect rectBkg = new Rect(
					rectSettings.x + PROG_PADDING,
					rectSettings.y + (rectSettings.height/2.0f - PROG_HEIGHT / 2.0f),
					rectSettings.width - (2f * PROG_PADDING),
					PROG_HEIGHT
				);
				Rect rectBar = new Rect(
					rectBkg.x,
					rectBkg.y,
					rectBkg.width * progress,
					rectBkg.height
				);

				EditorGUI.DrawRect(rectBkg, EditorApplication.isPlaying ? PROG_BCK_R : PROG_BCK_E);
				EditorGUI.DrawRect(rectBar, EditorApplication.isPlaying ? PROG_BAR_R : PROG_BAR_E);
			}
        }

		protected override bool CanMultiSelect(TreeViewItem item) { return false; }      
		protected override bool CanRename(TreeViewItem item) { return true; }

		protected override void RenameEnded(RenameEndedArgs args)
		{
			if (this.databaseQuestsEditor.InstanceIDToObject(args.itemID) == null) return;
			base.RenameEnded(args);

			string name = this.databaseQuestsEditor.RenameItem(args.itemID, args.newName);
			this.treeItems[args.itemID].displayName = name;
		}

		// DRAG AND DROP: -------------------------------------------------------------------------      

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return args.draggedItemIDs.Count == 1;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (this.hasSearch) return;

            DragAndDrop.PrepareStartDrag();

            int itemID = args.draggedItemIDs[0];
            if (!this.treeItems.ContainsKey(itemID)) return;
            TreeViewItem item = this.treeItems[itemID];

            DragAndDrop.SetGenericData(KEY_DRAG, item);
			DragAndDrop.objectReferences = new UnityEngine.Object[] 
			{
				(IQuest)this.databaseQuestsEditor.questsEditors[item.id].target
			};
            DragAndDrop.StartDrag(item.displayName);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            TreeViewItem draggedItem = DragAndDrop.GetGenericData(KEY_DRAG) as TreeViewItem;
            if (draggedItem == null) return DragAndDropVisualMode.None;

            if (args.dragAndDropPosition == DragAndDropPosition.UponItem ||
                args.dragAndDropPosition == DragAndDropPosition.BetweenItems)
            {
                bool validDrag = ValidDrag(args.parentItem, draggedItem);
                if (args.performDrop && validDrag)
                {
                    this.OnDropItemAtIndex(
                        draggedItem,
                        args.parentItem,
                        args.insertAtIndex == -1 ? 0 : args.insertAtIndex
                    );
                }

                return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
            }
            else if (args.dragAndDropPosition == DragAndDropPosition.OutsideItems)
            {
                bool validDrag = ValidDrag(this.rootItem, draggedItem);
                if (args.performDrop && validDrag)
                {
                    this.OnDropItemAtIndex(
                        draggedItem,
                        this.rootItem,
                        this.rootItem.children.Count
                    );
                }

                return DragAndDropVisualMode.Move;
            }

            return DragAndDropVisualMode.None;
        }

        private void OnDropItemAtIndex(TreeViewItem draggedItem, TreeViewItem parent, int insertIndex)
        {
            this.databaseQuestsEditor.MoveItemTo(draggedItem, parent, insertIndex);
            this.Reload();

            this.SetFocusAndEnsureSelectedItem();
            this.SetSelection(new List<int> { draggedItem.id }, TreeViewSelectionOptions.RevealAndFrame);
        }

        private bool ValidDrag(TreeViewItem parentItem, TreeViewItem draggedItem)
        {
            IQuest parentItemInstance = this.databaseQuestsEditor.InstanceIDToObject(parentItem.id);         
            if (parentItemInstance == null) return false;

			if (!this.databaseQuestsEditor.questsEditors.ContainsKey(draggedItem.id)) return false;         
			if (!this.databaseQuestsEditor.questsEditors[draggedItem.id].CanHaveParent(parentItemInstance))
			{
				return false;
			}
            
            TreeViewItem currentParent = parentItem;
            while (currentParent != null)
            {
                if (draggedItem == currentParent) return false;
                currentParent = currentParent.parent;
            }

            return true;
        }      
    }
}