namespace MiTschMR.Skills
{
    using GameCreator.Core;
    using GameCreator.Stats;
    using GameCreator.Variables;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Events;
    using static MiTschMR.Skills.DatabaseSkills.SkillSettings;

    [AddComponentMenu("MiTschMR Studios/Skills/Skills")]
    public class Skills : GlobalID, IGameSave
    {
        [Serializable]
        public class SerialSkill
        {
            public int skillUniqueID = 0;
            public bool skillUnlocked = false;
            public int skillType = -1;
            public float castTime = -1f;
            public float executionTime = -1f;
            public float cooldownTime = -1f;
            public float cooldownTimeBetween = -1f;
            public float maxAmount = -1f;
            public float startAmount = -1f;
            public bool addedToSkillBar = false;
            public int addedToSkillBarIndex = -1;

            public SerialSkill(int uniqueID, bool skillUnlocked, int skillType, float castTime, float executionTime, float cooldownTime, float cooldownTimeBetween, float maxAmount, float startAmount, bool addedToSkillBar, int addedToSkillBarIndex)
            {
                this.skillUniqueID = uniqueID;
                this.skillUnlocked = skillUnlocked;
                this.skillType = skillType;
                this.castTime = castTime;
                this.executionTime = executionTime;
                this.cooldownTime = cooldownTime;
                this.cooldownTimeBetween = cooldownTimeBetween;
                this.maxAmount = maxAmount;
                this.startAmount = startAmount;
                this.addedToSkillBar = addedToSkillBar;
                this.addedToSkillBarIndex = addedToSkillBarIndex;
            }
        }

        [Serializable]
        public class SerialData
        {
            public SerialSkill[] skills = new SerialSkill[0];
        }

        // EVENTS: --------------------------------------------------------------------------------

        public class EventArgs
        {
            public enum Operation
            {
                Unlock,
                Lock
            }

            public int uuid;
            public Operation operation;

            public EventArgs(int uuid, Operation operation)
            {
                this.uuid = uuid;
                this.operation = operation;
            }
        }

        public class EventSkillActivation : UnityEvent<SkillAsset, bool> { }
        public class EventSkillsReset : UnityEvent { }

        // STATIC AND CONSTANTS: ------------------------------------------------------------------

        protected Dictionary<int, SkillAsset> SKILLS = null;
        protected DatabaseSkills.SkillSettings SKILLSSETTINGS = null;

        public List<SkillAsset> skillsList = new List<SkillAsset>();

        protected const string MSG_ERROR_LEVELNOTASSIGNED = "No stat or global variable was assigned. Please select either a stat or a global number variable for the level";
        protected const string MSG_ERROR_SKILLPOINTSNOTASSIGNED = "No stat or global variable was assigned. Please select either a stat or a global number variable for the skill points";
        protected const string MSG_ERROR_STATSCOMPONENTNOTASSIGNED = "No stat component assigned to the character. Please add the stats component";

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool useSkillBar = true;
        [SerializeField] protected SkillBarElements skillBarElements;
        [SerializeField] protected bool saveSkills = true;
        public bool skillBarConfigurationModeOn = false;
        [HideInInspector] public bool skillTreeWindowOpen = false;
        [HideInInspector] public SkillTreeUIManager skillTreeUIManager;

        protected Coroutine storedSkillCooldown;
        protected Action<EventArgs> onChangeSkill;

        public EventSkillActivation eventOnActivateStart = new EventSkillActivation();
        public EventSkillActivation eventOnActivateEnd = new EventSkillActivation();
        public EventSkillActivation eventOnCastStart = new EventSkillActivation();
        public EventSkillActivation eventOnCastEnd = new EventSkillActivation();
        public EventSkillActivation eventOnExecuteStart = new EventSkillActivation();
        public EventSkillActivation eventOnExecuteEnd = new EventSkillActivation();
        public EventSkillActivation eventOnFinishingStart = new EventSkillActivation();
        public EventSkillActivation eventOnFinishingEnd = new EventSkillActivation();
        public EventSkillsReset eventSkillsReset = new EventSkillsReset();

        public bool IsActivatingSkill { get; protected set; }
        public bool IsCastingSkill { get; protected set; }
        public bool IsExecutingSkill { get; protected set; }
        public bool IsFinishingSkill { get; protected set; }

        // INITIALIZERS: --------------------------------------------------------------------------
        protected override void Awake()
        {
            base.Awake();
            if (!Application.isPlaying) return;
            this.RequireInit();
        }

        protected void Start()
        {
            if (!Application.isPlaying) return;
            SaveLoadManager.Instance.Initialize(this);

            for (int i = 0; i < this.skillsList.Count; i++)
            {
                if (this.skillsList[i].skillState == Skill.SkillState.Unlocked && this.skillsList[i].skillExecutionType == Skill.SkillExecutionType.PermanentExecute)
                {
                    this.StartCoroutine(this.ExecuteSkillPermanent(this.skillsList[i].uuid));
                }
                else if (this.skillsList[i].skillState == Skill.SkillState.Unlocked && this.skillsList[i].skillExecutionType == Skill.SkillExecutionType.Stored && this.storedSkillCooldown == null)
                {
                    this.storedSkillCooldown = this.StartCoroutine(this.StartCooldownStored(this.GetSkill(this.skillsList[i].uuid)));
                }
            }
        }

