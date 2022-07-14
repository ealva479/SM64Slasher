namespace GameCreator.Quests
{
    using System;
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;

    public static class QuestUtilities
    {
        private const string ROOT_PATH = "Assets/Plugins/GameCreatorData/Quests/";
        private const string ROOT_NAME = "Quests.asset";

		private const string REACTIONS_PATH = "Assets/Plugins/GameCreatorData/Quests/Reactions/";
		private const string REACTIONS_NAME = "{0}.prefab";

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static Quests GetQuestsRoot()
        {
			string path = QuestUtilities.GetQuestsRootPath();
            Quests quests = AssetDatabase.LoadAssetAtPath<Quests>(path);

            if (quests == null)
            {
                quests = CreateAsset<Quests>(
                    ROOT_PATH,
                    ROOT_NAME
                );
            }

            return quests;
        }

		public static string GetQuestsRootPath()
		{
			return Path.Combine(ROOT_PATH, ROOT_NAME);
		}

		public static T CreateIQuest<T>() where T : IQuest
        {
            T iquest = ScriptableObject.CreateInstance<T>();

			string uniqueID = Guid.NewGuid().ToString("N");
			iquest.name = uniqueID;
			iquest.uniqueID = uniqueID;
            iquest.internalName = QuestUtilities.GetInternalName();

			iquest.reactions = CreateReaction(
				string.Format(REACTIONS_NAME, iquest.name), 
				REACTIONS_PATH
			);

            Quests root = QuestUtilities.GetQuestsRoot();

            AssetDatabase.AddObjectToAsset(iquest, root);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(Path.Combine(ROOT_PATH, ROOT_NAME));
            return iquest;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static T CreateAsset<T>(string filepath, string filename) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            GameCreatorUtilities.CreateFolderStructure(filepath);
            string path = Path.Combine(filepath, filename);
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.ImportAsset(path);
            return asset;
        }

		private static QuestReaction CreateReaction(string filename, string pathname)
        {
            GameObject sceneInstance = new GameObject("Reaction");
			sceneInstance.AddComponent<QuestReaction>();

            GameCreatorUtilities.CreateFolderStructure(pathname);
            string path = Path.Combine(pathname, filename);         
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            
            GameObject prefabInstance = PrefabUtility.SaveAsPrefabAsset(sceneInstance, path);
            UnityEngine.Object.DestroyImmediate(sceneInstance);

			return prefabInstance.GetComponent<QuestReaction>();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // INTERNAL NAME GENERATOR: ---------------------------------------------------------------

        private const string TASK_NAME = "{0}-{1}-{2}";
        private static readonly string[] TASK_VERB = new string[]
        {
            "destroy",
            "find",
            "craft",
            "pick",
            "discover",
            "eat",
            "punch",
            "wink",
            "curse",
            "steal"
        };

        private const int TASK_AMOUNT_MIN = 2;
        private const int TASK_AMOUNT_MAX = 12;

        private static readonly string[] TASK_TARGET = new string[]
        {
            "princesses",
            "apples",
            "swords",
            "green-teas",
            "wyvern-eyes",
            "recycle-bins",
            "houses",
            "inns",
            "fangs",
            "potions"
        };

        private static string GetInternalName()
        {
            int verb = UnityEngine.Random.Range(0, TASK_VERB.Length);
            int amount = UnityEngine.Random.Range(TASK_AMOUNT_MIN, TASK_AMOUNT_MAX);
            int target = UnityEngine.Random.Range(0, TASK_TARGET.Length);
            return string.Format(TASK_NAME, TASK_VERB[verb], amount, TASK_TARGET[target]);
        }
    }
}