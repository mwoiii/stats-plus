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
    public class FullStatsRecord
    {
        StatsRecord[] records = new StatsRecord[2];

        public FullStatsRecord(PlayerCharacterMasterController instance, string name)
        {
            records[0] = new BaseStatsRecord(instance, $"{name}");
            records[1] = new CustomStatsRecord(instance, $"{name}Custom");
        }

        public object Get(string name)
        {
            object stat;
            foreach (StatsRecord i in records)
            {
                stat = i.Get(name);
                if (stat != null) { return stat; }
            }
            return null;
        }

        public string GetAllAsString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (StatsRecord i in records) { sb.AppendLine(i.GetAllAsString()); }
            return sb.ToString();
        }

        public string GetName() { return records[0].GetName(); }

        public bool BelongsTo(PlayerCharacterMasterController instance) { return records[0].BelongsTo(instance); }
    }
}
