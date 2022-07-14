namespace GameCreator.Quests
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;

    [AddComponentMenu("")]
    public class IgniterQuestsCompleted : Igniter 
    {
        #if UNITY_EDITOR
        public new static string NAME = "Quests/On Quests Complete";
        
        public new static string ICON_PATH = "Assets/Plugins/GameCreator/Quests/Icons/Igniters/";
        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Quests/Icons/Igniters/";

        public new static string COMMENT = "Leave quest empty to trigger any change";
        #endif

        private void Update()
        {

            Quest[] roots = QuestsManager.Instance.GetRootQuests(-1);
            List<IQuest> listOfActiveTasks = new List<IQuest>();

            foreach (Quest root in roots)
            {
				
                List<IQuest> activeTasks = QuestsManager.Instance.GetSubTasks(root, 2, true);

				
                listOfActiveTasks.AddRange(activeTasks);
            }


            if (listOfActiveTasks.Count < 1)

            {
                this.ExecuteTrigger(gameObject);
            }
        }
    }
}