using RoR2;
using System.Collections.Generic;
using System;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod;
using UnityEngine.Networking;
using UnityEngine;

namespace StatsMod.CustomStats
{
    internal class TimeStill : Stat
    {
        private static Dictionary<PlayerCharacterMasterController, float> timeStillDict = [];  // How long each player has been standing still
        private static Dictionary<PlayerCharacterMasterController, float> timeStillUnsafeDict = [];  // How long each player has been standing still in conditions that are considered unsafe
        new public static void Init()
        {
            On.RoR2.Run.OnFixedUpdate += StillTrack;

            Tracker.statsTable.Add("timeStill", timeStillDict);
            Tracker.statsTable.Add("timeStillUnsafe", timeStillUnsafeDict);
        }

        private static void StillTrack(On.RoR2.Run.orig_OnFixedUpdate orig, Run self)
        {
            if (NetworkServer.active && PlayerCharacterMasterController.instances.Count > 0)  // These checks may not be necessary but it is safe
            {
                foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                {
                    if (!Run.instance.isRunStopwatchPaused)
                    {
                        bool isStill = false;
                        try { isStill = player.master.GetBody().GetNotMoving(); }
                        catch (NullReferenceException) { continue; }  // Player may be dead, or not properly spawned yet
                        bool isSafe = Util.IsSafe();
                        if (isStill)
                        {
                            if (timeStillDict.ContainsKey(player))
                            {
                                timeStillDict[player] += Time.fixedDeltaTime;
                                if (!isSafe) { timeStillUnsafeDict[player] += Time.fixedDeltaTime; }
                            }
                            else
                            {
                                timeStillDict.Add(player, Time.fixedDeltaTime);
                                if (!isSafe) { timeStillUnsafeDict.Add(player, Time.fixedDeltaTime); }
                                else { timeStillUnsafeDict.Add(player, 0); }
                            }
                        }
                    }
                }
            }
            orig(self);
        }
    }
}
