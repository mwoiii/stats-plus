using BepInEx;
using R2API;
using RoR2;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

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

        // Code that creates a record of statistics for each player at the end of each stage.
        private List<PlayerStatsDatabase> StatsDatabase;

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);

            Enable();
            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };  // This just allows connecting to a local server (for multiplayer testing with only one device)

        }

        private void Enable()  // When this method is called, enabling all mod features
        {
            CustomStatsHolder.Enable();
            Run.onRunStartGlobal += ResetData;
            SceneExitController.onBeginExit += OnBeginExit;
        }

        private void Disable() // When this method is called, disabling all mod features
        {
            CustomStatsHolder.Disable();
            Run.onRunStartGlobal -= ResetData;
            SceneExitController.onBeginExit -= OnBeginExit;
        }

        // Emptying all the data dictionaries. Called at the start of a run
        private void ResetData(Run run)
        {
            Log.Info("New run, resetting data dicts");

            CustomStatsHolder.ResetData();
            SetupDatabase();
        }

        private void SetupDatabase()
        {
            StatsDatabase = [];
            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances) { StatsDatabase.Add(new PlayerStatsDatabase(player)); }
            Log.Info($"Successfully setup full database for {StatsDatabase.Count} players");
        }

        // Actually taking the records at the end of the stage
        private void OnBeginExit(SceneExitController a)
        {
            float time = Run.instance.GetRunStopwatch();
            foreach (PlayerStatsDatabase i in StatsDatabase)
            {
                Log.Info("Trying to make a record...");
                i.TakeRecord($"{time}");
                Log.Info($"Record made at {time}");
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
