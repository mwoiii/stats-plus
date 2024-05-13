using RoR2;
using System.Collections.Generic;
using System;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine.Networking;
using UnityEngine;

namespace StatsMod.CustomStats
{
    internal class FallDamage : Stat
    {
        private static Dictionary<PlayerCharacterMasterController, float> fallDamageDict = [];  // How much fall damage each player has taken

        new public static void Init()
        {
            On.RoR2.CharacterBody.OnTakeDamageServer += FallDamageTrack;

            Tracker.statsTable.Add("fallDamage", fallDamageDict);
        }

        private static void FallDamageTrack(On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, CharacterBody self, DamageReport damageReport)
        {
            bool isPlayerFall;
            try { isPlayerFall = damageReport.victimBody.isPlayerControlled && damageReport.isFallDamage; }
            catch (NullReferenceException) { return; } // Body no longer exists?

            if (isPlayerFall)
            {
                PlayerCharacterMasterController player = damageReport.victimMaster.GetComponent<PlayerCharacterMasterController>();
                if (fallDamageDict.ContainsKey(player)) { fallDamageDict[player] += damageReport.damageDealt; }
                else { fallDamageDict.Add(player, damageReport.damageDealt); }
            }
            orig(self, damageReport);
        }
    }
}
