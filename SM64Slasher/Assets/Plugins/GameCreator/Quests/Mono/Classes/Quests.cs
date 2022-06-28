namespace GameCreator.Quests
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Quests : IQuest
    {
        public override bool IsTracking()
        {
            return false;
        }
    }
}