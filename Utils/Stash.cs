using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace StatsStoreHelper.Utils
{
    public class Stash
    {
        public static readonly string StashPath = Path.Combine(BepInEx.Paths.CachePath, PluginInfo.PLUGIN_NAME);
        private string stashFilePath;
        private Dictionary<string, List<Dictionary<string, object>>> stash;
        private List<Dictionary<string, object>> playerStash;
        private string playerName;

        public Stash(string playerName)
        {
            this.stashFilePath = Path.Combine(StashPath, "stash.json");
            this.playerName = playerName;

            if(!Directory.Exists(StashPath))
                Directory.CreateDirectory(StashPath);

            if(!File.Exists(stashFilePath))
                File.WriteAllText(stashFilePath, "{}");

            string stashContent = File.ReadAllText(stashFilePath);

            try
            {
                this.stash = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(stashContent);
                if(stash.ContainsKey(playerName))
                    this.playerStash = stash[playerName];
                else
                    CreatePlayerStash();
            }
            catch
            {
                StatsStoreHelper.Logger.LogError("Stats cache corrupted. Creating new");
                this.stash = new Dictionary<string, List<Dictionary<string, object>>>();
                CreatePlayerStash();
            }
        }

        private void CreatePlayerStash()
        {
            this.playerStash = new List<Dictionary<string, object>>();
            this.stash.Add(playerName, this.playerStash);
        }

        public void Add(Dictionary<string, object> stats)
        {
            if(!playerStash.Contains(stats))
                playerStash.Add(stats);
        }

        public void Remove(Dictionary<string, object> songStats)
        {
            if(playerStash.Contains(songStats))
                playerStash.Remove(songStats);
        }

        public void Save()
        {
            string stashContent = JsonConvert.SerializeObject(stash);
            File.WriteAllText(stashFilePath, stashContent);
        }

        public List<Dictionary<string, object>> Get()
        {
            return new List<Dictionary<string, object>>(this.playerStash);
        }
    }
}