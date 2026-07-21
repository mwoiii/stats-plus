using Newtonsoft.Json.Linq;
using RoR2;
using System;
using System.Collections.Generic;

namespace StatsMod.CustomStats {
    internal class Avenges : BaseCustomStat {
        private static Dictionary<string, uint> avengesDict = [];  // How many times a player has avenged another (killing an enemy that hurt another player)
        private static Dictionary<CharacterMaster, List<PlayerCharacterMasterController>> avengeHitList = [];  // Dictionary for recording which enemies are avenge targets, and which players they've hit

        public override void Init() {
            base.Init();
            GlobalEventManager.onCharacterDeathGlobal += AvengesTrack;
            On.RoR2.DamageReport.ctor += RecordHitList;
            On.RoR2.Run.BeginStage += ClearDicts;
        }

        private static void RecordHitList(On.RoR2.DamageReport.orig_ctor orig, DamageReport self, DamageInfo damageInfo, HealthComponent victim, float damageDealt, float combinedHealthBeforeDamage) {
            orig(self, damageInfo, victim, damageDealt, combinedHealthBeforeDamage);

            CharacterBody victimBody = victim ? victim.body : null;
            CharacterBody attackerBody = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
            CharacterMaster attackerMaster = attackerBody?.master;
            try {
                if ((victimBody?.isPlayerControlled ?? false) && (!attackerBody?.isChampion ?? false))  // Given a victim and attacker exist, is the victim a player and the attacker not a tp boss?
                {
                    PlayerCharacterMasterController victimController = victimBody.master.GetComponent<PlayerCharacterMasterController>();
                    if (avengeHitList.ContainsKey(attackerMaster)) {
                        if (!avengeHitList[attackerMaster].Contains(victimController)) { avengeHitList[attackerMaster].Add(victimController); }
                    } else { avengeHitList.Add(attackerMaster, [victimController]); }
                }
            } catch (ArgumentNullException) { }  // This can happen if the CharacterBody of the attacker is long-gone, like with the Glacial explosions. Because of reflection (or something), value isn't null
            // ^ so, attackerBody itself isn't null, but what it refers to *is* null, I think, which sucks
        }

        private static void AvengesTrack(DamageReport damageReport) {
            CharacterMaster victimMaster = damageReport.victimMaster;
            if (victimMaster == null) { return; }
            if (avengeHitList.ContainsKey(victimMaster) && (damageReport.attackerBody?.isPlayerControlled ?? false)) {
                PlayerCharacterMasterController attackerController = damageReport.attackerMaster.GetComponent<PlayerCharacterMasterController>();
                if (avengeHitList[victimMaster].Count > 1 || avengeHitList[victimMaster][0] != attackerController) {
                    string playerName = RecordHandler.masterControllerToName[attackerController];
                    if (avengesDict.ContainsKey(playerName)) { avengesDict[playerName]++; } else { avengesDict.Add(playerName, 1); }
                }
                avengeHitList.Remove(victimMaster);
            }
        }

        private static void ClearDicts(On.RoR2.Run.orig_BeginStage orig, Run self) {
            avengeHitList = [];
            orig(self);
        }

        public override void ConfigureStatsTable() {
            CustomStatTracker.statsTable.Add("avenges", avengesDict);
        }

        public override void Deserialize(Dictionary<string, JToken> restored) {
            if (restored.CanDeserialize("avenges")) {
                avengesDict = restored["avenges"].ToObject<Dictionary<string, uint>>();
            }
        }
    }
}
