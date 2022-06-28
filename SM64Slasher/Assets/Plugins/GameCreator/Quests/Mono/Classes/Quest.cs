namespace GameCreator.Quests
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

	public class Quest : IQuest 
	{
		[SerializeField] private bool isTracking = false;

		// PUBLIC METHODS: ------------------------------------------------------------------------

        public override bool IsTracking()
        {
            return this.isTracking && this.status == Status.Active;
        }

        public void SetIsTracking(bool isTracking)
        {
            if (this.status == Status.Active)
            {
                this.isTracking = isTracking;
                QuestsManager.Instance.questEvents.OnTrack(this.uniqueID);
            }
        }

        public override bool IsQuestRoot()
        {
            return true;
        }
    }   
}