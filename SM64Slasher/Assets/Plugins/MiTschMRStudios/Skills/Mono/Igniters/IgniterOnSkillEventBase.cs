namespace MiTschMR.Skills
{
    using GameCreator.Core;
    using UnityEngine;

    [AddComponentMenu("")]
    public abstract class IgniterOnSkillEventBase : Igniter
    {
        public TargetGameObject skillExecuter = new TargetGameObject(TargetGameObject.Target.Player);
        public SkillHolder skill;

        protected Skills GetSkillExecuter() => this.skillExecuter.GetComponent<Skills>(gameObject);
    }
}