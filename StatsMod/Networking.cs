using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
namespace StatsMod;
using R2API.Networking;
using UnityEngine;

[System.Serializable]
public class IndependentEntry {
    public readonly string playerName;

    public readonly Dictionary<string, List<object>> Database = [];

    public IndependentEntry(Dictionary<string, List<object>> Database, string playerName) {
        this.Database = Database;
        this.playerName = playerName;
    }

    public List<object> GetStatSeries(string name) {
        return Database[name];
    }

    public string GetStatSeriesAsString(string name, bool rVector = false) // For logging porpoises
    {
        List<object> series = GetStatSeries(name);
        StringBuilder a = new();
        foreach (object entry in series) {
            if (!rVector) { a.Append($"{entry}, "); } else { a.Append($"{PlayerStatsDatabase.Numberise(entry)}, "); }
        }
        string b = a.ToString().Substring(0, Math.Max(0, a.Length - 2));

        if (!rVector) { return $"{name}: {b}"; } else { return $"{name} <- c({b})"; }

    }

    public Dictionary<string, object> GetRecord(int index) {
        if (index < 0) { index = Database["maxHealth"].Count + index; }
        Dictionary<string, object> Record = [];
        foreach (string statName in PlayerStatsDatabase.allStats) { Record.Add(statName, Database[statName][index]); }
        return Record;
    }

    public Dictionary<string, object> GetRecord(float time) {
        List<object> timestamps = Database["timestamps"];
        int index;

        try { index = timestamps.IndexOf(time); } catch { index = timestamps.IndexOf(timestamps.OrderBy(x => Math.Abs(float.Parse(x.ToString()) - time)).First()); }

        return GetRecord(index);
    }
}

public class DatabaseSender : MonoBehaviour {
    int index = 0;

    List<string> substrings;

    private void Awake() {
        var serializedDB = JsonConvert.SerializeObject(RecordHandler.independentDatabase);
        int interval = 256;
        substrings = new List<string>();

        for (int i = 0; i < serializedDB.Length; i += interval) {
            int length = Math.Min(interval, serializedDB.Length - i);
            substrings.Add(serializedDB.Substring(i, length));
        }

    }
    private void Update() {
        if (index >= substrings.Count) {
            Log.Info("Finished sending database");
            Destroy(this);
            return;
        }
        Log.Info($"Sending database substring {index}");
        NetMessageExtensions.Send(new SyncDatabase(substrings[index], index == substrings.Count - 1), (NetworkDestination)1);
        index++;
    }
}

public class SyncDatabase : INetMessage, ISerializableObject {
    private static string serializedDB = "";

    string chunk;

    bool end;

    public SyncDatabase() { }

    public SyncDatabase(string chunk, bool end) {
        this.chunk = chunk;
        this.end = end;
    }

    public void Serialize(NetworkWriter writer) {
        writer.Write(chunk);
        writer.Write(end);
    }

    public void Deserialize(NetworkReader reader) {
        this.chunk = reader.ReadString();
        this.end = reader.ReadBoolean();
    }

    public void OnReceived() {
        if (NetworkServer.active) {
            return;
        }
        serializedDB = serializedDB + this.chunk;
        Log.Info("Received a database chunk...");
        if (this.end) {
            Log.Info("Finished receiving database!");
            RecordHandler.independentDatabase = JsonConvert.DeserializeObject<List<IndependentEntry>>(serializedDB);
            serializedDB = "";
        }
    }

}
