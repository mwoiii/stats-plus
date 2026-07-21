using Newtonsoft.Json.Linq;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace StatsMod.CustomStats {
    internal class TimeLowHealth : BaseCustomStat {
        private static Dictionary<string, float> timeLowHealthDict = [];  // How long each player has been below 25% health

        public override void Init() {
            base.Init();
            On.RoR2.Run.OnFixedUpdate += LowHealthTrack;
        }

        private static void LowHealthTrack(On.RoR2.Run.orig_OnFixedUpdate orig, Run self) {
            if (NetworkServer.active && PlayerCharacterMasterController.instances.Count > 0)  // These checks may not be necessary but I am too lazy to confirm, it works at least
            {
                foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances) {
                    try {
                        if (player.master.GetBody().healthComponent.isHealthLow) {
                            string playerName = RecordHandler.masterControllerToName[player];
                            try { timeLowHealthDict[playerName] += Time.fixedDeltaTime; } catch (KeyNotFoundException) { timeLowHealthDict.Add(playerName, Time.fixedDeltaTime); }
                        }
                    } catch (NullReferenceException) { continue; }  // Player may be dead, or not properly spawned yet
                }
            }
            orig(self);
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("timeLowHealth", timeLowHealthDict);
        }

        public override void Deserialize(Dictionary<string, JToken> restored) {
            if (restored.CanDeserialize("timeLowHealth")) {
                timeLowHealthDict = restored["timeLowHealth"].ToObject<Dictionary<string, float>>();
            }
        }
    }
}
