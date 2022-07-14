namespace GameCreator.Traversal
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
	using GameCreator.Characters;

	[AddComponentMenu("")]
	public class ActionClimb : IAction
	{
		public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);
		public bool skipIfBusy = false;

		[Space] 
		public TargetGameObject climbable = new TargetGameObject(TargetGameObject.Target.GameObject);

		public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
	        Character c = this.character.GetCharacter(target);
	        if (c == null) return true;
	        if (this.skipIfBusy && c.characterLocomotion.isBusy) return true;

	        TraversalCharacter traverser = c.GetComponent<TraversalCharacter>();
	        if (traverser == null) traverser = c.gameObject.AddComponent<TraversalCharacter>();

	        Climbable climb = this.climbable.GetComponent<Climbable>(target);
	        if (climb == null) return true;

	        traverser.StartCoroutine(traverser.SetActive(climb));
	        return true;
        }

		#if UNITY_EDITOR
        public new static string NAME = "Traversal/Start Climbing";

        private const string NODE_TITLE = "Character {0} Climb {1}";

        public override string GetNodeTitle()
        {
	        return string.Format(NODE_TITLE, this.character, this.climbable);
        }

		#endif
	}
}
