using RoR2;
using RoR2.Stats;
using BepInEx;
using R2API;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine.SceneManagement;

namespace StatsMod
{
    public static class CustomStatsTracker
    {
        // This class implements and holds the values of the custom stats, listed below
        private static Dictionary<PlayerCharacterMasterController, uint> shrinePurchases = [];  // How many times each player has used a shrine of chance
        private static Dictionary<PlayerCharacterMasterController, uint> shrineWins = [];  // How many times each player has won a shrine of chance
        private static Dictionary<PlayerCharacterMasterController, uint> orderHits = [];  // How many times each player has used a shrine of order
        private static Dictionary<PlayerCharacterMasterController, float> timeStill = [];  // How long each player has been standing still
        private static Dictionary<PlayerCharacterMasterController, float> timeStillUnsafe = [];  // How long each player has been standing still in conditions that are considered unsafe
        private static Dictionary<PlayerCharacterMasterController, float> timeLowHealth = [];  // How long each player has been below 25% health
        private static Dictionary<PlayerCharacterMasterController, float> fallDamage = [];  // How much fall damage each player has taken
        private static Dictionary<PlayerCharacterMasterController, uint> coinsSpent = [];  // How many lunar coins each player has spent this run
        private static Dictionary<PlayerCharacterMasterController, uint> avenges = [];  // How many times a player has avenged another (killing an enemy that hurt another player)
        private static Dictionary<PlayerCharacterMasterController, uint> timesLastStanding = [];  // How many times a player has been the last man standing before the end of the tp event

        // Data structures supporting actual stats
        private static Dictionary<CharacterMaster, List<PlayerCharacterMasterController>> avengeHitList = [];  // Dictionary for recording which enemies are avenge targets, and which players they've hit

        public static void Enable()
        {
            ShrineChanceBehavior.onShrineChancePurchaseGlobal += ShrineTrack;
            On.RoR2.ShrineRestackBehavior.AddShrineStack += OrderTrack;
            On.RoR2.Run.OnFixedUpdate += StillTrack;
            On.RoR2.Run.OnFixedUpdate += LowHealthTrack;
            On.RoR2.CharacterBody.OnTakeDamageServer += FallDamageTrack;
            On.RoR2.NetworkUser.DeductLunarCoins += CoinsTrack;
            GlobalEventManager.onCharacterDeathGlobal += AvengesTrack;
            On.RoR2.GlobalEventManager.OnPlayerCharacterDeath += LastStandingTrack;

            On.RoR2.DamageReport.ctor += RecordHitList;
            On.RoR2.Run.BeginStage += ClearHitList;
        }

        public static void Disable()
        {
            ShrineChanceBehavior.onShrineChancePurchaseGlobal -= ShrineTrack;
            On.RoR2.ShrineRestackBehavior.AddShrineStack -= OrderTrack;
            On.RoR2.Run.OnFixedUpdate -= StillTrack;
            On.RoR2.Run.OnFixedUpdate -= LowHealthTrack;
            On.RoR2.CharacterBody.OnTakeDamageServer -= FallDamageTrack;
            On.RoR2.NetworkUser.DeductLunarCoins -= CoinsTrack;
            GlobalEventManager.onCharacterDeathGlobal -= AvengesTrack;
            On.RoR2.GlobalEventManager.OnPlayerCharacterDeath -= LastStandingTrack;

            On.RoR2.DamageReport.ctor -= RecordHitList;
            On.RoR2.Run.BeginStage -= ClearHitList;
        }

        public static void ResetData()
        {
            shrinePurchases = [];
            shrineWins = [];
            orderHits = [];
            timeStill = [];
            timeStillUnsafe = [];
            timeLowHealth = [];
            fallDamage = [];
            coinsSpent = [];
            avenges = [];
            timesLastStanding = [];

            avengeHitList = [];
        }

