using UnityEngine;
using Path = System.IO.Path;

namespace StatsMod {
    public static class Assets {
        public static AssetBundle assetBundle = null;
        internal static string assetBundleName = "statsmodassets";

        internal static string assemblyDir {
            get {
                return Path.GetDirectoryName(StatsMod.pluginInfo.Location);
            }
        }

        public static void Init() {
            assetBundle = AssetBundle.LoadFromFile(Path.Combine(assemblyDir, assetBundleName));
        }
    }
}