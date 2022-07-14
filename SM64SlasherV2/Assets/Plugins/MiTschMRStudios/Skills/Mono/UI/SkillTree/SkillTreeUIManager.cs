namespace MiTschMR.Skills
{
    using GameCreator.Core;
    using UnityEngine;
	using UnityEngine.UI;

	public class SkillTreeUIManager : MonoBehaviour
	{
		protected const int TIME_LAYER = 200;

		protected static DatabaseSkills DATABASE_SKILLS;

		protected Animator skillTreeAnimator;
		protected GameObject skillTreeRoot;
		protected bool isOpen = false;
		protected CanvasScaler canvasScaler;
		protected RectTransform floatingSkillRt;

		[HideInInspector] public Skills skillTreeOpener;
		public SkillBarSkillTreeElements skillBarElements;
		[SerializeField] protected Text skillPointsLeft;
		public SkillTreeItemsUI skillsContent = null;

		public Image floatingSkill;
		
		[HideInInspector] public GameObject skillContextMenu;

		// INITIALIZERS: --------------------------------------------------------------------------

		protected virtual void Awake()
		{
			if (transform.childCount >= 1)
			{
				this.skillTreeRoot = transform.GetChild(0).gameObject;
				this.skillTreeAnimator = this.skillTreeRoot.GetComponent<Animator>();
			}

			if (this.floatingSkill != null) this.floatingSkillRt = this.floatingSkill.GetComponent<RectTransform>();
			this.OnDragSkill(null, false);
		}

		// PUBLIC METHODS: ------------------------------------------------------------------------

		public virtual void Open(Skills characterSkills)
		{
			if (this.isOpen) return;

			this.ChangeState(true);

			this.SetSkillTreeOpenerReferences(characterSkills);

			this.UpdateSkillPoints();

			if (DATABASE_SKILLS.GetSkillSettings().GetPauseTimeOnUI())
			{
				TimeManager.Instance.SetTimeScale(0f, TIME_LAYER);
			}
		}

		public virtual void Close()
		{
			if (!this.isOpen) return;

			this.skillTreeOpener.skillTreeWindowOpen = false;
			this.LeaveSkillBarConfigurationMode();

			if (DATABASE_SKILLS.GetSkillSettings().GetPauseTimeOnUI())
			{
				TimeManager.Instance.SetTimeScale(1f, TIME_LAYER);
			}

			this.ChangeState(false);
		}

		public virtual void EnterSkillBarConfigurationMode(SkillAsset skill)
		{
			this.skillTreeOpener.skillBarConfigurationModeOn = true;
			for (int i = 0; i < this.skillBarElements.skillBarElements.Length; i++)
			{
				this.skillBarElements.skillBarElements[i].skillInConfigurationMode = skill;
				this.skillBarElements.skillBarElements[i].EnterSkillBarConfigurationMode();
			}
		}

		public virtual void LeaveSkillBarConfigurationMode()
		{
			this.skillTreeOpener.skillBarConfigurationModeOn = false;
			for (int i = 0; i < this.skillBarElements.skillBarElements.Length; i++)
			{
				this.skillBarElements.skillBarElements[i].LeaveSkillBarConfigurationMode();
			}
		}

		public virtual void SkillRemoveFromSkillBar(SkillAsset skill)
		{
			for (int i = 0; i < this.skillBarElements.skillBarElements.Length; i++)
			{
				this.skillBarElements.skillBarElements[i].RemoveSkillFromSkillBar(skill);
			}
		}

		public virtual void ResetAllSkills()
		{
			this.skillTreeOpener.FireResetSkillsEvent();
			this.skillTreeOpener.CancelExecutingSkills(false, true);
			this.skillTreeOpener.ResetAllSkills();
			this.skillTreeOpener.ResetPhaseProperties();
			if (this.skillTreeOpener.useSkillBar) this.ResetSkillBar();
			this.UpdateSkillPoints();
			this.skillsContent.UpdateSkills(this.skillTreeOpener);
		}

		public virtual void ResetSkill(SkillHolder skillHolder)
        {
			SkillAsset skill = this.skillTreeOpener.GetSkill(skillHolder.skill.uuid);
			this.skillTreeOpener.CancelExecutingSkill(skill, false, true);
			this.skillTreeOpener.ResetSkill(skill);
			this.skillTreeOpener.ResetPhaseProperties();
			if (this.skillTreeOpener.useSkillBar) this.ResetSkillBar(skill);
			this.UpdateSkillPoints();
			this.skillsContent.UpdateSkills(this.skillTreeOpener);
		}

		public virtual void UpdateSkillPoints() => this.skillPointsLeft.text = this.skillTreeOpener.GetSkillPoints().ToString();

		public virtual void HideSkillContextMenu()
		{
			try
			{
				this.skillContextMenu.SetActive(false);
			}
			catch
			{
				this.skillContextMenu = null;
			}
		}

		public virtual void UpdateSkillBar(SkillHolder skillHolder)
		{
			SkillAsset skill = this.skillTreeOpener.GetSkill(skillHolder.skill.uuid);
			foreach (SkillBarSkillTreeElement skillBarSkillTreeElement in this.skillBarElements.skillBarElements)
			{
				if (skillBarSkillTreeElement.GetSkill() != null && skillBarSkillTreeElement.GetSkill().uuid == skill.uuid)
				{
					skillBarSkillTreeElement.UnbindSkill();
					skillBarSkillTreeElement.BindSkill(skill);
					this.skillTreeOpener.SyncSkillBarSkills(skillBarSkillTreeElement, skill);
				}
			}
		}

		public virtual void OnDragSkill(Sprite sprite, bool dragging)
		{
			if (!this.floatingSkill) return;

			this.floatingSkill.gameObject.SetActive(dragging);
			if (!dragging) return;

			this.floatingSkill.sprite = sprite;

			Vector2 position = this.GetPointerPositionUnscaled(Input.mousePosition);
			this.floatingSkillRt.anchoredPosition = position;
		}

		// PROTECTED METHODS: -----------------------------------------------------------------------

		protected virtual bool IsOpen() => this.isOpen;

		protected virtual void ChangeState(bool toOpen)
		{
			if (this.skillTreeRoot == null)
			{
				Debug.LogError("Unable to find skillTreeRoot");
				return;
			}

			this.isOpen = toOpen;

			if (this.skillTreeAnimator == null)
			{
				this.skillTreeRoot.SetActive(toOpen);
				return;
			}
				
			this.skillTreeAnimator.SetBool("State", toOpen);
		}

		protected virtual void SetSkillTreeOpenerReferences(Skills characterSkills)
		{
			characterSkills.skillTreeUIManager = this;
			this.skillTreeOpener = characterSkills;
		}

		protected virtual void ResetSkillBar()
        {
			foreach (SkillBarSkillTreeElement skillBarSkillTreeElement in this.skillBarElements.skillBarElements)
            {
				if (skillBarSkillTreeElement.GetSkill() != null) skillBarSkillTreeElement.UnbindSkill();
				this.skillTreeOpener.SyncSkillBarSkills(skillBarSkillTreeElement, null);
			}
        }

		protected virtual void ResetSkillBar(SkillAsset skill)
		{
			foreach (SkillBarSkillTreeElement skillBarSkillTreeElement in this.skillBarElements.skillBarElements)
			{
				if (skillBarSkillTreeElement.GetSkill() != null && skillBarSkillTreeElement.GetSkill().uuid == skill.uuid) skillBarSkillTreeElement.UnbindSkill();
				this.skillTreeOpener.SyncSkillBarSkills(skillBarSkillTreeElement, null);
			}
		}

		protected virtual Vector2 GetPointerPositionUnscaled(Vector2 mousePosition)
		{
			if (this.canvasScaler == null) this.canvasScaler = transform.GetComponentInParent<CanvasScaler>();
			if (this.canvasScaler == null) return mousePosition;

			Vector2 referenceResolution = this.canvasScaler.referenceResolution;
			Vector2 currentResolution = new Vector2(Screen.width, Screen.height);

			float widthRatio = currentResolution.x / referenceResolution.x;
			float heightRatio = currentResolution.y / referenceResolution.y;
			float ratio = Mathf.Lerp(widthRatio, heightRatio, this.canvasScaler.matchWidthOrHeight);

			return mousePosition / ratio;
		}
	}
}

