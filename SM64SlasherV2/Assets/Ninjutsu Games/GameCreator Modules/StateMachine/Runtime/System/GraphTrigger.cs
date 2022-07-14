using System;
using GameCreator.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NJG.Graph
{
    public class GraphTrigger : Trigger
    {
        public Action<GameObject> onTriggerExecute; 

        public override void Execute()
        {
            // Debug.LogWarning("1 GRAPH TRIGGER EXEUTE! on: "+gameObject, gameObject);
            if (!Application.isPlaying) return;
            onTriggerExecute?.Invoke(gameObject);
            base.Execute();
        }

        public override void Execute(GameObject target, params object[] parameters)
        {
            // Debug.LogWarning("2 GRAPH TRIGGER EXEUTE! target: "+target+" go: "+gameObject, gameObject);
            if (!Application.isPlaying) return;
            onTriggerExecute?.Invoke(target);
            base.Execute(target, parameters);
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                //Debug.Log("GraphTrigger On Destroy " + this, this);
                if (igniters.Count > 0)
                {
                    foreach (var ig in igniters)
                    {
                        DestroyImmediate(ig.Value, true);
                    }
                }
            }

            onTriggerExecute = null;
        }

#if UNITY_EDITOR
        private SerializedObject serializedObject;
        private bool registered;
        
        private void Awake()
        {
            EventSystemManager.Instance.Wakeup();
            SetupPlatformIgniter();
        }
        
        private void SetupPlatformIgniter()
        {
            bool overridePlatform = false;

#if UNITY_STANDALONE
            if (!overridePlatform) overridePlatform = CheckPlatformIgniter(Platforms.Desktop);
#endif

#if UNITY_EDITOR
            if (!overridePlatform) overridePlatform = CheckPlatformIgniter(Platforms.Editor);
#endif

#if UNITY_ANDROID || UNITY_IOS
			if (!overridePlatform) overridePlatform = this.CheckPlatformIgniter(Platforms.Mobile);
#endif

#if UNITY_TVOS
			if (!overridePlatform) overridePlatform = this.CheckPlatformIgniter(Platforms.tvOS);
#endif

#if UNITY_PS4
			if (!overridePlatform) overridePlatform = this.CheckPlatformIgniter(Platforms.PS4);
#endif

#if UNITY_XBOXONE
			if (!overridePlatform) overridePlatform = this.CheckPlatformIgniter(Platforms.XBoxOne);
#endif

#if UNITY_WIIU
			if (!overridePlatform) overridePlatform = this.CheckPlatformIgniter(Platforms.WiiU);
#endif

#if UNITY_SWITCH
			if (!overridePlatform) overridePlatform = this.CheckPlatformIgniter(Platforms.Switch);
#endif

            if (!overridePlatform)
            {
                if (!igniters.ContainsKey(ALL_PLATFORMS_KEY))
                {
                    Igniter igniter = gameObject.AddComponent<IgniterStart>();
                    igniter.Setup(this);
                    igniters.Add(ALL_PLATFORMS_KEY, igniter);
                }

                igniters[ALL_PLATFORMS_KEY].enabled = true;
            }
        }
        
        private bool CheckPlatformIgniter(Platforms platform)
        {
            if (igniters.ContainsKey((int)platform)) 
            {
                igniters[(int)Platforms.Editor].enabled = true;
                return true;
            }

            return false;
        }

        private void OnEnable()
        {
            if (Application.isPlaying) return;

            if(serializedObject == null) serializedObject = new SerializedObject(this);
            SerializedProperty spIgniters = serializedObject.FindProperty("igniters");
            SerializedProperty spValues = spIgniters.FindPropertyRelative("values");

            bool updated = false;
            int spValuesSize = spValues.arraySize;
            for (int i = 0; i < spValuesSize; ++i)
            {
                SerializedProperty spIgniter = spValues.GetArrayElementAtIndex(i);
                Igniter igniter = spIgniter.objectReferenceValue as Igniter;
                if (igniter != null && igniter.gameObject != gameObject)
                {
                    Igniter newIgniter = gameObject.AddComponent(igniter.GetType()) as Igniter;
                    EditorUtility.CopySerialized(igniter, newIgniter);
                    spIgniter.objectReferenceValue = newIgniter;
                    updated = true;
                }
            }

            //if(Application.isPlaying) Debug.LogError("OnEnable " + this + " / " + updated, gameObject);

            if (updated)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(gameObject);
            }

            //Validate();
        }

        protected void OnValidate()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !registered)
            {
                hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                registered = true;
            }
        }
#endif
    }
}
