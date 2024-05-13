using RoR2;
using System.Collections.Generic;
using System;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StatsMod.CustomStats
{
    internal class TimesLastStanding : Stat
    {
        private static Dictionary<PlayerCharacterMasterController, uint> timesLastStandingDict = [];  // How many times a player has been the last man standing before the end of the tp event]

        new public static void Init()
        {
            On.RoR2.GlobalEventManager.OnPlayerCharacterDeath += LastStandingTrack;

            Tracker.statsTable.Add("timesLastStanding", timesLastStandingDict);
        }

        private static void LastStandingTrack(On.RoR2.GlobalEventManager.orig_OnPlayerCharacterDeath orig, GlobalEventManager self, DamageReport damageReport, NetworkUser victimNetworkUser)
        {
            if (Util.IsSafe() || SceneManager.GetActiveScene().name == "bazaar") { return; }
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
                if (timesLastStandingDict.ContainsKey(alivePlayer)) { timesLastStandingDict[alivePlayer]++; }
                else { timesLastStandingDict.Add(alivePlayer, 1); }
            }
            orig(self, damageReport, victimNetworkUser);
        }
    }
}
