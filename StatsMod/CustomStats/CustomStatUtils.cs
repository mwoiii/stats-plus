using RoR2;
using System.Collections.Generic;

namespace StatsMod.CustomStats {
    public static class CustomStatUtils {
        public static bool IsSafe() {
            bool voidLocusSafe = VoidStageMissionController.instance?.numBatteriesActivated >= VoidStageMissionController.instance?.numBatteriesSpawned && VoidStageMissionController.instance?.numBatteriesSpawned > 0;
            return TeleporterInteraction.instance?.isCharged ?? ArenaMissionController.instance?.clearedEffect.activeSelf ?? voidLocusSafe;
        }

        public static bool CanDeserialize<T1, T2>(this Dictionary<T1, T2> dict, T1 key) {
            if (dict.ContainsKey(key) && dict[key] != null) {
                return true;
            } else {
                Log.Error($"Error deserializing custom stat - could not find key '{key}'");
                return false;
            }
        }
    }
}
