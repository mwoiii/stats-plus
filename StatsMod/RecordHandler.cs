using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatsMod.CustomStats;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using RoR2;
namespace StatsMod
{
    public static class RecordHandler
    {
        private static List<PlayerStatsDatabase> statsDatabase;
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

        private static void ResetDatabase(Run run) // Empties all the data dictionaries & sets up a new database
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

        private static void GameOverReport(Run run, GameEndingDef gameEndingDef)
        {
            if (!NetworkServer.active) { return; }

            TakeRecord();
            ReportToLog(); // TEST: Automatically logs the end of game stats
            Analyser analyser = new Analyser(statsDatabase);
        }

        // Misc methods

        private static void SetupDatabase() // Sets up StatsDatabase for the players of a new run
        {
            if (!NetworkServer.active) { return; }

            statsDatabase = [];
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

        public static void GetRScript()
        {
            if (!NetworkServer.active) { return; }

            StringBuilder a = new();

            List<string> names = [];

            a.AppendLine(statsDatabase[0].GetStatSeriesAsString("timestamps", true));
            a.AppendLine();

            foreach (PlayerStatsDatabase i in statsDatabase)
            {
                names.Add(i.GetPlayerName().Replace(" ", ""));

                foreach (string j in PlayerStatsDatabase.allStats)
                {
                    a.AppendLine($"{names.Last()}.{i.GetStatSeriesAsString(j, true)}");
                }

                a.AppendLine();
            }

            string[] cols = ["red", "blue", "green", "yellow"]; // This assumes there is no mod increasing the player count from max of 4

            string colIndicator = "#";
            for (int i = 0; i < names.Count; i++)
            {
                colIndicator += $"{cols[i]} for {names[i]}, ";
            }
            colIndicator = colIndicator.Substring(0, Math.Max(0, colIndicator.Length - 2));

            foreach (string stat in PlayerStatsDatabase.allStats)
            {
                a.AppendLine(colIndicator);
                string b = "";
                foreach (string name in names) { b += ($"{name}.{stat}, "); }
                b = b.Substring(0, Math.Max(0, b.Length - 2));
                a.AppendLine($"yLimit <- c(min({b}), max({b}))");

                a.AppendLine($"plot(timestamps, {names[0]}.{stat}, type = \"b\", col = \"{cols[0]}\", ylab = \"{stat}\", xlab = \"timestamp\", main = \"{stat}\", ylim = yLimit)");
                for (int i = 1; i < names.Count; i++)
                {
                    a.AppendLine($"points(timestamps, {names[i]}.{stat}, type = \"b\", col=\"{cols[i]}\")");
                }
                a.AppendLine();
            }
            Log.Info(a.ToString());
        }

        private static void ResetBodyCounter()
        {
            bodiesCounter = 0;
            CharacterBody.onBodyStartGlobal += CheckTakeRecord;
        }
    }
}