        public static object GetStat(PlayerCharacterMasterController player, string statName)
        {
            try
            {
                switch (statName)
                {

                    case "shrinePurchases":
                        return shrinePurchases[player];

                    case "shrineWins":
                        return shrineWins[player];

                    case "orderHits":
                        return orderHits[player];

                    case "timeStill":
                        return timeStill[player];

                    case "timeStillUnsafe":
                        return timeStillUnsafe[player];

                    case "timeLowHealth":
                        return timeLowHealth[player];

                    case "fallDamage":
                        return fallDamage[player];

                    case "coinsSpent":
                        return coinsSpent[player];

                    case "avenges":
                        return avenges[player];

                    case "timesLastStanding":
                        return timesLastStanding[player];

                    default:
                        Log.Error("Cannot find specified custom stat, returning 0");
                        return (uint)0;

                }
            }
            catch (KeyNotFoundException) { return (uint)0; }  // If a player is not in a dict., then it is because that stat is 0
            // ^ Set to uint for now because type casting is weird. Could specify type for each one with ContainsKey, or find a better way to do this..?
        }

        private static bool IsSafe()
        {
            bool voidLocusSafe = (VoidStageMissionController.instance?.numBatteriesActivated >= VoidStageMissionController.instance?.numBatteriesSpawned) && VoidStageMissionController.instance?.numBatteriesSpawned > 0;
            return TeleporterInteraction.instance?.isCharged ?? ArenaMissionController.instance?.clearedEffect.activeSelf ?? voidLocusSafe;
        }

        private static void ShrineTrack(bool failed, Interactor activator)
        {
            var player = activator.GetComponent<CharacterBody>().master.playerCharacterMasterController;
            if (shrinePurchases.ContainsKey(player))
            {
                shrinePurchases[player]++;
                if (!failed) { shrineWins[player]++; }
            }
            else {
                shrinePurchases.Add(player, 1);
                if (!failed) { shrineWins.Add(player, 1); }
                else { shrineWins.Add(player, 0); }
            }
        }

        private static void OrderTrack(On.RoR2.ShrineRestackBehavior.orig_AddShrineStack orig, ShrineRestackBehavior self, Interactor interactor)
        {
            var player = interactor.GetComponent<CharacterBody>().master.playerCharacterMasterController;
            if (orderHits.ContainsKey(player)) { orderHits[player]++; }
            else { orderHits.Add(player, 1); }
            orig(self, interactor);
        }

        private static void CoinsTrack(On.RoR2.NetworkUser.orig_DeductLunarCoins orig, NetworkUser self, uint count)
        {
            var player = self.masterController;
            if (coinsSpent.ContainsKey(player)) { coinsSpent[player] += count; }
            else { coinsSpent.Add(player, count); }
            orig(self, count);
        }

        private static void FallDamageTrack(On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, CharacterBody self, DamageReport damageReport)
        {
            bool isPlayerFall;
            try { isPlayerFall = damageReport.victimBody.isPlayerControlled && damageReport.isFallDamage; }
            catch (NullReferenceException) { return; } // Body no longer exists?

            if (isPlayerFall)
            {
                PlayerCharacterMasterController player = damageReport.victimMaster.GetComponent<PlayerCharacterMasterController>();
                if (fallDamage.ContainsKey(player)) { fallDamage[player] += damageReport.damageDealt; }
                else { fallDamage.Add(player, damageReport.damageDealt); }
            }
            orig(self, damageReport);
        }

        private static void RecordHitList(On.RoR2.DamageReport.orig_ctor orig, DamageReport self, DamageInfo damageInfo, HealthComponent victim, float damageDealt, float combinedHealthBeforeDamage)
        {
            orig(self, damageInfo, victim, damageDealt, combinedHealthBeforeDamage);

            CharacterBody victimBody = (victim ? victim.body : null);
            CharacterBody attackerBody = (damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null);
            CharacterMaster attackerMaster = attackerBody?.master;
            try
            {
                if ((victimBody?.isPlayerControlled ?? false) && ((!attackerBody?.isChampion) ?? false))  // Given a victim and attacker exist, is the victim a player and the attacker not a tp boss?
                {
                    PlayerCharacterMasterController victimController = victimBody.master.GetComponent<PlayerCharacterMasterController>();
                    if (avengeHitList.ContainsKey(attackerMaster))
                    {
                        if (!avengeHitList[attackerMaster].Contains(victimController)) { avengeHitList[attackerMaster].Add(victimController); }
                    }
                    else { avengeHitList.Add(attackerMaster, [victimController]); }
                }
            }
            catch (ArgumentNullException) { }  // This can happen if the CharacterBody of the attacker is long-gone, like with the Glacial explosions. Because of reflection (or something), value isn't null
            // ^ so, attackerBody itself isn't null, but what it refers to *is* null, I think, which sucks
        }

