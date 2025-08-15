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
            Language.Add(bodyPrefix + "totalMinionDamageDealt", "The total damage dealt by all minions belonging to an individual player.");

            Language.Add(titlePrefix + "totalDamageTaken", "Total Damage Taken");
            Language.Add(bodyPrefix + "totalDamageTaken", "The total damage that each individual player took.");

            Language.Add(titlePrefix + "totalHealthHealed", "Total Health Healed");
            Language.Add(bodyPrefix + "totalHealthHealed", "The total amount of health that each individual player restored.");

            Language.Add(titlePrefix + "highestDamageDealt", "Highest Damage Dealt");
            Language.Add(bodyPrefix + "highestDamageDealt", "The highest damage attack landed by each player.");

            Language.Add(titlePrefix + "goldCollected", "Gold Collected");
            Language.Add(bodyPrefix + "goldCollected", "The total gold collected by each individual player");

            Language.Add(titlePrefix + "totalItemsCollected", "Total Items Collected");
            Language.Add(bodyPrefix + "totalItemsCollected", "Each player's total amount of items.");

            Language.Add(titlePrefix + "totalDistanceTraveled", "Total Distance Traveled");
            Language.Add(bodyPrefix + "totalDistanceTraveled", "The distance that each individual player covered, in metres.");

            Language.Add(titlePrefix + "totalPurchases", "Total Purchases");
            Language.Add(bodyPrefix + "totalPurchases", "The total amount of interactable purchases made by each player");

            Language.Add(titlePrefix + "totalGoldPurchases", "Total Gold Purchases");
            Language.Add(bodyPrefix + "totalGoldPurchases", "The total amount of interactable purchases costing gold made by each player.");

            Language.Add(titlePrefix + "totalBloodPurchases", "Total Blood Purchases");
            Language.Add(bodyPrefix + "totalBloodPurchases", "The total amount of interactable purchases costing health made by each player.");

            Language.Add(titlePrefix + "totalLunarPurchases", "Total Lunar Purchases");
            Language.Add(bodyPrefix + "totalLunarPurchases", "The total amount of interactable purchases costing lunar coins made by each player.");

            Language.Add(titlePrefix + "totalTier1Purchases", "Total Tier 1 Purchases");
            Language.Add(bodyPrefix + "totalTier1Purchases", "The total amount of interactable purchases costing common (white) items made by each player");

            Language.Add(titlePrefix + "totalTier2Purchases", "Total Tier 2 Purchases");
            Language.Add(bodyPrefix + "totalTier2Purchases", "The total amount of interactable purchases costing uncommon (green) items made by each player");

            Language.Add(titlePrefix + "totalTier3Purchases", "Total Tier 3 Purchases");
            Language.Add(bodyPrefix + "totalTier3Purchases", "The total amount of interactable purchases costing legendary (red) items made by each player");

            Language.Add(titlePrefix + "totalDronesPurchased", "Total Drones Purchased");
            Language.Add(bodyPrefix + "totalDronesPurchased", "The total amount of drone interactables purchased by each individual player");

            Language.Add(titlePrefix + "totalTurretsPurchased", "Total Turrets Purchased");
            Language.Add(bodyPrefix + "totalTurretsPurchased", "The total amount of turret interactables purchased by each individual player");

            Language.Add(titlePrefix + "totalGreenSoupsPurchased", "Total Green Soups Purchased");
            Language.Add(bodyPrefix + "totalGreenSoupsPurchased", "The total amount of common-to-uncommon item cauldrons interacted with by each individual player");

            Language.Add(titlePrefix + "totalRedSoupsPurchased", "Total Red Soups Purchased");
            Language.Add(bodyPrefix + "totalRedSoupsPurchased", "The total amount of uncommon-to-legendary item cauldrons interacted with by each individual player");
            #endregion

            #region Custom Stats
            Language.Add(titlePrefix + "shrinePurchases", "Shrine of Chance Purchases");
            Language.Add(bodyPrefix + "shrinePurchases", "The total amount of times a Shrine of Chance was interacted with by an individual player.");

            Language.Add(titlePrefix + "shrineWins", "Shrine of Chance Wins");
            Language.Add(bodyPrefix + "shrineWins", "The total amount of times a Shrine of Chance was successfully interacted with by an individual player.");

            Language.Add(titlePrefix + "orderHits", "Shrine of Order Hits");
            Language.Add(bodyPrefix + "orderHits", "The total amount of times a Shrine of Order was interacted with by an individual player.");

            Language.Add(titlePrefix + "timeStill", "Time Still");
            Language.Add(bodyPrefix + "timeStill", "The total amount of time in seconds each player spent standing still while the stopwatch was active.");

            Language.Add(titlePrefix + "timeStillUnsafe", "Time Still While Unsafe");
            Language.Add(bodyPrefix + "timeStillUnsafe", "The total amount of time in seconds each player spent standing still while the stopwatch was active, and the environment is considered" +
                "\"unsafe\": teleporter is not charged, or void fields / void locus is not completed");

            Language.Add(titlePrefix + "timeLowHealth", "Time At Low Health");
            Language.Add(bodyPrefix + "timeLowHealth", "The total amount of time in seconds each player spent with their healh below 25%.");

            Language.Add(titlePrefix + "fallDamage", "Fall Damage");
            Language.Add(bodyPrefix + "fallDamage", "The total amount of fall damage received by each player.");

            Language.Add(titlePrefix + "coinsSpent", "Lunar Coins Spent");
            Language.Add(bodyPrefix + "coinsSpent", "The total amount of lunar coins spent by each player.");

            Language.Add(titlePrefix + "avenges", "Avenges");
            Language.Add(bodyPrefix + "avenges", "How many times each player killed an enemy that had harmed another player.");

            Language.Add(titlePrefix + "timesLastStanding", "Times Last Standing");
            Language.Add(bodyPrefix + "timesLastStanding", "The amount of times a player was the last player standing.");

            Language.Add(titlePrefix + "itemLead", "Item Lead");
            Language.Add(bodyPrefix + "itemLead", "The item lead of the player with the most items to the player with the second most items.");

            Language.Add(titlePrefix + "nonScrapPrinted", "Non-Scrap Printed");
            Language.Add(bodyPrefix + "nonScrapPrinted", "The amount of non-scrap items spent by each individual player.");

            Language.Add(titlePrefix + "likelyDonations", "Likely Donations");
            Language.Add(bodyPrefix + "likelyDonations", "The total amount of items that each player is estimated to have donated to other players.");

            Language.Add(titlePrefix + "allies", "Allies");
            Language.Add(bodyPrefix + "allies", "The total permanent allies that each player possessed. Counting is finicky - may not support certain modded allies.");

            #endregion
        }
    }
}