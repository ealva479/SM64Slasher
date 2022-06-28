using GameCreator.ModuleManager;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NJG.Graph
{
    public static class ModuleInitilize
    {
        public const string SYMBOL_NPC = "SM_RPG";
        public const string SYMBOL_PHOTON = "PHOTON_MODULE";
        public const string AI_PATH = "Assets/Ninjutsu Games/GameCreator Modules/RPG";
        public const string PHOTON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon";

        /// <summary>
        /// Add define symbols as soon as Unity gets done compiling.
        /// </summary>
#if PHOTON_UNITY_NETWORKING && !PHOTON_RPG
        [InitializeOnLoadMethod]
        static void Initilize()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                try
                {
                    var module2 = ModuleManager.GetAssetModule("com.ninjutsugames.modules.rpg");
                    bool HasAI = module2 != null && module2.module != null && ModuleManager.IsEnabled(module2.module);
                    if (HasAI)
                    {
                        AddScriptingDefineSymbolToAllBuildTargetGroups(SYMBOL_NPC);
                    }
                }
                catch
                {
                    //
                }
            }
        }
#endif


        /// <summary>
        /// Adds a given scripting define symbol to all build target groups
        /// You can see all scripting define symbols ( not the internal ones, only the one for this project), in the PlayerSettings inspector
        /// </summary>
        /// <param name="defineSymbol">Define symbol.</param>
        public static void AddScriptingDefineSymbolToAllBuildTargetGroups(string defineSymbol)
        {
            foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
            {
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

                if (group == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';')
                    .Select(d => d.Trim()).ToList();

                if (!defineSymbols.Contains(defineSymbol))
                {
                    defineSymbols.Add(defineSymbol);

                    try
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(group,
                            string.Join(";", defineSymbols.ToArray()));
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Could not set StateMachine " + defineSymbol + " defines for build target: " +
                                  target + " group: " + group + " " + e);
                    }
                }
            }
        }

        public static void CleanUpDefineSymbols()
        {
            foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
            {
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

                if (group == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group)
                    .Split(';')
                    .Select(d => d.Trim())
                    .ToList();

                List<string> newDefineSymbols = new List<string>();
                foreach (var symbol in defineSymbols)
                {
                    if (SYMBOL_NPC.Equals(symbol) || symbol.StartsWith(SYMBOL_NPC) ||
                        SYMBOL_PHOTON.Equals(symbol) || symbol.StartsWith(SYMBOL_PHOTON))
                    {
                        continue;
                    }

                    newDefineSymbols.Add(symbol);
                }

                try
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group,
                        string.Join(";", newDefineSymbols.ToArray()));
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat(
                        "Could not clean up RPG's define symbols for build target: {0} group: {1}, {2}", target, group,
                        e);
                }
            }
        }
    }

    public class CleanUpDefinesOnPunDelete : UnityEditor.AssetModificationProcessor
    {
        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions rao)
        {
            if (ModuleInitilize.AI_PATH.Equals(assetPath) || ModuleInitilize.PHOTON_PATH.Equals(assetPath))
            {
                ModuleInitilize.CleanUpDefineSymbols();
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}