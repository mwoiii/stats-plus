using Newtonsoft.Json.Linq;
using RoR2;
using System.Collections.Generic;

namespace StatsMod.CustomStats {
    internal class OrderHits : BaseCustomStat {
        private static Dictionary<string, uint> orderHitsDict = [];  // How many times each player has used a shrine of order
        public override void Init() {
            base.Init();
            On.RoR2.ShrineRestackBehavior.AddShrineStack += OrderTrack;
        }

        private static void OrderTrack(On.RoR2.ShrineRestackBehavior.orig_AddShrineStack orig, ShrineRestackBehavior self, Interactor interactor) {
            var player = interactor.GetComponent<CharacterBody>().master.playerCharacterMasterController;
            string playerName = RecordHandler.masterControllerToName[player];
            if (orderHitsDict.ContainsKey(playerName)) { orderHitsDict[playerName]++; } else { orderHitsDict.Add(playerName, 1); }
            orig(self, interactor);
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("orderHits", orderHitsDict);
        }

        public override void Deserialize(Dictionary<string, JToken> restored) {
            if (restored.CanDeserialize("orderHits")) {
                orderHitsDict = restored["orderHits"].ToObject<Dictionary<string, uint>>();
            }
        }
    }
}
