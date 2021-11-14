using BepInEx;
using HarmonyLib;
using System;

namespace StatsStoreHelper.MyWrappers
{
    public class MyCHPlayer : MyWrapper
    {
        public MyCHPlayer(object _chplayer)
            : base(_chplayer, "\u0315\u0313\u0315\u030F\u030E\u030D\u0314\u031C\u0313\u0316\u0313")
        {
        }

        public MyPlayerProfile PlayerProfile
        {
            get => new MyPlayerProfile(GetFieldValue("\u0313\u0318\u0314\u0316\u0313\u0317\u0312\u0317\u031A\u0314\u031A"));
        }
    }
}