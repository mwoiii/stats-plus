using RoR2;
using System.Collections.Generic;

namespace StatsMod.CustomStats {
    internal class ShrinePurchases : BaseCustomStat {
        private static Dictionary<PlayerCharacterMasterController, uint> shrinePurchasesDict = [];  // How many times each player has used a shrine of chance
        private static Dictionary<PlayerCharacterMasterController, uint> shrineWinsDict = [];  // How many times each player has won a shrine of chance

        public override void Init() {
            base.Init();
            ShrineChanceBehavior.onShrineChancePurchaseGlobal += ShrineTrack;
        }

        private static void ShrineTrack(bool failed, Interactor activator) {
            var player = activator.GetComponent<CharacterBody>().master.playerCharacterMasterController;
            if (shrinePurchasesDict.ContainsKey(player)) {
                shrinePurchasesDict[player]++;
                if (!failed) { shrineWinsDict[player]++; }
            } else {
                shrinePurchasesDict.Add(player, 1);
                if (!failed) { shrineWinsDict.Add(player, 1); } else { shrineWinsDict.Add(player, 0); }
            }
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("shrinePurchases", shrinePurchasesDict);
            CustomStatTracker.statsTable.Add("shrineWins", shrineWinsDict);
        }

        public override void Deserialize(Dictionary<string, object> restored) {
            if (restored.ReportContainsKey("shrinePurchases")) {
                shrinePurchasesDict = (Dictionary<PlayerCharacterMasterController, uint>)restored["shrinePurchases"];
            }
            if (restored.ReportContainsKey("shrineWins")) {
                shrineWinsDict = (Dictionary<PlayerCharacterMasterController, uint>)restored["shrineWins"];
            }
        }
    }
}
