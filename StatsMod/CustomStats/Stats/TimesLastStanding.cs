using RoR2;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace StatsMod.CustomStats {
    internal class TimesLastStanding : BaseCustomStat {
        private static Dictionary<PlayerCharacterMasterController, uint> timesLastStandingDict = [];  // How many times a player has been the last man standing before the end of the tp event]

        public override void Init() {
            base.Init();
            On.RoR2.GlobalEventManager.OnPlayerCharacterDeath += LastStandingTrack;
        }

        private static void LastStandingTrack(On.RoR2.GlobalEventManager.orig_OnPlayerCharacterDeath orig, GlobalEventManager self, DamageReport damageReport, NetworkUser victimNetworkUser) {
            if (CustomStatUtils.IsSafe() || SceneManager.GetActiveScene().name == "bazaar") { return; }
            int alivePlayers = 0;
            PlayerCharacterMasterController alivePlayer = null;
            foreach (PlayerCharacterMasterController instance in PlayerCharacterMasterController.instances) {
                if (!instance.master.IsDeadAndOutOfLivesServer()) {
                    alivePlayers++;
                    alivePlayer = instance;
                }
            }
            if (alivePlayers == 1) {
                if (timesLastStandingDict.ContainsKey(alivePlayer)) { timesLastStandingDict[alivePlayer]++; } else { timesLastStandingDict.Add(alivePlayer, 1); }
            }
            orig(self, damageReport, victimNetworkUser);
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("timesLastStanding", timesLastStandingDict);
        }

        public override void Deserialize(Dictionary<string, object> restored) {
            if (restored.ReportContainsKey("timesLastStanding")) {
                timesLastStandingDict = (Dictionary<PlayerCharacterMasterController, uint>)restored["timesLastStanding"];
            }
        }
    }
}
