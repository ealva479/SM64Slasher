using System;

namespace GameCreator.Traversal
{
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;

    [AddComponentMenu("")]
    public class TriggerOnGrabClimbable : Igniter 
    {
        #if UNITY_EDITOR
        public new static string NAME = "Traversal/On Grab Climbable";
        public new static bool REQUIRES_COLLIDER = false;
        #endif

        public Climbable climbable = null;
        public bool onlyPlayer = false;

        private void Start()
        {
            if (this.climbable == null) return;
            this.climbable.EventGrab += this.OnActivate;
        }

        private void OnDestroy()
        {
            if (this.climbable == null) return;
            this.climbable.EventGrab -= this.OnActivate;
        }

        private void OnActivate(TraversalCharacter traverser)
        {
            if (this.onlyPlayer && traverser.gameObject != HookPlayer.Instance.gameObject) return;
            this.ExecuteTrigger(traverser.gameObject);
        }
    }
}