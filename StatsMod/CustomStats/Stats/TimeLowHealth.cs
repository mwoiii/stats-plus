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
    internal class TimeLowHealth : Stat
    {
        private static Dictionary<PlayerCharacterMasterController, float> timeLowHealthDict = [];  // How long each player has been below 25% health

        new public static void Init()
        {
            On.RoR2.Run.OnFixedUpdate += LowHealthTrack;

            Tracker.statsTable.Add("timeLowHealth", timeLowHealthDict);
        }

        private static void LowHealthTrack(On.RoR2.Run.orig_OnFixedUpdate orig, Run self)
        {
            if (NetworkServer.active && PlayerCharacterMasterController.instances.Count > 0)  // These checks may not be necessary but I am too lazy to confirm, it works at least
            {
                foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                {
                    try
                    {
                        if (player.master.GetBody().healthComponent.isHealthLow)
                        {
                            try { timeLowHealthDict[player] += Time.fixedDeltaTime; }
                            catch (KeyNotFoundException) { timeLowHealthDict.Add(player, Time.fixedDeltaTime); }
                        }
                    }
                    catch (NullReferenceException) { continue; }  // Player may be dead, or not properly spawned yet
                }
            }
            orig(self);
        }
    }
}
