namespace MiTschMR.Skills
{
    using GameCreator.Core;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Events;
    using UnityEngine.UI;

    [AddComponentMenu("Game Creator/Skills/Skill Bar Element")]
    public class SkillBarElement : MonoBehaviour
    {
        [SerializeField] protected TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);

        public KeyCode keyCode = KeyCode.Space;

        [SerializeField] protected Image skillIcon;
        [SerializeField] protected Image skillNotAssigned;
        [SerializeField] protected Image skillCooldownIcon;
        [SerializeField] protected bool showSkillExecutingIcon = true;
        [SerializeField] protected Image skillExecutingIcon;
        [SerializeField] protected bool showKeyCodeText = true;
        [SerializeField] protected Text keyCodeText;
        [SerializeField] protected bool showExecutingTimeCounter = false;
        [SerializeField] protected Text executingTimeCounter;
        [SerializeField] protected bool showCooldownCounter = false;
        [SerializeField] protected Text cooldownCounter;
        [SerializeField] protected bool showAmountCounter = false;
        [SerializeField] protected Text amountCounter;

        public SkillAsset skill = null;
        protected Skills executer = null;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected virtual void Start()
        {
            this.executer = target.GetGameObject(gameObject).GetComponent<Skills>();
            this.UpdateUI();
        }

        protected virtual void SetupEvents(EventTriggerType eventType, UnityAction<BaseEventData> callback)
        {
            EventTrigger.Entry eventTriggerEntry = new EventTrigger.Entry { eventID = eventType };
            eventTriggerEntry.callback.AddListener(callback);

            EventTrigger eventTrigger = gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null) eventTrigger = gameObject.AddComponent<EventTrigger>();

