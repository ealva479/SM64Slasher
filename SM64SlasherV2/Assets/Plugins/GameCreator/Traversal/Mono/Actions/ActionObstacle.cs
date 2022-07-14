namespace GameCreator.Traversal
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Characters;

    [AddComponentMenu("")]
    public class ActionObstacle : IAction
    {
        public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);
        public bool skipIfBusy = false;

        [Space] 
        public TargetGameObject obstacle = new TargetGameObject(TargetGameObject.Target.GameObject);

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
        {
            Character c = this.character.GetCharacter(target);
            if (c != null && (!this.skipIfBusy || !c.characterLocomotion.isBusy))
            {
                TraversalCharacter traverser = c.GetComponent<TraversalCharacter>();
                if (traverser == null) traverser = c.gameObject.AddComponent<TraversalCharacter>();

                Obstacle instance = this.obstacle.GetComponent<Obstacle>(target);
                if (instance != null) yield return traverser.SetActive(instance);
            }

            yield return 0;
        }

        #if UNITY_EDITOR
        
        public new static string NAME = "Traversal/Overcome Obstacle";
        private const string NODE_TITLE = "Character {0} obstacle {1}";

        public override string GetNodeTitle()
        {
            return string.Format(NODE_TITLE, this.character, this.obstacle);
        }

        #endif
    }
}