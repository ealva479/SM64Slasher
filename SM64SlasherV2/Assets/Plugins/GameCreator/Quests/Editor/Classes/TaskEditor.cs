namespace GameCreator.Quests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using GameCreator.Core;

    [CustomEditor(typeof(Task), true)]
    public class TaskEditor : IQuestEditor
    {
		private const string TEXTURE_PATH = "Assets/Plugins/GameCreator/Quests/Icons/Types/Task.png";
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
            bool acceptedType = (selectionType == typeof(Task) || selectionType == typeof(Quest));
            return selectionCount == 1 && acceptedType;
        }

        public static void AddElement(DatabaseQuestsEditor databaseQuestsEditor)
        {
            List<int> selections = new List<int> { databaseQuestsEditor.editorRoot.target.GetInstanceID() };
            List<int> nextSelections = new List<int>();

            if (databaseQuestsEditor.questsTree.HasSelection())
            {
                selections = new List<int>(databaseQuestsEditor.questsTree.GetSelection());
            }

            for (int i = 0; i < selections.Count; ++i)
            {
                int selectionID = selections[i];
				IQuest instance = databaseQuestsEditor.InstanceIDToObject(selectionID);

                Task itemInstance = databaseQuestsEditor.CreateItem<Task>();
                nextSelections.Add(itemInstance.GetInstanceID());

				if (instance != null && instance.GetType() == typeof(Quest) && 
				    databaseQuestsEditor.questsEditors.ContainsKey(instance.GetInstanceID()))
                {
					databaseQuestsEditor.questsEditors[instance.GetInstanceID()].AddChild(
                        itemInstance,
                        (IQuest)instance
                    );
                }
				else if (instance != null && instance.GetType() == typeof(Task) &&
                    databaseQuestsEditor.questsEditors.ContainsKey(instance.GetInstanceID()))
                {
					databaseQuestsEditor.questsEditors[instance.GetInstanceID()].AddChild(
                        itemInstance,
                        (IQuest)instance
                    );
                }
                else
                {
                    Debug.LogError("Unknown type: " + instance.GetType());
                }

                databaseQuestsEditor.questsEditors.Add(
                    itemInstance.GetInstanceID(),
                    IQuestEditor.CreateEditor(itemInstance)
                );

                databaseQuestsEditor.questsTree.Reload();

                databaseQuestsEditor.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                databaseQuestsEditor.serializedObject.Update();
            }

            databaseQuestsEditor.questsTree.SetFocusAndEnsureSelectedItem();
            databaseQuestsEditor.questsTree.SetSelection(nextSelections, TreeViewSelectionOptions.RevealAndFrame);
        }

		public override Texture2D GetIcon()
        {
			if (Application.isPlaying)
			{
				if (RUNTIME_ICONS == null) this.InitializeIcons(ref RUNTIME_ICONS, "Task");
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
			if (parent.GetType() == typeof(Quest)) return true;
			if (parent.GetType() == typeof(Task)) return true;         
			return false;
        }
    }
}