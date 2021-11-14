using HarmonyLib;
using System;

namespace StatsStoreHelper.MyWrappers
{
    public class MyPlayerSongStats : MyWrapper
    {
        public readonly int notesHit;
        public readonly int notesAll;
        public readonly int combo;
        public readonly int spPhrasesHit;
        public readonly int spPhrasesAll;

        public MyPlayerSongStats(object _chPlayerSongStats)
            : base(_chPlayerSongStats, "\u030E\u0310\u0316\u031B\u0319\u031C\u0317\u0313\u030F\u0313\u0310")
        {
            this.notesHit = (int) GetFieldValue("\u031B\u030E\u0310\u0317\u0312\u0317\u030E\u0318\u0319\u0314\u0314");
            this.notesAll = (int) GetFieldValue("\u031C\u0310\u031C\u031C\u031B\u031A\u0318\u0311\u0316\u031B\u0312");
            this.combo = (int) GetFieldValue("\u0318\u0316\u031B\u0311\u031B\u0313\u0315\u030D\u031B\u0310\u031A");
            this.spPhrasesHit = (int) GetFieldValue("\u0315\u0311\u031C\u0314\u031C\u031C\u031C\u0318\u0318\u0314\u0310");
            this.spPhrasesAll = (int) GetFieldValue("\u0311\u0319\u031A\u0315\u0319\u0317\u0318\u0311\u030F\u030D\u031B");
        }

        public int Score
        {
            get { return (int) GetPropertyValue("\u0310\u031B\u0312\u031B\u0319\u030D\u0318\u030D\u0313\u0317\u0313"); }
        }

        public string Accuracy
        {
            get { return (string) GetPropertyValue("\u0318\u031A\u0318\u0316\u030D\u031A\u0316\u0311\u0313\u0314\u0312"); }
        }

        public float AvgMultiplier
        {
            get { return (float) GetPropertyValue("\u030D\u0318\u030D\u0319\u0311\u031B\u0311\u031A\u030F\u031A\u0310"); }
        }
    }
}