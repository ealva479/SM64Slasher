namespace GameCreator.Dialogue
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;

    #if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    #endif

    [AddComponentMenu("Game Creator/Dialogue", 0)]
    public class Dialogue : GlobalID, IGameSave
    {
        [Serializable]
        public class Revisits : SerializableDictionaryBase<string, bool> 
        { }

        [Serializable]
        public class Information
        {
            public Revisits revisits = new Revisits();
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public DialogueItemRoot dialogue;
        public IDialogueItem[] itemInstances = new IDialogueItem[0];

        public Information information = new Information();

        public bool overrideConfig = false;
        public DatabaseDialogue.ConfigData config = new DatabaseDialogue.ConfigData();

        #if UNITY_EDITOR
        public TreeViewState dialogueTreeState = new TreeViewState();
        #endif

        private bool stopDialogue;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void RunDialogue()
        {
            StartCoroutine(this.Run());
        }
        
        public IEnumerator Run()
        {
            this.stopDialogue = false;

            Stack<IDialogueItem> stackItems = new Stack<IDialogueItem>();
            stackItems.Push(this.dialogue);

            DialogueUI.BeginDialogue();

            while (!this.stopDialogue && stackItems.Count > 0)
            {
                IDialogueItem item = stackItems.Pop();
                if (!item.CheckConditions()) continue;

                yield return item.Run();

                if (this.stopDialogue || item.afterRun == IDialogueItem.AfterRunBehaviour.Exit)
                {
                    stackItems.Clear();
                    DialogueUI.EndDialogue();

                    yield break;
                }
                else if (item.afterRun == IDialogueItem.AfterRunBehaviour.Jump && item.jumpTo != null)
                {
                    stackItems.Clear();

                    int jumpToID = item.jumpTo.GetInstanceID();
                    List<IDialogueItem> parentChildren = item.jumpTo.parent.children;
                    int parentChildrenCount = parentChildren.Count;

                    for (int i = parentChildrenCount - 1; i >= 0; --i)
                    {
                        if (parentChildren[i] == null) continue;
                        stackItems.Push(parentChildren[i]);
                        if (parentChildren[i].GetInstanceID() == jumpToID) break;
                    }
                }
                else
                {
                    IDialogueItem[] childItems = item.GetNextItem();
                    if (childItems != null)
                    {
                        int numChildItems = childItems.Length;
                        for (int i = numChildItems - 1; i >= 0; --i)
                        {
                            if (childItems[i] == null) continue;
                            stackItems.Push(childItems[i]);
                        }
                    }
                }
            }

            DialogueUI.EndDialogue();
        }

        public void Stop()
        {
            this.stopDialogue = true;
        }

        public bool IsStoppingDialogue()
        {
            return this.stopDialogue;
        }

        // GAME LOAD/SAVE: ------------------------------------------------------------------------

        public object GetSaveData()
        {
            return this.information;
        }

        public Type GetSaveDataType()
        {
            return typeof(Information);
        }

        public string GetUniqueName()
        {
            return string.Format("dialogue:{0}", this.GetID());
        }

        public void OnLoad(object generic)
        {
            this.information = (generic as Information);
        }

        public void ResetData()
        {
            this.information = new Information();
        }
    }
}