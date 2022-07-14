namespace GameCreator.Quests
{
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    [CustomEditor(typeof(IQuest), true)]
    public class IQuestEditor : Editor
    {
        public const string PROP_CHILDREN = "children";
        public const string PROP_PARENT = "parent";      
		public const string PROP_INTERNAL_NAME = "internalName";

        private const string PROP_TITLE = "title";
        private const string PROP_DESCR = "description";
		private const string PROP_SPRITE = "sprite";
        private const string PROP_STATUS = "status";
        private const string PROP_IS_HIDDEN = "isHidden";
        private const string PROP_CAN_BEA_BANDONED = "canBeAbandoned";
        private const string PROP_REACTION = "reactions";
        private const string PROP_PROGRESS_MAX_VALUE = "progressMaxValue";

        private const string PROP_TYPE = "type";
		private const string PROP_PROGRESS = "progress";

		private const string ICONS_PATH = "Assets/Plugins/GameCreator/Quests/Icons/Types";

		private static readonly string[] OPTIONS = new string[]
		{
            "General",
            "Conditions",
            "On Complete",
            "On Fail"
		};

		protected static readonly string[] TEXTURE_SUFFIX = new string[]
		{
			"inactive",
			"active",
			"complete",
			"failed",
			"abandoned"
		};

        // PROPERTIES: ----------------------------------------------------------------------------

        public IQuest iquest;
        public SerializedProperty spChildren;
        public SerializedProperty spParent;      
		public SerializedProperty spInternalName;

        protected SerializedProperty spTitle;
        protected SerializedProperty spDescription;
		protected SerializedProperty spSprite;
        protected SerializedProperty spStatus;
        protected SerializedProperty spIsHidden;
        protected SerializedProperty spCanBeAbandoned;
        protected SerializedProperty spProgressMaxValue;

		protected SerializedProperty spType;
		protected SerializedProperty spProgress;
  
		protected QuestReactionEditor editorQuestReactions;

		private int optionIndex = 0;

		// INITIALIZERS: --------------------------------------------------------------------------

		protected void OnEnableBase()
		{
			this.iquest = (IQuest)this.target;

            this.spChildren = serializedObject.FindProperty(PROP_CHILDREN);
            this.spParent = serializedObject.FindProperty(PROP_PARENT);

            this.spInternalName = serializedObject.FindProperty(PROP_INTERNAL_NAME);
            this.spTitle = serializedObject.FindProperty(PROP_TITLE);
            this.spDescription = serializedObject.FindProperty(PROP_DESCR);
			this.spSprite = serializedObject.FindProperty(PROP_SPRITE);
            this.spStatus = serializedObject.FindProperty(PROP_STATUS);
            this.spIsHidden = serializedObject.FindProperty(PROP_IS_HIDDEN);
            this.spCanBeAbandoned = serializedObject.FindProperty(PROP_CAN_BEA_BANDONED);
            this.spProgressMaxValue = serializedObject.FindProperty(PROP_PROGRESS_MAX_VALUE);

            this.spType = serializedObject.FindProperty(PROP_TYPE);
			this.spProgress = serializedObject.FindProperty(PROP_PROGRESS);

			UnityEngine.Object reactions = serializedObject.FindProperty(PROP_REACTION).objectReferenceValue;
			this.editorQuestReactions = (QuestReactionEditor)Editor.CreateEditor(reactions);
		}

		protected Texture2D LoadIcon(string iconName, string suffix)
		{
			string path = string.Format(
				"{0}/{1}@{2}.png",
				ICONS_PATH,
				iconName,
				suffix
			);
            
			return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
		}

        public static IQuestEditor CreateEditor(IQuest instance)
        {
            return (IQuestEditor)Editor.CreateEditor(instance);
        }

		protected void InitializeIcons(ref Texture2D[] runtimeIcons, string iconName)
        {
            runtimeIcons = new Texture2D[5];
            for (int i = 0; i < runtimeIcons.Length; ++i)
            {
                runtimeIcons[i] = this.LoadIcon(iconName, TEXTURE_SUFFIX[i]);
            }
        }

        // PAINT METHODS: -------------------------------------------------------------------------

		public override void OnInspectorGUI()
		{
			this.optionIndex = GUILayout.Toolbar(this.optionIndex, OPTIONS);
			EditorGUILayout.Space();

			switch (this.optionIndex)
			{
				case 0 : this.PaintGeneral(); break;
				case 1 : this.PaintConditions(); break;
				case 2 : this.PaintOnComplete(); break;
				case 3 : this.PaintOnFail(); break;
			}
		}

		protected virtual void PaintGeneral()
		{
            EditorGUILayout.HelpBox(
                "You can use 'global[variable-name]' in Title and Description to display global " +
                "variable values",
                MessageType.Info, true
            );

			EditorGUILayout.PropertyField(this.spTitle);
			EditorGUILayout.PropertyField(this.spSprite);
			EditorGUILayout.PropertyField(this.spStatus);
            EditorGUILayout.PropertyField(this.spIsHidden);
            EditorGUILayout.PropertyField(this.spCanBeAbandoned);

            EditorGUILayout.Space();
			EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(this.spDescription);

            if (this.PaintProgress())
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Progress", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(this.spType);
                if (this.spType.enumValueIndex == (int)IQuest.ProgressType.Incremental)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(this.spProgressMaxValue);
                    EditorGUI.indentLevel--;
                }
            }
		}

        protected virtual bool PaintProgress()
        {
            return true;
        }

        protected virtual void PaintConditions()
        {
			this.editorQuestReactions.PaintConditions();
        }

        protected virtual void PaintOnComplete()
        {
			this.editorQuestReactions.PaintOnComplete();
        }

        protected virtual void PaintOnFail()
        {
			this.editorQuestReactions.PaintOnFail();
        }

		// PUBLIC METHODS: ------------------------------------------------------------------------

		public string GetInternalName()
        {
            return this.iquest.internalName;
        }

        public virtual Texture2D GetIcon()
        {
            return null;
        }

        public virtual bool CanHaveParent(IQuest parent)
        {
            return true;
        }

        public void AddChild(IQuest item, IQuest parent)
        {
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.Update();

            if (parent == null)
            {
                Debug.LogError("Adding null parent");
				return;
            }

            SerializedProperty children = serializedObject.FindProperty(PROP_CHILDREN);
            SerializedObject itemSerializedObject = new SerializedObject(item);
            itemSerializedObject.FindProperty(PROP_PARENT).objectReferenceValue = parent;
            itemSerializedObject.ApplyModifiedPropertiesWithoutUndo();
            itemSerializedObject.Update();

            children.AddToObjectArray<IQuest>(item);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.Update();
        }

        public void AddSibling(IQuest item, IQuest sibling, int siblingID = -1)
        {
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.Update();

			if (sibling == null || sibling.parent == null)
            {
                Debug.LogError("Adding null parent or sibling");
                return;
            }

			SerializedObject itemSerializedObject = new SerializedObject(item);
			itemSerializedObject.FindProperty(PROP_PARENT).objectReferenceValue = sibling.parent;         
			itemSerializedObject.ApplyModifiedPropertiesWithoutUndo();
            itemSerializedObject.Update();

			SerializedObject soParent = new SerializedObject(sibling.parent);
            SerializedProperty spParentChildren = soParent.FindProperty(PROP_CHILDREN);

			int index = spParentChildren.arraySize;
            if (siblingID != -1)
            {
				int childrenCount = item.parent.children.Count;
                for (int i = 0; i < childrenCount; ++i)
                {
                    if (item.parent.children[i].GetInstanceID() == siblingID)
                    {
                        index = i + 1;
                        break;
                    }
                }
            }

            spParentChildren.InsertArrayElementAtIndex(index);
            spParentChildren.GetArrayElementAtIndex(index).objectReferenceValue = item;        

			soParent.ApplyModifiedPropertiesWithoutUndo();
			soParent.Update();

			serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.Update(); 
        }

		public bool ShowProgress()
        {
            return this.spType.intValue == (int)IQuest.ProgressType.Incremental;
        }

        public float GetProgress()
        {
            if (Application.isPlaying)
            {
                IQuest quest = QuestsManager.Instance.GetQuest(this.target.name) as Task;
                if (quest == null) return 0f;
                return quest.ProgressPercent;
            }

            return (this.spProgress.floatValue / this.spProgressMaxValue.floatValue);
        }
	}
}