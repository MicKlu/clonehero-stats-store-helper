using System.Reflection;
using BepInEx.Logging;

namespace StatsStoreHelper.MyWrappers
{
    public class MyPlayerProfile : MyWrapper
    {
        public readonly string playerName;

        public MyPlayerProfile(object _playerProfile)
            : base(_playerProfile, "\u0311\u0316\u0315\u031B\u0310\u0314\u0316\u030E\u0311\u0315\u031B")
        {
            try
            {
                this.playerName = (string) GetFieldValue("\u031A\u0311\u0312\u030D\u0315\u0310\u0311\u0316\u030D\u0311\u0312");
            }
            catch(TargetException)
            {
                this.playerName = "Unknown Player";
                StatsStoreHelper.Logger.LogError("Can't get player's profile. Did they drop out?");
            }
        }
    }
}