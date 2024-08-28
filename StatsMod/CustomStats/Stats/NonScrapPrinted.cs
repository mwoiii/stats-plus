using RoR2;
using System.Collections.Generic;
using System;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace StatsMod.CustomStats
{
    internal class NonScrapPrinted : Stat
    {
        private static Dictionary<PlayerCharacterMasterController, uint> nonScrapPrintedDict = [];  // The amount of items used in printers and soups that were not scrap
        new public static void Init()
        {
            IL.RoR2.PurchaseInteraction.OnInteractionBegin += NonScrapTrack;

            Tracker.statsTable.Add("nonScrapPrinted", nonScrapPrintedDict);
        }

        private static void NonScrapTrack(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdloc(13),
                x => x.MatchLdloc(2),
                x => x.MatchBeq(out var _)
                );
            c.Emit(OpCodes.Ldloc_0);
            c.Emit(OpCodes.Ldloc, 13);
            c.EmitDelegate<Action<CharacterBody, ItemIndex>>((interactorBody, item) =>
            {
                ItemIndex[] itemBlacklist =
                [
                    (ItemIndex)151, // RegeneratingScrap
                    (ItemIndex)156, // ScrapGreen
                    (ItemIndex)158, // ScrapRed
                    (ItemIndex)160, // ScrapWhite
                    (ItemIndex)162  // ScrapYellow
                ];
                if (!itemBlacklist.Contains(item))
                {
                    var player = interactorBody.master.playerCharacterMasterController;
                    if (nonScrapPrintedDict.ContainsKey(player)) { nonScrapPrintedDict[player]++; }
                    else { nonScrapPrintedDict.Add(player, 1); }
                }
            });
            /*
            CreateItemTakenOrb(component.corePosition, base.gameObject, item);
            if (item != itemIndex) <-- matching
            {
                < delegate emitted here >
                PurchaseInteraction.onItemSpentOnPurchase?.Invoke(this, activator);
            }
            */
        }
    }
}
