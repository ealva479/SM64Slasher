namespace GameCreator.Quests
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;

    #if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    #endif

    public class DatabaseQuests : IDatabase
    {
		[System.Serializable]
		public class Settings
		{
			public GameObject journalSkin;
            public GameObject questsHUDSkin;
		}

        // PROPERTIES: ----------------------------------------------------------------------------

        public Quests quests = null;
		public List<IQuest> list = new List<IQuest>();
		[SerializeField] private Settings settings = new Settings();

        #if UNITY_EDITOR
		public TreeViewState questsTreeState = new TreeViewState();
        #endif

		// PUBLIC METHODS: ------------------------------------------------------------------------

		public GameObject GetJournalPrefab()
		{
            if (this.settings.journalSkin != null) return this.settings.journalSkin;
            return Resources.Load<GameObject>("GameCreator/QuestsJournal");
		}

        public GameObject GetQuestsHUDPrefab()
        {
            if (this.settings.questsHUDSkin != null) return this.settings.questsHUDSkin;
            return Resources.Load<GameObject>("GameCreator/QuestsHUD");
        }

        // PUBLIC STATIC METHODS: -----------------------------------------------------------------

        public static DatabaseQuests Load()
        {
            return IDatabase.LoadDatabase<DatabaseQuests>();
        }

        // OVERRIDE METHODS: ----------------------------------------------------------------------

        #if UNITY_EDITOR

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            IDatabase.Setup<DatabaseQuests>();
        }

        protected override string GetProjectPath()
        {
            return "Assets/Plugins/GameCreatorData/Quests/Resources";
        }

        #endif
    }
}