namespace MiTschMR.Skills
{
    using System.Collections.Generic;
    using UnityEngine;

    [AddComponentMenu("MiTschMR Studios/UI/Skill Tree Items List Automatical")]
    public class SkillTreeItemsUIAutomatical : SkillTreeItemsUI
    {
        [SerializeField] protected RectTransform skillsContainer;

        [SerializeField] protected GameObject prefabSkill;
        [SerializeField] protected GameObject prefabSkillConnectorEnabled;
        [SerializeField] protected GameObject prefabSkillConnectorDisabled;
        [SerializeField] protected float distanceBetweenSkillsInLine = 200f;
        [SerializeField] protected GameObject prefabSkillHolder;
        protected GameObject skillUIHolder;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void UpdateSkills(Skills characterSkills)
        {
            List<int> uuidRow = new List<int>();
            foreach (KeyValuePair<int, SkillAsset> entry in characterSkills.GetSkillsDictionary())
            {
                if (this.prefabSkill == null)
                {
                    Debug.LogError($"No skill prefab found in {gameObject.name}, you can find it in the prefab {transform.root.name}");
                }
                
                // Skills that don't rely on other skills
                if (entry.Value.reliesOnSkills == null)
                {
                    GameObject skillUIAsset = null;
                    SkillUIAutomatical skillUI;

                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform find = transform.Find($"Type{i}/SkillHolderPrefab(Clone)/{entry.Value.uuid}");
                        if (find != null) skillUIAsset = find.gameObject;
                    }

                    if (skillUIAsset != null)
                    {
                        skillUI = skillUIAsset.GetComponent<SkillUIAutomatical>();
                        skillUI.Setup(characterSkills, entry.Value, entry.Value.skillState);
                        uuidRow.Add(entry.Value.uuid);
                        continue;
                    }

                    this.skillUIHolder = Instantiate(this.prefabSkillHolder, Vector3.zero, this.skillsContainer.rotation, gameObject.transform.Find($"Type{entry.Value.skillType}").transform);
                    skillUIAsset = Instantiate(this.prefabSkill, Vector3.zero, this.skillsContainer.rotation, this.skillUIHolder.transform);
                    skillUIAsset.name = entry.Value.uuid.ToString();

                    if (entry.Value.skillState == Skill.SkillState.Unlocked)
                    {
                        SkillUIAutomatical skillUIEnabled = skillUIAsset.GetComponent<SkillUIAutomatical>();
                        skillUIEnabled.skillIconEnabled.GetComponent<Animator>().enabled = false;
                    }

                    skillUI = skillUIAsset.GetComponent<SkillUIAutomatical>();
                    skillUI.Setup(characterSkills, entry.Value, entry.Value.skillState);
                    uuidRow.Add(entry.Value.uuid);
                }
                else
                {
                    // Skills that rely on other skills
                    if (uuidRow.Contains(entry.Value.reliesOnSkills[0].skill.uuid))
                    {
                        GameObject skillUIAsset = null;
                        SkillUIAutomatical skillUI;
                        GameObject skillConnectorEnabled;
                        GameObject skillConnectorDisabled;
                        for (int i = 0; i <= transform.childCount; i++)
                        {
                            Transform find = transform.Find($"Type{i}/SkillHolderPrefab(Clone)/{entry.Value.uuid}");
                            if (find != null) skillUIAsset = find.gameObject;
                        }
                        
                        if (skillUIAsset != null)
                        {
                            skillUI = skillUIAsset.GetComponent<SkillUIAutomatical>();
                            skillUI.Setup(characterSkills, entry.Value, entry.Value.skillState);
                            uuidRow.Add(entry.Value.uuid);
                            continue;
                        }

                        skillUIAsset = Instantiate(this.prefabSkill, Vector3.zero, this.skillsContainer.rotation, this.skillUIHolder.transform);
                        skillUIAsset.GetComponent<RectTransform>().localPosition = new Vector3(this.distanceBetweenSkillsInLine * (this.skillUIHolder.transform.childCount - 1), 0, 0);
                        skillUIAsset.name = entry.Value.uuid.ToString();

                        if (entry.Value.skillState == Skill.SkillState.Unlocked)
                        {
                            SkillUIAutomatical skillUIEnabled = skillUIAsset.GetComponent<SkillUIAutomatical>();
                            skillUIEnabled.skillIconEnabled.GetComponent<Animator>().enabled = false;
                            skillConnectorEnabled = Instantiate(this.prefabSkillConnectorEnabled, Vector3.zero, this.skillsContainer.rotation, this.skillUIHolder.transform.Find(entry.Value.reliesOnSkills[0].skill.uuid.ToString()).transform);
                            skillConnectorEnabled.GetComponent<RectTransform>().localPosition = new Vector3(this.distanceBetweenSkillsInLine / 2, 0, 0);

                            skillConnectorDisabled = Instantiate(this.prefabSkillConnectorDisabled, Vector3.zero, this.skillsContainer.rotation, this.skillUIHolder.transform.Find(entry.Value.reliesOnSkills[0].skill.uuid.ToString()).transform);
                            skillConnectorDisabled.GetComponent<RectTransform>().localPosition = new Vector3(this.distanceBetweenSkillsInLine / 2, 0, 0);
                            skillConnectorDisabled.gameObject.SetActive(false);
                        }
                        else
                        {
                            skillConnectorDisabled = Instantiate(this.prefabSkillConnectorDisabled, Vector3.zero, this.skillsContainer.rotation, this.skillUIHolder.transform.Find(entry.Value.reliesOnSkills[0].skill.uuid.ToString()).transform);
                            skillConnectorDisabled.GetComponent<RectTransform>().localPosition = new Vector3(distanceBetweenSkillsInLine / 2, 0, 0);

                            skillConnectorEnabled = Instantiate(this.prefabSkillConnectorEnabled, Vector3.zero, this.skillsContainer.rotation, this.skillUIHolder.transform.Find(entry.Value.reliesOnSkills[0].skill.uuid.ToString()).transform);
                            skillConnectorEnabled.GetComponent<RectTransform>().localPosition = new Vector3(distanceBetweenSkillsInLine / 2, 0, 0);
                            skillConnectorEnabled.gameObject.SetActive(false);
                        }

                        skillUI = skillUIAsset.GetComponent<SkillUIAutomatical>();
                        skillUI.skillConnectorEnabled = skillConnectorEnabled;
                        skillUI.skillConnectorDisabled = skillConnectorDisabled;
                        skillUI.Setup(characterSkills, entry.Value, entry.Value.skillState);
                        uuidRow.Add(entry.Value.uuid);
                    }
                }
            }
        }
    }
}