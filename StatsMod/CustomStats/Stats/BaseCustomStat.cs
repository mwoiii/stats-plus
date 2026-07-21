using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace StatsMod.CustomStats {
    public abstract class BaseCustomStat {
        public virtual void Init() {
            CustomStatTracker.registeredStats.Add(this);
            ConfigureStatsTable();
        }

        public abstract void ConfigureStatsTable();

        public void TryDeserialize(Dictionary<string, JToken> restored) {
            try {
                Deserialize(restored);
            } catch (System.Exception e) {
                Log.Error($"Failed to deserialize custom stat!\n{e}");
            }
        }

        public abstract void Deserialize(Dictionary<string, JToken> restored);
    }
}
