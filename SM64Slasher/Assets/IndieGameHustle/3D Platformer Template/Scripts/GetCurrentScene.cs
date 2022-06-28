namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.SceneManagement;
	using GameCreator.Variables;

	[AddComponentMenu("")]
	public class GetCurrentScene : IAction
	{
		int example = 0;

		[Space]
		
		
		[Header("Hover Below for Tooltip")]
		[Tooltip("Create GC Variable Type:String ( m_Scene ) Leave this space empty as it will simply get the current scene name and store it in your variable that was created in GC Variables")]
		public string sceneName;
		
		Scene m_Scene;

		[Header("Place your Local Variables Object")]
		[Tooltip("Only use this if you are using a Local Variable Game Object")]
		public GameObject VarSet;

		public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {

			m_Scene = SceneManager.GetActiveScene();
			sceneName = m_Scene.name;


			VariablesManager.SetGlobal("m_Scene", m_Scene.name);
			VariablesManager.SetLocal(VarSet,"m_Scene", m_Scene.name);

			Debug.Log(this.example);
            return true;
        }

		#if UNITY_EDITOR
        public static new string NAME = "Custom/GetCurrentScene";
		#endif
	}
}
