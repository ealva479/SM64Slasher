namespace GameCreator.Quests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
	using UnityEditor.IMGUI.Controls;
    using GameCreator.Core;

    [CustomEditor(typeof(Quest), true)]
    public class QuestEditor : IQuestEditor
    {
		private const string TEXTURE_PATH = "Assets/Plugins/GameCreator/Quests/Icons/Types/Quest.png";
		private static Texture2D TEXTURE_ICON;
		private static Texture2D[] RUNTIME_ICONS;

		// INITIALIZERS: --------------------------------------------------------------------------

		private void OnEnable()
		{
            if (target == null || serializedObject == null) return;
			this.OnEnableBase();
		}
        
		// PUBLIC METHODS: ------------------------------------------------------------------------

		public static bool CanAddElement(int selectionCount, Type selectionType)
        {
			return true;
        }

        public static void AddElement(DatabaseQuestsEditor databaseQuestsEditor)
        {                  
			List<int> selections = new List<int> { databaseQuestsEditor.editorRoot.target.GetInstanceID() };
            List<int> nextSelections = new List<int>();

            if (databaseQuestsEditor.questsTree.HasSelection())
            {
                selections = new List<int>(databaseQuestsEditor.questsTree.GetSelection());
            }
            
			int selectionID = QuestsTreeView.ROOT_ID;
			if (selections.Count == 1) selectionID = selections[0];
            IQuest instance = databaseQuestsEditor.InstanceIDToObject(selectionID);

            Quest itemInstance = databaseQuestsEditor.CreateItem<Quest>();
            nextSelections = new List<int>() { itemInstance.GetInstanceID() };

			if (instance != null && instance.GetType() == typeof(Quest))
			{
				databaseQuestsEditor.questsEditors[instance.GetInstanceID()].AddSibling(
					itemInstance,
					(IQuest)instance,
					selectionID
				);
			}
			else
			{
				Quests rootInstance = databaseQuestsEditor.databaseQuests.quests;
                databaseQuestsEditor.editorRoot.AddChild(
                    itemInstance,
                    rootInstance
                );
			}

            databaseQuestsEditor.questsEditors.Add(
                itemInstance.GetInstanceID(),
                IQuestEditor.CreateEditor(itemInstance)
            );

            databaseQuestsEditor.questsTree.Reload();

            databaseQuestsEditor.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            databaseQuestsEditor.serializedObject.Update();

            databaseQuestsEditor.questsTree.SetFocusAndEnsureSelectedItem();
			databaseQuestsEditor.questsTree.SetSelection(
				nextSelections, 
				TreeViewSelectionOptions.RevealAndFrame
			);
        }

		public override Texture2D GetIcon()
		{
			if (Application.isPlaying)
            {
                if (RUNTIME_ICONS == null) this.InitializeIcons(ref RUNTIME_ICONS, "Quest");
				IQuest quest = QuestsManager.Instance.GetQuest(this.target.name);
                if (quest != null) return RUNTIME_ICONS[(int)quest.status];
            }

			if (TEXTURE_ICON == null)
			{
				TEXTURE_ICON = AssetDatabase.LoadAssetAtPath<Texture2D>(TEXTURE_PATH);
			}

			return TEXTURE_ICON;
		}

		public override bool CanHaveParent(IQuest parent)
        {
			return parent.GetType() == typeof(Quests);
        }

        protected override bool PaintProgress()
        {
            return false;
        }
    }
}