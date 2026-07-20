using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace StatsMod.CustomStats {
    internal class Allies : BaseCustomStat {
        private static Dictionary<PlayerCharacterMasterController, uint> alliesDict = [];  // How many times each player has used a shrine of order

        public override void Init() {
            base.Init();
            SceneExitController.onBeginExit += AllyCountTrackLeaveStage;
            Run.onServerGameOver += AllyCountTrackRunOver;
        }

        private static void AllyCountTrackRunOver(Run run, GameEndingDef gameEndingDef) {
            CountAllies();
        }

        private static void AllyCountTrackLeaveStage(SceneExitController sceneExitController) {
            CountAllies();
        }

        private static void CountAllies() {
            // will not actually count all allies
            // just most of the relevant vanilla ones
            foreach (var player in PlayerCharacterMasterController.instances) {
                var inventory = player.master.inventory;
                CharacterMaster master = player.master;
                int droneCount = CharacterMaster.readOnlyInstancesList.Where(u => (u.name.Contains("Drone") || u.name.Contains("Turret")) && u.minionOwnership.ownerMaster == master).Count();
                int sum = (
                    master.GetDeployableSameSlotLimit(DeployableSlot.BeetleGuardAlly) +
                    master.GetDeployableSameSlotLimit(DeployableSlot.ParentAlly) +
                    master.GetDeployableSameSlotLimit(DeployableSlot.VoidMegaCrabItem) +
                    (inventory.GetItemCountPermanent(RoR2Content.Items.RoboBallBuddy) > 0 ? 2 : 0) +
                    droneCount
                    );
                if (alliesDict.ContainsKey(player)) { alliesDict[player] = (uint)sum; } else { alliesDict.Add(player, (uint)sum); }
            }
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("allies", alliesDict);
        }

        public override void Deserialize(Dictionary<string, object> restored) {
            if (restored.ReportContainsKey("allies")) {
                alliesDict = (Dictionary<PlayerCharacterMasterController, uint>)restored["allies"];
            }
        }
    }
}
