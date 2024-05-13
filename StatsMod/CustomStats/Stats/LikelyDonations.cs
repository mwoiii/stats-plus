using RoR2;
using System.Collections.Generic;
using System;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace StatsMod.CustomStats
{
    internal class LikelyDonations : Stat
    {
        private static Dictionary<PlayerCharacterMasterController, uint> likelyDonationsDict = [];   // The total number of times a player presumably bought an item that was taken by another player

        private static Dictionary<PickupDropletController, Vector3> dropletOrigin = [];
        private static Dictionary<PlayerCharacterMasterController, Vector3> lastPurchaseOrigin = [];
        private static Dictionary<GenericPickupController, PlayerCharacterMasterController> itemPurchaser = [];

        new public static void Init()
        {
            On.RoR2.GenericPickupController.AttemptGrant += DonationsTrack;
            IL.RoR2.PickupDropletController.OnCollisionEnter += GenericDropletHook;
            On.RoR2.PickupDropletController.Start += LogDropletOrigin;
            On.RoR2.Interactor.PerformInteraction += LogPurchaseInteraction;
            On.RoR2.Run.BeginStage += ClearDicts;

            Tracker.statsTable.Add("likelyDonations", likelyDonationsDict);
        }

        private static void GenericDropletHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdflda<PickupDropletController>("createPickupInfo"),
                x => x.MatchCallOrCallvirt<GenericPickupController>("CreatePickup"),
                x => x.MatchPop()
                );
            c.Index += 3;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<GenericPickupController, PickupDropletController>>((pickupController, dropletController) =>
            {
                if (pickupController != null)   // fixes the branch. by adding  branch. for the branch.
                {
                    Vector3 origin = dropletOrigin[dropletController];
                    foreach (var key in lastPurchaseOrigin.Keys)
                    {
                        if (Vector3.Distance(lastPurchaseOrigin[key], origin) < 5)
                        {
                            itemPurchaser.Add(pickupController, key);
                            break;
                        }
                    }
                    dropletOrigin.Remove(dropletController);
                }
            });
            c.Remove(); // removing the 'pop' as the return value is now being used. I hope this does not cause compatibility issues
            /*
		    if (shouldSpawn)
		    {
			    GenericPickupController.CreatePickup(in createPickupInfo); <-- matching
                <delegate here>
		    }
            */
        }

        private static void LogDropletOrigin(On.RoR2.PickupDropletController.orig_Start orig, PickupDropletController self)
        {
            dropletOrigin.Add(self, self.transform.position);
            orig(self);
        }

        private static void DonationsTrack(On.RoR2.GenericPickupController.orig_AttemptGrant orig, GenericPickupController self, CharacterBody body)
        {
            if (itemPurchaser.ContainsKey(self))
            {
                var purchaser = itemPurchaser[self];
                if (purchaser != body.master.playerCharacterMasterController)
                {
                    if (likelyDonationsDict.ContainsKey(purchaser)) { likelyDonationsDict[purchaser] += 1; }
                    else { likelyDonationsDict.Add(purchaser, 1); }
                }
                itemPurchaser.Remove(self);
            }
            orig(self, body);
        }

        private static void LogPurchaseInteraction(On.RoR2.Interactor.orig_PerformInteraction orig, Interactor self, GameObject interactable)
        {
            var lastPurchase = interactable.GetComponent<PurchaseInteraction>();
            if (lastPurchase)
            {
                var player = self.GetComponent<CharacterBody>().master.playerCharacterMasterController;
                var origin = lastPurchase.transform.position;
                if (lastPurchaseOrigin.ContainsKey(player)) { lastPurchaseOrigin[player] = origin; }
                else { lastPurchaseOrigin.Add(player, origin); }
            }
            orig(self, interactable);
        }

        private static void ClearDicts(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            itemPurchaser = [];
            dropletOrigin = [];
            orig(self);
        }

    }
}
