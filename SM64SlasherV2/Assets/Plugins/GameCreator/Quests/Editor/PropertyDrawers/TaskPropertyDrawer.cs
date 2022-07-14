namespace GameCreator.Quests
{
	using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

	[CustomPropertyDrawer(typeof(TaskAttribute))]
	public class TaskPropertyDrawer : IQuestPropertyDrawer
	{
		protected override bool CanAcceptType(Object dropObject)
		{
			return typeof(Task).IsAssignableFrom(dropObject.GetType());
		}
	}
}