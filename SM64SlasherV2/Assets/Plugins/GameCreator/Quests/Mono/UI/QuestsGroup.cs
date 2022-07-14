namespace GameCreator.Quests.UI
{
	using System.Collections;
	using System.Collections.Generic;
    using GameCreator.Core;
    using UnityEngine;
	using UnityEngine.UI;

    [AddComponentMenu("Game Creator/Quest/Quests Group")]
	public class QuestsGroup : MonoBehaviour
	{
        public class Cache
        {
            public GameObject instance;
            public string questID = "";
        }

        public enum InitType
        {
            Automatic,
            Manual
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public InitType initType = InitType.Automatic;

        public int statusFilter = 0;
        public bool onlyTracked = false;

        public ToggleGroup toggleGroup;
        public RectTransform container;
		public GameObject prefab;

        public QuestUI questUI;

        private Cache[] cache = new Cache[0];
        private bool isExittingApplication;

		// INITIALIZERS: --------------------------------------------------------------------------

		private void Start()
        {


            if (this.initType == InitType.Automatic)
            {
                this.Setup(QuestsManager.Instance.GetRootQuests(this.statusFilter));
                QuestsManager.Instance.questEvents.SetOnChange((string questID) =>
                {
                    this.Setup(QuestsManager.Instance.GetRootQuests(this.statusFilter));
                });
            }
        }

        private void OnApplicationQuit()
        {
            this.isExittingApplication = true;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(IQuest[] quests)
        {
            if (this.isExittingApplication) return;
            if (this.prefab == null)
            {
                Debug.LogError("Undefined prefab reference");
                return;
            }

            if (this.container == null)
            {
                Debug.LogError("Undefined container instance");
                return;
            }

            Dictionary<string, GameObject> removeCandidates = new Dictionary<string, GameObject>();
            for (int i = 0; i < this.cache.Length; ++i)
            {
                removeCandidates.Add(this.cache[i].questID, this.cache[i].instance);
            }

            List<Cache> nextCache = new List<Cache>();

            for (int i = 0; i < quests.Length; ++i)
            {
                Cache cacheItem = new Cache { questID = quests[i].uniqueID };
                if (this.onlyTracked && !quests[i].IsTracking()) continue;

                if (removeCandidates.ContainsKey(quests[i].uniqueID))
                {
                    cacheItem.instance = removeCandidates[quests[i].uniqueID];
                    removeCandidates.Remove(quests[i].uniqueID);
                }
                else
                {
                    cacheItem.instance = Instantiate(
                        this.prefab,
                        this.container
                    );

                    QuestUI questsUI = cacheItem.instance.GetComponent<QuestUI>();
                    if (questsUI != null) questsUI.Setup(quests[i]);

                    Toggle toggle = cacheItem.instance.GetComponent<Toggle>();
                    if (toggle != null)
                    {
                        this.SetupToggle(toggle, quests[i]);
                    }
                }

                nextCache.Add(cacheItem);
            }

            foreach (KeyValuePair<string, GameObject> removeItem in removeCandidates)
            {
                if (removeItem.Value == null) continue;

                if (this.toggleGroup != null)
                {
                    Toggle toggle = removeItem.Value.GetComponent<Toggle>();
                    if (toggle != null) toggleGroup.UnregisterToggle(toggle);
                }

                QuestUI removeQuestUI = removeItem.Value.GetComponent<QuestUI>();

                if (removeQuestUI == null) Destroy(removeItem.Value);
                else CoroutinesManager.Instance.StartCoroutine(removeQuestUI.DestroyQuest());
            }

            this.cache = nextCache.ToArray();

            if (this.toggleGroup != null)
            {
                if (this.cache.Length == 0 && this.questUI != null)
                {
                    this.questUI.gameObject.SetActive(false);
                }

                if (this.cache.Length > 0 && this.questUI != null && !this.toggleGroup.AnyTogglesOn())
                {
                    this.questUI.gameObject.SetActive(false);
                }
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupToggle(Toggle toggle, IQuest quest)
        {
            toggle.isOn = false;
            toggle.group = this.toggleGroup;
            toggle.onValueChanged.AddListener((bool state) =>
            {
                if (this.questUI != null) this.questUI.Setup(quest);
            });
        }
	}
}