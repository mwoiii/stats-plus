using Newtonsoft.Json.Linq;
using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace StatsMod.CustomStats {
    internal class Allies : BaseCustomStat {
        private static Dictionary<string, uint> alliesDict = [];  // How many times each player has used a shrine of order

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

                string playerName = RecordHandler.masterControllerToName[player];
                if (alliesDict.ContainsKey(playerName)) {
                    alliesDict[playerName] = (uint)sum;
                } else {
                    alliesDict.Add(playerName, (uint)sum);
                }
            }
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("allies", alliesDict);
        }

        public override void Deserialize(Dictionary<string, JToken> restored) {
            if (restored.CanDeserialize("allies")) {
                alliesDict = restored["allies"].ToObject<Dictionary<string, uint>>();
            }
        }
    }
}
