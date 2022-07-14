namespace GameCreator.Quests
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using GameCreator.Core;

    [AddComponentMenu("")]
	public class QuestReaction : MonoBehaviour
	{
        public enum ActionsType
        {
            OnComplete,
            OnFail
        }

        // PROPERTIES: ----------------------------------------------------------------------------

		public IConditionsList conditions;

		public IActionsList onComplete;
		public IActionsList onFail;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AfterCompletion()
        {
            Destroy(gameObject);
        }
	}
}