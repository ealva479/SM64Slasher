namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using GameCreator.Characters;
    using UnityEngine.SceneManagement;
    using GameCreator.Variables;

    [AddComponentMenu("")]
    public class GetObjectPosRot : Igniter 
	{
		#if UNITY_EDITOR
        public new static string NAME = "My Igniters/GetObjectPosRot";
        //public new static string COMMENT = "Uncomment to add an informative message";
        //public new static bool REQUIRES_COLLIDER = true; // uncomment if the igniter requires a collider
        #endif

        //public bool example = false;

        public Vector3 startPosition;

        public Vector3 startRot;

        public GameObject localVariablesObject;

        public string sPos;
        public string sRot;


        private void Start()
        {
            startPosition = this.transform.position;
            startRot = this.transform.rotation.eulerAngles;


            VariablesManager.SetLocal(localVariablesObject, sPos, startPosition);
            VariablesManager.SetLocal(localVariablesObject, sRot, startPosition);

            Debug.Log(startPosition);
            Debug.Log(startRot);

        }

        private void Update()
        {
            //if (this.example)
            //{
             //   this.ExecuteTrigger(gameObject);
            //    this.example = false;
           // }


            

            

        }





	}
}