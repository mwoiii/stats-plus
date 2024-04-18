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
using System.Xml.Schema;
using UnityEngine;
using TMPro;

namespace StatsMod
{
    public class PlayerStatsDatabase
    {
        private readonly PlayerCharacterMasterController player;
        private readonly int playerIndex;
        private readonly string playerName;

        private readonly Dictionary<string, List<object>> Database = [];

        // Stats that are useless/unusable ?
        // "critHeal", "multiKillCount", "level", "acceleration", "experience", "isPlayerControlled", "isSprinting", "outOfDanger", "jumpPower", "maxJumpHeight", "shouldAim", "bestFitRadius", "spreadBloomAngle", "corePosition", "footPosition", "radius", "aimOrigin", "isBoss"
        public static readonly string[] charBodyStats = ["maxHealth", "regen", "maxShield", "moveSpeed", "maxJumpCount", "damage", "attackSpeed", "crit", "armor", "isElite"];
        // "totalStagesCompleted", "maxGoldCollected", "highestLevel", "totalGamesPlayed", "highestItemsCollected", "highestStagesCompleted", "highestPurchases", "highestGoldPurchases", "highestBloodPurchases", "highestLunarPurchases", "highestTier1Purchases", "highestTier2Purchases", "highestTier3Purchases", "suicideHermitCrabsAchievementProgress", "firstTeleporterCompleted"
        public static readonly string[] statSheetStats = ["totalTimeAlive", "totalKills", "totalMinionKills", "totalDeaths", "totalDamageDealt", "totalMinionDamageDealt", "totalDamageTaken", "totalHealthHealed", "highestDamageDealt",  "goldCollected", "totalDistanceTraveled", "totalItemsCollected", "totalPurchases", "totalGoldPurchases", "totalBloodPurchases", "totalLunarPurchases", "totalTier1Purchases", "totalTier2Purchases", "totalTier3Purchases", "totalDronesPurchased", "totalTurretsPurchased", "totalGreenSoupsPurchased", "totalRedSoupsPurchased"];
        public static readonly string[] customStats = ["shrinePurchases", "shrineWins", "orderHits", "timeStill", "timeStillUnsafe", "timeLowHealth", "fallDamage", "coinsSpent", "avenges"];

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
                Log.Warning($"No body found for {playerName} at {timestamp}. Duplicate entries added to database");
                foreach (string i in charBodyStats) { Database[i].Add(Database[i].Last()); }
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
                Log.Warning($"No stat sheet found for {playerName} at {timestamp}. Duplicate entries added to database");
                foreach (string i in statSheetStats) { Database[i].Add(Database[i].Last()); }
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
                    var stat = CustomStatsTracker.GetStat(player, i);
                    Database[i].Add(stat);
                }
                catch (Exception e)
                {
                    Database[i].Add(Database[i].Last());
                    Log.Warning($"Failed customStat {i} for {playerName} at {timestamp}, duplicate entry added. \n {e.Message}");
                }
            }

            return timestamp;
        }

        public List<object> GetStatSeries(string name)
        {
            return Database[name];
        }

        public string GetStatSeriesAsString(string name, bool rVector = false) // For logging porpoises
        {
            List<object> series = GetStatSeries(name);
            StringBuilder a = new();
            foreach (object entry in series)
            {
                if (!rVector) { a.Append($"{entry}, "); }
                else { a.Append($"{Numberise(entry)}, "); }
            }
            string b = a.ToString().Substring(0, Math.Max(0, a.Length - 2));

            if (!rVector) { return $"{name}: {b}"; }
            else { return $"{name} <- c({b})"; }
            
        }

        public Dictionary<string, object> GetRecord(int index)
        {
            if (index < 0) { index = Database["maxHealth"].Count + index; }
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

        public static object Numberise(object value) // Takes non numerical object types that can represent numbers and turns them into numerical types to be interpreted.
        {
            if (value.GetType() == typeof(bool)) { return (bool)value ? 1 : 0; } // bools turned to 0s or 1s
            
            string valueString = value.ToString();
            
            if (valueString.Contains(',')) // numbers represented with commmas have commas removed
            {
                try { return Int64.Parse(valueString, System.Globalization.NumberStyles.AllowThousands); }
                catch ( OverflowException ) {  return -1; }  // Safeguarding, to prevent data from not being recorded at all
            }
            else if (valueString.Contains(':')) // minute:second representation turned to just seconds
            {
                string[] time = valueString.Split(':');
                return int.Parse(time[0]) * 60 + int.Parse(time[1]);
            }
            else if (valueString.Contains("marathons")) // "marathons" representation have the "marathons" part removed
            {
                return float.Parse(valueString.Substring(0, valueString.Length - 10));
            }
            return value;
        }

        public bool BelongsTo(PlayerCharacterMasterController instance) { return player == instance; }

        public PlayerCharacterMasterController GetPlayer() { return player; }

        public string GetPlayerName() { return playerName; }

        public int GetPlayerIndex() { return playerIndex; }
    }
}
