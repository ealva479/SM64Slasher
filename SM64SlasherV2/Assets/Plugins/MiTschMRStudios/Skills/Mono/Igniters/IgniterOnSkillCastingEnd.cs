namespace MiTschMR.Skills
{
    using UnityEngine;

    [AddComponentMenu("")]
    public class IgniterOnSkillCastingEnd : IgniterOnSkillEventBase
    {
        #if UNITY_EDITOR
        public new static string NAME = "Skills/On Skill Casting End";
        #endif

        private void Start()
        {
            Skills charSkillExecuter = this.GetSkillExecuter();
            if (charSkillExecuter != null) charSkillExecuter.eventOnCastEnd.AddListener(this.Callback);
        }

        private void OnDestroy()
        {
            Skills charSkillExecuter = this.GetSkillExecuter();
            if (charSkillExecuter != null) charSkillExecuter.eventOnCastEnd.RemoveListener(this.Callback);
        }

        private void Callback(SkillAsset skill, bool active)
        {
            if (!(skill.uuid == this.skill.skill.uuid && !active)) return;
            Skills charSkillExecuter = this.GetSkillExecuter();
            if (charSkillExecuter != null) this.ExecuteTrigger(charSkillExecuter.gameObject);
        }
    }
}