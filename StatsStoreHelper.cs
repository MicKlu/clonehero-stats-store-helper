using BepInEx;
using HarmonyLib;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Http;
using StatsStoreHelper.MyWrappers;
using System;
using System.Collections.Generic;
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
                UserCredential credentials = await GoogleAuthorization();
            }
            catch(Exception e)
            {
                Logger.LogError(e.ToString());
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace);
                Logger.LogError(e.GetType());
            }
        }

        private async Task<UserCredential> GoogleAuthorization()
        {
            ClientSecrets clientSecrets = new ClientSecrets
            {
                ClientId = "1043533161342-rb1s1n4pcstiuc58tptkj3a6ju611guo.apps.googleusercontent.com",
                ClientSecret = "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"
            };
            List<string> scopes = new List<string>
            {
                "https://www.googleapis.com/auth/photoslibrary.appendonly",
                "https://www.googleapis.com/auth/photoslibrary.readonly.appcreateddata",
                "https://www.googleapis.com/auth/drive.file"
            };
            
            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                scopes,
                "user",
                CancellationToken.None,
                new FileDataStore($"{BepInEx.Paths.ConfigPath}/{PluginInfo.PLUGIN_NAME}", true));
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
