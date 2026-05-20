using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Unforgettable
{
    [HarmonyPatch(typeof(BlockEntityFirepit), nameof(BlockEntityFirepit.FromTreeAttributes))]
    public static class FirepitPatch
    {
        public static void Postfix(BlockEntityFirepit __instance, ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            if (worldForResolving.Side != EnumAppSide.Client) return;
            FirepitAlarmSystem.Instance?.OnFirepitSynced(__instance, tree);
        }
    }
}
