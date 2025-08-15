namespace StatsMod {
    public static class StatTokens {
        public const string titlePrefix = $"STATSMOD_STAT_TITLE_";
        public const string bodyPrefix = $"STATSMOD_STAT_BODY_";

        public static void Init() {
            AddStatsModTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("StatsMod.txt");
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddStatsModTokens() {
            #region Base Game Stats
            Language.Add(titlePrefix + "maxHealth", "Max Health");
            Language.Add(bodyPrefix + "maxHealth", "The size of each player's maximum health (the green portion of the health bar).");

            Language.Add(titlePrefix + "regen", "Regen");
            Language.Add(bodyPrefix + "regen", "Each player's health regeneration (in hp/s).");

            Language.Add(titlePrefix + "maxShield", "Max Shield");
            Language.Add(bodyPrefix + "maxShield", "The size of each player's maximum shield (the blue portion of the health bar).");

            Language.Add(titlePrefix + "moveSpeed", "Move Speed");
            Language.Add(bodyPrefix + "moveSpeed", "Each player's base movement speed.");

            Language.Add(titlePrefix + "maxJumpCount", "Max Jump Count");
            Language.Add(bodyPrefix + "maxJumpCount", "Each player's maximum jump count");

            Language.Add(titlePrefix + "damage", "Damage");
            Language.Add(bodyPrefix + "damage", "Each player's base damage stat.");

            Language.Add(titlePrefix + "attackSpeed", "Attack Speed");
            Language.Add(bodyPrefix + "attackSpeed", "Each player's attack speed multiplier.");

            Language.Add(titlePrefix + "crit", "Crit Chance");
            Language.Add(bodyPrefix + "crit", "Each player's critical hit chance.");

            Language.Add(titlePrefix + "armor", "Armor");
            Language.Add(bodyPrefix + "armor", "Each player's armor stat. Damage reduction multiplier = armor / (100 + armor).");

            Language.Add(titlePrefix + "totalTimeAlive", "Total Time Alive");
            Language.Add(bodyPrefix + "totalTimeAlive", "The total time in seconds that each player was alive for while the run stopwatch was active.");

            Language.Add(titlePrefix + "totalKills", "Total Kills");
            Language.Add(bodyPrefix + "totalKills", "The total enemy kills of each player.");

            Language.Add(titlePrefix + "totalMinionKills", "Total Minion Kills");
            Language.Add(bodyPrefix + "totalMinionKills", "The total kills of all minions belonging to an individual player.");

            Language.Add(titlePrefix + "totalDeaths", "Total Deaths");
            Language.Add(bodyPrefix + "totalDeaths", "Each player's total amount of deaths.");

            Language.Add(titlePrefix + "totalDamageDealt", "Total Damage Dealt");
            Language.Add(bodyPrefix + "totalDamageDealt", "The total amount of damage dealt by each individual player.");

            Language.Add(titlePrefix + "totalMinionDamageDealt", "Total Minion Damage Dealt");
            Language.Add(bodyPrefix + "totalMinionDamageDealt", "placeholder");

            Language.Add(titlePrefix + "totalDamageTaken", "Total Damage Taken");
            Language.Add(bodyPrefix + "totalDamageTaken", "placeholder");

            Language.Add(titlePrefix + "totalHealthHealed", "Total Health Healed");
            Language.Add(bodyPrefix + "totalHealthHealed", "placeholder");

            Language.Add(titlePrefix + "highestDamageDealt", "Highest Damage Dealt");
            Language.Add(bodyPrefix + "highestDamageDealt", "placeholder");

            Language.Add(titlePrefix + "goldCollected", "Gold Collected");
            Language.Add(bodyPrefix + "goldCollected", "placeholder");

            Language.Add(titlePrefix + "totalItemsCollected", "Total Items Collected");
            Language.Add(bodyPrefix + "totalItemsCollected", "placeholder");

            Language.Add(titlePrefix + "totalDistanceTraveled", "Total Distance Traveled");
            Language.Add(bodyPrefix + "totalDistanceTraveled", "placeholder");

            Language.Add(titlePrefix + "totalPurchases", "Total Purchases");
            Language.Add(bodyPrefix + "totalPurchases", "placeholder");

            Language.Add(titlePrefix + "totalGoldPurchases", "Total Gold Purchases");
            Language.Add(bodyPrefix + "totalGoldPurchases", "placeholder");

            Language.Add(titlePrefix + "totalBloodPurchases", "Total Blood Purchases");
            Language.Add(bodyPrefix + "totalBloodPurchases", "placeholder");

            Language.Add(titlePrefix + "totalLunarPurchases", "Total Lunar Purchases");
            Language.Add(bodyPrefix + "totalLunarPurchases", "placeholder");

            Language.Add(titlePrefix + "totalTier1Purchases", "Total Tier 1 Purchases");
            Language.Add(bodyPrefix + "totalTier1Purchases", "placeholder");

            Language.Add(titlePrefix + "totalTier2Purchases", "Total Tier 2 Purchases");
            Language.Add(bodyPrefix + "totalTier2Purchases", "placeholder");

            Language.Add(titlePrefix + "totalTier3Purchases", "Total Tier 3 Purchases");
            Language.Add(bodyPrefix + "totalTier3Purchases", "placeholder");

            Language.Add(titlePrefix + "totalDronesPurchased", "Total Drones Purchased");
            Language.Add(bodyPrefix + "totalDronesPurchased", "placeholder");

            Language.Add(titlePrefix + "totalTurretsPurchased", "Total Turrets Purchased");
            Language.Add(bodyPrefix + "totalTurretsPurchased", "placeholder");

            Language.Add(titlePrefix + "totalGreenSoupsPurchased", "Total Green Soups Purchased");
            Language.Add(bodyPrefix + "totalGreenSoupsPurchased", "placeholder");

            Language.Add(titlePrefix + "totalRedSoupsPurchased", "Total Red Soups Purchased");
            Language.Add(bodyPrefix + "totalRedSoupsPurchased", "placeholder");
            #endregion

            #region Custom Stats
            Language.Add(titlePrefix + "shrinePurchases", "Shrine of Chance Purchases");
            Language.Add(bodyPrefix + "shrinePurchases", "placeholder");

            Language.Add(titlePrefix + "shrineWins", "Shrine of Chance Wins");
            Language.Add(bodyPrefix + "shrineWins", "placeholder");

            Language.Add(titlePrefix + "orderHits", "Shrine of Order Hits");
            Language.Add(bodyPrefix + "orderHits", "placeholder");

            Language.Add(titlePrefix + "timeStill", "Time Still");
            Language.Add(bodyPrefix + "timeStill", "The total amount of time in seconds each player spent standing still while the stopwatch was active.");

            Language.Add(titlePrefix + "timeStillUnsafe", "Time Still While Unsafe");
            Language.Add(bodyPrefix + "timeStillUnsafe", "The total amount of time in seconds each player spent standing still while the stopwatch was active, and the environment is considered" +
                "\"unsafe\": teleporter is not charged, or void fields / void locus is not completed");

            Language.Add(titlePrefix + "timeLowHealth", "Time At Low Health");
            Language.Add(bodyPrefix + "timeLowHealth", "The total amount of time in seconds each player spent with their healh below 25%.");

            Language.Add(titlePrefix + "fallDamage", "Fall Damage");
            Language.Add(bodyPrefix + "fallDamage", "placeholder");

            Language.Add(titlePrefix + "coinsSpent", "Lunar Coins Spent");
            Language.Add(bodyPrefix + "coinsSpent", "placeholder");

            Language.Add(titlePrefix + "avenges", "Avenges");
            Language.Add(bodyPrefix + "avenges", "How many times each player killed an enemy that had harmed another player.");

            Language.Add(titlePrefix + "timesLastStanding", "Times Last Standing");
            Language.Add(bodyPrefix + "timesLastStanding", "placeholder");

            Language.Add(titlePrefix + "itemLead", "Item Lead");
            Language.Add(bodyPrefix + "itemLead", "placeholder");

            Language.Add(titlePrefix + "nonScrapPrinted", "Non-Scrap Printed");
            Language.Add(bodyPrefix + "nonScrapPrinted", "placeholder");

            Language.Add(titlePrefix + "likelyDonations", "Likely Donations");
            Language.Add(bodyPrefix + "likelyDonations", "The total amount of items that each player is estimated to have donated to other players.");

            Language.Add(titlePrefix + "allies", "Allies");
            Language.Add(bodyPrefix + "allies", "The total permanent allies that each player possessed. Counting is finicky - may not support certain modded allies.");

            #endregion
        }
    }
}