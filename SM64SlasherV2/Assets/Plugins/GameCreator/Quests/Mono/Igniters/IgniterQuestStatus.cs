namespace GameCreator.Quests
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;

    [AddComponentMenu("")]
    public class IgniterQuestStatus : Igniter 
	{
        [IQuest] public IQuest quest;
        public IQuest.Status toStatus = IQuest.Status.Active;

		#if UNITY_EDITOR
        public new static string NAME = "Quests/On Quest Status";
        public new static string ICON_PATH = "Assets/Plugins/GameCreator/Quests/Icons/Igniters/";
        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Quests/Icons/Igniters/";

        public new static string COMMENT = "Leave quest empty to trigger any change";
        #endif

        private void Start()
        {
            if (this.quest == null)
            {
                QuestsManager.Instance.questEvents.SetOnChange(this.OnGenericChangeStatus);
            }
            else
            {
                QuestsManager.Instance.questEvents.SetOnQuestChange(
                    this.quest.uniqueID, 
                    this.OnChangeQuestStatus
                );
            }
        }

        private void OnChangeQuestStatus(IQuest.Status status)
        {
            if (status == this.toStatus) 
            {
                GameObject target = (HookPlayer.Instance == null 
                    ? gameObject 
                    : HookPlayer.Instance.gameObject
                );
                
                this.ExecuteTrigger(target);
            }
        }

        private void OnGenericChangeStatus(string questID)
        {
            if (QuestsManager.Instance.GetQuest(questID).status == this.toStatus)
            {
                GameObject target = (HookPlayer.Instance == null
                    ? gameObject
                    : HookPlayer.Instance.gameObject
                );

                this.ExecuteTrigger(target);
            }
        }
    }
}