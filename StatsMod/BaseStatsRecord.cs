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
    public class BaseStatsRecord : StatsRecord
    {
        // The names of the base stats to be recorded
        public static ReadOnlyCollection<string> charBodyStats = new(["isPlayerControlled", "isSprinting", "outOfDanger", "experience", "level", "maxHealth", "regen", "maxShield", "moveSpeed", "acceleration", "jumpPower", "maxJumpCount", "maxJumpHeight", "damage", "attackSpeed", "crit", "armor", "critHeal", "shouldAim", "bestFitRadius", "spreadBloomAngle", "multiKillCount", "corePosition", "footPosition", "radius", "aimOrigin", "isElite", "isBoss"]);
        public static ReadOnlyCollection<string> statSheetStats = new(["totalGamesPlayed", "totalTimeAlive", "totalKills", "totalDeaths", "totalDamageDealt", "totalDamageTaken", "totalHealthHealed", "highestDamageDealt", "highestLevel", "goldCollected", "maxGoldCollected", "totalDistanceTraveled", "totalItemsCollected", "highestItemsCollected", "totalStagesCompleted", "highestStagesCompleted", "totalPurchases", "highestPurchases", "totalGoldPurchases", "highestGoldPurchases", "totalBloodPurchases", "highestBloodPurchases", "totalLunarPurchases", "highestLunarPurchases", "totalTier1Purchases", "highestTier1Purchases", "totalTier2Purchases", "highestTier2Purchases", "totalTier3Purchases", "highestTier3Purchases", "totalDronesPurchased", "totalGreenSoupsPurchased", "totalRedSoupsPurchased", "suicideHermitCrabsAchievementProgress", "firstTeleporterCompleted"]);

        public BaseStatsRecord(PlayerCharacterMasterController instance, string name) : base(instance, name) { }

        protected override Dictionary<string,object> GetStats()
        {
            CharacterBody CachedCharacterBody = PlayerCharacterMasterController.instances[playerIndex].master.GetBody();  // Getting reference to specific player

            Dictionary<string, object> Stats = [];

            // Getting charBody stats
            foreach (string i in charBodyStats)
            {
                object stat = typeof(CharacterBody).GetProperty(i).GetValue(CachedCharacterBody);
                Stats.Add(i, stat);
            }

            // Getting statSheet stats
            RunReport runReport = RunReport.Generate(Run.instance, GameEndingCatalog.GetGameEndingDef((GameEndingIndex)2));
            RunReport.PlayerInfo playerInfo = null;

            playerInfo = runReport.GetPlayerInfo(playerIndex);

            if (playerInfo != null)
            {
                foreach (string i in statSheetStats)
                {
                    StatDef statDef = (StatDef)typeof(StatDef).GetField(i).GetValue(null);
                    object stat = playerInfo.statSheet.GetStatDisplayValue(statDef);
                    Stats.Add(i, stat);
                }
            }

            return Stats;
        }
    }
}
