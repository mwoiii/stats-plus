using RoR2;
using System.Collections.Generic;
using System;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace StatsMod.CustomStats
{
    internal class OrderHits : Stat
    {
        private static Dictionary<PlayerCharacterMasterController, uint> orderHitsDict = [];  // How many times each player has used a shrine of order
        new public static void Init()
        {
            On.RoR2.ShrineRestackBehavior.AddShrineStack += OrderTrack;

            Tracker.statsTable.Add("orderHits", orderHitsDict);
        }

        private static void OrderTrack(On.RoR2.ShrineRestackBehavior.orig_AddShrineStack orig, ShrineRestackBehavior self, Interactor interactor)
        {
            var player = interactor.GetComponent<CharacterBody>().master.playerCharacterMasterController;
            if (orderHitsDict.ContainsKey(player)) { orderHitsDict[player]++; }
            else { orderHitsDict.Add(player, 1); }
            orig(self, interactor);
        }
    }
}
