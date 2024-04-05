using RoR2;
using RoR2.Stats;
using BepInEx;
using R2API;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Text;

namespace StatsMod
{
    public abstract class StatsRecord
    {
        protected int playerIndex; // Specifies what player the record is recording
        protected string name;

        protected Dictionary<string, object> Stats; // Stores stat name with stat value

        public StatsRecord(PlayerCharacterMasterController instance, string name) // Instantiating method
        {
            this.name = name;
            playerIndex = GetPlayerIndex(instance);

            Stats = GetStats();
        }
        protected abstract Dictionary<string,object> GetStats(); // This method is intended to create the Stats dictionary in inheriting classes

        public static int GetPlayerIndex(PlayerCharacterMasterController instance)
        {
            return PlayerCharacterMasterController.instances.IndexOf(instance);
        }

        public bool BelongsTo(PlayerCharacterMasterController instance) { return GetPlayerIndex(instance) == playerIndex; }

        public string GetName() { return name; }

        public object Get(string name)
        {
            if (Stats.ContainsKey(name)) { return Stats[name]; }
            else
            {
                return null; // No error raised here as the FullRecord class will cause this often
            }
        }
        public string GetAllAsString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string i in Stats.Keys) { sb.AppendLine($"{i}: {Get(i)}"); }
            return sb.ToString();
        }
    }
}