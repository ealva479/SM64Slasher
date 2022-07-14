namespace MiTschMR.Skills
{
    using GameCreator.Core;
    using UnityEngine;

    [AddComponentMenu("")]
    public class IgniterOnSkillsReset : Igniter
    {
        public TargetGameObject skillsResetter = new TargetGameObject(TargetGameObject.Target.Player);

        #if UNITY_EDITOR
        public new static string NAME = "Skills/On Skills Reset";
        #endif

        private void Start()
        {
            Skills charSkillExecuter = this.GetSkillExecuter();
            if (charSkillExecuter != null) charSkillExecuter.eventSkillsReset.AddListener(this.Callback);
        }

        private void OnDestroy()
        {
            Skills charSkillExecuter = this.GetSkillExecuter();
            if (charSkillExecuter != null) charSkillExecuter.eventSkillsReset.RemoveListener(this.Callback);
        }

        private Skills GetSkillExecuter() => this.skillsResetter.GetComponent<Skills>(gameObject);

        private void Callback()
        {
            Skills charSkillExecuter = this.GetSkillExecuter();
            if (charSkillExecuter != null) this.ExecuteTrigger(charSkillExecuter.gameObject);
        }
    }
}