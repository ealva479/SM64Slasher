namespace GameCreator.Traversal
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Characters;

    [AddComponentMenu("")]
    public class ActionReachableDisconnect : IAction
    {
        public TraversableComponent disconnect;
        public Climbable fromClimbable;

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (this.fromClimbable != null && this.disconnect != null)
            {
                List<TraversableComponent> list = new List<TraversableComponent>();

                foreach (TraversableComponent traversableComponent in this.fromClimbable.reachables)
                {
                    if (traversableComponent != null && traversableComponent != this.disconnect)
                    {
                        list.Add(traversableComponent);
                    }
                }
                
                this.fromClimbable.reachables = list.ToArray();
            }
            
            return true;
        }

        #if UNITY_EDITOR
        
        public new static string NAME = "Traversal/Disconnect From Climbable";
        private const string NODE_TITLE = "Disconect {0} from {1}";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.disconnect != null ? this.disconnect.gameObject.name : "(none)",
                this.fromClimbable != null ? this.fromClimbable.gameObject.name : "(none)"
            );
        }

        #endif
    }
}