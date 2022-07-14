namespace GameCreator.Quests
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Task : IQuest
    {
        public override bool IsTracking()
        {
            if (this.parent == null) return false;
            return parent.IsTracking();
        }
    }
}