            eventTrigger.triggers.Add(eventTriggerEntry);
        }

        // UPDATE METHODS: ------------------------------------------------------------------------

        protected virtual void Update()
        {
            if (this.skill == null) return;
            if (this.showAmountCounter && this.amountCounter != null) this.amountCounter.text = this.skill.currentAmount.ToString();
            if (Input.GetKeyDown(this.keyCode) 
                && this.executer.useSkillBar
                && !this.executer.skillTreeWindowOpen
                && !this.executer.IsActivatingSkill
                && !this.executer.IsCastingSkill
                && !this.executer.IsExecutingSkill
                && !this.executer.IsFinishingSkill
                )
            {
                this.ExecuteSkill();
            }
            else return;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public virtual void ExecuteSkill()
        {
            if (this.skill == null) return;
            if (this.skill.skillExecutionType == Skill.SkillExecutionType.Stored && this.skill.currentAmount == 0) { }
            else
            {
                this.executer.StartCoroutine(this.executer.ExecuteSkill(this.skill));
                this.StartCoroutine(StartSkillExecutionGUI());
            }
        }

        public virtual void SyncSkill(SkillAsset skill)
        {
            if (skill != null && skill.skillState == Skill.SkillState.Locked) return;
            this.skill = skill;
            this.StopAllCoroutines();
            this.StartCoroutine(this.StartSkillExecutionGUI());
            this.UpdateUI();
        }

        public virtual IEnumerator StartSkillExecutionGUI()
        {
            if (this.skill == null)
            {
                if (this.skillExecutingIcon != null) this.skillExecutingIcon.gameObject.SetActive(false);
                if (this.skillCooldownIcon != null) this.skillCooldownIcon.gameObject.SetActive(false);
                yield break;
            }
            else this.skillCooldownIcon.gameObject.SetActive(false);

            if (this.executingTimeCounter != null) this.executingTimeCounter.gameObject.SetActive(false);
            if (this.cooldownCounter != null) this.cooldownCounter.gameObject.SetActive(false);

            if (this.skill != null && this.skill.isExecuting)
            {
                if (this.showSkillExecutingIcon) this.skillExecutingIcon.gameObject.SetActive(true);
                while (this.skill != null && this.skill.isExecuting)
                {
                    if (this.showExecutingTimeCounter && this.executingTimeCounter != null && this.executer.IsExecutingSkill)
                    {
                        this.executingTimeCounter.gameObject.SetActive(true);
                        this.executingTimeCounter.text = (this.skill.executionTimeLeft > 10) ? Mathf.Round(this.skill.executionTimeLeft).ToString() : this.skill.executionTimeLeft.ToString();
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                if (this.executingTimeCounter != null) this.executingTimeCounter.gameObject.SetActive(false);

                if (this.skill == null) yield break;

                if (this.skillExecutingIcon != null) this.skillExecutingIcon.gameObject.SetActive(false);
                if (this.skillCooldownIcon != null) this.skillCooldownIcon.gameObject.SetActive(true);

                while (this.skill != null && this.skill.cooldownTimeLeft > 0)
                {
                    if (this.skillCooldownIcon != null)
                    {
                        this.skillCooldownIcon.fillAmount = 1f / (this.skill.isInCooldownBetween ? this.skill.cooldownTimeBetween : this.skill.cooldownTime) * this.skill.cooldownTimeLeft;
                        if (this.showCooldownCounter && this.cooldownCounter != null)
                        {
                            this.cooldownCounter.gameObject.SetActive(true);
                            this.cooldownCounter.text = (this.skill.cooldownTimeLeft > 10) ? Mathf.Round(this.skill.cooldownTimeLeft).ToString() : this.skill.cooldownTimeLeft.ToString();
                        }
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                if (this.skill == null)
                {
                    if (this.skillCooldownIcon != null) this.skillCooldownIcon.gameObject.SetActive(false);
                    yield break;
                }
            }
            else if ((this.skill != null && this.skill.isInCooldown) || (this.skill != null && this.skill.isInCooldownAfter))
            {
                if (this.showSkillExecutingIcon) this.skillExecutingIcon.gameObject.SetActive(false);
                if (this.skillCooldownIcon != null) this.skillCooldownIcon.gameObject.SetActive(true);
                while (this.skill != null && this.skill.cooldownTimeLeft > 0)
                {
                    if (this.skillCooldownIcon != null)
                    {
                        this.skillCooldownIcon.fillAmount = 1f / this.skill.cooldownTime * this.skill.cooldownTimeLeft;
                        if (this.showCooldownCounter && this.cooldownCounter != null)
                        {
                            this.cooldownCounter.gameObject.SetActive(true);
                            this.cooldownCounter.text = (this.skill.cooldownTimeLeft > 10) ? Mathf.Round(this.skill.cooldownTimeLeft).ToString() : this.skill.cooldownTimeLeft.ToString();
                        }
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                if (this.skill == null)
                {
                    if (this.skillCooldownIcon != null) this.skillCooldownIcon.gameObject.SetActive(false);
                    yield break;
                }
            }

            if (this.cooldownCounter != null) this.cooldownCounter.gameObject.SetActive(false);
            if (this.executingTimeCounter != null) this.executingTimeCounter.gameObject.SetActive(false);
            if (this.skillCooldownIcon != null) this.skillCooldownIcon.gameObject.SetActive(false);
        }

        protected virtual void UpdateUI()
        {
            if (this.skill != null) this.skillNotAssigned.enabled = false;
            else this.skillNotAssigned.enabled = true;

            if (this.skillIcon != null) this.skillIcon.overrideSprite = this.skill?.icon;

            if (this.showKeyCodeText)
            {
                if (this.keyCodeText != null) this.keyCodeText.gameObject.SetActive(true);
            }
            else
            {
                if (this.keyCodeText != null) this.keyCodeText.gameObject.SetActive(false);
            }

            if (this.executingTimeCounter != null) this.executingTimeCounter.gameObject.SetActive(false);
            if (this.cooldownCounter != null) this.cooldownCounter.gameObject.SetActive(false);
            if (this.skill != null && this.skill.skillExecutionType == Skill.SkillExecutionType.Stored && this.showAmountCounter && this.amountCounter != null) this.amountCounter.gameObject.SetActive(true);
            else
            {
                if (this.showAmountCounter && this.amountCounter != null) this.amountCounter.gameObject.SetActive(false);
            }
        }

        public virtual void CancelSkillBarSkillExecution()
        {
            this.StopAllCoroutines();
            this.skillExecutingIcon.gameObject.SetActive(false);
            this.skillCooldownIcon.gameObject.SetActive(false);
        }
    }
}