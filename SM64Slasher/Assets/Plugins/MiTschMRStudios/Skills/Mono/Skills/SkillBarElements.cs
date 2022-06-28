namespace MiTschMR.Skills
{
    using UnityEngine;

    public class SkillBarElements : MonoBehaviour
    {
        [HideInInspector] public SkillBarElement[] skillBarElements;

        protected virtual void Start() => this.skillBarElements = this.GetComponentsInChildren<SkillBarElement>();
    }
}