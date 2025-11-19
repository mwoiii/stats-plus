using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;

namespace StatsMod.CustomStats {
    internal class NonScrapPrinted : Stat {
        private static Dictionary<PlayerCharacterMasterController, uint> nonScrapPrintedDict = [];  // The amount of items used in printers and soups that were not scrap
        new public static void Init() {
            IL.RoR2.PurchaseInteraction.OnInteractionBegin += NonScrapTrack;

            Tracker.statsTable.Add("nonScrapPrinted", nonScrapPrintedDict);
        }

        private static void NonScrapTrack(ILContext il) {
            ILCursor c = new ILCursor(il);
            int itemIndexLoc = 20;
            if (c.TryGotoNext(
                x => x.MatchLdloc(out itemIndexLoc),
                x => x.MatchLdfld<Inventory.ItemAndStackValues>("itemIndex"),
                x => x.MatchLdloc(out _),
                x => x.MatchBeq(out var _)
                )) {

                c.Emit(OpCodes.Ldloc_0);
                c.Emit(OpCodes.Ldloc, itemIndexLoc);
                c.EmitDelegate<Action<CharacterBody, Inventory.ItemAndStackValues>>((interactorBody, item) => {
                    ItemDef itemDef = ItemCatalog.GetItemDef(item.itemIndex);
                    ItemDef[] itemBlacklist = {
                    RoR2Content.Items.ScrapWhite,
                    RoR2Content.Items.ScrapGreen,
                    RoR2Content.Items.ScrapRed,
                    RoR2Content.Items.ScrapYellow,
                    DLC1Content.Items.RegeneratingScrap
                };
                    if (!itemBlacklist.Contains(itemDef)) {
                        var player = interactorBody.master.playerCharacterMasterController;
                        if (nonScrapPrintedDict.ContainsKey(player)) { nonScrapPrintedDict[player]++; } else { nonScrapPrintedDict.Add(player, 1); }
                    }
                });
            } else {
                Log.Error("NonScrapTrack IL hook borked.");
            }
            /*
            CreateItemTakenOrb(component.corePosition, base.gameObject, item);
            if (item.index != itemIndex) <-- matching
            {
                < delegate emitted here >
                PurchaseInteraction.onItemSpentOnPurchase?.Invoke(this, activator);
            }
            */
        }
    }
}
