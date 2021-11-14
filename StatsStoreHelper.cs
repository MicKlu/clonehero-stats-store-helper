using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace StatsStoreHelper
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class StatsStoreHelper : BaseUnityPlugin
    {
        public static StatsStoreHelper Instance { get; private set; }

        internal new static ManualLogSource Logger;

        private async void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            await UserConfig.Authorize();
            Logger.LogInfo("Initializing GoogleApi");
            GoogleApi.GoogleApi.GetInstance().Init(UserConfig.GoogleUserCredentials);
            
            var harmony = new Harmony("com.github.mgrinz.clonehero-stats-store-helper");
            harmony.PatchAll();
        }

        private void LateUpdate()
        {
        }

        
    }
}
