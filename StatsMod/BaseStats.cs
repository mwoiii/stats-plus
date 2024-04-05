using BepInEx;
using R2API;
using RoR2;
using RoR2.Stats;
using System.IO;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StatsMod
{
    public class BaseStatsRecord
    {
        // The names of the base stats to be recorded
        private ReadOnlyCollection<string> charBodyStats = new ReadOnlyCollection<string>(["isPlayerControlled", "isSprinting", "outOfDanger", "experience", "level", "maxHealth", "regen", "maxShield", "moveSpeed", "acceleration", "jumpPower", "maxJumpCount", "maxJumpHeight", "damage", "attackSpeed", "crit", "armor", "critHeal", "shouldAim", "bestFitRadius", "spreadBloomAngle", "multiKillCount", "corePosition", "footPosition", "radius", "aimOrigin", "isElite", "isBoss"]);
        private ReadOnlyCollection<string> statSheetStats = new ReadOnlyCollection<string>(["totalGamesPlayed", "totalTimeAlive", "totalKills", "totalDeaths", "totalDamageDealt", "totalDamageTaken", "totalHealthHealed", "highestDamageDealt", "highestLevel", "goldCollected", "maxGoldCollected", "totalDistanceTraveled", "totalItemsCollected", "highestItemsCollected", "totalStagesCompleted", "highestStagesCompleted", "totalPurchases", "highestPurchases", "totalGoldPurchases", "highestGoldPurchases", "totalBloodPurchases", "highestBloodPurchases", "totalLunarPurchases", "highestLunarPurchases", "totalTier1Purchases", "highestTier1Purchases", "totalTier2Purchases", "highestTier2Purchases", "totalTier3Purchases", "highestTier3Purchases", "totalDronesPurchased", "totalGreenSoupsPurchased", "totalRedSoupsPurchased", "suicideHermitCrabsAchievementProgress", "firstTeleporterCompleted"]);

        private int playerIndex;
        private string name;

        private Dictionary<string, object> Stats = new Dictionary<string, object>();

        public BaseStatsRecord(PlayerCharacterMasterController instance, string name)
        {
            this.name = name;
            playerIndex = GetPlayerIndex(instance);
            
            GetBaseStats();
        }

        private void GetBaseStats()
        {
            CharacterBody CachedCharacterBody = PlayerCharacterMasterController.instances[playerIndex].master.GetBody();  // Getting reference to specific player

            // Getting charBody stats
            foreach (string i in charBodyStats)
            {
                try
                {
                    object stat = typeof(CharacterBody).GetProperty(i).GetValue(CachedCharacterBody);
                    Stats.Add(i, stat);
                }
                catch
                {
                    Stats.Add(i, null);
                }
            }

            // Getting statSheet stats
            RunReport runReport = RunReport.Generate(Run.instance, GameEndingCatalog.GetGameEndingDef((GameEndingIndex)2));
            RunReport.PlayerInfo playerInfo = null;

            playerInfo = runReport.GetPlayerInfo(playerIndex);

            if (playerInfo != null)
            {
                foreach (string i in statSheetStats)
                {
                    try
                    {
                        StatDef statDef = (StatDef)typeof(StatDef).GetField(i).GetValue(null);
                        object stat = playerInfo.statSheet.GetStatDisplayValue(statDef);
                        Stats.Add(i, stat);
                    }
                    catch
                    {
                        Stats.Add(i, null);
                    }
                }
            }
        }

        private int GetPlayerIndex(PlayerCharacterMasterController instance)
        {
            return PlayerCharacterMasterController.instances.IndexOf(instance);
        }

        public bool BelongsTo(PlayerCharacterMasterController instance)
        {
            return GetPlayerIndex(instance) == playerIndex;
        }

        public string GetName()
        {
            return name;
        }

        public object Get(string stat)
        {
            if (charBodyStats.Contains(stat) | statSheetStats.Contains(stat)) { return Stats[stat]; }
            else
            {
                Log.Error($"Stat {stat} asked for but does not exist. null returned.");
                return null;
            }
        }

        public string GetAllAsString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string i in charBodyStats) { sb.AppendLine($"{i}: {Get(i)}"); }
            foreach (string i in statSheetStats) { sb.AppendLine($"{i}: {Get(i)}"); }
            return sb.ToString();
        }
    }
}