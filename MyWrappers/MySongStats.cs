using HarmonyLib;
using System;

namespace StatsStoreHelper.MyWrappers
{
    public static class MySongStats
    {
        public static int Score
        {
            get { return (int) GetPropertyValue("\u0313\u030E\u0313\u030E\u0318\u030F\u031C\u0312\u0311\u0310\u031C"); }
        }

        public static int HighestStreak
        {
            get { return (int) GetPropertyValue("\u030D\u031A\u0310\u0312\u031A\u030E\u0315\u0319\u0317\u031C\u0310"); }
        }

        public static int Stars
        {
            get { return (int) GetPropertyValue("\u030D\u0319\u0315\u031C\u0318\u030D\u0319\u0310\u0310\u0312\u0312"); }
        }

        public static string[] Sections
        {
            get { return (string[]) GetPropertyValue("\u030F\u030D\u0317\u030D\u030E\u031A\u0316\u030D\u0312\u0313\u0315"); }
        }

        public static MyPlayerSongStats[] PlayerSongStats
        {
            get
            {
                MyPlayerSongStats[] playerSongStats = new MyPlayerSongStats[4];
                var chPlayerSongStats = (object[]) GetPropertyValue("\u030D\u0313\u031A\u0314\u030E\u031B\u030E\u0315\u030D\u031A\u031A");
                for(int i = 0; i < 4; i++)
                    playerSongStats[i] = new MyPlayerSongStats(chPlayerSongStats[i]);
                return playerSongStats;
            }
        }

        private static object GetPropertyValue(string propertyName)
        {
            var chSongStatsType = AccessTools.TypeByName("\u031B\u0318\u0313\u030E\u0315\u0311\u030F\u0315\u0316\u031C\u0313");
            var property = AccessTools.Property(chSongStatsType, propertyName);
            return property.GetValue(null);
        }
    }
}