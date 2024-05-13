using RoR2;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace StatsMod.CustomStats
{
    public static class Tracker
    {
        public static Hashtable statsTable = [];

        public static object GetStat(PlayerCharacterMasterController player, string statName)
        {
            if (statsTable.ContainsKey(statName))
            {
                var stat = statsTable[statName];
                if (stat is IDictionary dict)
                {
                    var value = dict[player];
                    if (value is null) { return 0; }
                    return value;
                }
                return 0;
            }
            else { Log.Error("Stat not found, returning 0"); return 0; }
        }

        public static void Enable()
        {
            var statTypes = typeof(Stat).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Stat)));
            foreach (var stat in statTypes) { stat.GetMethod("Init", BindingFlags.Public | BindingFlags.Static).Invoke(null, null); }
        }

        public static void ResetData()
        {
            foreach (var stat in statsTable.Keys)
            {
                if (stat is IDictionary dict) { dict.Clear(); }
            }
        }
    }
}
