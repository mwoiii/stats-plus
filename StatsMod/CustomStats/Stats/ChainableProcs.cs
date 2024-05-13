using RoR2;
using System.Collections.Generic;
using System;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace StatsMod.CustomStats
{
    internal class ChainableProcs : Stat
    {
        private static Dictionary<PlayerCharacterMasterController, uint> chainableProcsDict = [];  // The total number of times a player proc'd a vanilla item that can contribute to a proc chain
        new public static void Init()
        {
            IL.RoR2.GlobalEventManager.OnHitEnemy += chainableProcTrack;

            Tracker.statsTable.Add("chainableProcs", chainableProcsDict);
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
                        if (chainableProcsDict.ContainsKey(player)) { chainableProcsDict[player]++; }
                        else { chainableProcsDict.Add(player, 1); }
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

            // sticky bomb has no proc coeff
            /*
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
                x => x.MatchCallOrCallvirt(typeof(RoR2.Util), "CheckRoll"),
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
    }
}