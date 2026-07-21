using Newtonsoft.Json.Linq;
using RoR2;
using System;
using System.Collections.Generic;

namespace StatsMod.CustomStats {
    internal class FallDamage : BaseCustomStat {
        private static Dictionary<string, float> fallDamageDict = [];  // How much fall damage each player has taken

        public override void Init() {
            base.Init();
            On.RoR2.CharacterBody.OnTakeDamageServer += FallDamageTrack;
        }

        private static void FallDamageTrack(On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, CharacterBody self, DamageReport damageReport) {
            bool isPlayerFall;
            try { isPlayerFall = damageReport.victimBody.isPlayerControlled && damageReport.isFallDamage; } catch (NullReferenceException) { return; } // Body no longer exists?

            if (isPlayerFall) {
                PlayerCharacterMasterController player = damageReport.victimMaster.GetComponent<PlayerCharacterMasterController>();
                string playerName = RecordHandler.masterControllerToName[player];
                if (fallDamageDict.ContainsKey(playerName)) { fallDamageDict[playerName] += damageReport.damageDealt; } else { fallDamageDict.Add(playerName, damageReport.damageDealt); }
            }
            orig(self, damageReport);
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("fallDamage", fallDamageDict);
        }

        public override void Deserialize(Dictionary<string, JToken> restored) {
            if (restored.CanDeserialize("fallDamage")) {
                fallDamageDict = restored["fallDamage"].ToObject<Dictionary<string, float>>();
            }
        }
    }
}
