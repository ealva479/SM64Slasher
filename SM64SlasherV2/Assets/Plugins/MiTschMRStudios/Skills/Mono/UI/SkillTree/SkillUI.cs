namespace MiTschMR.Skills
{
	using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class SkillUI : MonoBehaviour
    {
		protected static DatabaseSkills DATABASE_SKILLS;

		// PROPERTIES: ----------------------------------------------------------------------------

		public SkillAsset SkillAsset { get; protected set; }

		protected Button button;

		[SerializeField] protected SkillTreeUIManager skillTreeUIManager;
		public Skills skillTreeOpener;

		public GameObject skillConnectorEnabled;
		public GameObject skillConnectorDisabled;

		public Image skillIconEnabled;
		public Image skillIconDisabled;

		[SerializeField] protected Image tooltipIcon;
		[SerializeField] protected Text tooltipName;
		[SerializeField] protected Text tooltipDescription;
		[SerializeField] protected Text tooltipLevelRequiredValue;
		[SerializeField] protected Text tooltipSkillPointsRequiredValue;
		[SerializeField] protected Text tooltipSkillStateValue;
		[SerializeField] protected Color tooltipSkillStateValueColorUnlocked = Color.green;
		[SerializeField] protected Color tooltipSkillStateValueColorLocked = Color.red;

		public GameObject imageLocked;
		public GameObject imageUnlocked;

		[SerializeField] protected RectTransform skillTransform;
		[SerializeField] protected GameObject prefabContextMenu;

		[SerializeField] protected GameObject skillContextMenu;

		[SerializeField] protected UnityEvent eventOnHoverEnter;
		[SerializeField] protected UnityEvent eventOnHoverExit;

		// CONSTRUCTOR & UPDATER: -----------------------------------------------------------------

		public virtual void Setup(Skills characterSkills, SkillAsset skill, Skill.SkillState locked)
        {
			this.UpdateUI(skill, locked);
			this.button = gameObject.GetComponentInChildren<Button>();

			if (DATABASE_SKILLS == null) DATABASE_SKILLS = DatabaseSkills.Load();
			if (DATABASE_SKILLS.GetSkillSettings().GetDragDropSkills())
			{
				this.SetupEvents(EventTriggerType.BeginDrag, this.OnDragBegin);
				this.SetupEvents(EventTriggerType.EndDrag, this.OnDragEnd);
				this.SetupEvents(EventTriggerType.Drag, this.OnDragMove);
			}

			this.SetupEvents(EventTriggerType.PointerEnter, this.OnPointerEnter);
			this.SetupEvents(EventTriggerType.PointerExit, this.OnPointerExit);
		}

		// PUBLIC METHODS: ------------------------------------------------------------------------

		public virtual void UpdateUI(SkillAsset skill, Skill.SkillState locked) 
		{
			this.SkillAsset = skill;

			if (this.skillIconEnabled != null && skill.icon != null) this.skillIconEnabled.sprite = skill.icon;
			if (this.skillIconDisabled != null && skill.icon != null) this.skillIconDisabled.sprite = skill.icon;

			if (this.tooltipIcon != null) this.tooltipIcon.sprite = skill.icon;
			if (this.tooltipName != null) this.tooltipName.text = skill.skillName.GetText();
			if (this.tooltipDescription != null) this.tooltipDescription.text = skill.skillDescription.GetText();
			if (this.tooltipLevelRequiredValue != null && skill.requiresLevel)
			{
				this.tooltipLevelRequiredValue.text = skill.level.ToString();

				if (skill.level > this.skillTreeOpener.GetLevel()) this.tooltipLevelRequiredValue.color = Color.red;
				if (skill.level <= this.skillTreeOpener.GetLevel()) this.tooltipLevelRequiredValue.color = Color.green;
			}
			else if (this.tooltipLevelRequiredValue != null && skill.requiresLevel == false)
			{
				this.tooltipLevelRequiredValue.text = "none";
				this.tooltipLevelRequiredValue.color = Color.green;
			}

			if (this.tooltipSkillPointsRequiredValue != null && skill.useSkillPoints)
			{
				this.tooltipSkillPointsRequiredValue.text = skill.skillPointsNeeded.ToString();

				if (skill.skillPointsNeeded > this.skillTreeOpener.GetSkillPoints()) this.tooltipSkillPointsRequiredValue.color = Color.red;
				if (skill.skillPointsNeeded <= this.skillTreeOpener.GetSkillPoints()) this.tooltipSkillPointsRequiredValue.color = Color.green;
			}
			else if (this.tooltipSkillPointsRequiredValue != null && skill.useSkillPoints == false)
			{
				this.tooltipSkillPointsRequiredValue.text = "none";
				this.tooltipSkillPointsRequiredValue.color = Color.green;
			}

			if (this.tooltipSkillStateValue != null && skill.skillState == Skill.SkillState.Locked)
			{
				this.tooltipSkillStateValue.text = "Locked";
				this.tooltipSkillStateValue.color = this.tooltipSkillStateValueColorLocked;
			}
			else if (this.tooltipSkillStateValue != null && skill.skillState == Skill.SkillState.Unlocked)
			{
				this.tooltipSkillStateValue.text = "Unlocked";
				this.tooltipSkillStateValue.color = this.tooltipSkillStateValueColorUnlocked;
			}
		}

		public virtual void OnClick()
		{
			if (skillTreeUIManager.skillContextMenu != null) HideSkillContextMenu();
		}

		public virtual void OnClickRight() => ShowSkillContextMenu(this.SkillAsset.uuid, prefabContextMenu, skillTransform);

		// PROTECTED METHODS: ------------------------------------------------------------------------

		protected virtual void ShowSkillContextMenu(int uuid, GameObject prefabContextMenu, RectTransform skillTransform)
		{
			if (this.skillContextMenu == null && this.skillTreeUIManager.skillContextMenu == null)
			{
				this.skillContextMenu = Instantiate(prefabContextMenu, skillTransform);
				this.skillContextMenu.transform.localScale = Vector3.one;

				SkillContextMenuUI skillContextMenuUI = this.skillContextMenu.GetComponent<SkillContextMenuUI>();

				AddButtonListeners(skillContextMenuUI, uuid);

				SetButtonVisibility(uuid, skillContextMenuUI);

				this.skillTreeUIManager.skillContextMenu = this.skillContextMenu;
			}
			else if (this.skillContextMenu == null && skillTreeUIManager.skillContextMenu != null)
			{
				HideSkillContextMenu();
				this.skillContextMenu = Instantiate(prefabContextMenu, skillTransform);
				this.skillContextMenu.transform.localScale = Vector3.one;

				SkillContextMenuUI skillContextMenuUI = this.skillContextMenu.GetComponent<SkillContextMenuUI>();

				AddButtonListeners(skillContextMenuUI, uuid);

				SetButtonVisibility(uuid, skillContextMenuUI);

				skillTreeUIManager.skillContextMenu = this.skillContextMenu;
			}
			else
			{
				HideSkillContextMenu();
				this.skillContextMenu.SetActive(true);

				SkillContextMenuUI skillContextMenuUI = this.skillContextMenu.GetComponent<SkillContextMenuUI>();

				SetButtonVisibility(uuid, skillContextMenuUI);

				skillTreeUIManager.skillContextMenu = this.skillContextMenu;
			}
		}

		public virtual void HideSkillContextMenu()
		{
			try
			{
				this.skillTreeUIManager.skillContextMenu.SetActive(false);
			}
			catch
			{
				this.skillTreeUIManager.skillContextMenu = null;
			}
		}

		protected virtual void AddButtonListeners(SkillContextMenuUI skillContextMenuUI, int uuid)
		{
			Button btnUnlock = skillContextMenuUI.buttonUnlock.GetComponent<Button>();
			Button btnAddToSkillBar = skillContextMenuUI.buttonAddToSkillBar.GetComponent<Button>();
			Button btnRemoveFromSkillBar = skillContextMenuUI.buttonRemoveFromSkillBar.GetComponent<Button>();

			btnUnlock.onClick.AddListener(() => UnlockSkill(uuid));
			btnAddToSkillBar.onClick.AddListener(() => SkillAddToSkillBar(uuid));
			btnRemoveFromSkillBar.onClick.AddListener(() => SkillRemoveFromSkillBar(uuid));
		}

		protected virtual void SetButtonVisibility(int uuid, SkillContextMenuUI skillContextMenuUI)
		{
			SkillAsset skill = this.skillTreeOpener.GetSkill(uuid);

			GameObject buttonUnlock = skillContextMenuUI.buttonUnlock;
			GameObject buttonAddToSkillBar = skillContextMenuUI.buttonAddToSkillBar;
			GameObject buttonRemoveFromSkillBar = skillContextMenuUI.buttonRemoveFromSkillBar;

			if (skill.skillState == Skill.SkillState.Locked)
			{
				buttonUnlock.SetActive(true);
				buttonAddToSkillBar.SetActive(false);
				buttonRemoveFromSkillBar.SetActive(false);
			}
			else if (skill.skillState == Skill.SkillState.Unlocked && skill.assignableToSkillBar && skill.addedToSkillBar == false)
			{
				buttonUnlock.SetActive(false);
				buttonAddToSkillBar.SetActive(true);
				buttonRemoveFromSkillBar.SetActive(false);
			}
			else if (skill.skillState == Skill.SkillState.Unlocked && skill.addedToSkillBar)
			{
				buttonUnlock.SetActive(false);
				buttonAddToSkillBar.SetActive(false);
				buttonRemoveFromSkillBar.SetActive(true);
			}
			else
			{
				buttonUnlock.SetActive(false);
				buttonAddToSkillBar.SetActive(false);
				buttonRemoveFromSkillBar.SetActive(false);
			}
		}

		protected virtual void UnlockSkill(int uuid)
		{
			if (this.skillTreeOpener.UnlockSkill(this.skillTreeOpener.GetSkill(uuid)))
			{
				if (this.skillConnectorDisabled != null) this.skillConnectorDisabled.SetActive(false);
				if (this.skillConnectorDisabled != null) this.skillConnectorEnabled.SetActive(true);
				if (this.skillIconDisabled != null) this.skillIconDisabled.gameObject.SetActive(false);
				if (this.skillIconEnabled != null) this.skillIconEnabled.gameObject.SetActive(true);
				if (this.imageLocked != null) this.imageLocked.SetActive(false);
				if (this.imageUnlocked != null) this.imageUnlocked.SetActive(true);
				this.skillTreeUIManager.UpdateSkillPoints();
				this.UpdateUI(this.skillTreeOpener.GetSkill(uuid), this.skillTreeOpener.GetSkillState(uuid));

				if (this.skillTreeOpener.GetSkill(uuid).skillExecutionType == Skill.SkillExecutionType.PermanentExecute) this.skillTreeOpener.StartCoroutine(this.skillTreeOpener.ExecuteSkillPermanent(uuid));
			}

			HideSkillContextMenu();
		}

		protected virtual void SkillAddToSkillBar(int uuid)
		{
			this.skillTreeUIManager.EnterSkillBarConfigurationMode(this.skillTreeOpener.GetSkill(uuid));
			HideSkillContextMenu();
		}
		protected virtual void SkillRemoveFromSkillBar(int uuid)
		{
			this.skillTreeUIManager.SkillRemoveFromSkillBar(this.skillTreeOpener.GetSkill(uuid));
			HideSkillContextMenu();
		}

		protected virtual void SetupEvents(EventTriggerType eventType, UnityAction<BaseEventData> callback)
		{
            EventTrigger.Entry eventTriggerEntry = new EventTrigger.Entry { eventID = eventType };
            eventTriggerEntry.callback.AddListener(callback);

			EventTrigger eventTrigger = gameObject.GetComponent<EventTrigger>();
			if (eventTrigger == null) eventTrigger = gameObject.AddComponent<EventTrigger>();

			eventTrigger.triggers.Add(eventTriggerEntry);
		}

		protected virtual void OnPointerEnter(BaseEventData eventData)
		{
			if (this.eventOnHoverEnter != null) this.eventOnHoverEnter.Invoke();
		}

		protected virtual void OnPointerExit(BaseEventData eventData)
		{
			if (this.eventOnHoverExit != null) this.eventOnHoverExit.Invoke();
		}

		protected virtual void OnDragBegin(BaseEventData eventData)
		{
			if (DATABASE_SKILLS != null &&
				DATABASE_SKILLS.GetSkillSettings().GetDragDropSkills()
				&& this.skillTreeOpener.GetSkill(this.SkillAsset.uuid).skillState == Skill.SkillState.Unlocked
				&& this.skillTreeOpener.GetSkill(this.SkillAsset.uuid).assignableToSkillBar
				&& !this.skillTreeOpener.GetSkill(this.SkillAsset.uuid).addedToSkillBar)
			{
				this.skillTreeUIManager.OnDragSkill(this.SkillAsset.icon, true);
				Cursor.SetCursor(
					null,
					DATABASE_SKILLS.GetSkillSettings().GetDragDropHotspot(),
					CursorMode.Auto
				);
				this.skillTreeUIManager.EnterSkillBarConfigurationMode(this.SkillAsset);

				eventData.Use();
			}
		}

		protected virtual void OnDragEnd(BaseEventData eventData)
		{
			if (DATABASE_SKILLS != null &&
				DATABASE_SKILLS.GetSkillSettings().GetDragDropSkills() && this.SkillAsset.icon != null
				&& this.skillTreeOpener.GetSkill(this.SkillAsset.uuid).skillState == Skill.SkillState.Unlocked
				&& this.skillTreeOpener.GetSkill(this.SkillAsset.uuid).assignableToSkillBar
				&& !this.skillTreeOpener.GetSkill(this.SkillAsset.uuid).addedToSkillBar)
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

						if (targetSkillBarSkillTreeElements[i].GetSkill() != null) targetSkillBarSkillTreeElements[i].UnbindSkill();
						targetSkillBarSkillTreeElements[i].BindSkill(this.SkillAsset);
						eventData.Use();
					}
				}

				this.skillTreeUIManager.LeaveSkillBarConfigurationMode();
			}
		}

		protected virtual void OnDragMove(BaseEventData eventData)
		{
			PointerEventData pointerData = ((PointerEventData)eventData);
			if (pointerData == null) return;

			if (DATABASE_SKILLS != null &&
				DATABASE_SKILLS.GetSkillSettings().GetDragDropSkills() && this.SkillAsset.icon != null
				&& this.skillTreeOpener.GetSkill(this.SkillAsset.uuid).skillState == Skill.SkillState.Unlocked
				&& this.skillTreeOpener.GetSkill(this.SkillAsset.uuid).assignableToSkillBar
				&& !this.skillTreeOpener.GetSkill(this.SkillAsset.uuid).addedToSkillBar)
			{
				this.skillTreeUIManager.OnDragSkill(this.SkillAsset.icon, true);
			}
		}
	}
}