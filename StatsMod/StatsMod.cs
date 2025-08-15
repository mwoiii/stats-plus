using BepInEx;
using R2API.Networking;
using StatsMod.CustomStats;
using UnityEngine;
using UnityEngine.Networking;

namespace StatsMod {

    [BepInDependency(NetworkingAPI.PluginGUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class StatsMod : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pond";
        public const string PluginName = "StatsPlus";
        public const string PluginVersion = "1.1.1";
        public static PluginInfo pluginInfo;
        public static StatsMod instance;
        public void Awake() {
            instance = this;
            pluginInfo = Info;
            Log.Init(Logger);
            StatTokens.Init();
            NetworkingAPI.RegisterMessageType<SyncDatabase>();
            Assets.PopulateAssets();
            Tracker.Init();
            RecordHandler.Init();
            StatsScreen.Init();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.F2) & NetworkServer.active) {
                Log.Info(RecordHandler.GetRScript());
            }
            /*   
               else if (Input.GetKeyDown(KeyCode.F3))
               {
                   On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
                   Log.Info("Singleplayer server testing enabled");
               }
            */
        }
    }
}
