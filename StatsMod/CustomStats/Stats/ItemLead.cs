using Newtonsoft.Json.Linq;
using RoR2;
using System.Collections.Generic;

namespace StatsMod.CustomStats {
    internal class ItemLead : BaseCustomStat {
        private static Dictionary<string, uint> itemLeadDict = [];  // The current item lead of a player: 0 if not in the lead, >0 otherwise

        public override void Init() {
            base.Init();
            SceneExitController.onBeginExit += ItemLeadTrack;
        }

        private static void ItemLeadTrack(SceneExitController sceneExitController) {
            if (PlayerCharacterMasterController.instances.Count < 2) { return; }

            uint highestLead = 0;
            uint highestItems = 0;
            string leadingPlayer = null;
            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances) {
                string playerName = RecordHandler.masterControllerToName[player];
                itemLeadDict[playerName] = 0;
                uint playerItems = (uint)player.master.inventory.permanentItemStacks.GetTotalItemStacks();
                if (playerItems >= highestItems) // Wauce
                {
                    highestLead = playerItems - highestItems;
                    highestItems = playerItems;
                    leadingPlayer = playerName;
                } else if (highestItems - playerItems < highestLead) { highestLead = highestItems - playerItems; }
            }
            if (itemLeadDict.ContainsKey(leadingPlayer)) { itemLeadDict[leadingPlayer] = highestLead; } else { itemLeadDict.Add(leadingPlayer, highestLead); }
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("itemLead", itemLeadDict);
        }

        public override void Deserialize(Dictionary<string, JToken> restored) {
            if (restored.CanDeserialize("itemLead")) {
                itemLeadDict = restored["itemLead"].ToObject<Dictionary<string, uint>>();
            }
        }
    }
}
