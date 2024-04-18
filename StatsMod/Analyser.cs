using RoR2;
using RoR2.Stats;
using BepInEx;
using R2API;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Text;

// stats todo: total procs (REQUIRES IL: to check if item procs off a hit), printing w/o scrap (REQUIRES IL: to access item being used to print),
// allies health healed (REQUIRES IL: to even be able to track who the healing came from),
// items donated (REQUIRES IL: probably. can't even figure out how to do this one), items CLEARLY donated (pinged after purchase) (REQUIRES IL: see before),

// minion-related items at end of run (or peak), "stupid" deaths, stupid things to get hit by?

namespace StatsMod
{
    public class Analyser
    {

        private List<PlayerStatsDatabase> statsDatabase;

        private List<Award> awards = [];

        // Choose a selection of awards for the run? Or calculate for all awards and choose most prevalent?

        // Method for each award analysis

        public Analyser(List<PlayerStatsDatabase> statsDatabase)
        {
            this.statsDatabase = statsDatabase;
            
            EnlightenedAward();
            TricksterAward();
        }

        private void EnlightenedAward()
        {
            // Sufficient portion time still in danger, and other "bold" behaviours tbd
            string highestPlayer = null;
            float highestTime = -1;

            foreach (PlayerStatsDatabase db in statsDatabase) 
            {
                Dictionary<string, object> playerStats = db.GetRecord(-1);
                float playerTime = (float)playerStats["timeStillUnsafe"];
                bool isLegible = (playerTime / (float)Convert.ToDouble(PlayerStatsDatabase.Numberise(playerStats["totalTimeAlive"]))) >= 0.2;  // Required proportion in order to be legible for award
                if (isLegible)
                {
                    if (playerTime > highestTime)
                    {
                        highestPlayer = db.GetPlayerName();
                        highestTime = playerTime;
                    }
                }
            }
            
            if (highestPlayer != null)
            {
                const string name = "Enlightened";
                const string desc = "There's never a bad time to stop, think, and reflect on your journey. This player pondered the meaning of life, and found it, hopefully.";
                awards.Add(new Award(name, desc, highestPlayer));
                // Log.Info($"{highestPlayer} is the most enlightened!");
            }
        }

        private void ChefAward()
        {
            // Identifying significant spikes in certain stats across stages, e.g. total damage dealt, max damage dealt. Correlate with printer usage
            const string name = "Chef";
            const string desc = "It's no wonder it smells so good in here, because damn, this player cooked.";
        }

        private void GamblerAward()
        {
            // Shrine of chance hits, shrine of order hits, uses of printers/soups without scrap
            const string name = "Gambler";
            const string desc = "This player knew that 99% of gamblers quit right before they hit it big, and made sure to be part of the 1%.";
        }

        private void TeamPlayerAward()
        {
            // (MULTIPLAYER ONLY) -- Killing enemies that hurt teammates, healing allies with bungus/woodsprite, purchasing items and having them be picked up by others (bonus for pings?)
            const string name = "Team Player";
            const string desc = "Through charity or physical support, this player had everybody's backs.";
        }

        private void IronWillAward()
        {
            // Risky things done without ever dying e.g. shrine of order, time on low health, being last one standing for a stage
            const string name = "Iron Will";
            const string desc = "Be it bad luck or a self-imposed challenge, this player went through it all- and survived.";
        }

        private void SenileAward()
        {
            // Fall damage taken, low move speed / distance travelled
            const string name = "Senile";
            const string desc = "These old bones don't get you very far... especially not when they're all broken from repeated falls.";
        }

        private void CompanionAward()
        {
            // Drones & turrets purchased consistency, other minion items quantity at end of game, minion damage
            const string name = "Companion";
            const string desc = "You can never have too many friends. This player forged many bonds, and stayed true until the end.";
        }

        private void MiserAward()
        {
            // (MULTIPLAYER ONLY) items taken at a sufficient item lead (of course, if there are other players alive)
            const string name = "Miser";
            const string desc = "Bah, humbug. This player was uncharitable at other's times of need.";
        }

        private void JinxedAward()
        {
            // (MULTIPLAYER ONLY) Lots of deaths, more than average shrine fails, significantly lower stats compared to other players
            const string name = "Jinxed";
            const string desc = "...At least it's over now. This player didn't seem to have a very good run. Offer them a hug.";
        }

        private void AthleteAward()
        {
            // High move speed, distance travelled, not much damage
            const string name = "Athlete";
            const string desc = "It would appear that this player was more concerned with running laps than defeating the enemies.";
        }

        private void AnarchistAward()
        {
            // End game: high move speed, high attack speed, highest damage dealt, shrines of mountain hit, lunar purchases, high amount of procs
            const string name = "Anarchist";
            const string desc = "";
        }

        private void TricksterAward()
        {
            // Mass lunar coin spending (>=22000). Customise description to include a calculation for how many months/years it would take to get that many lunar coins to spend
            // 12 coins in 30 minutes - lower bound (1 coin per 150s), 14 coins in 25 minutes - upper bound (1 coin per 107 seconds)
            string highestPlayer = null;
            float highestCoins = -1;

            foreach (PlayerStatsDatabase db in statsDatabase)
            {
                Dictionary<string, object> playerStats = db.GetRecord(-1);
                uint coins = (uint)playerStats["coinsSpent"];
                bool isLegible = coins >= 22000;
                if (isLegible)
                {
                    if (coins > highestCoins)
                    {
                        highestPlayer = db.GetPlayerName();
                        highestCoins = coins;
                    }
                }
            }

            if (highestPlayer != null)
            {
                const float monthSeconds = 2592000;
                const float yearSeconds = 31536000;
                float monthsGathering = (highestCoins * 120) / monthSeconds;
                float yearsGathering = (highestCoins * 120) / yearSeconds;

                string timeTaken;
                if (monthsGathering <= 12) { timeTaken = $"{string.Format("{0:N1}", monthsGathering)} months"; }
                else { timeTaken = $"{string.Format("{0:N1}", yearsGathering)} years"; }

                const string name = "Trickster";
                string desc = $"It would have taken {timeTaken} of optimal runs to have gathered the amount of coins that this player spent...";
                awards.Add(new Award(name, desc, highestPlayer));
            }
        }

        private void BrawlerAward()
        {
            // High total damage, low procs (so mostly damage achieved multiplicatively through watches, focus crystals, crowbars etc)
            const string name = "Brawler";
            const string desc = "To hell with proc chains. This player stuck to their guns and used raw finesse to survive the planet.";
        }

        private void IdiotAward()
        {
            // Deaths in stupid places (bazaar), most stage hazards hit (so those poison sacks on that 1 stage 2 variant)
        }


        private void SampleAward()
        {
            const string name = "";
            const string desc = "";
        }

    }
   
}