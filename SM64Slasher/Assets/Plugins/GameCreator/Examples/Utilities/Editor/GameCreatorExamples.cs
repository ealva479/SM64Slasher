namespace GameCreator.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    internal static class GameCreatorExamples
    {
        private static readonly string[] SCENES = new string[]
        {
            "Assets/Plugins/GameCreator/Examples/Scenes/Example-Intro.unity",
            "Assets/Plugins/GameCreator/Examples/Scenes/Example-Hub.unity",
            "Assets/Plugins/GameCreator/Examples/Scenes/Example-1.unity",
            "Assets/Plugins/GameCreator/Examples/Scenes/Example-2.unity",
            "Assets/Plugins/GameCreator/Examples/Scenes/Example-3.unity",
        };

        // SETUP METHODS: -------------------------------------------------------------------------

        [InitializeOnLoadMethod]
        private static void Setup()
        {
            List<EditorBuildSettingsScene> editorBuildSettingsScenes;
            editorBuildSettingsScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            for (int i = 0; i < SCENES.Length; ++i)
            {
                bool sceneInBuildSettings = false;
                for (int j = 0; j < editorBuildSettingsScenes.Count; ++j)
                {
                    if (editorBuildSettingsScenes[j].path == SCENES[i])
                    {
                        sceneInBuildSettings = true;
                    }
                }

                if (sceneInBuildSettings) continue;

                EditorBuildSettingsScene scene = new EditorBuildSettingsScene(SCENES[i], true);
                if (!editorBuildSettingsScenes.Contains(scene))
                {
                    editorBuildSettingsScenes.Add(scene);
                }
            }

            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        }
    }
}