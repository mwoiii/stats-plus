using RoR2;
using System.Collections.Generic;
using System;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace StatsMod.CustomStats
{
    internal class AllyPoints : Stat
    {
        private static Dictionary<PlayerCharacterMasterController, uint> allyPointsDict = [];  // How many times each player has used a shrine of order
        new public static void Init()
        {
           SceneExitController.onBeginExit += AllyPointTrack;

            Tracker.statsTable.Add("allyPoints", allyPointsDict);
        }

        private static void AllyPointTrack(SceneExitController sceneExitController)
        {
            foreach (var player in PlayerCharacterMasterController.instances)
            {
                var inventory = player.master.inventory.itemStacks;
                var sum = inventory[160] / 2 // squid
                    + inventory[71] // mask
                    + inventory[44] // drone
                    + inventory[14] // gland
                    + (1 - Math.Pow(0, inventory[171])) // halcyon (0 if 0, 1 if != 0)
                    + inventory[139] // cores
                    + inventory[111] // nucleus
                    + inventory[178]; // zoea
                if (allyPointsDict.ContainsKey(player)) { allyPointsDict[player] = (uint)sum; }
                else { allyPointsDict.Add(player, (uint)sum); }
            }
        }
    }
}
