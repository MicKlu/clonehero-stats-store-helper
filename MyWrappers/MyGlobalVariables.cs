using HarmonyLib;

namespace StatsStoreHelper.MyWrappers
{
    public class MyGlobalVariables
    {
        private static MyGlobalVariables instance = null;
        private GlobalVariables globalVariables;

        private MyGlobalVariables()
        {
            this.globalVariables = (GlobalVariables) AccessTools.Field(typeof(GlobalVariables), "\u0312\u0313\u0310\u0315\u030E\u0319\u030D\u0318\u0313\u030E\u031A").GetValue(null);   
        }

        public static MyGlobalVariables GetInstance()
        {
            if(instance == null)
                instance = new MyGlobalVariables();
            return instance;
        }

        public MySongEntry CurrentSongEntry
        {
            get
            {
                var currentSongEntryField = AccessTools.Field(typeof(GlobalVariables), "\u0310\u030E\u0313\u0310\u0313\u0314\u030D\u031B\u0313\u031C\u0313");
                return new MySongEntry(currentSongEntryField.GetValue(this.globalVariables));
            }
        }

    }
}