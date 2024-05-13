using RoR2;
using System.Collections.Generic;
using System;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace StatsMod.CustomStats
{
    internal class ItemLead : Stat
    {
        private static Dictionary<PlayerCharacterMasterController, uint> itemLeadDict = [];  // The current item lead of a player: 0 if not in the lead, >0 otherwise

        new public static void Init()
        {
            SceneExitController.onBeginExit += ItemLeadTrack;

            Tracker.statsTable.Add("itemLead", itemLeadDict);
        }

        private static void ItemLeadTrack(SceneExitController sceneExitController)
        {
            if (PlayerCharacterMasterController.instances.Count < 2) { return; }

            itemLeadDict = itemLeadDict.Keys.ToDictionary(key => key, val => (uint)0);  // Set all values to 0

            uint highestLead = 0;
            uint highestItems = 0;
            PlayerCharacterMasterController leadingPlayer = null;
            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
            {
#pragma warning disable Publicizer001
                uint playerItems = (uint)player.master.inventory.itemStacks.Sum();
#pragma warning restore Publicizer001
                if (playerItems > highestItems)
                {
                    highestLead = playerItems - highestItems;
                    highestItems = playerItems;
                    leadingPlayer = player;
                }
                else if (highestItems - playerItems < highestLead) { highestLead = highestItems - playerItems; }
            }
            if (itemLeadDict.ContainsKey(leadingPlayer)) { itemLeadDict[leadingPlayer] = highestLead; }
            else { itemLeadDict.Add(leadingPlayer, highestLead); }
        }
    }
}
