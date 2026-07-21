using Newtonsoft.Json.Linq;
using RoR2;
using System.Collections.Generic;

namespace StatsMod.CustomStats {
    internal class CoinsSpent : BaseCustomStat {
        private static Dictionary<string, uint> coinsSpentDict = [];  // How many lunar coins each player has spent this run

        public override void Init() {
            base.Init();
            On.RoR2.NetworkUser.DeductLunarCoins += CoinsTrack;
        }

        private static void CoinsTrack(On.RoR2.NetworkUser.orig_DeductLunarCoins orig, NetworkUser self, uint count) {
            var player = self.masterController;
            if (player is not null) {
                string playerName = RecordHandler.masterControllerToName[player];
                if (coinsSpentDict.ContainsKey(playerName)) { coinsSpentDict[playerName] += count; } else { coinsSpentDict.Add(playerName, count); }
            }
            orig(self, count);
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("coinsSpent", coinsSpentDict);
        }

        public override void Deserialize(Dictionary<string, JToken> restored) {
            if (restored.CanDeserialize("coinsSpent")) {
                coinsSpentDict = restored["coinsSpent"].ToObject<Dictionary<string, uint>>();
            }
        }
    }
}
