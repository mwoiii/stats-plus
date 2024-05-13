using RoR2;
using System.Collections.Generic;
using System;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace StatsMod.CustomStats
{
    internal class ShrinePurchases : Stat
    {
        private static Dictionary<PlayerCharacterMasterController, uint> shrinePurchasesDict = [];  // How many times each player has used a shrine of chance
        private static Dictionary<PlayerCharacterMasterController, uint> shrineWinsDict = [];  // How many times each player has won a shrine of chance

        new public static void Init()
        {
            ShrineChanceBehavior.onShrineChancePurchaseGlobal += ShrineTrack;

            Tracker.statsTable.Add("shrinePurchases", shrinePurchasesDict);
            Tracker.statsTable.Add("shrineWins", shrineWinsDict);
        }

        private static void ShrineTrack(bool failed, Interactor activator)
        {
            var player = activator.GetComponent<CharacterBody>().master.playerCharacterMasterController;
            if (shrinePurchasesDict.ContainsKey(player))
            {
                shrinePurchasesDict[player]++;
                if (!failed) { shrineWinsDict [player]++; }
            }
            else
            {
                shrinePurchasesDict.Add(player, 1);
                if (!failed) { shrineWinsDict.Add(player, 1); }
                else { shrineWinsDict.Add(player, 0); }
            }
        }
    }
}
