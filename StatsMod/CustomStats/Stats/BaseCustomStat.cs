using System.Collections.Generic;

namespace StatsMod.CustomStats {
    public abstract class BaseCustomStat {
        public virtual void Init() {
            CustomStatTracker.registeredStats.Add(this);
            ConfigureStatsTable();
        }

        public abstract void ConfigureStatsTable();

        public abstract void Deserialize(Dictionary<string, object> restored);
    }
}
