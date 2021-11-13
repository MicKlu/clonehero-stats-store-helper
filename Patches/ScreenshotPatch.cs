using HarmonyLib;
using StatsStoreHelper.Utils;
using System;
using System.Reflection;

namespace StatsStoreHelper.Patches
{
    [HarmonyPatch]
    public class ScreenshotPatch
    {
        static MethodBase TargetMethod()
        {
            Type objectType = AccessTools.TypeByName("\u0318\u0319\u0319\u0313\u0310\u031A\u030D\u0312\u031A\u0319\u0319");
            return AccessTools.Method(objectType, "\u031B\u0314\u0316\u030D\u031A\u0319\u0315\u0311\u030F\u0312\u0310");
        }

        public static void Postfix(object __instance)
        {
            var statsSaver = StatsSaver.GetInstance();
            statsSaver.Save();
        }
    }
}