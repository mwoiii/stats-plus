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
    internal class CoinsSpent : Stat
    {
        private static Dictionary<PlayerCharacterMasterController, uint> coinsSpentDict = [];  // How many lunar coins each player has spent this run

        new public static void Init()
        {
            On.RoR2.NetworkUser.DeductLunarCoins += CoinsTrack;

            Tracker.statsTable.Add("coinsSpent", coinsSpentDict);
        }

        private static void CoinsTrack(On.RoR2.NetworkUser.orig_DeductLunarCoins orig, NetworkUser self, uint count)
        {
            var player = self.masterController;
            if (player is not null)
            {
                if (coinsSpentDict.ContainsKey(player)) { coinsSpentDict[player] += count; }
                else { coinsSpentDict.Add(player, count); }
            }
            orig(self, count);
        }
    }
}
