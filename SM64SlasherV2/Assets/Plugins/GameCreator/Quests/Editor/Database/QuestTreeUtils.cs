namespace GameCreator.Quests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    
    public abstract class QuestTreeUtils
    {
        public enum Icon
        {
            OnComplete,
            OnFail,
            OnConditions,
			OffComplete,
            OffFail,
            OffConditions,
        }

        private static readonly string[] TOOLTIPS = new string[]
        {
            "Executes actions on complete",
			"Executes actions on fail",
			"Has some conditions that must be met",
            "No actions on complete",
			"No actions on fail",
			"No conditions"
        };

        private const string ICONS_PATH = "Assets/Plugins/GameCreator/Quests/Icons/Rows/{0}";
        private static readonly string[] ICONS = new string[]
        {
            "OnComplete.png",
            "OnFail.png",
            "OnConditions.png",
			"OffComplete.png",
            "OffFail.png",
            "OffConditions.png"
        };

        // PROPERTIES: ----------------------------------------------------------------------------

		private static Dictionary<int, GUIContent> DATA;      

        // PUBLIC STATIC METHODS: -----------------------------------------------------------------
        
		public static GUIContent GetIcon(Icon icon)
        {
            QuestTreeUtils.RequireDataSetup();
			return DATA[(int)icon];
        }

        // PRIVATE STATIC METHODS: ----------------------------------------------------------------

        private static void RequireDataSetup()
        {
            if (DATA != null) return;

			DATA = new Dictionary<int, GUIContent>();

			int contentLength = Enum.GetNames(typeof(Icon)).Length;
            for (int i = 0; i < contentLength; ++i)
            {
                string path = string.Format(ICONS_PATH, ICONS[i]);
                Texture2D iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

				GUIContent data = new GUIContent(
                    iconTexture,
                    TOOLTIPS[i]
                );

                DATA.Add(i, data);
            }
        }
    }
}