namespace MiTschMR.Skills
{
    using UnityEngine;

    public class SkillBarSkillTreeElements : MonoBehaviour
    {
        [HideInInspector] public SkillBarSkillTreeElement[] skillBarElements;

        public virtual void Start()
        {
            this.skillBarElements = this.GetComponentsInChildren<SkillBarSkillTreeElement>();

            for (int i = 0; i < this.skillBarElements.Length; i++)
            {
                this.skillBarElements[i].skillBarSkillTreeElementIndex = i;
            }
        }
    }
}