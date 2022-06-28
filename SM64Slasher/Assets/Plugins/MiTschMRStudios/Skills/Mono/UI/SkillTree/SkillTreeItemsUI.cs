namespace MiTschMR.Skills
{
    using UnityEngine;

    public class SkillTreeItemsUI : MonoBehaviour
    {
        [HideInInspector] [SerializeField] protected SkillUI[] skills;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        protected virtual void Start() => this.skills = this.GetComponentsInChildren<SkillUI>();

        public virtual void UpdateSkills(Skills characterSkills)
        {
            this.Start();
            foreach (SkillUIManual skill in skills)
            {
                skill.Setup(characterSkills, characterSkills.GetSkill(skill.skill.skill.uuid), characterSkills.GetSkillState(skill.skill.skill.uuid));
            }
        }
    }
}