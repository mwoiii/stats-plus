using BepInEx;
using R2API;
using RoR2;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

namespace StatsMod
{

    // don't touch these
    [BepInDependency(LanguageAPI.PluginGUID)]

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class StatsMod : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pond";
        public const string PluginName = "StatsMod";
        public const string PluginVersion = "1.0.0";

        private IDictionary<NetworkUser, uint> shrinePurchases = new Dictionary<NetworkUser, uint>();  // Dictionary for recording how many times each player has used a shrine of chance
        private IDictionary<NetworkUser, uint> shrineWins = new Dictionary<NetworkUser, uint>();  // Dictionary for recording how many times each player has won a shrine of chance
        private IDictionary<NetworkUser, uint> shrineLoses = new Dictionary<NetworkUser, uint>();  // Dictionary for recording how many times each player has lost a shrine of chance

        private IDictionary<NetworkUser, uint> orderHits = new Dictionary<NetworkUser, uint>();  // Dictionary for recording how many times each player has used a shrine of order

        private IDictionary<NetworkUser, uint> timeStill = new Dictionary<NetworkUser, uint>();  // Dictionary for recording how long each player has been standing still

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);

            Enable();
            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };  // This just allows connecting to a local server (for multiplayer testing)

        }

        private void Enable()  // When this method is called, enabling all mod features
        {
            ShrineChanceBehavior.onShrineChancePurchaseGlobal += ShrineTrack;
            On.RoR2.ShrineRestackBehavior.AddShrineStack += OrderTrack;
            Run.onRunStartGlobal += ResetData;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
        }

        private void Disable() // When this method is called, disabling all mod features
        {
            ShrineChanceBehavior.onShrineChancePurchaseGlobal -= ShrineTrack;
            On.RoR2.ShrineRestackBehavior.AddShrineStack -= OrderTrack;
            Run.onRunStartGlobal -= ResetData;
        }

        // Tracks all the times shrines are hit. This method is only ever called on the host side
        private void ShrineTrack(bool failed, Interactor activator)
        {
            var networkPlayer = activator.GetComponent<CharacterBody>().master.playerCharacterMasterController.networkUser;  // Getting the networkUser (unique identification in multiplayer), calling it networkPlayer
            if (!shrinePurchases.ContainsKey(networkPlayer))  // If networkPlayer isn't in the shrinePurchases dictionary, adding them. Otherwise, incrementing counter by 1
            {
                shrinePurchases.Add(networkPlayer, 1);
                shrineWins.Add(networkPlayer, 0);
                shrineLoses.Add(networkPlayer, 0);
            }
            else
            {
                shrinePurchases[networkPlayer]++;
            }

            if (failed) // If lost shrine, increment lost counter
            {
                shrineLoses[networkPlayer]++;
            }
            else  // If won shrine, increment won counter
            {
                shrineWins[networkPlayer]++;
            }

            //Log.Info(shrinePurchases[networkPlayer]); // Just for demonstration purposes, you can see in the log that the counter goes up seperately for each player and is maintained across stages
            //Log.Info(shrineWins[networkPlayer]);
            //Log.Info(shrineLoses[networkPlayer]);

        }

        // Counting how long a player has stopped moving for
        public void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                if (PlayerCharacterMasterController.instances.Count > 0)  // Checking if the player is in a run by checking for existence of PlayerCharacterMasterController, which is created at the start of a run
                {
                    foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                    {
                        try
                        {
                            var isStill = player.master.GetBody().GetNotMoving();
                            if (isStill)
                            {
                                var networkPlayer = player.networkUser;
                                if (!timeStill.ContainsKey(networkPlayer))
                                {
                                    timeStill.Add(networkPlayer, 1);
                                }
                                else
                                {
                                    timeStill[networkPlayer]++;
                                }
                                // Log.Info(timeStill[networkPlayer] * Time.fixedDeltaTime);  // FixedUpdate is called a different amount of times depending on the framerate. Time.fixedDeltaTime is the frequency that it is called
                            }
                        }
                        catch (NullReferenceException)
                        {
                            // Player may be dead, or not properly spawned yet
                        }
                    }

                }
            }
        }

        // Emptying all the data dictionaries. Called at the start of a run
        private void ResetData(Run run)
        {
            Log.Info("New run, resetting data dicts");
            shrinePurchases = new Dictionary<NetworkUser, uint>();  // Dictionary for recording how many times each player has used a shrine of chance
            shrineWins = new Dictionary<NetworkUser, uint>();  // Dictionary for recording how many times each player has won a shrine of chance
            shrineLoses = new Dictionary<NetworkUser, uint>();  // Dictionary for recording how many times each player has lost a shrine of chance

            orderHits = new Dictionary<NetworkUser, uint>();  // Dictionary for recording how many times each player has used a shrine of order

            timeStill = new Dictionary<NetworkUser, uint>();  // Dictionary for recording how long each player has been standing still
        }
        
        // Counting how many times a player has hit a shrine of order
        void OrderTrack(On.RoR2.ShrineRestackBehavior.orig_AddShrineStack orig, ShrineRestackBehavior self, Interactor interactor)
        {
            var networkPlayer = interactor.GetComponent<CharacterBody>().master.playerCharacterMasterController.networkUser;  // Getting the networkUser (unique identification in multiplayer), calling it networkPlayer
            if (!orderHits.ContainsKey(networkPlayer))  // If networkPlayer isn't in the orderHits dictionary, adding them. Otherwise, incrementing counter by 1
            {
                orderHits.Add(networkPlayer, 1);
            }
            else
            {
                orderHits[networkPlayer]++;
            }

            orig(self, interactor);
        }

        // Test code for base stats that triggers on the death of any entity
        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
            {
                int id = BaseStats.GetPlayerIndex(player);  // Just a test that both methods work
                Log.Info(BaseStats.GetBaseStats(id));
            }

        }

        // Old implementation of the shrine hit method using hooking
        /*
        // Tracks all the times shrines are hit. This method is only ever called on the host side
        private void ShrineTrack(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, global::RoR2.ShrineChanceBehavior self, Interactor activator)
        {
            var networkPlayer = activator.GetComponent<CharacterBody>().master.playerCharacterMasterController.networkUser;  // Getting the networkUser (unique identification in multiplayer), calling it networkPlayer
            if (!shrinePurchases.ContainsKey(networkPlayer))  // If networkPlayer isn't in the shrinePurchases dictionary, adding them. Otherwise, incrementing counter by 1
            {
                shrinePurchases.Add(networkPlayer, 1);
            }
            else
            {
                shrinePurchases[networkPlayer]++;
            }

            Log.Info(shrinePurchases[networkPlayer]); // Just for demonstration purposes, you can see in the log that the counter goes up seperately for each player and is maintained across stages
            Log.Info(networkPlayer);

            orig(self, activator);  // Calling original method

        }
        */

        // Old test code for the spawning a behemoth on every bullet spawn
        /*
        private static bool OnShot(On.RoR2.BulletAttack.orig_DefaultHitCallbackImplementation orig, BulletAttack self, ref BulletAttack.BulletHit hitInfo)
        {
            if (NetworkServer.active)
            {
                Log.Info(LocalUserManager.GetFirstLocalUser().cachedBody);
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(RoR2Content.Items.Behemoth.itemIndex), transform.position, transform.up * 20f);
            }
            bool OrigReturn =  orig(self, ref hitInfo);

            return OrigReturn;
        }
        */

        // Old test code for spawning a behemoth and killing the player on f2 press
        /*
        // The Update() method is run on every frame of the game.
        private void Update()
        {

            // This if statement checks if the player has currently pressed F2.
            if (Input.GetKeyDown(KeyCode.F2))
            {
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                Log.Info($"Player pressed F2. Spawning our custom item at coordinates {transform.position}");
                Log.Info("Changes were applied");

                PlayerCharacterMasterController.instances[0].master.TrueKill();
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(RoR2Content.Items.Behemoth.itemIndex), transform.position, transform.forward * 20f);
            }
            
        }
        */

    }
    

}
