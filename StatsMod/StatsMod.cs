using BepInEx;
using R2API;
using RoR2;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using StatsMod.CustomStats;
using UnityEngine.AddressableAssets;
using R2API.Networking;

namespace StatsMod
{

    [BepInDependency(NetworkingAPI.PluginGUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class StatsMod : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pond";
        public const string PluginName = "StatsPlus";
        public const string PluginVersion = "1.1.0";
        public static PluginInfo pluginInfo;
        public static StatsMod instance;
        public void Awake()
        {
            instance = this;
            pluginInfo = Info;
            Log.Init(Logger);
            NetworkingAPI.RegisterMessageType<SyncDatabase>();
            Assets.PopulateAssets();
            Tracker.Init();
            RecordHandler.Init();
            StatsScreen.Init();
        }

        private void Update()
        {
         /*   
            
            if (Input.GetKeyDown(KeyCode.F2) & NetworkServer.active) { RecordHandler.GetRScript(); }

            else if (Input.GetKeyDown(KeyCode.F3))
            {
                On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
                Log.Info("Singleplayer server testing enabled");
            }
         */            
        }
    }
}
