using RoR2;
using System.Collections;
using System.Collections.Generic;

namespace StatsMod.CustomStats {
    public static class CustomStatTracker {
        public static List<BaseCustomStat> registeredStats = new List<BaseCustomStat>();
        public static Dictionary<string, object> statsTable = [];
        private static bool loaded = false;

        public static object GetStat(PlayerCharacterMasterController player, string statName) {
            if (statsTable.ContainsKey(statName)) {
                var stat = statsTable[statName];
                if (stat is IDictionary dict) {
                    var value = dict[player];
                    if (value is null) { return 0; }
                    return value;
                }
                return 0;
            } else { Log.Error("Stat not found, returning 0"); return 0; }
        }

        public static void Init() {
            if (!loaded) {
                InitCustomStats();
            }
            loaded = true;
        }

        public static void ResetData() {
            foreach (var stat in statsTable.Values) {
                if (stat is IDictionary dict) { dict.Clear(); }
            }
        }

        private static void InitCustomStats() {
            new Allies().Init();
            new Avenges().Init();
            new CoinsSpent().Init();
            new FallDamage().Init();
            new ItemLead().Init();
            new NonScrapPrinted().Init();
            new OrderHits().Init();
            new ShrinePurchases().Init();
            new TimeLowHealth().Init();
            new TimesLastStanding().Init();
            new TimeStill().Init();
        }
    }
}
