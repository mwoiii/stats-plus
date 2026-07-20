using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace StatsMod.CustomStats {
    internal class TimeStill : BaseCustomStat {
        private static Dictionary<PlayerCharacterMasterController, float> timeStillDict = [];  // How long each player has been standing still
        private static Dictionary<PlayerCharacterMasterController, float> timeStillUnsafeDict = [];  // How long each player has been standing still in conditions that are considered unsafe

        public override void Init() {
            base.Init();
            On.RoR2.Run.OnFixedUpdate += StillTrack;
        }

        private static void StillTrack(On.RoR2.Run.orig_OnFixedUpdate orig, Run self) {
            if (NetworkServer.active && PlayerCharacterMasterController.instances.Count > 0)  // These checks may not be necessary but it is safe
            {
                foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances) {
                    if (!Run.instance.isRunStopwatchPaused) {
                        bool isStill = false;
                        try { isStill = player.master.GetBody().GetNotMoving(); } catch (NullReferenceException) { continue; }  // Player may be dead, or not properly spawned yet
                        bool isSafe = CustomStatUtils.IsSafe();
                        if (isStill) {
                            if (timeStillDict.ContainsKey(player)) {
                                timeStillDict[player] += Time.fixedDeltaTime;
                                if (!isSafe) { timeStillUnsafeDict[player] += Time.fixedDeltaTime; }
                            } else {
                                timeStillDict.Add(player, Time.fixedDeltaTime);
                                if (!isSafe) { timeStillUnsafeDict.Add(player, Time.fixedDeltaTime); } else { timeStillUnsafeDict.Add(player, 0); }
                            }
                        }
                    }
                }
            }
            orig(self);
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("timeStill", timeStillDict);
            CustomStatTracker.statsTable.Add("timeStillUnsafe", timeStillUnsafeDict);
        }

        public override void Deserialize(Dictionary<string, object> restored) {
            if (restored.ReportContainsKey("timeStill")) {
                timeStillDict = (Dictionary<PlayerCharacterMasterController, float>)restored["timeStill"];
            }
            if (restored.ReportContainsKey("timeStillUnsafe")) {
                timeStillUnsafeDict = (Dictionary<PlayerCharacterMasterController, float>)restored["timeStillUnsafe"];
            }
        }
    }
}
