namespace GameCreator.Quests
{
    using System;
	using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
	using UnityEngine.Events;

    public class QuestEvents
    {
        public class QuestEventStatus : UnityEvent<IQuest.Status> { }

        // PROPERTIES: ----------------------------------------------------------------------------

        private Dictionary<string, QuestEventStatus> onQuestChange;

        private event Action<string> onChange;
        private event Action<string> onDeactivate;
        private event Action<string> onActivate;
        private event Action<string> onComplete;
        private event Action<string> onProgress;
        private event Action<string> onFail;
        private event Action<string> onAbandon;
        private event Action<string> onRestart;
        private event Action<string> onTrack;

		// INITIALIZERS: --------------------------------------------------------------------------

		public QuestEvents()
		{
            this.onQuestChange = new Dictionary<string, QuestEventStatus>();
		}

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void OnChange(string questID)
        {
            if (this.onChange != null) this.onChange.Invoke(questID);
        }

        public void OnDeactivate(string questID)
        {
            this.OnChange(questID);
            if (this.onDeactivate != null) this.onDeactivate.Invoke(questID);
            if (this.onQuestChange.ContainsKey(questID)) this.onQuestChange[questID].Invoke(IQuest.Status.Inactive);
        }

        public void OnActivate(string questID)
        {
            this.OnChange(questID);
            if (this.onActivate != null) this.onActivate.Invoke(questID);
            if (this.onQuestChange.ContainsKey(questID)) this.onQuestChange[questID].Invoke(IQuest.Status.Active);
        }

        public void OnComplete(string questID)
        {
            this.OnChange(questID);
            if (this.onComplete != null) this.onComplete.Invoke(questID);
            if (this.onQuestChange.ContainsKey(questID)) this.onQuestChange[questID].Invoke(IQuest.Status.Complete);
        }

        public void OnProgress(string questID)
        {
            this.OnChange(questID);
            if (this.onProgress != null) this.onProgress.Invoke(questID);
            if (this.onQuestChange.ContainsKey(questID)) this.onQuestChange[questID].Invoke(IQuest.Status.Active);
        }

        public void OnFail(string questID)
        {
            this.OnChange(questID);
            if (this.onFail != null) this.onFail.Invoke(questID);
            if (this.onQuestChange.ContainsKey(questID)) this.onQuestChange[questID].Invoke(IQuest.Status.Failed);
        }

        public void OnAbandon(string questID)
        {
            this.OnChange(questID);
            if (this.onAbandon != null) this.onAbandon.Invoke(questID);
            if (this.onQuestChange.ContainsKey(questID)) this.onQuestChange[questID].Invoke(IQuest.Status.Abandoned);
        }

        public void OnRestart(string questID)
        {
            this.OnChange(questID);
            if (this.onRestart != null) this.onRestart.Invoke(questID);
            if (this.onQuestChange.ContainsKey(questID)) this.onQuestChange[questID].Invoke(IQuest.Status.Active);
        }

        public void OnTrack(string questID)
        {
            this.OnChange(questID);
            if (this.onTrack != null) this.onTrack.Invoke(questID);
            if (this.onQuestChange.ContainsKey(questID)) this.onQuestChange[questID].Invoke(IQuest.Status.Active);
        }

        // SETTERS: -------------------------------------------------------------------------------

        public void SetOnChange(Action<string> action)
        {
            this.onChange += action;
        }

        public void SetOnDeactivate(Action<string> action)
        {
            this.onDeactivate += action;
        }

        public void SetOnActivate(Action<string> action)
        {
            this.onActivate += action;
        }

        public void SetOnComplete(Action<string> action)
        {
            this.onComplete += action;
        }

        public void SetOnProgress(Action<string> action)
        {
            this.onProgress += action;
        }

        public void SetOnFail(Action<string> action)
        {
            this.onFail += action;
        }

        public void SetOnAbandon(Action<string> action)
        {
            this.onAbandon += action;
        }

        public void SetOnRestart(Action<string> action)
        {
            this.onRestart += action;
        }

        public void SetOnTrack(Action<string> action)
        {
            this.onTrack += action;
        }

        public void SetOnQuestChange(string questID, UnityAction<IQuest.Status> action)
        {
            if (!this.onQuestChange.ContainsKey(questID))
            {
                QuestEventStatus questEvent = new QuestEventStatus();
                this.onQuestChange.Add(questID, questEvent);
            }

            this.onQuestChange[questID].AddListener(action);
        }

        // REMOVERS: ------------------------------------------------------------------------------

        public void RemoveOnChange(Action<string> action)
        {
            this.onChange -= action;
        }

        public void RemoveOnDeactivate(Action<string> action)
        {
            this.onDeactivate -= action;
        }

        public void RemoveOnActivate(Action<string> action)
        {
            this.onActivate -= action;
        }

        public void RemoveOnComplete(Action<string> action)
        {
            this.onComplete -= action;
        }

        public void RemoveOnProgress(Action<string> action)
        {
            this.onProgress -= action;
        }

        public void RemoveOnFail(Action<string> action)
        {
            this.onFail -= action;
        }

        public void RemoveOnAbandon(Action<string> action)
        {
            this.onAbandon -= action;
        }

        public void RemoveOnRestart(Action<string> action)
        {
            this.onRestart -= action;
        }

        public void RemoveOnTrack(Action<string> action)
        {
            this.onTrack -= action;
        }

        public void RemoveOnQuestChange(string questID, UnityAction<IQuest.Status> action)
        {
            if (this.onQuestChange.ContainsKey(questID))
            {
                this.onQuestChange[questID].RemoveListener(action);
            }
        }
    }
}
