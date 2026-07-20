using RoR2;
using System.Collections.Generic;

namespace StatsMod.CustomStats {
    internal class ItemLead : BaseCustomStat {
        private static Dictionary<PlayerCharacterMasterController, uint> itemLeadDict = [];  // The current item lead of a player: 0 if not in the lead, >0 otherwise

        public override void Init() {
            base.Init();
            SceneExitController.onBeginExit += ItemLeadTrack;
        }

        private static void ItemLeadTrack(SceneExitController sceneExitController) {
            if (PlayerCharacterMasterController.instances.Count < 2) { return; }

            uint highestLead = 0;
            uint highestItems = 0;
            PlayerCharacterMasterController leadingPlayer = null;
            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances) {
                itemLeadDict[player] = 0;
                uint playerItems = (uint)player.master.inventory.permanentItemStacks.GetTotalItemStacks();
                if (playerItems >= highestItems) // Wauce
                {
                    highestLead = playerItems - highestItems;
                    highestItems = playerItems;
                    leadingPlayer = player;
                } else if (highestItems - playerItems < highestLead) { highestLead = highestItems - playerItems; }
            }
            if (itemLeadDict.ContainsKey(leadingPlayer)) { itemLeadDict[leadingPlayer] = highestLead; } else { itemLeadDict.Add(leadingPlayer, highestLead); }
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("itemLead", itemLeadDict);
        }

        public override void Deserialize(Dictionary<string, object> restored) {
            if (restored.ReportContainsKey("itemLead")) {
                itemLeadDict = (Dictionary<PlayerCharacterMasterController, uint>)restored["itemLead"];
            }
        }
    }
}
