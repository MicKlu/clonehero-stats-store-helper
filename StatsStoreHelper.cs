using BepInEx;
using HarmonyLib;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Http;
using StatsStoreHelper.MyWrappers;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StatsStoreHelper
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class StatsStoreHelper : BaseUnityPlugin
    {
        private bool statsRead = false;

        private async void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            try
            {
                await UserConfig.Authorize();
                GoogleSpreadsheet spreadsheet = GoogleSpreadsheet.GetInstance();
                await spreadsheet.Init(UserConfig.GoogleUserCredentials, PluginInfo.PLUGIN_NAME, "MGRINZ");

                // Dictionary<string, object> query = new Dictionary<string, object>
                // {
                //     { "%hash%", hash }
                // };

                // int rowIndex = await spreadsheet.FindRow(query);
                // System.Console.WriteLine(rowIndex);

                // spreadsheet.AppendRow(statsRowBuilder.Build());
                //spreadsheet.UpdateRow(3, statsRowBuilder.Build());
            }
            catch(Exception e)
            {
                Logger.LogError(e.ToString());
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace);
                Logger.LogError(e.GetType());
            }
        }

        private string HashSong(string song)
        {
            using(SHA256Managed sha256 = new SHA256Managed())
            {
                string hash = "";
                byte[] buffer = Encoding.UTF8.GetBytes(song);
                byte[] hashBytes = sha256.ComputeHash(buffer);
                foreach(byte b in hashBytes)
                    hash += b.ToString("x2");
                return hash;
            }
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

            SaveStats();

            this.statsRead = true;
            Logger.LogInfo($"LastUpdate - End");
        }

        private async void SaveStats()
        {
            MySongEntry currentSongEntry = MyGlobalVariables.GetInstance().CurrentSongEntry;
            MyPlayerSongStats playerSongStats = MySongStats.PlayerSongStats[0];

            GoogleSpreadsheet spreadsheet = GoogleSpreadsheet.GetInstance();
            // TODO: Get player name from game
            await spreadsheet.Init(UserConfig.GoogleUserCredentials, PluginInfo.PLUGIN_NAME, "MGRINZ");

            string hash = HashSong(currentSongEntry.Name);

            StatsRowBuilder statsRowBuilder = new StatsRowBuilder();
            Dictionary<string, object> stats = new Dictionary<string, object>
            {
                { "%date%", DateTime.Now },
                { "%artist%", currentSongEntry.Artist },
                { "%song%", currentSongEntry.Name },
                { "%source%", currentSongEntry.iconName },
                { "%charter%", currentSongEntry.Charter },
                { "%score%", playerSongStats.Score },
                { "%stars%", MySongStats.Stars },
                { "%accuracy%", Convert.ToDouble(playerSongStats.Accuracy.TrimEnd('%')) / 100 },
                { "%sp%", $"{playerSongStats.spPhrasesHit}/{playerSongStats.spPhrasesAll}" },
                { "%fc%", (playerSongStats.combo == playerSongStats.notesAll) ? true : false },
                { "%screenshot%", "https://aniceimage/" },
                { "%hash%", hash }
            };

            foreach(string tag in UserConfig.StatsTags)
                statsRowBuilder.AddStat(tag, stats[tag]);

            // TODO: Find row, compare stats and decide whether to add or overwrite
            spreadsheet.AppendRow(statsRowBuilder.Build());
        }
    }
}
