namespace GameCreator.Core
{
    using GameCreator.Variables;
    using System.Collections;
	using System.Collections.Generic;
   
    using UnityEngine;

    [AddComponentMenu("")]
    public class ConveyorBelt : Igniter
    {
#if UNITY_EDITOR
        public new static string NAME = "My Igniters/ConveyorBelt";
        //public new static string COMMENT = "Uncomment to add an informative message";
        //public new static bool REQUIRES_COLLIDER = true; // uncomment if the igniter requires a collider
#endif

        

        private Rigidbody rb;

        private float speed;

        public GameObject conBelt;

        public GameObject[] mechTech;

        public GameObject LocalList;

        public bool GlobalVariable;

        public bool LocalVariable;


        private void FixedUpdate()
        {


         

            if (GlobalVariable == true)
            {
                if (VariablesManager.ExistsGlobal( "speed"))
                {
                   speed = (float)VariablesManager.GetGlobal("speed");
                }
            }

            if (LocalVariable == true)
            {
                if (VariablesManager.ExistsLocal(LocalList, "speed"))
                {
                    speed = (float)VariablesManager.GetLocal(LocalList, "speed");
                }
            }




            rb = this.GetComponent<Rigidbody>();
                rb.AddForce(conBelt.transform.forward * ( speed * 900) * Time.smoothDeltaTime, ForceMode.Force);
            
            


            for (int i = 0; i <= mechTech.Length - 1; i++)
            {
                Physics.IgnoreCollision(mechTech[i].GetComponent<Collider>(), GetComponent<Collider>());
            }

        }

        

      

    }
}