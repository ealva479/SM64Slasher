namespace MiTschMR.Skills
{
    using UnityEngine;

    [AddComponentMenu("")]
    public class IgniterOnSkillExecutionStart : IgniterOnSkillEventBase
    {
        #if UNITY_EDITOR
        public new static string NAME = "Skills/On Skill Execution Start";
        #endif

        private void Start()
        {
            Skills charSkillExecuter = this.GetSkillExecuter();
            if (charSkillExecuter != null) charSkillExecuter.eventOnExecuteStart.AddListener(this.Callback);
        }

        private void OnDestroy()
        {
            Skills charSkillExecuter = this.GetSkillExecuter();
            if (charSkillExecuter != null)
            charSkillExecuter.eventOnExecuteStart.RemoveListener(this.Callback);
        }

        private void Callback(SkillAsset skill, bool active)
        {
            if (!(skill.uuid == this.skill.skill.uuid && active)) return;
            Skills charSkillExecuter = this.GetSkillExecuter();
            if (charSkillExecuter != null)
            this.ExecuteTrigger(charSkillExecuter.gameObject);
        }
    }
}