using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatsMod.CustomStats;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using RoR2;
using R2API.Networking;
using System.Security.Principal;
using R2API.Networking.Interfaces;
using static Facepunch.Steamworks.LobbyList.Filter;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
namespace StatsMod
{
    public static class RecordHandler
    {
        public static List<PlayerStatsDatabase> statsDatabase
        {
            get { return _statsDatabase; }
        }

        private static List<PlayerStatsDatabase> _statsDatabase;

        public static List<IndependentEntry> independentDatabase;

        private static int bodiesCounter = 0;

        public static void Init()
        {
            Run.onRunStartGlobal += ResetDatabase;
            SceneExitController.onBeginExit += NextStageBodyReset;
            Run.onServerGameOver += GameOverReport;
            NetworkUser.onNetworkUserLost += DeleteUserRecord;
        }

        private static void DeleteUserRecord(NetworkUser networkUser)
        {
            if (!NetworkServer.active || networkUser.masterController is null) { return; }

            statsDatabase.RemoveAll((x) => x.BelongsTo(networkUser.masterController));
        }

        private static void ResetDatabase(Run run) // Empties all the data dictionaries & sets up a new databases
        {
            if (!NetworkServer.active) { return; }

            Tracker.ResetData();
            SetupDatabase();

            ResetBodyCounter();

            Log.Info("New run, resetting data dicts and database");
        }

        private static void CheckTakeRecord(CharacterBody self) // For a record to be taken at the start of each stage it is ensured that the body for each player exists
        {
            if (!NetworkServer.active) { return; }

            if (self.isPlayerControlled)
            {
                bodiesCounter++;
                if (bodiesCounter == statsDatabase.Count)
                {
                    CharacterBody.onBodyStartGlobal -= CheckTakeRecord; // Avoids this method being called after a record has been made for the stage
                    if (SceneManager.GetActiveScene().name != "bazaar")
                    {
                        TakeRecord();
                    }
                }
            }
        }

        private static void NextStageBodyReset(SceneExitController sceneExitController) // Sets up for a new record to be made by OnBodyStart on the next stage
        {
            if (!NetworkServer.active) { return; }

            ResetBodyCounter();
        }

        private static void CreateIndependentDatabase()
        {
            if (!NetworkServer.active) { return; }

            independentDatabase = [];
            foreach (PlayerStatsDatabase i in statsDatabase)
            {
                independentDatabase.Add(new IndependentEntry(i.Database, i.GetPlayerName()));
            }
        }

        private static void GameOverReport(Run run, GameEndingDef gameEndingDef)
        {
            if (!NetworkServer.active) { return; }

            TakeRecord();
            //ReportToLog(); // TEST: Automatically logs the end of game stats
            CreateIndependentDatabase();
            StatsMod.instance.gameObject.AddComponent<DatabaseSender>();
        }

        // Misc methods

        private static void SetupDatabase() // Sets up StatsDatabase for the players of a new run
        {
            if (!NetworkServer.active) { return; }

            _statsDatabase = [];
            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
            {
                statsDatabase.Add(new PlayerStatsDatabase(player));
            }
            Log.Info($"Successfully setup stats database for {statsDatabase.Count} players");
        }

        private static void TakeRecord() // Takes a record in StatsDatabase of each players associated stats
        {
            if (!NetworkServer.active) { return; }

            foreach (PlayerStatsDatabase i in statsDatabase)
            {
                float timestamp = i.TakeRecord();
                Log.Info($"Successfully made record at {timestamp} for {i.GetPlayerName()}");
            }
        }

        private static void ReportToLog() // Makes a nice report of all the recorded stats to the log.
        {
            if (!NetworkServer.active) { return; }

            StringBuilder a = new();
            foreach (PlayerStatsDatabase i in statsDatabase)
            {
                a.AppendLine($"{i.GetPlayerName()}");
                a.AppendLine(i.GetStatSeriesAsString("timestamps"));
                foreach (string j in PlayerStatsDatabase.allStats)
                {
                    a.AppendLine(i.GetStatSeriesAsString(j));
                }
                a.AppendLine("");
            }
            Log.Info(a.ToString());
        }

        public static string GetRScript()
        {
            if (independentDatabase == null) { CreateIndependentDatabase(); }
                
            StringBuilder a = new(independentDatabase[0].GetStatSeriesAsString("timestamps", true));
            a.AppendLine();

            List<string> names = [];
            string namevec = "";
            string colvec = "";

            for (int u = 0; u < independentDatabase.Count; u++)
            {
                IndependentEntry i = independentDatabase[u];
               
                names.Add(i.playerName);
                
                namevec += $"\"{i.playerName}\", ";
                colvec += $"rainbow({independentDatabase.Count})[{u+1}], ";

                foreach (string j in PlayerStatsDatabase.allStats)
                {
                    a.AppendLine($"player{u}.{i.GetStatSeriesAsString(j, true)}");
                }

                a.AppendLine();
            }
            
            namevec = namevec.Substring(0, Math.Max(0, namevec.Length - 2));
            colvec = colvec.Substring(0, Math.Max(0, colvec.Length - 2));
            string colkey = $"legend(\"topright\", inset=c(-0.25,0), legend=c({namevec}), col = c({colvec}), lty=1, pch=1, bty=\"n\")";

            a.AppendLine("par(xpd = TRUE, mar = c(5, 4, 4, 6))");
            a.AppendLine();

            foreach (string stat in PlayerStatsDatabase.allStats)
            {
                string b = "";
                for (int i = 0; i < names.Count; i++) { b += ($"player{i}.{stat}, "); }
                b = b.Substring(0, Math.Max(0, b.Length - 2));
                a.AppendLine($"yLimit <- c(min({b}), max({b}))");

                a.AppendLine($"plot(timestamps, player0.{stat}, type = \"b\", col = rainbow({names.Count})[1], ylab = \"{stat}\", xlab = \"timestamp\", main = \"{stat}\", ylim = yLimit)");
                for (int i = 1; i < names.Count; i++)
                {
                    a.AppendLine($"points(timestamps, player{i}.{stat}, type = \"b\", col=rainbow({names.Count})[{i+1}])");
                }
                a.AppendLine(colkey);
                a.AppendLine();
            }
            return a.ToString();
        }

        private static void ResetBodyCounter()
        {
            bodiesCounter = 0;
            CharacterBody.onBodyStartGlobal += CheckTakeRecord;
        }
    }
}
