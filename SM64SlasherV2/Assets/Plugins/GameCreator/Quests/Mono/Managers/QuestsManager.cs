namespace GameCreator.Quests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using GameCreator.Core;

    [AddComponentMenu("Game Creator/Managers/QuestsManager")]
    public class QuestsManager : Singleton<QuestsManager>, IGameSave
    {
        private static readonly int ANIM_OPEN_STATE = Animator.StringToHash("Open");

        private const int TIME_LAYER = 300;

        [Serializable]
        public class QuestData
        {
            public string questID;
            public IQuest.Status status;
            public float progress = 0.0f;
        }

        [Serializable] 
        public class QuestsStorage 
        { 
            public QuestData[] questsData; 
        }

		// PROPERTIES: ----------------------------------------------------------------------------

		public QuestEvents questEvents = new QuestEvents();

		private Dictionary<string, Quest> quests;
		private Dictionary<string, IQuest> collection;

        private GameObject journalInstance;
        private bool isJournalOpen = false;

        private GameObject questsHUDInstance;
        private bool isQuestsHUDOpen = false;

        // INITIALIZERS: --------------------------------------------------------------------------

		protected override void OnCreate()
		{
            base.OnCreate();
            this.LoadQuests();

            SaveLoadManager.Instance.Initialize(this);
		}

        private void LoadQuests()
        {
            DatabaseQuests database = DatabaseQuests.Load();

            this.collection = new Dictionary<string, IQuest>();
            this.quests = new Dictionary<string, Quest>();

            int questsCount = database.list.Count;
            for (int i = 0; i < questsCount; ++i)
            {
                IQuest reference = database.list[i];
                IQuest instance = Instantiate(reference);

                this.collection.Add(instance.uniqueID, instance);
                if (instance.IsQuestRoot()) this.quests.Add(instance.uniqueID, (Quest)instance);
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IQuest GetQuest(string name)
		{
			IQuest quest = null;
			if (this.collection.TryGetValue(name, out quest))
			{
				return quest;
			}

			return null;
		}

        public Quest GetRootQuest(IQuest reference)
        {
            IQuest quest = this.GetQuest(reference.uniqueID);
            while (!quest.IsQuestRoot())
            {
                quest = this.GetQuest(quest.parent.uniqueID);
            }

            return (Quest)quest;
        }

		public Quest[] GetRootQuests(int statusFilter)
		{
			List<Quest> questsList = new List<Quest>();
			foreach (KeyValuePair<string, Quest> quest in this.quests)
			{
				if (((1 << (int)quest.Value.status) & statusFilter) != 0)
				{
					questsList.Add(quest.Value);
				}
			}

			return questsList.ToArray();
		}

        public List<IQuest> GetSubTasks(IQuest parent, int statusFilter, bool includeHidden = false)
        {
            List<IQuest> subTasksList = new List<IQuest>();
            for (int i = 0; i < parent.children.Count; ++i)
            {
                IQuest child = this.GetQuest(parent.children[i].uniqueID);
                bool visible = !child.isHidden || includeHidden;
                bool filter = ((1 << (int)child.status) & statusFilter) != 0;

                if (visible && filter) subTasksList.Add(child);
                subTasksList.AddRange(this.GetSubTasks(child, statusFilter, includeHidden));
            }

            return subTasksList;
        }

		public void ChangeStatus(IQuest reference, IQuest.Status status)
		{
			IQuest quest = this.GetQuest(reference.uniqueID);
			if (quest != null) quest.ChangeStatus(status);
		}

        public void RestartQuest(IQuest reference)
        {
            IQuest quest = this.GetQuest(reference.uniqueID);
            if (quest != null) quest.Restart();
        }

        // JOURNAL: -------------------------------------------------------------------------------

        public void OpenJournal()
        {
            if (this.journalInstance == null)
            {
                DatabaseQuests db = DatabaseQuests.Load();
                GameObject prefab = db.GetJournalPrefab();

                if (prefab != null)
                {
                    this.journalInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                }
            }

            StopAllCoroutines();
            StartCoroutine(this.ChangeJournalState(true));
            this.isJournalOpen = true;
        }

        public void CloseJournal()
        {
            if (this.journalInstance == null) return;

            StopAllCoroutines();
            StartCoroutine(this.ChangeJournalState(false));
            this.isJournalOpen = false;
        }

        public void ToggleJournal()
        {
            switch (this.isJournalOpen)
            {
                case true : this.CloseJournal(); break;
                case false : this.OpenJournal(); break;
            }
        }

        private IEnumerator ChangeJournalState(bool open)
        {
            TimeManager.Instance.SetTimeScale((open ? 0f : 1f), TIME_LAYER);
            if (open) this.journalInstance.SetActive(true);

            Animator anim = this.journalInstance.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetBool(ANIM_OPEN_STATE, open);

                if (!open)
                {
                    WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.5f);
                    yield return wait;
                }
            }

            if (!open) this.journalInstance.SetActive(false);
        }

        // QUESTS HUD: ----------------------------------------------------------------------------

        public void OpenQuestsHUD()
        {
            if (this.questsHUDInstance == null)
            {
                DatabaseQuests db = DatabaseQuests.Load();
                GameObject prefab = db.GetQuestsHUDPrefab();

                if (prefab != null)
                {
                    this.questsHUDInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                }
            }

            StopAllCoroutines();
            StartCoroutine(this.ChangeQuestsHUDState(true));
            this.isQuestsHUDOpen = true;
        }

        public void CloseQuestsHUD()
        {
            if (this.questsHUDInstance == null) return;

            StopAllCoroutines();
            StartCoroutine(this.ChangeQuestsHUDState(false));
            this.isQuestsHUDOpen = false;
        }

        public void ToggleQuestsHUD()
        {
            switch (this.isQuestsHUDOpen)
            {
                case true: this.CloseQuestsHUD(); break;
                case false: this.OpenQuestsHUD(); break;
            }
        }

        private IEnumerator ChangeQuestsHUDState(bool open)
        {
            if (open) this.questsHUDInstance.SetActive(true);
            Animator anim = this.questsHUDInstance.GetComponent<Animator>();

            if (anim != null)
            {
                anim.SetBool(ANIM_OPEN_STATE, open);

                if (!open)
                {
                    WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.5f);
                    yield return wait;
                }
            }

            if (!open) this.questsHUDInstance.SetActive(false);
        }

        // INTERFACE ISAVELOAD: -------------------------------------------------------------------

        public string GetUniqueName()
        {
            return "quests";
        }

        public System.Type GetSaveDataType()
        {
            return typeof(QuestsStorage);
        }

        public System.Object GetSaveData()
        {
            QuestsStorage storage = new QuestsStorage();
            storage.questsData = new QuestData[this.collection.Count];

            int index = 0;
            foreach (KeyValuePair<string, IQuest> item in this.collection)
            {
                storage.questsData[index] = new QuestData()
                {
                    questID = item.Key,
                    status = item.Value.status,
                    progress = item.Value.progress
                };

                index++;
            }

            return storage;
        }

        public void ResetData()
        {
            this.LoadQuests();
        }

        public void OnLoad(System.Object generic)
        {
            QuestsStorage storage = (QuestsStorage)generic;
            Dictionary<string, QuestData> storeDatas = new Dictionary<string, QuestData>();
            for (int i = 0; i < storage.questsData.Length; ++i)
            {
                storeDatas.Add(storage.questsData[i].questID, storage.questsData[i]);
            }

            foreach (KeyValuePair<string, IQuest> collectionItem in this.collection)
            {
                if (storeDatas.ContainsKey(collectionItem.Key))
                {
                    QuestData storeData = storeDatas[collectionItem.Key];
                    collectionItem.Value.progress = storeData.progress;
                    collectionItem.Value.status = storeData.status;
                }
            }
        }
    }
}