using HarmonyLib;
using StatsStoreHelper.Utils;
using System;
using System.IO;
using System.Reflection;

namespace StatsStoreHelper.Patches
{
    [HarmonyPatch]
    public class ScreenshotPatch
    {
        private static Type objectType = AccessTools.TypeByName("\u0318\u0319\u0319\u0313\u0310\u031A\u030D\u0312\u031A\u0319\u0319");
        
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(objectType, "\u031B\u0314\u0316\u030D\u031A\u0319\u0315\u0311\u030F\u0312\u0310");
        }

        public static void Postfix(object __instance)
        {
            FieldInfo screenshotField = AccessTools.Field(objectType, "\u0319\u030D\u0314\u0310\u0319\u031A\u0319\u0318\u030F\u0319\u031B");
            string screenshotPath = (string) screenshotField.GetValue(__instance);
        
            var statsSaver = StatsSaver.GetInstance();
            statsSaver.ScreenshotPath = screenshotPath;
            statsSaver.Save();
        }
    }
}