        protected void OnDestroy()
        {
            base.OnDestroyGID();

            if (!Application.isPlaying) return;
            if (this.exitingApplication) return;
            SaveLoadManager.Instance.OnDestroyIGameSave(this);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public virtual Dictionary<int, SkillAsset> GetSkillsDictionary()
        {
            if (this.SKILLS != null) return this.SKILLS;
            else return null;
        }

        public virtual float GetSkillPoints()
        {
            switch (SKILLSSETTINGS.GetSkillPointsSelection())
            {
                case DatabaseSkills.SkillSettings.StatSelection.GlobalVariable:
                    return Convert.ToSingle(SKILLSSETTINGS.GetSkillPointsVariable().Get());

                case DatabaseSkills.SkillSettings.StatSelection.Stat:
                    return this.GetComponent<Stats>().GetStat(SKILLSSETTINGS.GetSkillPointsStat().stat.uniqueName);
            }
            return 0;
        }

        public virtual float GetLevel()
        {
            switch (SKILLSSETTINGS.GetLevelSelection())
            {
                case DatabaseSkills.SkillSettings.StatSelection.GlobalVariable:
                    return Convert.ToSingle(SKILLSSETTINGS.GetLevelVariable().Get());

                case DatabaseSkills.SkillSettings.StatSelection.Stat:
                    return this.GetComponent<Stats>().GetStat(SKILLSSETTINGS.GetLevelStat().stat.uniqueName);
            }
            return 0;
        }

        public virtual SkillAsset GetSkill(int uuid)
        {
            if (this.SKILLS.ContainsKey(uuid)) return this.SKILLS[uuid];
            return null;
        }

        public virtual List<SkillAsset> GetSkills()
        {
            List<SkillAsset> skills = new List<SkillAsset>();

            if (this.SKILLS == null) return null;
            foreach (KeyValuePair<int, SkillAsset> skill in this.SKILLS)
            {
                skills.Add(skill.Value);
            }

            return skills;
        }

        public virtual int GetSkillID(int uuid)
        {
            if (this.SKILLS.ContainsKey(uuid)) return this.SKILLS[uuid].uuid;
            return 0;
        }

        public virtual Skill.SkillState GetSkillState(int uuid)
        {
            if (this.SKILLS.ContainsKey(uuid)) return this.SKILLS[uuid].skillState;
            return Skill.SkillState.Locked;
        }

        public virtual bool GetSkillLockedState(int uuid, Skill.SkillState skillState)
        {
            if (this.SKILLS.ContainsKey(uuid))
            {
                switch (skillState)
                {
                    case Skill.SkillState.Locked:
                        return this.SKILLS[uuid].skillState == Skill.SkillState.Locked;

                    case Skill.SkillState.Unlocked:
                        return this.SKILLS[uuid].skillState == Skill.SkillState.Unlocked;
                }
            }
            return false;
        }

        public virtual bool GetSkillExecutionState(SkillAsset skill) => this.SKILLS[skill.uuid].isExecuting;

        protected virtual bool LockSkill(SkillAsset skill, bool informCallbacks = true)
        {
            skill.skillState = Skill.SkillState.Locked;
            if (skill.onChange != null) skill.onChange.Invoke();
            if (informCallbacks && this.onChangeSkill != null) this.onChangeSkill.Invoke(new EventArgs(
                skill.uuid, EventArgs.Operation.Lock
            ));
            return true;
        }

        public virtual bool UnlockSkill(SkillAsset skill, bool informCallbacks = true)
        {
            if (skill.skillState == Skill.SkillState.Unlocked) return true;
            if (this.CheckSkillRequirements(skill))
            {
                skill.skillState = Skill.SkillState.Unlocked;
                this.SKILLS[skill.uuid] = skill;

                foreach (KeyValuePair<int, SkillAsset> skillAsset in this.SKILLS)
                {
                    if (null == skillAsset.Value.reliesOnSkills) continue;
                    foreach (SkillHolderSerialized skillHolder in skillAsset.Value.reliesOnSkills)
                    {
                        if (skill.uuid == skillHolder.skill.uuid) skillHolder.skill.skillState = skill.skillState;
                    }
                }

                if (skill.onChange != null) skill.onChange.Invoke();
                if (informCallbacks && this.onChangeSkill != null) this.onChangeSkill.Invoke(new EventArgs(
                    skill.uuid, EventArgs.Operation.Unlock
                ));

                if (skill.skillExecutionType == Skill.SkillExecutionType.PermanentExecute) this.StartCoroutine(this.ExecuteSkillPermanent(skill.uuid));
                if (skill.skillExecutionType == Skill.SkillExecutionType.Stored) this.storedSkillCooldown = this.StartCoroutine(this.StartCooldownStored(skill));
                
                return true;
            }
            return false;
        }

        public virtual void UpdateSkillState(int uuid, Skill.SkillState state)
        {
            this.RequireInit();

            if (!this.SKILLS.ContainsKey(uuid))
            {
                Debug.LogError("Could not find skill UUID in the skill list");
                return;
            }

            SkillAsset skill = this.GetSkill(uuid);

            if (state == Skill.SkillState.Locked) this.LockSkill(skill);
            else this.UnlockSkill(skill);

            this.UpdateSkillsList();
        }

        protected virtual bool CheckSkillRequirements(SkillAsset skill)
        {
            bool levelPassed = CheckSkillLevel(skill);

            if (!levelPassed) return false;
            bool reliesOnSkillsPassed = CheckSkillReliesOnSkills(skill);

            if (!reliesOnSkillsPassed) return false;
            bool otherConditionsPassed = CheckSkillOtherConditions(skill);

            if (!otherConditionsPassed) return false;
            bool skillPointsPassed = CheckSkillSkillPoints(skill);

            return skillPointsPassed;
        }

        protected virtual bool CheckSkillLevel(SkillAsset skill)
        {
            bool checkPassed = false;
            if (skill.requiresLevel)
            {
                if (SKILLSSETTINGS.GetLevelSelection() == DatabaseSkills.SkillSettings.StatSelection.GlobalVariable)
                {
                    if (SKILLSSETTINGS.GetLevelVariable().variableType != VariableProperty.GetVarType.GlobalVariable) Debug.LogError(MSG_ERROR_LEVELNOTASSIGNED);
                    if (SKILLSSETTINGS.GetLevelVariable().Get() == null)
                    {
                        Debug.LogError(MSG_ERROR_LEVELNOTASSIGNED);
                        return false;
                    }

                    if (Convert.ToSingle(SKILLSSETTINGS.GetLevelVariable().global.Get()) >= skill.level) checkPassed = true;
                }
                else
                {
                    if (SKILLSSETTINGS.GetLevelStat() == null) Debug.LogError(MSG_ERROR_LEVELNOTASSIGNED);
                    if (this.gameObject.GetComponent<Stats>() == null) Debug.LogError(MSG_ERROR_STATSCOMPONENTNOTASSIGNED);
                    if (this.gameObject.GetComponent<Stats>().GetStat(SKILLSSETTINGS.GetLevelStat().stat.uniqueName) >= skill.level) checkPassed = true;
                }
            }
            else checkPassed = true;

            return checkPassed;
        }

        protected virtual bool CheckSkillReliesOnSkills(SkillAsset skill)
        {
            bool checkPassed;

            if (null == skill.reliesOnSkills) return true;
            if (skill.reliesOnSkills.Length == 0)
            {
                checkPassed = true;
                return checkPassed;
            }

            bool[] skillsUnlocked = new bool[skill.reliesOnSkills.Length];

            for (int i = 0; i < skill.reliesOnSkills.Length; i++)
            {
                skillsUnlocked[i] = skill.reliesOnSkills[i].skill.skillState == Skill.SkillState.Unlocked;
            }

            checkPassed = !skillsUnlocked.Contains(false);

            return checkPassed;
        }

        protected virtual bool CheckSkillOtherConditions(SkillAsset skill)
        {
            bool checkPassed;

            if (skill.conditionsRequirements.conditions.Length == 0) checkPassed = true;
            else
            {
                if (skill.conditionsRequirements.Check(this.gameObject)) checkPassed = true;
                else checkPassed = false;
            }

            return checkPassed;
        }

        protected virtual bool CheckSkillSkillPoints(SkillAsset skill)
        {
            bool checkPassed = false;

            if (skill.useSkillPoints)
            {
                if (SKILLSSETTINGS.GetSkillPointsSelection() == DatabaseSkills.SkillSettings.StatSelection.GlobalVariable)
                {
                    if (SKILLSSETTINGS.GetSkillPointsVariable().variableType != VariableProperty.GetVarType.GlobalVariable) Debug.LogError(MSG_ERROR_SKILLPOINTSNOTASSIGNED);
                    if (SKILLSSETTINGS.GetSkillPointsVariable().Get() == null)
                    {
                        Debug.LogError(MSG_ERROR_SKILLPOINTSNOTASSIGNED);
                        return false;
                    }

                    if (Convert.ToSingle(SKILLSSETTINGS.GetSkillPointsVariable().global.Get()) >= skill.skillPointsNeeded)
                    {
                        float value = Convert.ToSingle(SKILLSSETTINGS.GetSkillPointsVariable().global.Get());
                        value -= skill.skillPointsNeeded;
                        SKILLSSETTINGS.GetSkillPointsVariable().global.Set(value);

                        checkPassed = true;
                    }
                }
                else
                {
                    if (SKILLSSETTINGS.GetSkillPointsStat() == null) Debug.LogError(MSG_ERROR_SKILLPOINTSNOTASSIGNED);
                    if (this.gameObject.GetComponent<Stats>().GetStat(SKILLSSETTINGS.GetSkillPointsStat().stat.uniqueName) >= skill.skillPointsNeeded)
                    {
                        float value = this.gameObject.GetComponent<Stats>().GetStat(SKILLSSETTINGS.GetSkillPointsStat().stat.uniqueName);
                        value -= skill.skillPointsNeeded;
                        this.gameObject.GetComponent<Stats>().SetStatBase(SKILLSSETTINGS.GetSkillPointsStat().stat.uniqueName, value);

                        checkPassed = true;
                    }                        
                }
            }
            else checkPassed = true;

            return checkPassed;
        }

        protected virtual bool CheckSkillStoredAmount(SkillAsset skill) => skill.currentAmount > 0;

        public virtual IEnumerator ExecuteSkill(SkillHolder skillHolder)
        {
            SkillAsset skill = this.GetSkill(skillHolder.skill.uuid);
            if (skill.skillState == Skill.SkillState.Locked) yield break;
            if (skill.isExecuting) yield break;
            if (skill.isInCooldown) yield break;

            if (this.IsActivatingSkill || this.IsCastingSkill || this.IsExecutingSkill || this.IsFinishingSkill) yield break;

            if (skill.skillExecutionType == Skill.SkillExecutionType.Stored && !this.CheckSkillStoredAmount(skill)) yield break;

            if (!skill.conditionsExecutionRequirements.Check(this.gameObject))
            {
                skill.actionsExecutionFailed.Execute(gameObject, null);
                yield break;
            }

            skill.isExecuting = true;
            skill.actionsOnResetExecuted = false;

            switch (skill.skillExecutionType)
            {
                case Skill.SkillExecutionType.InstantExecute:
                    this.ExecuteStartActivationPhase(skill);
                    this.ExecuteEndActivationPhase(skill);

                    this.ExecuteStartExecutionPhase(skill);
                    yield return new WaitForSeconds(skill.executionTime);
                    this.ExecuteEndExecutionPhase(skill);

                    this.ExecuteStartFinishPhase(skill);
                    this.ExecuteEndFinishPhase(skill);
                    break;

                case Skill.SkillExecutionType.CastExecute:
                    this.ExecuteStartActivationPhase(skill);
                    this.ExecuteEndActivationPhase(skill);

                    this.ExecuteStartCastPhase(skill);
                    yield return new WaitForSeconds(skill.castTime);
                    this.ExecuteEndCastPhase(skill);

                    this.ExecuteStartExecutionPhase(skill);
                    yield return new WaitForSeconds(skill.executionTime);
                    this.ExecuteEndExecutionPhase(skill);

                    this.ExecuteStartFinishPhase(skill);
                    this.ExecuteEndFinishPhase(skill);
                    break;

                case Skill.SkillExecutionType.PermanentExecute:
                    this.ExecuteStartExecutionPhase(skill);
                    break;

                case Skill.SkillExecutionType.Stored:
                    this.ExecuteStartActivationPhase(skill);
                    this.ExecuteEndActivationPhase(skill);

                    this.ExecuteStartCastPhase(skill);
                    yield return new WaitForSeconds(skill.castTime);
                    this.ExecuteEndCastPhase(skill);

                    this.ExecuteStartExecutionPhase(skill);
                    yield return new WaitForSeconds(skill.executionTime);
                    this.ExecuteEndExecutionPhase(skill);

                    this.ExecuteStartFinishPhase(skill);
                    this.ExecuteEndFinishPhase(skill);
                    break;
            }
            this.UpdateSkillsList();
            yield break;
        }

        public virtual IEnumerator ExecuteSkill(SkillAsset skill)
        {
            if (skill.skillState == Skill.SkillState.Locked) yield break;
            if (skill.isExecuting) yield break;
            if (skill.isInCooldown) yield break;

            if (this.IsActivatingSkill || this.IsCastingSkill || this.IsExecutingSkill || this.IsFinishingSkill) yield break;

            if (skill.skillExecutionType == Skill.SkillExecutionType.Stored && !this.CheckSkillStoredAmount(skill)) yield break;

            if (!skill.conditionsExecutionRequirements.Check(this.gameObject))
            {
                skill.actionsExecutionFailed.Execute(gameObject, null);
                yield break;
            }

            skill.isExecuting = true;
            skill.actionsOnResetExecuted = false;

            switch (skill.skillExecutionType)
            {
                case Skill.SkillExecutionType.InstantExecute:
                    this.ExecuteStartActivationPhase(skill);
                    this.ExecuteEndActivationPhase(skill);

                    this.ExecuteStartExecutionPhase(skill);
                    yield return new WaitForSeconds(skill.executionTime);
                    this.ExecuteEndExecutionPhase(skill);

                    this.ExecuteStartFinishPhase(skill);
                    this.ExecuteEndFinishPhase(skill);
                    break;

                case Skill.SkillExecutionType.CastExecute:
                    this.ExecuteStartActivationPhase(skill);
                    this.ExecuteEndActivationPhase(skill);

                    this.ExecuteStartCastPhase(skill);
                    yield return new WaitForSeconds(skill.castTime);
                    this.ExecuteEndCastPhase(skill);

                    this.ExecuteStartExecutionPhase(skill);
                    yield return new WaitForSeconds(skill.executionTime);
                    this.ExecuteEndExecutionPhase(skill);

                    this.ExecuteStartFinishPhase(skill);
                    this.ExecuteEndFinishPhase(skill);
                    break;

                case Skill.SkillExecutionType.PermanentExecute:
                    this.ExecuteStartExecutionPhase(skill);
                    break;

                case Skill.SkillExecutionType.Stored:
                    this.ExecuteStartActivationPhase(skill);
                    this.ExecuteEndActivationPhase(skill);

                    this.ExecuteStartCastPhase(skill);
                    yield return new WaitForSeconds(skill.castTime);
                    this.ExecuteEndCastPhase(skill);

                    this.ExecuteStartExecutionPhase(skill);
                    yield return new WaitForSeconds(skill.executionTime);
                    this.ExecuteEndExecutionPhase(skill);

                    this.ExecuteStartFinishPhase(skill);
                    this.ExecuteEndFinishPhase(skill);
                    break;
            }
            this.UpdateSkillsList();
            yield break;
        }

        public virtual IEnumerator ExecuteSkillPermanent(int uuid)
        {
            SkillAsset skill = this.GetSkill(uuid);
            if (skill.skillState == Skill.SkillState.Locked) yield break;
            if (skill.isExecuting) yield break;

            if (skill.isInCooldown) yield break;

            if (!skill.conditionsExecutionRequirements.Check(this.gameObject))
            {
                skill.actionsExecutionFailed.Execute(gameObject, null);
                yield break;
            }

            skill.isExecuting = true;
            skill.actionsOnResetExecuted = false;

            switch (skill.skillExecutionType)
            {
                case Skill.SkillExecutionType.InstantExecute:
                    this.ExecuteStartActivationPhase(skill);
                    this.ExecuteEndActivationPhase(skill);

                    this.ExecuteStartExecutionPhase(skill);
                    yield return new WaitForSeconds(skill.executionTime);
                    this.ExecuteEndExecutionPhase(skill);

                    this.ExecuteStartFinishPhase(skill);
                    this.ExecuteEndFinishPhase(skill);
                    break;
                    
                case Skill.SkillExecutionType.CastExecute:
                    this.ExecuteStartActivationPhase(skill);
                    this.ExecuteEndActivationPhase(skill);

                    this.ExecuteStartCastPhase(skill);
                    yield return new WaitForSeconds(skill.castTime);
                    this.ExecuteEndCastPhase(skill);

                    this.ExecuteStartExecutionPhase(skill);
                    yield return new WaitForSeconds(skill.executionTime);
                    this.ExecuteEndExecutionPhase(skill);

                    this.ExecuteStartFinishPhase(skill);
                    this.ExecuteEndFinishPhase(skill);
                    break;

                case Skill.SkillExecutionType.PermanentExecute:
                    this.ExecuteStartExecutionPhase(skill);
                    break;
            }
            this.UpdateSkillsList();
            yield break;
        }

        protected virtual void ExecuteStartActivationPhase(SkillAsset skill)
        {
            this.eventOnActivateStart.Invoke(skill, true);
            this.IsActivatingSkill = true;
            this.UpdateSkillsList();

            if (skill.skillExecutionType == Skill.SkillExecutionType.Stored && this.storedSkillCooldown != null && skill.currentAmount > 0)
            {
                this.StopCoroutine(this.storedSkillCooldown);
                skill.cooldownTimeLeft = 0;
            }
            if (skill.skillExecutionType == Skill.SkillExecutionType.Stored)
            {
                this.UpdateSkillBar(skill);
                skill.currentAmount--;
            }

            if (skill.actionsOnActivate) skill.actionsOnActivate.Execute(gameObject, null);
        }

        protected virtual void ExecuteEndActivationPhase(SkillAsset skill)
        {
            this.IsActivatingSkill = false;
            this.eventOnActivateStart.Invoke(skill, false);
        }

        protected virtual void ExecuteStartCastPhase(SkillAsset skill)
        {
            this.eventOnCastStart.Invoke(skill, true);
            this.IsCastingSkill = true;
            this.UpdateSkillsList();

            if (skill.actionsOnCast) skill.actionsOnCast.Execute(gameObject, null);
        }

        protected virtual void ExecuteEndCastPhase(SkillAsset skill)
        {
            this.IsCastingSkill = false;
            this.eventOnCastEnd.Invoke(skill, false);
        }

        protected virtual void ExecuteStartExecutionPhase(SkillAsset skill)
        {
            this.eventOnExecuteStart.Invoke(skill, true);
            
            if (skill.skillExecutionType != Skill.SkillExecutionType.PermanentExecute) this.IsExecutingSkill = true;

            this.UpdateSkillsList();

            if (skill.actionsOnExecute) skill.actionsOnExecute.Execute(gameObject, null);

            this.StartCoroutine(this.StartExecution(skill));
        }

        protected void ExecuteEndExecutionPhase(SkillAsset skill)
        {
            this.IsExecutingSkill = false;
            this.eventOnExecuteEnd.Invoke(skill, false);
        }

        protected virtual void ExecuteStartFinishPhase(SkillAsset skill)
        {
            this.eventOnFinishingStart.Invoke(skill, true);
            this.IsFinishingSkill = true;

            if (skill.actionsOnFinish) skill.actionsOnFinish.Execute(gameObject, null);

            this.StartCoroutine(this.StartCooldown(skill));
        }

        protected virtual void ExecuteEndFinishPhase(SkillAsset skill)
        {
            skill.isExecuting = false;
            this.IsFinishingSkill = false;
            this.eventOnFinishingEnd.Invoke(skill, false);
            this.UpdateSkillsList();
        }

        protected virtual IEnumerator StartExecution(SkillAsset skill)
        {
            if (skill.executionTime <= 0) yield break;

            skill.executionTimeLeft = skill.executionTime;
            while (skill.executionTimeLeft > 0)
            {
                yield return new WaitForSeconds(.1f);
                skill.executionTimeLeft -= .1f;
            }

            yield return null;
        }

        protected virtual IEnumerator StartCooldown(SkillAsset skill)
        {
            if (skill.cooldownTime <= 0) yield break;

            skill.isInCooldown = true;
            if (skill.skillExecutionType == Skill.SkillExecutionType.Stored) skill.isInCooldownBetween = true;
            skill.cooldownTimeLeft = (skill.skillExecutionType == Skill.SkillExecutionType.Stored) ? skill.cooldownTimeBetween : skill.cooldownTime;
            
            while (skill.cooldownTimeLeft > 0)
            {
                yield return new WaitForSeconds(.1f);
                skill.cooldownTimeLeft -= .1f;
            }

            if (skill.skillExecutionType == Skill.SkillExecutionType.Stored) skill.isInCooldownBetween = false;
            skill.isInCooldown = false;

            if (skill.skillExecutionType == Skill.SkillExecutionType.Stored) this.storedSkillCooldown = this.StartCoroutine(this.StartCooldownStored(skill));

            yield return null;
        }

        protected virtual IEnumerator StartCooldownStored(SkillAsset skill)
        {
            skill.isInCooldownAfter = true;

            while (skill.currentAmount < skill.maxAmount)
            {
                skill.cooldownTimeLeft = skill.cooldownTime;

                while (skill.cooldownTimeLeft > 0)
                {
                    yield return new WaitForSeconds(.1f);
                    skill.cooldownTimeLeft -= .1f;
                }
                skill.currentAmount++;
            }

            skill.isInCooldownAfter = false;

            yield return null;
        }

        protected virtual void StopCooldown(SkillAsset skill)
        {
            skill.cooldownTimeLeft = 0;
            skill.isInCooldown = false;
        }

        protected virtual void CancelExecutingSkillBar(SkillAsset skill)
        {
            foreach (SkillBarElement skillBarElement in this.skillBarElements.skillBarElements)
            {
                if (skillBarElement.skill != null && skillBarElement.skill.uuid == skill.uuid) skillBarElement.CancelSkillBarSkillExecution();
            }
        }

        protected virtual void UpdateSkillBar(SkillAsset skill)
        {
            foreach (SkillBarElement skillBarElement in this.skillBarElements.skillBarElements)
            {
                if (skillBarElement.skill != null && skillBarElement.skill.uuid == skill.uuid) skillBarElement.StartCoroutine(skillBarElement.StartSkillExecutionGUI());
            }
        }

        public virtual void CancelExecutingSkill(SkillAsset skill, bool cancelAction = false, bool usedWithReset = false)
        {
            if (skill.isExecuting && skill.skillExecutionType != Skill.SkillExecutionType.PermanentExecute)
            {
                skill.actionsOnActivate.Stop();
                skill.actionsOnCast.Stop();
                skill.actionsOnExecute.Stop();
                skill.actionsOnFinish.Stop();

                skill.isExecuting = false;

                if (cancelAction)
                {
                    if (this.useSkillBar) this.CancelExecutingSkillBar(skill);
                    this.StartCoroutine(this.StartCooldown(skill));
                    if (this.useSkillBar) this.UpdateSkillBar(skill);
                    skill.actionsOnReset.Execute(gameObject, null);
                    skill.actionsOnResetExecuted = true;
                }
            }
            else if (skill.isExecuting && skill.skillExecutionType == Skill.SkillExecutionType.PermanentExecute && usedWithReset)
            {
                skill.actionsOnActivate.Stop();
                skill.actionsOnCast.Stop();
                skill.actionsOnExecute.Stop();
                skill.actionsOnFinish.Stop();

                skill.isExecuting = false;

                if (cancelAction)
                {
                    if (this.useSkillBar) this.CancelExecutingSkillBar(skill);
                    this.StartCoroutine(this.StartCooldown(skill));
                    if (this.useSkillBar) this.UpdateSkillBar(skill);
                    skill.actionsOnReset.Execute(gameObject, null);
                    skill.actionsOnResetExecuted = true;
                }
            }
        }

        public virtual void CancelExecutingSkill(SkillHolder skill, bool cancelAction = false, bool usedWithReset = false)
        {
            SkillAsset skillToCancel = this.GetSkill(skill.skill.uuid);

            if (skillToCancel.isExecuting && skillToCancel.skillExecutionType != Skill.SkillExecutionType.PermanentExecute)
            {
                skillToCancel.actionsOnActivate.Stop();
                skillToCancel.actionsOnCast.Stop();
                skillToCancel.actionsOnExecute.Stop();
                skillToCancel.actionsOnFinish.Stop();

                skillToCancel.isExecuting = false;

                if (cancelAction)
                {
                    if (this.useSkillBar) this.CancelExecutingSkillBar(skillToCancel);
                    this.StartCoroutine(this.StartCooldown(skillToCancel));
                    if (this.useSkillBar) this.UpdateSkillBar(skillToCancel);
                    skillToCancel.actionsOnReset.Execute(gameObject, null);
                    skillToCancel.actionsOnResetExecuted = true;
                }
            }
            else if (skillToCancel.isExecuting && skillToCancel.skillExecutionType == Skill.SkillExecutionType.PermanentExecute && usedWithReset)
            {
                skillToCancel.actionsOnActivate.Stop();
                skillToCancel.actionsOnCast.Stop();
                skillToCancel.actionsOnExecute.Stop();
                skillToCancel.actionsOnFinish.Stop();

                skillToCancel.isExecuting = false;

                if (cancelAction)
                {
                    if (this.useSkillBar) this.CancelExecutingSkillBar(skillToCancel);
                    this.StartCoroutine(this.StartCooldown(skillToCancel));
                    if (this.useSkillBar) this.UpdateSkillBar(skillToCancel);
                    skillToCancel.actionsOnReset.Execute(gameObject, null);
                    skillToCancel.actionsOnResetExecuted = true;
                }
            }
        }

        public virtual void CancelExecutingSkills(bool cancelAction = false, bool usedWithReset = false)
        {
            foreach (KeyValuePair<int, SkillAsset> skill in this.SKILLS)
            {
                this.CancelExecutingSkill(skill.Value, cancelAction, usedWithReset);
            }
        }

        public virtual void ResetAllSkills()
        {
            foreach (KeyValuePair<int, SkillAsset> skill in this.SKILLS)
            {
                if (skill.Value.skillState == Skill.SkillState.Unlocked)
                {
                    this.ResetSkill(skill.Value);
                }
            }
        }

        public virtual void ResetSkill(SkillAsset skill)
        {
            if (skill.skillState == Skill.SkillState.Unlocked)
            {
                List<Skill> database = DatabaseSkills.Load().GetSkillList();
                for (int i = 0; i < database.Count; i++)
                {
                    if (database[i].uuid == skill.uuid && database[i].skillState != Skill.SkillState.Unlocked)
                    {
                        switch (SKILLSSETTINGS.GetSkillPointsSelection())
                        {
                            case DatabaseSkills.SkillSettings.StatSelection.GlobalVariable:
                                SKILLSSETTINGS.GetSkillPointsVariable().Set(Convert.ToSingle(SKILLSSETTINGS.GetSkillPointsVariable().Get()) + skill.skillPointsNeeded);
                                break;

                            case DatabaseSkills.SkillSettings.StatSelection.Stat:
                                this.GetComponent<Stats>().AddStatBase(SKILLSSETTINGS.GetSkillPointsStat().stat.uniqueName, skill.skillPointsNeeded);
                                break;
                        }

                        skill.skillState = Skill.SkillState.Locked;
                    }

                    if (skill.isInCooldown) this.StopCooldown(skill);
                    if (!skill.actionsOnResetExecuted)
                    {
                        skill.actionsOnReset.Execute(gameObject, null);
                        skill.actionsOnResetExecuted = true;
                    }
                }
            }
        }

        public virtual void ResetSkill(SkillHolder skillHolder)
        {
            SkillAsset skill = this.GetSkill(skillHolder.skill.uuid);

            if (skill.skillState == Skill.SkillState.Unlocked)
            {
                List<Skill> database = DatabaseSkills.Load().GetSkillList();
                for (int i = 0; i < database.Count; i++)
                {
                    if (database[i].uuid == skill.uuid && database[i].skillState != Skill.SkillState.Unlocked)
                    {

                        switch (SKILLSSETTINGS.GetSkillPointsSelection())
                        {
                            case DatabaseSkills.SkillSettings.StatSelection.GlobalVariable:
                                SKILLSSETTINGS.GetSkillPointsVariable().Set(Convert.ToSingle(SKILLSSETTINGS.GetSkillPointsVariable().Get()) + skill.skillPointsNeeded);
                                break;

                            case DatabaseSkills.SkillSettings.StatSelection.Stat:
                                this.GetComponent<Stats>().AddStatBase(SKILLSSETTINGS.GetSkillPointsStat().stat.uniqueName, skill.skillPointsNeeded);
                                break;
                        }

                        skill.skillState = Skill.SkillState.Locked;
                    }

                    if (skill.isInCooldown) this.StopCooldown(skill);
                    if (!skill.actionsOnResetExecuted)
                    {
                        skill.actionsOnReset.Execute(gameObject, null);
                        skill.actionsOnResetExecuted = true;
                    }
                }
            }
        }

        public virtual void SyncSkillBarSkills(SkillBarSkillTreeElement skillBarSkillTreeElement, SkillAsset skill)
        {
            for (int i = 0; i < this.skillBarElements.skillBarElements.Length; i++)
            {
                if (this.skillBarElements.skillBarElements[i].keyCode == skillBarSkillTreeElement.keyCode)
                {
                    this.skillBarElements.skillBarElements[i].SyncSkill(skill);
                }
            }
        }

        public virtual void ChangeSkillType(SkillHolder skillHolder, int newSkillType)
        {
            SkillAsset skill = this.GetSkill(skillHolder.skill.uuid);
            skill.skillType = newSkillType;
        }

        public virtual void ChangeSkillExecutionSettings(SkillHolder skillHolder, float castTime, float executionTime, float cooldownTime, float cooldownTimeBetween, float maxAmount, float startAmount)
        {
            SkillAsset skill = this.GetSkill(skillHolder.skill.uuid);
            skill.castTime = castTime;
            skill.executionTime = executionTime;
            skill.cooldownTime = cooldownTime;
            skill.cooldownTimeBetween = cooldownTimeBetween;
            skill.maxAmount = maxAmount;
            skill.startAmount = startAmount;
        }

        public virtual void ResetPhaseProperties()
        {
            this.IsActivatingSkill = false;
            this.IsCastingSkill = false;
            this.IsExecutingSkill = false;
            this.IsFinishingSkill = false;
        }

        public virtual void FireResetSkillsEvent() => this.eventSkillsReset.Invoke();

        // PUBLIC CALLBACK METHODS: ---------------------------------------------------------------

        public virtual void AddOnChangeSkill(Action<EventArgs> callback) => this.onChangeSkill += (callback);

        public virtual void RemoveOnChangeSkill(Action<EventArgs> callback) => this.onChangeSkill -= callback;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        public virtual void UpdateSkillsList()
        {
            List<Skill> originSkills = DatabaseSkills.Load().GetSkillList();

            this.skillsList.Clear();
            for (int i = 0; i < originSkills.Count; ++i)
            {
                int skillID = originSkills[i].uuid;
                skillsList.Add(this.SKILLS[skillID]);
            }
        }

        protected virtual void RequireInit(bool forceInit = false)
        {
            if (!forceInit && this.SKILLS != null) return;

            List<Skill> originSkills = DatabaseSkills.Load().GetSkillList();

            if (SKILLS == null)
            {
                SKILLS = new Dictionary<int, SkillAsset>();
                for (int i = 0; i < originSkills.Count; ++i)
                {
                    SkillAsset skill = originSkills[i].CopySkill();
                    SKILLS.Add(
                        skill.uuid,
                        skill
                    );
                }
            }

            if (this.skillsList.Count == 0)
            {
                this.skillsList = new List<SkillAsset>();
                for (int i = 0; i < originSkills.Count; ++i)
                {
                    SkillAsset skill = originSkills[i].CopySkill();
                    skillsList.Add(skill);
                }
            }

            if (SKILLSSETTINGS == null)
            {
                SKILLSSETTINGS = new DatabaseSkills.SkillSettings();
                SKILLSSETTINGS = DatabaseSkills.Load().GetSkillSettings();
            }
        }

        // SAVE LOAD SYSTEM: ---------------------------------------------------

        public string GetUniqueName() => string.Format("skill:{0}", this.GetUniqueID());

        protected virtual string GetUniqueID() => this.GetID();

        public Type GetSaveDataType() => typeof(SerialData);

        public object GetSaveData()
        {
            SerialData skills = new SerialData();
            if (this.SKILLS == null) return skills;

            skills.skills = new SerialSkill[this.SKILLS.Count];

            int index = 0;
            foreach (KeyValuePair<int, SkillAsset> skill in this.SKILLS)
            {
                bool unlocked = skill.Value.skillState != Skill.SkillState.Locked;
                int skillType = skill.Value.skillType;
                float castTime = skill.Value.castTime;
                float executionTime = skill.Value.executionTime;
                float cooldownTime = skill.Value.cooldownTime;
                float cooldownTimeBetween = skill.Value.cooldownTimeBetween;
                float maxAmount = skill.Value.maxAmount;
                float startAmount = skill.Value.startAmount;
                bool addedToSkillBar = skill.Value.addedToSkillBar;
                int addedToSkillBarIndex = skill.Value.addedToSkillBarIndex;
                skills.skills[index] = new SerialSkill(skill.Key, unlocked, skillType, castTime, executionTime, cooldownTime, cooldownTimeBetween, maxAmount, startAmount, addedToSkillBar, addedToSkillBarIndex);
                index++;
            }

            return skills;
        }

        public void ResetData() => this.RequireInit(true);

        public void OnLoad(object generic)
        {
            if (generic == null) return;
            SerialData skills = (SerialData)generic;

            this.RequireInit();

            if (this.useSkillBar)
            {
                if (this.skillTreeUIManager == null)
                {
                    SkillTreeSelection skillTreeSelection = DatabaseSkills.Load().GetSkillSettings().GetSkillTreeSelection();

                    if (skillTreeSelection == SkillTreeSelection.Automatic) SkillTreeUIManagerAutomatical.OpenSkillTree(this);
                    else SkillTreeUIManagerManual.OpenSkillTree(this);

                    if (skillTreeSelection == SkillTreeSelection.Automatic) SkillTreeUIManagerAutomatical.CloseSkillTree(this);
                    else SkillTreeUIManagerManual.CloseSkillTree(this);
                }

                this.skillTreeUIManager.skillBarElements.Start();

                for (int i = 0; i < this.skillTreeUIManager.skillBarElements.skillBarElements.Length; i++)
                {
                    this.skillTreeUIManager.skillBarElements.skillBarElements[i].UnbindSkill();
                    this.SyncSkillBarSkills(
                        this.skillTreeUIManager.skillBarElements.skillBarElements[i],
                        null
                        );
                }
            }

            for (int i = 0; i < skills.skills.Length; ++i)
            {
                int key = skills.skills[i].skillUniqueID;
                bool unlocked = skills.skills[i].skillUnlocked;
                int skillType = skills.skills[i].skillType;
                float castTime = skills.skills[i].castTime;
                float executionTime = skills.skills[i].executionTime;
                float cooldownTime = skills.skills[i].cooldownTime;
                float cooldownTimeBetween = skills.skills[i].cooldownTimeBetween;
                float maxAmount = skills.skills[i].maxAmount;
                float startAmount = skills.skills[i].startAmount;
                bool addedToSkillBar = skills.skills[i].addedToSkillBar;
                int addedToSkillBarIndex = skills.skills[i].addedToSkillBarIndex;
                this.SKILLS[key].skillState = unlocked ? Skill.SkillState.Unlocked : Skill.SkillState.Locked;
                this.SKILLS[key].addedToSkillBar = addedToSkillBar;
                this.SKILLS[key].addedToSkillBarIndex = addedToSkillBarIndex;

                if (this.useSkillBar && addedToSkillBar)
                {
                    this.skillTreeUIManager.skillBarElements.skillBarElements[addedToSkillBarIndex].BindSkill(this.SKILLS[key]);
                    this.SyncSkillBarSkills(
                        this.skillTreeUIManager.skillBarElements.skillBarElements[addedToSkillBarIndex],
                        this.SKILLS[key]
                        );
                }
                else if (this.useSkillBar && !addedToSkillBar && addedToSkillBarIndex != -1)
                {
                    this.skillTreeUIManager.skillBarElements.skillBarElements[addedToSkillBarIndex].UnbindSkill();
                    this.SyncSkillBarSkills(
                        this.skillTreeUIManager.skillBarElements.skillBarElements[addedToSkillBarIndex],
                        null
                        );
                }

                this.UpdateSkillsList();

                if (this.onChangeSkill != null) this.onChangeSkill.Invoke(new EventArgs(key, EventArgs.Operation.Unlock));
            }
        }
    }
}