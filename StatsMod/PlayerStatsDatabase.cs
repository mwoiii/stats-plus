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
using System.Linq;
using Facepunch.Steamworks;
using System.Linq.Expressions;

namespace StatsMod
{
    public class PlayerStatsDatabase
    {
        private readonly PlayerCharacterMasterController player;
        private readonly int playerIndex;
        private readonly string playerName;

        private readonly Dictionary<string, List<object>> Database = [];

        // Stats that are useless/unusable ?
        // "experience", "isPlayerControlled", "isSprinting", "outOfDanger", "jumpPower", "maxJumpHeight", "shouldAim", "bestFitRadius", "spreadBloomAngle", "corePosition", "footPosition", "radius", "aimOrigin", "isBoss"
        // "totalGamesPlayed", "highestItemsCollected", "highestStagesCompleted", "highestPurchases", "highestGoldPurchases", "highestBloodPurchases", "highestLunarPurchases", "highestTier1Purchases", "highestTier2Purchases", "highestTier3Purchases", "suicideHermitCrabsAchievementProgress", "firstTeleporterCompleted"
        public static readonly string[] charBodyStats = ["level", "maxHealth", "regen", "maxShield", "moveSpeed", "acceleration", "maxJumpCount", "damage", "attackSpeed", "crit", "armor", "critHeal", "multiKillCount", "isElite"];
        public static readonly string[] statSheetStats = ["totalTimeAlive", "totalKills", "totalDeaths", "totalDamageDealt", "totalDamageTaken", "totalHealthHealed", "highestDamageDealt", "highestLevel", "goldCollected", "maxGoldCollected", "totalDistanceTraveled", "totalItemsCollected", "totalStagesCompleted", "totalPurchases", "totalGoldPurchases", "totalBloodPurchases", "totalLunarPurchases", "totalTier1Purchases", "totalTier2Purchases", "totalTier3Purchases", "totalDronesPurchased", "totalGreenSoupsPurchased", "totalRedSoupsPurchased"];
        public static readonly string[] customStats = ["shrinePurchases", "shrineWins", "orderHits", "timeStill", "timeStillPreTP"];

        public static IEnumerable<string> allStats = charBodyStats.Union(statSheetStats).Union(customStats);

        public PlayerStatsDatabase(PlayerCharacterMasterController instance)
        {
            player = instance;
            playerIndex = PlayerCharacterMasterController.instances.IndexOf(player);
            
            playerName = player.networkUser.userName;
            if (playerName.Length == 0)
            {
                Log.Warning($"No name found for player with index {playerIndex}, will use index to reference instead. Can by caused by singleplayer server testing.");
                playerName = $"Player {playerIndex}";
            }

            Database.Add("timestamp", []);
            foreach (string statName in allStats) { Database.Add(statName, []); }
        }

        public float TakeRecord()
        {
            float timestamp = Run.instance.GetRunStopwatch();
            Database["timestamp"].Add(timestamp);

            // Getting charBody stats
            CharacterBody CachedCharacterBody = player.master.GetBody();  // Getting reference to specific player
            if (CachedCharacterBody == null)
            {
                Log.Error($"No body found for {playerName} at {timestamp}. Null entries added to database");
                foreach (string i in charBodyStats) { Database[i].Add(null); }
            }
            else
            {
                foreach(string i in charBodyStats)
                {
                    object stat = typeof(CharacterBody).GetProperty(i).GetValue(CachedCharacterBody);
                    Database[i].Add(stat);
                }
            }

            // Getting statSheet stats
            RunReport runReport = RunReport.Generate(Run.instance, GameEndingCatalog.GetGameEndingDef((GameEndingIndex)2));
            RunReport.PlayerInfo playerInfo = runReport.GetPlayerInfo(playerIndex);
            if (playerInfo == null)
            {
                Log.Error($"No stat sheet found for {playerName} at {timestamp}. Null entries added to database");
                foreach (string i in statSheetStats) { Database[i].Add(null); }
            }
            else
            {
                foreach (string i in statSheetStats)
                {
                    StatDef statDef = (StatDef)typeof(StatDef).GetField(i).GetValue(null);
                    object stat = playerInfo.statSheet.GetStatDisplayValue(statDef);
                    Database[i].Add(stat);
                }
            }

            // Getting custom stats
            foreach (string i in customStats)
            {
                try
                {
                    uint stat = CustomStatsTracker.GetStat(player, i);
                    Database[i].Add(stat);
                }
                catch (Exception e)
                {
                    Database[i].Add(null);
                    Log.Error($"Failed customStat {i} for {playerName} at {timestamp}, null entry added. \n {e.Message}");
                }
            }

            return timestamp;
        }

        public List<object> GetStatSeries(string name)
        {
            return Database[name];
        }

        public string GetStatSeriesAsString(string name) // For logging porpoises
        {
            List<object> series = GetStatSeries(name);
            StringBuilder a = new();
            foreach (object entry in series)
            {
                a.Append($"{entry}, ");
            }
            string b = a.ToString();
            return $"{name}: {b.Substring(0, Math.Max(0, b.Length - 2))}";
        }

        public Dictionary<string, object> GetRecord(int index)
        {
            Dictionary<string, object> Record = [];
            foreach (string statName in allStats) { Record.Add(statName, Database[statName][index]); }
            return Record;
        }

        public Dictionary<string, object> GetRecord(float time)
        {
            List<object> timestamps = Database["timestamp"];
            int index;

            try { index = timestamps.IndexOf(time); }
            catch { index = timestamps.IndexOf(timestamps.OrderBy(x => Math.Abs(float.Parse(x.ToString()) - time)).First()); }

            return GetRecord(index);
        }

        public bool BelongsTo(PlayerCharacterMasterController instance) { return player == instance; }

        public PlayerCharacterMasterController GetPlayer() { return player; }

        public string GetPlayerName() { return playerName; }

        public int GetPlayerIndex() { return playerIndex; }
    }
}
