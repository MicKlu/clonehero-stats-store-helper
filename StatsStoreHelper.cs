using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using StatsStoreHelper.MyWrappers;

namespace StatsStoreHelper
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class StatsStoreHelper : BaseUnityPlugin
    {
        private bool statsRead = false;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void LateUpdate()
        {
            Scene currentScene = SceneManager.GetActiveScene();

            if(currentScene.name != "EndOfSong")
            {
                this.statsRead = false;
                return;
            }

            if(this.statsRead)
                return;

            Logger.LogInfo($"LastUpdate - Start");
            
            MySongEntry currentSongEntry = MyGlobalVariables.GetInstance().CurrentSongEntry;
            
            Logger.LogInfo($"{currentSongEntry.Artist} - {currentSongEntry.Name}");
            Logger.LogInfo($"  Charter: {currentSongEntry.Charter}");
            Logger.LogInfo($"  Icon: {currentSongEntry.iconName}");

            try
            {
                MyPlayerSongStats[] playerSongStats = MySongStats.PlayerSongStats;
                Logger.LogInfo("  Results:");
                int i = 0;
                foreach(MyPlayerSongStats pss in playerSongStats)
                {
                    Logger.LogInfo($"    Player: {i}");
                    Logger.LogInfo($"      Score: {pss.Score}");
                    Logger.LogInfo($"      Stars: {MySongStats.Stars}");
                    Logger.LogInfo($"      Accuracy: {pss.notesHit}/{pss.notesAll} ({pss.Accuracy})");
                    Logger.LogInfo($"      SP: {pss.spPhrasesHit}/{pss.spPhrasesAll}");
                    Logger.LogInfo($"      Combo: {pss.combo}");
                    Logger.LogInfo($"      Avg. multiplier: {pss.AvgMultiplier}");
                    i++;
                }
            } catch(Exception e)
            {
                Logger.LogError(e.Message);
            }

            this.statsRead = true;
            Logger.LogInfo($"LastUpdate - End");
        }
    }
}
