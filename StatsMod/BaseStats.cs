using BepInEx;
using R2API;
using RoR2;
using RoR2.Stats;
using System.IO;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace StatsMod
{
    public class BaseStats
    {
        public static string GetBaseStats(int id)
        {
            CharacterBody CachedCharacterBody = PlayerCharacterMasterController.instances[id].master.GetBody();  // Getting reference to specific player

            // Stats to collect
            string[] charBodyStats = ["isPlayerControlled", "isSprinting", "outOfDanger", "experience", "level", "maxHealth", "regen", "maxShield", "moveSpeed", "acceleration", "jumpPower", "maxJumpCount", "maxJumpHeight", "damage", "attackSpeed", "crit", "armor", "critHeal", "shouldAim", "bestFitRadius", "spreadBloomAngle", "multiKillCount", "corePosition", "footPosition", "radius", "aimOrigin", "isElite", "isBoss"];
            string[] statSheetStats = ["totalGamesPlayed", "totalTimeAlive", "totalKills", "totalDeaths", "totalDamageDealt", "totalDamageTaken", "totalHealthHealed", "highestDamageDealt", "highestLevel", "goldCollected", "maxGoldCollected", "totalDistanceTraveled", "totalItemsCollected", "highestItemsCollected", "totalStagesCompleted", "highestStagesCompleted", "totalPurchases", "highestPurchases", "totalGoldPurchases", "highestGoldPurchases", "totalBloodPurchases", "highestBloodPurchases", "totalLunarPurchases", "highestLunarPurchases", "totalTier1Purchases", "highestTier1Purchases", "totalTier2Purchases", "highestTier2Purchases", "totalTier3Purchases", "highestTier3Purchases", "totalDronesPurchased", "totalGreenSoupsPurchased", "totalRedSoupsPurchased", "suicideHermitCrabsAchievementProgress", "firstTeleporterCompleted"];

            StringBuilder sb = new StringBuilder();

            // Getting charBody stats
            foreach (string i in charBodyStats)
            {
                try
                {
                    object stat = typeof(CharacterBody).GetProperty(i).GetValue(CachedCharacterBody);
                    sb.AppendLine($"{i}:{stat}");
                }
                catch
                {
                    sb.AppendLine($"didn't like {i}");
                }
            }

            // Getting statSheet stats
            RunReport runReport = RunReport.Generate(Run.instance, GameEndingCatalog.GetGameEndingDef((GameEndingIndex)2));
            RunReport.PlayerInfo playerInfo = null;

            playerInfo = runReport.GetPlayerInfo(id);

            if (playerInfo != null)
            {
                foreach (string i in statSheetStats)
                {
                    try
                    {
                        StatDef statDef = (StatDef)typeof(StatDef).GetField(i).GetValue(null);
                        sb.AppendLine($"{i}:{playerInfo.statSheet.GetStatDisplayValue(statDef)}");
                    }
                    catch
                    {
                        sb.AppendLine($"didn't like {i}");
                    }
                }
            }

            // Returning stats
            return sb.ToString();
        }

        public static int GetPlayerIndex(PlayerCharacterMasterController instance)
        {
            return PlayerCharacterMasterController.instances.IndexOf(instance);
        }
    }
}