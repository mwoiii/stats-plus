using RoR2;
using System.Collections.Generic;

namespace StatsMod.CustomStats {
    internal class OrderHits : BaseCustomStat {
        private static Dictionary<PlayerCharacterMasterController, uint> orderHitsDict = [];  // How many times each player has used a shrine of order
        public override void Init() {
            base.Init();
            On.RoR2.ShrineRestackBehavior.AddShrineStack += OrderTrack;
        }

        private static void OrderTrack(On.RoR2.ShrineRestackBehavior.orig_AddShrineStack orig, ShrineRestackBehavior self, Interactor interactor) {
            var player = interactor.GetComponent<CharacterBody>().master.playerCharacterMasterController;
            if (orderHitsDict.ContainsKey(player)) { orderHitsDict[player]++; } else { orderHitsDict.Add(player, 1); }
            orig(self, interactor);
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("orderHits", orderHitsDict);
        }

        public override void Deserialize(Dictionary<string, object> restored) {
            if (restored.ReportContainsKey("orderHits")) {
                orderHitsDict = (Dictionary<PlayerCharacterMasterController, uint>)restored["orderHits"];
            }
        }
    }
}
