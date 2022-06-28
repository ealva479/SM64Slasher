namespace GameCreator.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;
    using TMPro;
    using GameCreator.Variables;

    [AddComponentMenu("")]
    public class OnButtonClick : Igniter 
	{
		#if UNITY_EDITOR
        public new static string NAME = "UI/OnButtonClick";
        //public new static string COMMENT = "Uncomment to add an informative message";
        //public new static bool REQUIRES_COLLIDER = true; // uncomment if the igniter requires a collider
        #endif

        //public bool example = false;

        [Space]
        [Tooltip("Place your Button Object Here")]
        public Button yourButton;

        private void Start()
        {

            Button btn = yourButton.GetComponent<Button>();
            btn.onClick.AddListener(TaskOnClick);

            void TaskOnClick()
            {
                Debug.Log("You have clicked the button!");
                this.ExecuteTrigger(gameObject);
            }

            /*
            if (this.example)
            {
                this.ExecuteTrigger(gameObject);
                this.example = false;
            }
            */
        }
	}
}