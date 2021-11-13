using HarmonyLib;
using StatsStoreHelper.Utils;
using System;
using System.Reflection;

namespace StatsStoreHelper.Patches
{
    [HarmonyPatch(typeof(EndOfSong), "Start")]
    public class EndOfSongPatch
    {
        public static void Postfix(object __instance)
        {
            var statsSaver = StatsSaver.GetInstance();
            statsSaver.Reset();
        }
    }
}