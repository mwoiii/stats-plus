using RoR2;

namespace StatsMod.CustomStats
{
    public static class Util
    {
        public static bool IsSafe()
        {
            bool voidLocusSafe = VoidStageMissionController.instance?.numBatteriesActivated >= VoidStageMissionController.instance?.numBatteriesSpawned && VoidStageMissionController.instance?.numBatteriesSpawned > 0;
            return TeleporterInteraction.instance?.isCharged ?? ArenaMissionController.instance?.clearedEffect.activeSelf ?? voidLocusSafe;
        }
    }
}
