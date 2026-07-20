using RoR2;
using System.Collections.Generic;

namespace StatsMod.CustomStats {
    internal class CoinsSpent : BaseCustomStat {
        private static Dictionary<PlayerCharacterMasterController, uint> coinsSpentDict = [];  // How many lunar coins each player has spent this run

        public override void Init() {
            base.Init();
            On.RoR2.NetworkUser.DeductLunarCoins += CoinsTrack;
        }

        private static void CoinsTrack(On.RoR2.NetworkUser.orig_DeductLunarCoins orig, NetworkUser self, uint count) {
            var player = self.masterController;
            if (player is not null) {
                if (coinsSpentDict.ContainsKey(player)) { coinsSpentDict[player] += count; } else { coinsSpentDict.Add(player, count); }
            }
            orig(self, count);
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("coinsSpent", coinsSpentDict);
        }

        public override void Deserialize(Dictionary<string, object> restored) {
            if (restored.ReportContainsKey("coinsSpent")) {
                coinsSpentDict = (Dictionary<PlayerCharacterMasterController, uint>)restored["coinsSpent"];
            }
        }
    }
}
