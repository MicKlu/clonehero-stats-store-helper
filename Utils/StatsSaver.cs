using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StatsStoreHelper.GoogleApi;
using StatsStoreHelper.MyWrappers;

namespace StatsStoreHelper.Utils
{
    public class StatsSaver
    {
        private static StatsSaver instance;
        private static readonly object instanceLock = new object();
        private bool statsSaved = false;

        private StatsSaver() {}

        internal static StatsSaver GetInstance()
        {
            if(instance == null)
                lock(instanceLock)
                    if(instance == null)
                        instance = new StatsSaver();
            return instance;
        }

        public async void Save()
        {
            if(this.statsSaved)
                return;
                
            MySongEntry currentSongEntry = MyGlobalVariables.GetInstance().CurrentSongEntry;
            MyPlayerSongStats playerSongStats = MySongStats.PlayerSongStats[0];

            string hash = currentSongEntry.GetSHA256Hash();
            
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

            foreach(string tag in UserConfig.UserStatsTags)
                statsRowBuilder.AddStat(tag, stats[tag]);

            StatsRow currentStats = statsRowBuilder.Build();

            try
            {
                await SaveToSpreadsheet(currentStats, hash);
            }
            catch
            {
                Stash(currentStats);
            }
            
            this.statsSaved = true;
        }

        private async Task SaveToSpreadsheet(StatsRow currentStats, string songHash)
        {
            GoogleSpreadsheet spreadsheet = GoogleSpreadsheet.GetInstance();
            // TODO: Get player name from game
            await spreadsheet.Init(UserConfig.GoogleUserCredentials, PluginInfo.PLUGIN_NAME, "MGRINZ");

            FindRowResult findRowResult = await spreadsheet.FindRow(new Dictionary<string, object> () { { "%hash%", songHash } });
            if(findRowResult.RowData == null)
            {
                System.Console.WriteLine("New Song! – Adding");
                spreadsheet.AppendRow(currentStats.RowData);
            }
            else
            {
                System.Console.WriteLine("Existing Song – Checking");
                StatsRow storedStats = new StatsRow(findRowResult.RowData);
                if(storedStats.CompareTo(currentStats) > 0)
                {
                    System.Console.WriteLine("Better stats! – Updating");
                    spreadsheet.UpdateRow(findRowResult.Index, currentStats.RowData);
                }
                else
                    System.Console.WriteLine("No improvement :( – Leaving");
            }
        }

        private void Stash(StatsRow currentStats)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            this.statsSaved = false;
        }
    }
}