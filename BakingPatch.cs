using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Unforgettable
{
    [HarmonyPatch(typeof(BlockEntityOven), nameof(BlockEntityOven.FromTreeAttributes))]
    public static class BakingPatch
    {
        public static void Postfix(BlockEntityOven __instance, ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            if (worldForResolving.Side != EnumAppSide.Client) return;
            AlarmSystem.Instance?.OnOvenSynced(__instance, tree);
        }
    }
}
