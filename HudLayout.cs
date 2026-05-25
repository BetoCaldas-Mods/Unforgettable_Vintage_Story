using System;
using Vintagestory.API.Client;

namespace Unforgettable
{
    internal static class HudLayout
    {
        private const int StationBandCount = 3;
        public const int PreviewIconsPerBand = 2;

        public static (float Width, float Height) GetViewportSize(ICoreClientAPI api) =>
            (api.Render.FrameWidth, api.Render.FrameHeight);

        public static float CalculateBlockHeight(float iconSize, float bandGap) =>
            StationBandCount * iconSize + (StationBandCount - 1) * bandGap;

        public static float CalculateBandWidth(float iconSize, float iconGap, int iconCount)
        {
            if (iconCount <= 0) return 0f;
            return iconCount * iconSize + (iconCount - 1) * iconGap;
        }

        public static float ResolveX(float horizontalPercent, float screenWidth, float layoutWidth)
        {
            float clamped = NormalizePercent(horizontalPercent);
            return clamped * Math.Max(0f, screenWidth - layoutWidth);
        }

        public static float ResolveBlockTopY(float verticalPercent, float screenHeight, float blockHeight)
        {
            float clamped = NormalizePercent(verticalPercent);
            return clamped * Math.Max(0f, screenHeight - blockHeight);
        }

        public static float NormalizePercent(float value) => Math.Clamp(value, 0f, 1f);
    }
}