        private static void ClearHitList(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            avengeHitList = [];
            orig(self);
        }

        private static void AvengesTrack(DamageReport damageReport)
        {
            CharacterMaster victimMaster = damageReport.victimMaster;
            if (victimMaster == null) { return; }
            if (avengeHitList.ContainsKey(victimMaster) && (damageReport.attackerBody?.isPlayerControlled ?? false))
            {
                PlayerCharacterMasterController attackerController = damageReport.attackerMaster.GetComponent<PlayerCharacterMasterController>();
                if (avengeHitList[victimMaster].Count > 1 || avengeHitList[victimMaster][0] != attackerController)
                {
                    if (avenges.ContainsKey(attackerController)) { avenges[attackerController]++; }
                    else { avenges.Add(attackerController, 1); }
                }
                avengeHitList.Remove(victimMaster);
            }
        }

        private static void StillTrack(On.RoR2.Run.orig_OnFixedUpdate orig, Run self)
        {
            if (NetworkServer.active && (PlayerCharacterMasterController.instances.Count > 0))  // These checks may not be necessary but I am too lazy to confirm, it works at least
            {
                foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                {
                    if (!Run.instance.isRunStopwatchPaused)
                    {
                        bool isStill = false;
                        try { isStill = player.master.GetBody().GetNotMoving(); }
                        catch (NullReferenceException) { continue; }  // Player may be dead, or not properly spawned yet
                        bool isSafe = IsSafe();
                        if (isStill)
                        {
                            if (timeStill.ContainsKey(player))
                            {
                                timeStill[player] += Time.fixedDeltaTime;
                                if (!isSafe) { timeStillUnsafe[player] += Time.fixedDeltaTime; }
                            }
                            else
                            {
                                timeStill.Add(player, Time.fixedDeltaTime);
                                if (!isSafe) { timeStillUnsafe.Add(player, Time.fixedDeltaTime); }
                                else { timeStillUnsafe.Add(player, 0); }
                            }
                        }
                    }
                }
            }
            orig(self);
        }

        private static void LastStandingTrack(On.RoR2.GlobalEventManager.orig_OnPlayerCharacterDeath orig, GlobalEventManager self, DamageReport damageReport, NetworkUser victimNetworkUser)
        {
            if (IsSafe() || SceneManager.GetActiveScene().name == "bazaar") { return; }
            int alivePlayers = 0;
            PlayerCharacterMasterController alivePlayer = null;
            foreach (PlayerCharacterMasterController instance in PlayerCharacterMasterController.instances)
            {
                if (!instance.master.IsDeadAndOutOfLivesServer())
                {
                    alivePlayers++;
                    alivePlayer = instance;
                }
            }
            if (alivePlayers == 1) 
            {
                if (timesLastStanding.ContainsKey(alivePlayer)) { timesLastStanding[alivePlayer]++; }
                else { timesLastStanding.Add(alivePlayer, 1); }
            }
            orig(self, damageReport, victimNetworkUser);
        }

        // Lots of this code is largely redundant as it can be implemented in StillTrack, but for clarity it's nice in a separate method. Consider revising if change in naming convention
        private static void LowHealthTrack(On.RoR2.Run.orig_OnFixedUpdate orig, Run self)
        {
            if (NetworkServer.active && (PlayerCharacterMasterController.instances.Count > 0))  // These checks may not be necessary but I am too lazy to confirm, it works at least
            {
                foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                {
                    try
                    {
                        if (player.master.GetBody().healthComponent.isHealthLow)
                        {
                            try { timeLowHealth[player] += Time.fixedDeltaTime; }
                            catch (KeyNotFoundException) { timeLowHealth.Add(player, Time.fixedDeltaTime); }
                        }
                    }
                    catch (NullReferenceException) { continue; }  // Player may be dead, or not properly spawned yet
                }
            }
            orig(self);
        }

    }
}
