using System;

namespace GameCreator.Traversal
{
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;

    [AddComponentMenu("")]
    public class TriggerOnExitObstacle : Igniter 
    {
        #if UNITY_EDITOR
        public new static string NAME = "Traversal/On Exit Obstacle";
        public new static bool REQUIRES_COLLIDER = false;
        #endif

        public Obstacle obstacle = null;
        public bool onlyPlayer = false;

        private void Start()
        {
            if (this.obstacle == null) return;
            this.obstacle.EventComplete += this.OnActivate;
        }

        private void OnDestroy()
        {
            if (this.obstacle == null) return;
            this.obstacle.EventComplete -= this.OnActivate;
        }

        private void OnActivate(TraversalCharacter traverser, ObstacleStep[] steps)
        {
            if (this.onlyPlayer && traverser.gameObject != HookPlayer.Instance.gameObject) return;
            this.ExecuteTrigger(traverser.gameObject);
        }
    }
}