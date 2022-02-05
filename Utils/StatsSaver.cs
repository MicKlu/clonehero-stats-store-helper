
using Newtonsoft.Json;
using StatsStoreHelper.Apis.GoogleApi;
using StatsStoreHelper.MyWrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StatsStoreHelper.Utils
{
    public class StatsSaver
    {
        private static StatsSaver instance;
        private static readonly object instanceLock = new object();
        private bool statsSaved;
        private string playerName;

        public string ScreenshotPath { get; set; }

        private StatsSaver()
        {
            this.statsSaved = false;
        }

        public static StatsSaver GetInstance()
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

            this.playerName = MyGlobalVariables.GetInstance().CHPlayers[0].PlayerProfile.playerName;

            var stash = new Stash(this.playerName);
            List<Dictionary<string, object>> queue = stash.Get();

            MySongEntry currentSongEntry = MyGlobalVariables.GetInstance().CurrentSongEntry;
            MyPlayerSongStats playerSongStats = MySongStats.PlayerSongStats[0];

            List<string> hashes = new List<string>();
            hashes.Add(currentSongEntry.GetMD5Hash());
            hashes.Add(currentSongEntry.GetSHA256Hash());
            
            if(UserConfig.UserStatsTags.Contains("%screenshot%"))
                BackUpScreenshot();

            Dictionary<string, object> stats = new Dictionary<string, object>
            {
                { "%date%", DateTime.Now },
                { "%artist%", currentSongEntry.Artist },
                { "%album%", currentSongEntry.Album },
                { "%song%", currentSongEntry.Name },
                { "%genre%", currentSongEntry.Genre },
                { "%source%", currentSongEntry.iconName },
                { "%charter%", currentSongEntry.Charter },
                { "%score%", playerSongStats.Score },
                { "%multiplier%", playerSongStats.AvgMultiplier },
                { "%stars%", MySongStats.Stars },
                { "%notes%", $"{playerSongStats.notesHit}/{playerSongStats.notesAll}" },
                { "%accuracy%", (1.0 * playerSongStats.notesHit / playerSongStats.notesAll) },
                { "%combo%", playerSongStats.combo },
                { "%sp%", $"{playerSongStats.spPhrasesHit}/{playerSongStats.spPhrasesAll}" },
                { "%fc%", (playerSongStats.combo == playerSongStats.notesAll) ? true : false },
                { "%screenshot%", ScreenshotPath },
                { "%screenshotdelete%", "" },
                { "%hash%", hashes[0] },
                { "%null%", "" }
            };
            queue.Add(stats);
            
            StatsRowBuilder statsRowBuilder = new StatsRowBuilder();
            foreach(var songStats in queue)
            {
                statsRowBuilder.Reset();
                
                foreach(string tag in UserConfig.UserStatsTags)
                    statsRowBuilder.AddStat(tag, songStats[tag]);
                
                StatsRow currentStats = statsRowBuilder.Build();

                try
                {
                    await SaveToSpreadsheet(currentStats, hashes);

                    if(UserConfig.UserStatsTags.Contains("%screenshot%") && File.Exists((string) songStats["%screenshot%"]))
                        File.Delete((string) songStats["%screenshot%"]);
                    
                    stash.Remove(songStats);
                }
                catch(Exception e)
                {
                    StatsStoreHelper.Logger.LogError("Can't save stats to spreadsheet. Storing it locally.");
                    if(e.GetType().Name == "HttpRequestException")
                        StatsStoreHelper.Logger.LogError("  Can't send request.");
                    else
                    {
                        StatsStoreHelper.Logger.LogError(e.GetType().Name);
                        StatsStoreHelper.Logger.LogError(e.Message);
                        StatsStoreHelper.Logger.LogError(e.StackTrace);
                    }

                    stash.Add(stats);
                }
            }
            
            stash.Save();
            this.statsSaved = true;
        }

        private void BackUpScreenshot()
        {
            var oldScreenshotPath = ScreenshotPath;
            ScreenshotPath = Path.Combine(Stash.StashPath, Path.GetFileName(oldScreenshotPath));

            if(!Directory.Exists(Stash.StashPath))
                Directory.CreateDirectory(Stash.StashPath);

            File.Copy(oldScreenshotPath, ScreenshotPath);
        }

        private async Task SaveToSpreadsheet(StatsRow currentStats, List<string> songHashes)
        {
            GoogleSpreadsheet spreadsheet = GoogleSpreadsheet.GetInstance();
            
            await spreadsheet.Init(UserConfig.GoogleUserCredentials, playerName);
            
            FindRowResult findRowResult = null;
            foreach(string hash in songHashes)
            {
                findRowResult = await spreadsheet.FindRow(new Dictionary<string, object> () { { "%hash%", hash } });
                if(findRowResult.RowData != null)
                    break;
            }

            if(findRowResult == null)
                throw new Exception("Querying spreadsheet didn't return any valid results.");
            
            if(findRowResult.RowData == null)
            {
                System.Console.WriteLine("New Song! – Adding");
                currentStats.CheckChorus();
                await currentStats.UploadScreenshot();
                spreadsheet.AppendRow(currentStats.RowData);
            }
            else
            {
                System.Console.WriteLine("Existing Song – Checking");
                StatsRow storedStats = new StatsRow(findRowResult.RowData);
                if(storedStats.CompareTo(currentStats) > 0)
                {
                    System.Console.WriteLine("Better stats! – Updating");
                    currentStats.CheckChorus();
                    storedStats.DeleteScreenshot();
                    await currentStats.UploadScreenshot();
                    spreadsheet.UpdateRow(findRowResult.Index, currentStats.RowData);
                }
                else
                    System.Console.WriteLine("No improvement :( – Leaving");
            }
        }

        public void Reset()
        {
            this.statsSaved = false;
        }
    }
}