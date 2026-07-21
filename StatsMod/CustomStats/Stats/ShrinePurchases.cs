using Newtonsoft.Json.Linq;
using RoR2;
using System.Collections.Generic;

namespace StatsMod.CustomStats {
    internal class ShrinePurchases : BaseCustomStat {
        private static Dictionary<string, uint> shrinePurchasesDict = [];  // How many times each player has used a shrine of chance
        private static Dictionary<string, uint> shrineWinsDict = [];  // How many times each player has won a shrine of chance

        public override void Init() {
            base.Init();
            ShrineChanceBehavior.onShrineChancePurchaseGlobal += ShrineTrack;
        }

        private static void ShrineTrack(bool failed, Interactor activator) {
            var player = activator.GetComponent<CharacterBody>().master.playerCharacterMasterController;
            string playerName = RecordHandler.masterControllerToName[player];
            if (shrinePurchasesDict.ContainsKey(playerName)) {
                shrinePurchasesDict[playerName]++;
                if (!failed) { shrineWinsDict[playerName]++; }
            } else {
                shrinePurchasesDict.Add(playerName, 1);
                if (!failed) { shrineWinsDict.Add(playerName, 1); } else { shrineWinsDict.Add(playerName, 0); }
            }
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("shrinePurchases", shrinePurchasesDict);
            CustomStatTracker.statsTable.Add("shrineWins", shrineWinsDict);
        }

        public override void Deserialize(Dictionary<string, JToken> restored) {
            if (restored.CanDeserialize("shrinePurchases")) {
                shrinePurchasesDict = restored["shrinePurchases"].ToObject<Dictionary<string, uint>>();
            }
            if (restored.CanDeserialize("shrineWins")) {
                shrineWinsDict = restored["shrineWins"].ToObject<Dictionary<string, uint>>();
            }
        }
    }
}
