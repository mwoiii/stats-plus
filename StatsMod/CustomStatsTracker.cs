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

namespace StatsMod
{
    public static class CustomStatsTracker
    {
        // This class implements and holds the values of the custom stats
        private static Dictionary<PlayerCharacterMasterController, uint> shrinePurchases = [];  // Dictionary for recording how many times each player has used a shrine of chance
        private static Dictionary<PlayerCharacterMasterController, uint> shrineWins = [];  // Dictionary for recording how many times each player has won a shrine of chance
        private static Dictionary<PlayerCharacterMasterController, uint> orderHits = [];  // Dictionary for recording how many times each player has used a shrine of order
        private static Dictionary<PlayerCharacterMasterController, float> timeStill = [];  // Dictionary for recording how long each player has been standing still
        private static Dictionary<PlayerCharacterMasterController, float> timeStillUnsafe = [];  // Dictionary for recording how long each player has been standing still in conditions that are considered unsafe
        private static Dictionary<PlayerCharacterMasterController, float> timeLowHealth = [];  // Dictionary for recording how long each player has been below 25% health
        private static Dictionary<PlayerCharacterMasterController, float> fallDamage = [];  // Dictionary for recording how much fall damage each player has taken
        private static Dictionary<PlayerCharacterMasterController, uint> coinsSpent = [];  // Dictionary for recording how many lunar coins each player has spent this run

        public static void Enable()
        {
            ShrineChanceBehavior.onShrineChancePurchaseGlobal += ShrineTrack;
            On.RoR2.ShrineRestackBehavior.AddShrineStack += OrderTrack;
            On.RoR2.Run.OnFixedUpdate += StillTrack;
            On.RoR2.Run.OnFixedUpdate += LowHealthTrack;
            On.RoR2.CharacterBody.OnTakeDamageServer += FallDamageTrack;
            On.RoR2.NetworkUser.DeductLunarCoins += CoinsTrack;
        }

        public static void Disable()
        {
            ShrineChanceBehavior.onShrineChancePurchaseGlobal -= ShrineTrack;
            On.RoR2.ShrineRestackBehavior.AddShrineStack -= OrderTrack;
            On.RoR2.Run.OnFixedUpdate -= StillTrack;
            On.RoR2.Run.OnFixedUpdate -= LowHealthTrack;
            On.RoR2.CharacterBody.OnTakeDamageServer -= FallDamageTrack;
            On.RoR2.NetworkUser.DeductLunarCoins -= CoinsTrack;
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

                    default:
                        Log.Error("Cannot find specified custom stat, returning 0");
                        return 0;

                }
            }
            catch (KeyNotFoundException) { return 0; }  // If a player is not in a dict., then it is because that stat is 0
        }


        private static void ShrineTrack(bool failed, Interactor activator)
        {
            var player = activator.GetComponent<CharacterBody>().master.playerCharacterMasterController;  // Getting the networkUser (unique identification in multiplayer), calling it player
            try
            {
                shrinePurchases[player]++;
                if (!failed) { shrineWins[player]++; }
            }
            catch (KeyNotFoundException) {
                shrinePurchases.Add(player, 1);
                if (!failed) { shrineWins.Add(player, 1); }
                else { shrineWins.Add(player, 0); }
            }
        }

        // Counting how many times a player has hit a shrine of order
        private static void OrderTrack(On.RoR2.ShrineRestackBehavior.orig_AddShrineStack orig, ShrineRestackBehavior self, Interactor interactor)
        {
            var player = interactor.GetComponent<CharacterBody>().master.playerCharacterMasterController;
            try { orderHits[player]++; }
            catch (KeyNotFoundException) { orderHits.Add(player, 1); }
            orig(self, interactor);
        }

        private static void CoinsTrack(On.RoR2.NetworkUser.orig_DeductLunarCoins orig, NetworkUser self, uint count)
        {
            var player = self.masterController;
            try { coinsSpent[player] += count; }
            catch (KeyNotFoundException) { coinsSpent.Add(player, count); }
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
                try { fallDamage[player] += damageReport.damageDealt; }
                catch (KeyNotFoundException) { fallDamage.Add(player, damageReport.damageDealt); }
            }
            orig(self, damageReport);
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


        // Counting how long a player has stopped moving for
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
                        var voidLocusSafe = (VoidStageMissionController.instance?.numBatteriesActivated >= VoidStageMissionController.instance?.numBatteriesSpawned) && VoidStageMissionController.instance?.numBatteriesSpawned > 0;
                        var isSafe = TeleporterInteraction.instance?.isCharged ?? ArenaMissionController.instance?.clearedEffect.activeSelf ?? voidLocusSafe;
                        if (isStill)
                        {
                            try
                            {
                                timeStill[player] += Time.fixedDeltaTime;
                                if (!isSafe) { timeStillUnsafe[player] += Time.fixedDeltaTime; }
                            }
                            catch (KeyNotFoundException) {
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
    }
}
