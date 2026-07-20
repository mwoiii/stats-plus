using StatsMod.CustomStats;
using System;
using System.Collections.Generic;

namespace StatsMod {
    public static class ProperSaveCompat {
        private static bool? _enabled;

        public static bool enabled {
            get {
                if (_enabled == null) {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.KingEnderBrine.ProperSave");
                }
                return (bool)_enabled;
            }
        }

        public static void Init() {
            ProperSave.SaveFile.OnGatherSaveData += SaveStats;
            ProperSave.Loading.OnLoadingEnded += LoadStats;
        }


        private static void SaveStats(Dictionary<String, Object> dict) {
            if (RecordHandler.statsDatabase != null) {
                var databases = new Dictionary<string, Dictionary<string, List<object>>>();
                foreach (var entry in RecordHandler.statsDatabase) {
                    databases[entry.GetPlayerName()] = entry.Database;
                }
                var properSaveObj = new StatsPlusProperSaveObj(databases, new Dictionary<string, object>(CustomStatTracker.statsTable));
                dict["StatsPlus_Save"] = properSaveObj;
            }
        }

        private static void LoadStats(ProperSave.SaveFile saveFile) {
            if (saveFile.ModdedData.ContainsKey("StatsPlus_Save")) {
                var restored = saveFile.GetModdedData<StatsPlusProperSaveObj>("StatsPlus_Save");
                foreach (var entry in RecordHandler.statsDatabase) {
                    if (restored.databases.TryGetValue(entry.GetPlayerName(), out var a)) {
                        entry.RestoreFrom(a);
                    }
                }
                CustomStatTracker.statsTable.Clear();
                foreach (var customStat in CustomStatTracker.registeredStats) {
                    customStat.Deserialize(restored.customStatsTable);
                    customStat.ConfigureStatsTable();
                }
                Log.Info("Loaded database from ProperSave");
            }
        }

        private class StatsPlusProperSaveObj {
            public Dictionary<string, Dictionary<string, List<object>>> databases;
            public Dictionary<string, object> customStatsTable;

            public StatsPlusProperSaveObj(Dictionary<string, Dictionary<string, List<object>>> databases, Dictionary<string, object> customStatsTable) {
                this.databases = databases;
                this.customStatsTable = customStatsTable;
            }
        }
    }
}
