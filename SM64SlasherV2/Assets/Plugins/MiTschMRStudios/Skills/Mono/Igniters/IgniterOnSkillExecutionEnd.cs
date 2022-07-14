namespace MiTschMR.Skills
{
    using UnityEngine;

    [AddComponentMenu("")]
    public class IgniterOnSkillExecutionEnd : IgniterOnSkillEventBase
    {
        #if UNITY_EDITOR
        public new static string NAME = "Skills/On Skill Execution End";
        #endif

        private void Start()
        {
            Skills charSkillExecuter = this.GetSkillExecuter();
            if (charSkillExecuter != null) charSkillExecuter.eventOnExecuteEnd.AddListener(this.Callback);
        }

        private void OnDestroy()
        {
            Skills charSkillExecuter = this.GetSkillExecuter();
            if (charSkillExecuter != null) charSkillExecuter.eventOnExecuteEnd.RemoveListener(this.Callback);
        }

        private void Callback(SkillAsset skill, bool active)
        {
            if (!(skill.uuid == this.skill.skill.uuid && !active)) return;
            Skills charSkillExecuter = this.GetSkillExecuter();
            if (charSkillExecuter != null) this.ExecuteTrigger(charSkillExecuter.gameObject);
        }
    }
}