using RoR2;
using RoR2.Stats;
using BepInEx;
using R2API;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Text;
using HarmonyLib;

namespace StatsMod
{
    public class PlayerStatsDatabase // Holds various StatsRecord for a specific instance of a player
    {
        private readonly List<StatsRecord[]> Database = []; // First index refers to order of records, second refers to base or custom record
        private PlayerCharacterMasterController player;

        public PlayerStatsDatabase(PlayerCharacterMasterController instance)
        {
            player = instance;
        }

        public void TakeRecord(string name)
        {
            Database.Add([new BaseStatsRecord(player, $"{name}Base"), new CustomStatsRecord(player, $"{name}Custom")]);
        }

        public object GetStat(int orderIndex, string name, int loc = -1)
        {
            if (loc == -1) { loc = Find(name); }

            return Database[orderIndex][loc].Get(name);
        }

        public object[] GetStatSeries(string name)
        {
            int n = Database.Count;
            int loc = Find(name);

            object[] Series = new object[n];

            for (int i = 0; i < n; i++)
            {
                Series[i] = GetStat(i, name, loc);
            }
            return Series;
        }

        private int Find(string name)
        {
            if (CustomStatsRecord.customStats.Contains(name)) { return 1; }
            else { return 0; }
        }

        public bool BelongsTo(PlayerCharacterMasterController instance) { return player == instance; }
    }
}
