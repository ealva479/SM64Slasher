namespace GameCreator.Quests
{
	using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

	[CustomPropertyDrawer(typeof(QuestAttribute))]
	public class QuestPropertyDrawer : IQuestPropertyDrawer
	{
		protected override bool CanAcceptType(Object dropObject)
		{
			return typeof(Quest).IsAssignableFrom(dropObject.GetType());
		}
	}
}