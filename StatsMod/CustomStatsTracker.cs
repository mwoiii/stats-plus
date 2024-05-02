using RoR2;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using R2API.Utils;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.ComponentModel;

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
        private static Dictionary<PlayerCharacterMasterController, uint> timesLastStanding = [];  // How many times a player has been the last man standing before the end of the tp event]
        private static Dictionary<PlayerCharacterMasterController, uint> currentItemLead = [];  // The current item lead of a player: 0 if not in the lead, >0 otherwise
        private static Dictionary<PlayerCharacterMasterController, uint> nonScrapPrinted = [];  // The amount of items used in printers and soups that were not scrap
        private static Dictionary<PlayerCharacterMasterController, uint> chainableProcs = [];  // The total number of times a player proc'd a vanilla item that can contribute to a proc chain 

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
            SceneExitController.onBeginExit += ItemLeadTrack;
            IL.RoR2.PurchaseInteraction.OnInteractionBegin += NonScrapTrack;
            IL.RoR2.GlobalEventManager.OnHitEnemy += chainableProcTrack;

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
            SceneExitController.onBeginExit -= ItemLeadTrack;
            IL.RoR2.PurchaseInteraction.OnInteractionBegin -= NonScrapTrack;
            IL.RoR2.GlobalEventManager.OnHitEnemy -= chainableProcTrack;

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
            currentItemLead = [];
            nonScrapPrinted = [];
            chainableProcs = [];

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

                    case "itemLead":
                        return currentItemLead[player];

                    case "nonScrapPrinted":
                        return nonScrapPrinted[player];

                    case "chainableProcs":
                        return chainableProcs[player];

                    default:
                        Log.Error("Cannot find specified custom stat, returning 0");
                        return (uint)0;

                }
            }
            catch (KeyNotFoundException) { return (uint) 0; }  // If a player is not in a dict., then it is because that stat is 0
            // ^ Set to uint for now because type casting is weird. Could specify type for each one with ContainsKey, or find a better way to do this..?
        }

        private static bool IsSafe()
        {
            bool voidLocusSafe = (VoidStageMissionController.instance?.numBatteriesActivated >= VoidStageMissionController.instance?.numBatteriesSpawned) && VoidStageMissionController.instance?.numBatteriesSpawned > 0;
            return TeleporterInteraction.instance?.isCharged ?? ArenaMissionController.instance?.clearedEffect.activeSelf ?? voidLocusSafe;
        }

        private static void chainableProcTrack(ILContext il)
        {
            var procDelegate = delegate (DamageInfo damageInfo)
            {
                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody?.isPlayerControlled ?? false)
                {
                    try
                    {
                        var player = attackerBody.master.GetComponent<PlayerCharacterMasterController>();
                        if (chainableProcs.ContainsKey(player)) { chainableProcs[player]++; }
                        else { chainableProcs.Add(player, 1); }
                    }
                    catch (Exception e) { Log.Error(e); }
                }
            };

            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdloc(1),
                x => x.MatchCallOrCallvirt<CharacterBody>("get_corePosition"),
                x => x.MatchLdloc(1),
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<DamageInfo>("procChainMask"),
                x => x.MatchLdarg(2),
                x => x.MatchLdloc(34),
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<DamageInfo>("crit"),
                x => x.MatchLdsfld(typeof(GlobalEventManager.CommonAssets), "missilePrefab"),
                x => x.MatchLdcI4(3),
                x => x.MatchLdcI4(1),
                x => x.MatchCallOrCallvirt(typeof(MissileUtils), "FireMissile")
                );
            c.Index += 13;
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate(procDelegate);
            /*
            ...
            float missileDamage = Util.OnHitProcDamage(damageInfo.damage, component2.damage, damageCoefficient);    
            MissileUtils.FireMissile(component2.corePosition, component2, damageInfo.procChainMask, victim, missileDamage, damageInfo.crit, CommonAssets.missilePrefab, DamageColorIndex.Item, addMissileProc: true); <-- matching
            <delegate here>
            ...
            */

            c.GotoNext(
                x => x.MatchLdloc(54),
                x => x.MatchCallOrCallvirt<UnityEngine.Object>("op_Implicit"),
                x => x.MatchBrfalse(out var _)
                );
            c.Index += 3;
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate(procDelegate);
            /*
            ...
			HurtBox hurtBox2 = lightningOrb2.PickNextTarget(damageInfo.position);
			if ((bool)hurtBox2) <-- matching
			{
                <delegate here>
				lightningOrb2.target = hurtBox2;
            ...
            */

            c.GotoNext(
                x => x.MatchNewobj("RoR2.Orbs.VoidLightningOrb"),
                x => x.MatchStloc(59)
                );
            c.Index += 2;
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate(procDelegate);
            /*
            ...
			float damageValue4 = Util.OnHitProcDamage(damageInfo.damage, component2.damage, damageCoefficient5);
			VoidLightningOrb voidLightningOrb = new VoidLightningOrb(); <-- matching
            <delegate here>
			voidLightningOrb.origin = damageInfo.position;
            ...
            */

            c.GotoNext(
                x => x.MatchNewobj(typeof(List<HealthComponent>)),
                x => x.MatchDup(),
                x => x.MatchLdarg(2),
                x => x.MatchCallOrCallvirt<GameObject>("GetComponent"),
                x => x.MatchCallOrCallvirt(typeof(List<HealthComponent>).GetMethod("Add")),
                x => x.MatchStloc(63)
                );
            c.Index += 6;
            var front = c.Index;
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate(procDelegate);
            var end = c.MarkLabel();
            c.Index = front;
            c.Emit(OpCodes.Ldloc, 62);
            c.EmitDelegate<Func<List<HurtBox>, bool>>((list) => { return list.Count > 0; });
            c.Emit(OpCodes.Brtrue, end);
            /*
            ...
			CollectionPool<HealthComponent, List<HealthComponent>>.ReturnCollection(list2);
			List<HealthComponent> bouncedObjects = new List<HealthComponent> { victim.GetComponent<HealthComponent>() }; <-- matching
            <delegate here>
			float damageCoefficient6 = 1f;
            ...
            */

            c.GotoNext(
                x => x.MatchLdloc(14),
                x => x.MatchLdcI4(0),
                x => x.MatchBle(out var _),
                x => x.MatchLdcR4(5),
                x => x.MatchLdloc(14),
                x => x.MatchConvR4(),
                x => x.MatchMul(),
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<DamageInfo>("procCoefficient"),
                x => x.MatchMul(),
                x => x.MatchLdloc(4),
                x => x.MatchCallOrCallvirt(typeof(Util), "CheckRoll"),
                x => x.MatchBrfalse(out var _)
                );
            c.Index += 13;
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate(procDelegate);
            /*
            ...
		    int itemCount10 = inventory.GetItemCount(RoR2Content.Items.StickyBomb);
		    if (itemCount10 > 0 && Util.CheckRoll(5f * (float)itemCount10 * damageInfo.procCoefficient, master) && (bool)characterBody) <-- matching
		    {
                <delegate here>
			    bool alive = characterBody.healthComponent.alive;
            ...
            */

            c.GotoNext(
                x => x.MatchLdstr("Prefabs/Effects/MuzzleFlashes/MuzzleflashFireMeatBall"),
                x => x.MatchCallOrCallvirt(typeof(LegacyResourcesAPI), "Load"),        
                x => x.MatchLdloc(116),
                x => x.MatchLdcI4(1),
                x => x.MatchCallOrCallvirt(typeof(EffectManager), "SpawnEffect")
                );
            c.Index += 5;
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate(procDelegate);
            /*
            ...
			float num12 = 20f;
			if (Util.CheckRoll(10f * damageInfo.procCoefficient, master)) <-- matching
			{
                <delegate here>
				EffectData effectData = new EffectData
            ...
            */

            c.GotoNext(
                x => x.MatchLdloc(18),
                x => x.MatchLdcI4(0),
                x => x.MatchBle(out var _),
                x => x.MatchLdarg(1),
                x => x.MatchLdflda<DamageInfo>("procChainMask"),
                x => x.MatchLdcI4(18),
                x => x.MatchCallOrCallvirt<ProcChainMask>("HasProc"),
                x => x.MatchBrtrue(out var _),
                x => x.MatchLdcR4(10),
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<DamageInfo>("procCoefficient"),
                x => x.MatchMul(),
                x => x.MatchLdloc(4),
                x => x.MatchCallOrCallvirt(typeof(Util), "CheckRoll"),
                x => x.MatchBrfalse(out var _)
                );
            c.Index += 15;
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate(procDelegate);
            /*
            ...
		    int itemCount16 = inventory.GetItemCount(RoR2Content.Items.LightningStrikeOnHit);
		    if (itemCount16 > 0 && !damageInfo.procChainMask.HasProc(ProcType.LightningStrikeOnHit) && Util.CheckRoll(10f * damageInfo.procCoefficient, master)) <-- matching
		    {
                <delegate here>
			    float damageValue6 = Util.OnHitProcDamage(damageInfo.damage, component2.damage, 5f * (float)itemCount16);
            ...
            */
        }

        private static void NonScrapTrack(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdloc(6),
                x => x.MatchLdloc(1),
                x => x.MatchBeq(out var _)
                );
            c.Index += 3;
            c.Emit(OpCodes.Ldloc_0);
            c.Emit(OpCodes.Ldloc, 6);
            c.EmitDelegate<Action<CharacterBody, ItemIndex>>((interactorBody, item) =>
            {
                ItemIndex[] itemBlacklist = 
                [
                    (ItemIndex)136, // RegeneratingScrap
                    (ItemIndex)140, // ScrapGreen
                    (ItemIndex)142, // ScrapRed
                    (ItemIndex)144, // ScrapWhite
                    (ItemIndex)146  // ScrapYellow
                ];
                if (!itemBlacklist.Contains(item))
                {
                    var player = interactorBody.master.playerCharacterMasterController;
                    if (nonScrapPrinted.ContainsKey(player)) { nonScrapPrinted[player]++; }
                    else { nonScrapPrinted.Add(player, 1); }
                }
            });
            /*
            CreateItemTakenOrb(component.corePosition, base.gameObject, item);
            if (item != itemIndex) <-- matching
            {
                < delegate emitted here >
                PurchaseInteraction.onItemSpentOnPurchase?.Invoke(this, activator);
            }
            */
        }

        private static void ItemLeadTrack(SceneExitController sceneExitController)
        {
            if (PlayerCharacterMasterController.instances.Count < 2) { return; }

            currentItemLead = currentItemLead.Keys.ToDictionary(key => key, val => (uint)0);  // Set all values to 0

            uint highestLead = 0;
            uint highestItems = 0;
            PlayerCharacterMasterController leadingPlayer = null;
            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
            {
                #pragma warning disable Publicizer001
                uint playerItems = (uint) player.master.inventory.itemStacks.Sum();
                #pragma warning restore Publicizer001
                if (playerItems > highestItems) 
                {
                    highestLead = playerItems - highestItems;
                    highestItems = playerItems;
                    leadingPlayer = player;
                }
                else if (highestItems - playerItems < highestLead) { highestLead = highestItems - playerItems; }
            }
            if (currentItemLead.ContainsKey(leadingPlayer)) { currentItemLead[leadingPlayer] = highestLead; }
            else { currentItemLead.Add(leadingPlayer, highestLead); }
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
