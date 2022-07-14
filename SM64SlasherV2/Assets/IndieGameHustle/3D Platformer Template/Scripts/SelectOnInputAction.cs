namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;

	[AddComponentMenu("")]
	public class SelectOnInputAction : IAction
	{
		//public int example = 0;
		//public EventSystem eventSystem;
		//public GameObject selectedObject;
		public Button myButton;
		

		public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
			//Debug.Log(this.example);
			//eventSystem.SetSelectedGameObject(selectedObject);

			
			myButton.Select();


			return true;
        }



		#if UNITY_EDITOR
        public static new string NAME = "Custom/SelectOnInputAction";
		#endif
	}
}
