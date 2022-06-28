namespace GameCreator.Quests.UI
{
    using System.Text;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
    using GameCreator.Core;

    [AddComponentMenu("Game Creator/Quest/Quests UI")]
	public class QuestUI : MonoBehaviour
	{
        private const string FMT_TITLE_PROGRESS = "{0} ({1}/{2})";

        public enum DescriptionType
        {
            CurrentQuest,
            RootQuest,
            CompleteAndActive
        }

        private const int COMPLETE_OR_ACTIVE = (
            (1 << (int)IQuest.Status.Complete) |
            (1 << (int)IQuest.Status.Active)
        );

        // PROPERTIES: ----------------------------------------------------------------------------

        public IQuest quest;

        public bool chooseQuest = false;
        [IQuest] public IQuest chosenQuest;

        public Text title;
        public bool includeProgress = true;

        public Image image;
        public DescriptionType descriptionType = DescriptionType.CurrentQuest;
        public Text description;

        public Animator animator;
        public string triggerComplete = "Complete";
        public string triggerAbandon = "Abandon";
        public string triggerFail = "Fail";
        public string triggerUpdate = "Update";

        public GameObject incrementalContainer;
        public Image imageProgress;
        public Text textCurrentProgress;
        public Text textMaxProgress;

        public Toggle toggleTrack;
        public Button buttonActivate;
        public Button buttonAbandon;

        public GameObject statusTracking;
        public GameObject statusInactive;
        public GameObject statusActive;
        public GameObject statusComplete;
        public GameObject statusFailed;
        public GameObject statusAbandoned;

        public QuestsGroup subQuestsGroup;

        private bool isExittingApplication;

        private IQuest.Status cacheStatus;
        private float cacheProgress;

        // INITIALIZERS: --------------------------------------------------------------------------      

        private void Start()
        {
            if (this.chooseQuest)
            {
                this.Setup(this.chosenQuest);
            }
        }

        private void OnDestroy()
		{
            if (this.isExittingApplication) return;
            if (this.quest == null) return;

            if (QuestsManager.Instance == null) return;
            QuestsManager.Instance.questEvents.RemoveOnChange(this.OnQuestChange);
		}

        private void OnApplicationQuit()
        {
            this.isExittingApplication = true;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(IQuest quest)
		{
            gameObject.SetActive(true);

			this.quest = quest;
			this.UpdateQuest();

            this.cacheStatus = this.quest.status;
            this.cacheProgress = this.quest.progress;

            QuestsManager.Instance.questEvents.SetOnChange(this.OnQuestChange);
		}

        public IEnumerator DestroyQuest()
        {
            if (this.animator)
            {
                WaitForSecondsRealtime wait = new WaitForSecondsRealtime(1.0f);

                switch (this.quest.status)
                {
                    case IQuest.Status.Complete:
                        this.animator.SetTrigger(this.triggerComplete);
                        break;

                    case IQuest.Status.Abandoned:
                        this.animator.SetTrigger(this.triggerAbandon);
                        break;

                    case IQuest.Status.Failed:
                        this.animator.SetTrigger(this.triggerFail);
                        break;

                    default: wait = null; break;
                }

                if (wait != null) yield return wait;
            }

            Destroy(gameObject);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnQuestChange(string questID)
        {
            this.UpdateQuest();

            if (this.quest.status != this.cacheStatus ||
                !Mathf.Approximately(this.quest.progress, this.cacheProgress))
            {
                this.cacheStatus = this.quest.status;
                this.cacheProgress = this.quest.progress;

                if (this.animator)
                {
                    this.animator.SetTrigger(this.triggerUpdate);
                }
            }
        }

		private void UpdateQuest()
		{
            Quest rootQuest = QuestsManager.Instance.GetRootQuest(this.quest);

            this.SetupTitle();
            this.SetupDescription();

            if (this.image)
            {
				this.image.gameObject.SetActive(this.quest.sprite);
				if (this.quest.sprite) this.image.sprite = this.quest.sprite;
            }

            bool isIncremental = this.quest.type == IQuest.ProgressType.Incremental;
            if (this.incrementalContainer) this.incrementalContainer.SetActive(isIncremental);
            if (this.imageProgress) this.imageProgress.fillAmount = this.quest.ProgressPercent;
            if (this.textCurrentProgress) this.textCurrentProgress.text = this.quest.progress.ToString();
            if (this.textMaxProgress) this.textMaxProgress.text = this.quest.progressMaxValue.ToString();

            if (this.toggleTrack) this.SetupToggleTrack(rootQuest);
            if (this.buttonActivate) this.SetupButtonActivate();
            if (this.buttonAbandon) this.SetupButtonAbandon();

            if (this.statusTracking) this.statusTracking.SetActive(rootQuest.IsTracking());
            if (this.statusInactive) this.statusInactive.SetActive(this.quest.status == IQuest.Status.Inactive);
            if (this.statusActive) this.statusActive.SetActive(this.quest.status == IQuest.Status.Active);
            if (this.statusComplete) this.statusComplete.SetActive(this.quest.status == IQuest.Status.Complete);
            if (this.statusFailed) this.statusFailed.SetActive(this.quest.status == IQuest.Status.Failed);
            if (this.statusAbandoned) this.statusAbandoned.SetActive(this.quest.status == IQuest.Status.Abandoned);

            if (this.subQuestsGroup)
            {
                IQuest[] tasks = QuestsManager
                    .Instance
                    .GetSubTasks(this.quest, this.subQuestsGroup.statusFilter)
                    .ToArray();

                this.subQuestsGroup.Setup(tasks);
            }

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		}

        private void SetupTitle()
        {
            if (this.title)
            {
                string textTitle = this.quest.GetTitle();
                if (string.IsNullOrEmpty(textTitle)) textTitle = this.quest.internalName;

                if (this.quest.type == IQuest.ProgressType.Incremental &&
                    this.includeProgress)
                {
                    textTitle = string.Format(
                        FMT_TITLE_PROGRESS,
                        textTitle,
                        this.quest.progress,
                        this.quest.progressMaxValue
                    );
                }

                this.title.text = textTitle;
            }
        }

        private void SetupDescription()
        {
            if (this.description)
            {
                Quest rootQuest = QuestsManager.Instance.GetRootQuest(this.quest);
                switch (this.descriptionType)
                {
                    case DescriptionType.CurrentQuest:
                        this.description.text = this.quest.GetDescription();
                        break;

                    case DescriptionType.RootQuest:
                        this.description.text = rootQuest.GetDescription();
                        break;

                    case DescriptionType.CompleteAndActive:
                        StringBuilder builder = new StringBuilder(rootQuest.GetDescription());
                        IQuest[] quests = QuestsManager
                            .Instance
                            .GetSubTasks(rootQuest, COMPLETE_OR_ACTIVE)
                            .ToArray();

                        for (int i = 0; i < quests.Length; ++i)
                        {
                            string text = quests[i].GetDescription();
                            if (string.IsNullOrEmpty(text)) continue;
                            builder.Append("\n\n").Append(text);
                        }

                        this.description.text = builder.ToString();
                        break;
                }
            }
        }

        private void SetupToggleTrack(Quest rootQuest)
        {
            this.toggleTrack.onValueChanged.RemoveAllListeners();
            this.toggleTrack.onValueChanged.AddListener((bool status) =>
            {
                rootQuest.SetIsTracking(status);
            });

            this.toggleTrack.gameObject.SetActive(this.quest.status == IQuest.Status.Active);
            this.toggleTrack.isOn = rootQuest.IsTracking();
        }

        private void SetupButtonActivate()
        {
            this.buttonActivate.onClick.RemoveAllListeners();
            this.buttonActivate.onClick.AddListener(() => { this.quest.Activate(); });
            this.buttonActivate.gameObject.SetActive(this.quest.status == IQuest.Status.Inactive);
        }

        private void SetupButtonAbandon()
        {
            this.buttonAbandon.onClick.RemoveAllListeners();
            this.buttonAbandon.onClick.AddListener(() => { this.quest.Abandon(); });
            this.buttonAbandon.gameObject.SetActive(
                this.quest.canBeAbandoned &&
                this.quest.status == IQuest.Status.Active
            );
        }
	}
}