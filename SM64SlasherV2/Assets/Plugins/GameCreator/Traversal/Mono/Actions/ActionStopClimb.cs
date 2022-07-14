namespace GameCreator.Traversal
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Characters;

    [AddComponentMenu("")]
    public class ActionStopClimb : IAction
    {
        public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
        {
            Character c = this.character.GetCharacter(target);
            if (c != null)
            {
                TraversalCharacter traverser = c.GetComponent<TraversalCharacter>();
                if (traverser == null) traverser = c.gameObject.AddComponent<TraversalCharacter>();

                yield return traverser.SetActive(null);
            }

            yield return 0;
        }

        #if UNITY_EDITOR
        
        public new static string NAME = "Traversal/Stop Climbing";
        private const string NODE_TITLE = "Character {0} let go of Climbable";

        public override string GetNodeTitle()
        {
            return string.Format(NODE_TITLE, this.character);
        }

        #endif
    }
}