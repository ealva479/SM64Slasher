namespace MiTschMR.Skills
{
    using GameCreator.Core;
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using static UnityEngine.UI.Button;

    #if UNITY_EDITOR
    using UnityEditor.Events;
    #endif

    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Actions))]
    [AddComponentMenu("Game Creator/UI/Button Skill", 10)]
    public class ButtonActionsSkills : Selectable, IPointerClickHandler, ISubmitHandler
    {
        [Serializable]
        public class ButtonActionsEvent : UnityEvent<GameObject> { }

        // PROPERTIES: ----------------------------------------------------------------------------

        public Actions actions = null;

        [SerializeField]
        protected ButtonClickedEvent onClickRight = new ButtonClickedEvent();

        [SerializeField]
        protected ButtonActionsEvent onClick = new ButtonActionsEvent();

        // INITIALIZERS: --------------------------------------------------------------------------

        protected new virtual void Awake()
        {
            base.Awake();

            if (!Application.isPlaying) return;
            EventSystemManager.Instance.Wakeup();
        }

        // VALIDATE: ------------------------------------------------------------------------------

        #if UNITY_EDITOR
        protected new virtual void OnValidate()
        {
            base.OnValidate();

            if (this.actions == null)
            {
                this.actions = gameObject.GetComponent<Actions>();
                if (this.actions == null) return;

                this.onClick.RemoveAllListeners();
                UnityEventTools.AddObjectPersistentListener<GameObject>(
                    this.onClick,
                    this.actions.ExecuteWithTarget,
                    gameObject
                );
            }

            this.actions.hideFlags = HideFlags.HideInInspector;
        }
        #endif

        // INTERFACES: ----------------------------------------------------------------------------

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) this.Press();
            else if (eventData.button == PointerEventData.InputButton.Right) this.PressRight();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            this.Press();

            if (!IsActive() || !IsInteractable()) return;
            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());
        }

        protected virtual IEnumerator OnFinishSubmit()
        {
            var fadeTime = colors.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            DoStateTransition(currentSelectionState, false);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        protected virtual void Press()
        {
            if (!IsActive() || !IsInteractable()) return;
            if (this.onClick != null) this.onClick.Invoke(gameObject);
        }

        protected virtual void PressRight()
        {
            if (!IsActive() || !IsInteractable()) return;
            if (this.onClickRight != null) this.onClickRight.Invoke();
        }
    }
}