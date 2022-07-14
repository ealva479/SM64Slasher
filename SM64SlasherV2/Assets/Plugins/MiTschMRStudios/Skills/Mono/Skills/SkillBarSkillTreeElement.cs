namespace MiTschMR.Skills
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Events;
    using UnityEngine.UI;

    [AddComponentMenu("Game Creator/Skills/Skill Bar Skill Tree Element")]
    public class SkillBarSkillTreeElement : MonoBehaviour
    {
        protected static DatabaseSkills DATABASE_SKILLS;

        // PROPERTIES: ----------------------------------------------------------------------------

        public KeyCode keyCode = KeyCode.Space;

        [SkillsSingleSkillType]
        [SerializeField] protected int acceptSkillTypes = -1;

        [SerializeField] protected Image skillIcon;
        [SerializeField] protected Image skillNotAssigned;
        [SerializeField] protected Sprite iconSkillBarConfigurationModeOn;
        [SerializeField] protected Sprite iconSkillBarConfigurationModeOff;
        [SerializeField] protected Text keyCodeText;
        [SerializeField] protected bool showKeyCodeText = true;
        [SerializeField] protected bool allowReplacement = false;
        [SerializeField] protected bool allowQuickRemoval = false;

        public SkillAsset skillInConfigurationMode;

        [SerializeField] protected UnityEvent eventOnHoverEnter;
        [SerializeField] protected UnityEvent eventOnHoverExit;

        protected SkillAsset skill = null;
        protected Skills executer = null;
        public bool skillBarConfigurationModeOn = false;
        protected SkillTreeUIManager skillTreeUIManager;
        public int skillBarSkillTreeElementIndex = -1;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected virtual void Start()
        {
            if (DATABASE_SKILLS == null) DATABASE_SKILLS = DatabaseSkills.Load();
            if (DATABASE_SKILLS.GetSkillSettings().GetDragDropSkills())
            {
                this.SetupEvents(EventTriggerType.BeginDrag, this.OnDragBegin);
                this.SetupEvents(EventTriggerType.EndDrag, this.OnDragEnd);
                this.SetupEvents(EventTriggerType.Drag, this.OnDragMove);
            }

            this.SetupEvents(EventTriggerType.PointerEnter, this.OnPointerEnter);
            this.SetupEvents(EventTriggerType.PointerExit, this.OnPointerExit);

            if (DATABASE_SKILLS.GetSkillSettings().GetSkillTreeSelection() == DatabaseSkills.SkillSettings.SkillTreeSelection.Automatic)
            {
                this.skillTreeUIManager = transform.GetComponentInParent<SkillTreeUIManagerAutomatical>();
            }
            else this.skillTreeUIManager = transform.GetComponentInParent<SkillTreeUIManagerManual>();

            this.executer = this.skillTreeUIManager.skillTreeOpener;

            this.UpdateUI();
        }

        protected virtual void SetupEvents(EventTriggerType eventType, UnityAction<BaseEventData> callback)
        {
            EventTrigger.Entry eventTriggerEntry = new EventTrigger.Entry(); { eventTriggerEntry.eventID = eventType; }
            eventTriggerEntry.callback.AddListener(callback);

            EventTrigger eventTrigger = gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null) eventTrigger = gameObject.AddComponent<EventTrigger>();

            eventTrigger.triggers.Add(eventTriggerEntry);
        }

        // UPDATE METHODS: ------------------------------------------------------------------------

        protected virtual void Update()
        {
            if (Input.GetKeyDown(this.keyCode) && this.skillBarConfigurationModeOn) this.BindSkill(this.skillInConfigurationMode);
            else return;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public virtual bool BindSkill(SkillAsset skill)
        {
            this.Start();
            if (skill == null) return false;
            if (this.skill != null) return false;
            if (!skill.assignableToSkillBar) return false;
            if (skill.skillState == Skill.SkillState.Locked) return false;

            this.skill = skill;
            this.skill.addedToSkillBar = true;
            this.skill.addedToSkillBarIndex = this.skillBarSkillTreeElementIndex;
            this.skillNotAssigned.enabled = false;
            this.UpdateUI();
            this.skillTreeUIManager.LeaveSkillBarConfigurationMode();
            this.executer.SyncSkillBarSkills(this, this.skill);

            return true;
        }

        public virtual bool UnbindSkill()
        {
            if (this.skill == null) return true;

            this.skill.addedToSkillBar = false;
            this.skill.addedToSkillBarIndex = -1;
            this.skill = null;
            this.skillNotAssigned.enabled = true;

            this.OnPointerExit(null);
            this.UpdateUI();
            this.skillTreeUIManager.LeaveSkillBarConfigurationMode();
            this.executer.SyncSkillBarSkills(this, this.skill);

            return true;
        }

        public virtual void EnterSkillBarConfigurationMode()
        {
            if ((this.acceptSkillTypes != -1) && (this.skillInConfigurationMode.skillType != this.acceptSkillTypes)) return;
            if (this.skill != null && !this.allowReplacement) return;
            this.skillBarConfigurationModeOn = true;
            if (this.showKeyCodeText) this.keyCodeText.gameObject.SetActive(true);
            this.GetComponent<Image>().sprite = this.iconSkillBarConfigurationModeOn;
        }

        public virtual void LeaveSkillBarConfigurationMode()
        {
            this.skillBarConfigurationModeOn = false;
            this.keyCodeText.gameObject.SetActive(false);
            this.GetComponent<Image>().sprite = this.iconSkillBarConfigurationModeOff;
        }

        public virtual void RemoveSkillFromSkillBar(SkillAsset skill)
        {
            if (this.skill == skill) this.UnbindSkill();
        }

        public virtual SkillAsset GetSkill() => this.skill;

        // CALLBACK METHODS: ----------------------------------------------------------------------

        protected virtual void UpdateUI()
        {
            if (this.skillIcon != null) this.skillIcon.overrideSprite = this.skill?.icon;
        }

        protected virtual void OnPointerEnter(BaseEventData eventData)
        {
            if (this.skill == null) return;
            if (this.eventOnHoverEnter != null && this.skillBarConfigurationModeOn != true) this.eventOnHoverEnter.Invoke();
        }

        protected virtual void OnPointerExit(BaseEventData eventData)
        {
            if (this.eventOnHoverExit != null && this.skillBarConfigurationModeOn != true) this.eventOnHoverExit.Invoke();
        }

        protected virtual void OnDragBegin(BaseEventData eventData)
        {
            if (DATABASE_SKILLS != null &&
                DATABASE_SKILLS.GetSkillSettings().GetDragDropSkills()
                && this.skill != null)
            {
                this.skillTreeUIManager.OnDragSkill(this.skill.icon, true);
                Cursor.SetCursor(
                    null,
                    DATABASE_SKILLS.GetSkillSettings().GetDragDropHotspot(),
                    CursorMode.Auto
                );
                this.skillTreeUIManager.EnterSkillBarConfigurationMode(this.skill);

                eventData.Use();
            }
        }

        protected virtual void OnDragEnd(BaseEventData eventData)
        {
            if (DATABASE_SKILLS != null &&
                DATABASE_SKILLS.GetSkillSettings().GetDragDropSkills())
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                this.skillTreeUIManager.OnDragSkill(null, false);

                PointerEventData pointerData = ((PointerEventData)eventData);
                if (pointerData == null) return;
                if (pointerData.pointerCurrentRaycast.gameObject == null) return;

                GameObject target = pointerData.pointerCurrentRaycast.gameObject;
                SkillBarSkillTreeElement[] targetSkillBarSkillTreeElements = target.GetComponentsInParent<SkillBarSkillTreeElement>();
                if (targetSkillBarSkillTreeElements.Length > 0)
                {
                    for (int i = 0; i < targetSkillBarSkillTreeElements.Length; ++i)
                    {
                        if (!targetSkillBarSkillTreeElements[i].skillBarConfigurationModeOn) continue;

                        SkillAsset skill = this.skill;
                        this.UnbindSkill();
                        if (targetSkillBarSkillTreeElements[i].GetSkill() != null) targetSkillBarSkillTreeElements[i].UnbindSkill();
                        targetSkillBarSkillTreeElements[i].BindSkill(skill);
                        eventData.Use();
                    }
                }
                else
                {
                    if (this.allowQuickRemoval) this.UnbindSkill();
                }

                this.skillTreeUIManager.LeaveSkillBarConfigurationMode();
            }
        }

        protected virtual void OnDragMove(BaseEventData eventData)
        {
            PointerEventData pointerData = ((PointerEventData)eventData);
            if (pointerData == null) return;

            if (DATABASE_SKILLS != null
                && DATABASE_SKILLS.GetSkillSettings().GetDragDropSkills()
                && this.skill != null
                && this.skill.icon != null)
            {
                this.skillTreeUIManager.OnDragSkill(this.skill.icon, true);
            }
        }
    }
}