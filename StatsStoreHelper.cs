using BepInEx;
using HarmonyLib;
using StatsStoreHelper.MyWrappers;
using StatsStoreHelper.GoogleApi;
using StatsStoreHelper.Utils;
using System;
using System.Collections.Generic;

namespace StatsStoreHelper
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class StatsStoreHelper : BaseUnityPlugin
    {
        public static StatsStoreHelper Instance { get; private set; }

        private async void Awake()
        {
            Instance = this;
            
            try
            {
                await UserConfig.Authorize();
                GoogleSpreadsheet spreadsheet = GoogleSpreadsheet.GetInstance();
                await spreadsheet.Init(UserConfig.GoogleUserCredentials, PluginInfo.PLUGIN_NAME, "MGRINZ");

                System.Console.WriteLine("Patching...");

                var harmony = new Harmony("com.github.mgrinz.clonehero-stats-store-helper");
                harmony.PatchAll();

                System.Console.WriteLine("Patched");
            }
            catch(Exception e)
            {
                Logger.LogError(e.ToString());
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace);
                Logger.LogError(e.GetType());
            }
        }

        private void LateUpdate()
        {
        }

        
    }
}
