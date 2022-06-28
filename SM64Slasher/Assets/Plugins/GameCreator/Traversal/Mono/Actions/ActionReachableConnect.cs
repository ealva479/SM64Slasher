namespace GameCreator.Traversal
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Characters;

    [AddComponentMenu("")]
    public class ActionReachableConnect : IAction
    {
        public Climbable climbable;
        public TraversableComponent connectTo;

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (this.climbable != null && this.connectTo != null)
            {
                TraversableComponent[] list = new TraversableComponent[this.climbable.reachables.Length + 1];
                for (int i = 0; i < this.climbable.reachables.Length; i++)
                {
                    TraversableComponent traversableComponent = this.climbable.reachables[i];
                    if (traversableComponent == this.connectTo) return true;
                    
                    list[i] = traversableComponent;
                }

                list[list.Length - 1] = this.connectTo;
                this.climbable.reachables = list;
            }
            
            return true;
        }

        #if UNITY_EDITOR
        
        public new static string NAME = "Traversal/Connect Climbable To";
        private const string NODE_TITLE = "Connect {0} to {1}";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE, 
                this.climbable != null ? this.climbable.gameObject.name : "(none)",
                this.connectTo != null ? this.connectTo.gameObject.name : "(none)"
            );
        }

        #endif
    }
}