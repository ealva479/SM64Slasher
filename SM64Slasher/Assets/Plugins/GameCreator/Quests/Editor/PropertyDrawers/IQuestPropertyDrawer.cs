namespace GameCreator.Quests
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using GameCreator.Core;
    
	[CustomPropertyDrawer(typeof(IQuestAttribute))]
	public class IQuestPropertyDrawer : PropertyDrawer
	{
		private const string DRAG_MSG = "Drag & Drop a Quest from the Game Creator window";
		private static readonly GUIContent NOTIF_DRAG = new GUIContent(DRAG_MSG);

		private const float QUEST_BTN_W = 50f;

		private bool isDragDrop = false;
		private GUIStyle styleQuestNormal;
		private GUIStyle styleQuestActive;

		// PAINT METHODS: -------------------------------------------------------------------------

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Rect labelRect = new Rect(
				position.x,
				position.y,
				EditorGUIUtility.labelWidth,
				position.height
			);
            
			Rect fieldRectQst = new Rect(
				labelRect.x + labelRect.width,
                position.y,
				position.width - labelRect.width - QUEST_BTN_W + 1f,
                position.height
            );

			Rect fieldRectBtn = new Rect(
				fieldRectQst.x + fieldRectQst.width,
                position.y,
				QUEST_BTN_W,
                position.height
            );         

			IQuest quest = null;
			string displayName = "(none)";

			if (property.objectReferenceValue != null)
			{
				quest = (IQuest)property.objectReferenceValue;
				displayName = quest.internalName;
			}

			EditorGUI.LabelField(labelRect, label);

			this.InitilizeStyles();

			GUIStyle style = this.isDragDrop ? this.styleQuestActive : this.styleQuestNormal;         
			if (GUI.Button(fieldRectQst, displayName, style))
			{
				SceneView.focusedWindow.ShowNotification(NOTIF_DRAG);
			}

			if (GUI.Button(fieldRectBtn, "Quests", EditorStyles.miniButtonRight))
			{
				PreferencesWindow.OpenWindowTab("Quests");
			}

			this.DropQuest(fieldRectQst, property);
		}

		public void DropQuest(Rect dropRect, SerializedProperty property)
        {
			UnityEngine.Event currentEvent = UnityEngine.Event.current;

            switch (currentEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:

					this.isDragDrop = false;

					if (!dropRect.Contains(currentEvent.mousePosition)) return;
					if (DragAndDrop.objectReferences.Length != 1) return;               

					this.isDragDrop = true;

					UnityEngine.Object dropObject = DragAndDrop.objectReferences[0];
					if (!this.CanAcceptType(dropObject)) return;

					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (currentEvent.type == EventType.DragPerform)
                    {
						DragAndDrop.AcceptDrag();
						property.objectReferenceValue = dropObject;

						property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
						property.serializedObject.Update();
                    }
                    break;

				case EventType.Repaint:
					this.isDragDrop = false;
					break;
            }
        }

		// PRIVATE METHODS: -----------------------------------------------------------------------

		private void InitilizeStyles()
		{
			this.styleQuestNormal = new GUIStyle(EditorStyles.textField);

			this.styleQuestActive = new GUIStyle(this.styleQuestNormal);
			this.styleQuestActive.normal = this.styleQuestActive.focused;
			this.styleQuestActive.hover = this.styleQuestActive.focused;
			this.styleQuestActive.active = this.styleQuestActive.focused;
		}

		// PROTECTED METHODS: ---------------------------------------------------------------------

		protected virtual bool CanAcceptType(UnityEngine.Object dropObject)
		{
			return typeof(IQuest).IsAssignableFrom(dropObject.GetType());
		}
	}
}