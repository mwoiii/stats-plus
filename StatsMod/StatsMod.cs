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

namespace StatsMod
{

    [BepInDependency(LanguageAPI.PluginGUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class StatsMod : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pond";
        public const string PluginName = "StatsMod";
        public const string PluginVersion = "1.0.0";

        public void Awake()
        {
            Log.Init(Logger);
            Init();
        }

        private void Init()
        {
            Tracker.Init();
            RecordHandler.Init();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2) & NetworkServer.active) { RecordHandler.GetRScript(); }

            else if (Input.GetKeyDown(KeyCode.F3))
            {
                On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
                Log.Info("Singleplayer server testing enabled");
            }
        }
    }
}
