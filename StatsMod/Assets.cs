using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using Path = System.IO.Path;

namespace StatsMod
{
    public static class Assets
    {
        public static AssetBundle mainAssetBundle = null;
        internal static string assetBundleName = "statsmodassets";

        internal static string assemblyDir
        {
            get
            {
                return Path.GetDirectoryName(StatsMod.pluginInfo.Location);
            }
        }

        public static void PopulateAssets()
        {
            mainAssetBundle = AssetBundle.LoadFromFile(Path.Combine(assemblyDir, assetBundleName));
        }
    }
}