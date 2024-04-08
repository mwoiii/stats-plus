using RoR2;
using RoR2.Stats;
using BepInEx;
using R2API;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Text;

namespace StatsMod
{
    public class CustomStatsRecord(PlayerCharacterMasterController instance, string name) : StatsRecord(instance, name)
    {
        // The names of the custom stats to be recorded
        public static ReadOnlyCollection<string> customStats = new(["shrinePurchases", "shrineWins", "orderHits", "timeStill"]);

        protected override Dictionary<string,object> GetStats()
        {
            Dictionary<string, object> Stats = [];

            foreach (string i in customStats)
            {
                uint stat = CustomStatsTracker.GetStat(PlayerCharacterMasterController.instances[playerIndex], i);
                Stats.Add(i, stat);
            }

            // This method will record the custom stats to the inherited Stats dictionary
            return Stats;
        }
    }
}
