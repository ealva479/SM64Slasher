namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

    using GameCreator.Characters;
    using UnityEngine.SceneManagement;
    using GameCreator.Variables;

    [AddComponentMenu("")]
    public class JoystickTriggerAnalogAxis : Igniter 
	{
		#if UNITY_EDITOR
        public new static string NAME = "My Igniters/JoystickTriggerAnalogAxis";
        //public new static string COMMENT = "Uncomment to add an informative message";
        //public new static bool REQUIRES_COLLIDER = true; // uncomment if the igniter requires a collider
        #endif

        

        float H;
        float V;

        public string hName;
        public string vName;

        public GameObject localVariablesObject;

        

        private void Update()
        {

            float H = Input.GetAxis(hName);
            float V = Input.GetAxis(vName);
            

            if ( H > 0 || H < 0 || V > 0 || V < 0 )
            {
                this.ExecuteTrigger(gameObject);
                
            }


            VariablesManager.SetGlobal(hName, H);
            VariablesManager.SetGlobal(vName, V);
            VariablesManager.SetLocal(localVariablesObject, hName, H);
            VariablesManager.SetLocal(localVariablesObject, vName, V);


        }
	}
}