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
    public class Analyser
    {

        private List<PlayerStatsDatabase> statsDatabase;

        // Choose a selection of awards for the run? Or calculate for all awards and choose most prevalent?

        // Method for each award analysis

        public Analyser(List<PlayerStatsDatabase> statsDatabase)
        {
            this.statsDatabase = statsDatabase;
            
            EnlightenedAward();
        }

        private void EnlightenedAward() // If time still before tp is sufficient, award to player with highest value
        {
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
                Log.Info($"{highestPlayer} is the most enlightened!");
            }
        }

        private void CookAward()
        {

        }

        private void GamblerAward()
        {

        }

        private void TeamPlayerAward()
        {

        }
    }
   
}