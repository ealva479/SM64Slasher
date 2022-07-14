namespace MiTschMR.Skills
{
    using GameCreator.Core;
    using UnityEngine;

    [AddComponentMenu("")]
    public class IgniterSkillStateChange : Igniter
    {
        public enum Detection
        {
            OnUnlock,
            OnLock
        }

        #if UNITY_EDITOR
        public new static string NAME = "Skills/On Skill State Change";
        #endif

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);

        public SkillHolder skill;
        public Detection detect = Detection.OnUnlock;

        private Skills component;
        private bool unlocked;

        private void Start()
        {
            GameObject targetGO = this.target.GetGameObject(gameObject);
            if (!targetGO)
            {
                Debug.LogError("Trigger Skill State Change: No target defined");
                return;
            }

            this.component = targetGO.GetComponentInChildren<Skills>();
            if (!this.component)
            {
                Debug.LogError("Trigger Skill State Change: Could not get Skills component in target");
                return;
            }

            this.unlocked = this.component.GetSkillLockedState(this.skill.skill.uuid, Skill.SkillState.Unlocked);
            this.component.AddOnChangeSkill(this.OnChangeSkill);
        }

        private void OnDestroy()
        {
            if (this.isExitingApplication || !this.component) return;
            this.component.RemoveOnChangeSkill(this.OnChangeSkill);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnChangeSkill(Skills.EventArgs args)
        {
            bool current = false;
            switch (args.operation)
            {
                case Skills.EventArgs.Operation.Unlock:
                    current = this.component.GetSkillLockedState(this.skill.skill.uuid, Skill.SkillState.Unlocked);
                    break;

                case Skills.EventArgs.Operation.Lock:
                    current = this.component.GetSkillLockedState(this.skill.skill.uuid, Skill.SkillState.Locked);
                    break;
            }
            
            switch (this.detect)
            {
                case Detection.OnUnlock:
                    if (!(current == this.unlocked)) this.ExecuteTrigger(component.gameObject); break;

                case Detection.OnLock:
                    if (current == this.unlocked) this.ExecuteTrigger(component.gameObject); break;
            }

            this.unlocked = current;
        }
    }
}
