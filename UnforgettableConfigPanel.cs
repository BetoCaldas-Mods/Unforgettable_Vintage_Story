using System.Numerics;
using ConfigLib;
using ImGuiNET;
using Vintagestory.API.Common;

namespace Unforgettable
{
    internal static class UnforgettableConfigPanel
    {
        private const float PreviewWidth = 420f;
        private const float PreviewHeight = 236f;

        private static ICoreAPI? _api;

        public static void BindApi(ICoreAPI api) => _api = api;

        public static ControlButtons Draw(string id, ControlButtons buttons)
        {
            DrawPreview();
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            DrawHudLayoutSettings();
            DrawOvenSettings();
            DrawPotSettings();
            DrawCrucibleSettings();
            return buttons;
        }

        private static void DrawPreview()
        {
            ImGui.Text("Layout preview");
            var cfg = ModConfig.Current;
            float scale = PreviewWidth / 1920f;
            float scaledIconSize = cfg.HudIconSize * scale;
            float scaledGap = cfg.HudIconGap * scale;
            float scaledBandGap = cfg.HudBandGap * scale;
            float scaledBlockHeight = HudLayout.CalculateBlockHeight(scaledIconSize, scaledBandGap);
            float scaledBlockWidth = HudLayout.CalculateBandWidth(
                scaledIconSize,
                scaledGap,
                HudLayout.PreviewIconsPerBand);
            var canvasSize = new Vector2(PreviewWidth, PreviewHeight);
            var drawList = ImGui.GetWindowDrawList();
            var canvasOrigin = ImGui.GetCursorScreenPos();
            DrawScreenBackground(drawList, canvasOrigin, canvasSize);

            float baseX = HudLayout.ResolveX(cfg.HudHorizontalPercent, PreviewWidth, scaledBlockWidth);
            float blockTopY = HudLayout.ResolveBlockTopY(cfg.HudVerticalPercent, PreviewHeight, scaledBlockHeight);
            DrawStationBand(drawList, canvasOrigin, baseX, blockTopY, scaledIconSize, scaledGap, 0xFF6B9BD1);
            float potY = blockTopY + scaledIconSize + scaledBandGap;
            DrawStationBand(drawList, canvasOrigin, baseX, potY, scaledIconSize, scaledGap, 0xFF6BD18A);
            float crucibleY = potY + scaledIconSize + scaledBandGap;
            DrawStationBand(drawList, canvasOrigin, baseX, crucibleY, scaledIconSize, scaledGap, 0xFFD1A86B);
            ImGui.Dummy(canvasSize);
        }

        private static void DrawHudLayoutSettings()
        {
            if (!ImGui.CollapsingHeader("HUD layout", ImGuiTreeNodeFlags.DefaultOpen)) return;

            var cfg = ModConfig.Current;
            float horizontal = cfg.HudHorizontalPercent * 100f;
            float vertical = cfg.HudVerticalPercent * 100f;
            float iconSize = cfg.HudIconSize;
            float iconGap = cfg.HudIconGap;
            float bandGap = cfg.HudBandGap;
            bool changed = false;

            if (ImGui.SliderFloat("Left margin", ref horizontal, 0f, 100f, "%.0f%%"))
            {
                cfg.HudHorizontalPercent = horizontal / 100f;
                changed = true;
            }

            if (ImGui.SliderFloat("Top margin", ref vertical, 0f, 100f, "%.0f%%"))
            {
                cfg.HudVerticalPercent = vertical / 100f;
                changed = true;
            }

            if (ImGui.SliderFloat("Icon size", ref iconSize, 32f, 256f, "%.0f px"))
            {
                cfg.HudIconSize = iconSize;
                changed = true;
            }

            if (ImGui.SliderFloat("Gap between icons", ref iconGap, 0f, 64f, "%.0f px"))
            {
                cfg.HudIconGap = iconGap;
                changed = true;
            }

            if (ImGui.SliderFloat("Gap between stations", ref bandGap, 0f, 64f, "%.0f px"))
            {
                cfg.HudBandGap = bandGap;
                changed = true;
            }

            if (changed) PersistChanges();
        }

        private static void DrawOvenSettings()
        {
            if (!ImGui.CollapsingHeader("Clay oven", ImGuiTreeNodeFlags.DefaultOpen)) return;

            var cfg = ModConfig.Current;
            bool repeatAlarm = cfg.OvenRepeatAlarm;
            bool showIcon = cfg.OvenShowIconWhenDone;
            int interval = cfg.OvenAlarmIntervalSeconds;
            bool changed = false;

            if (ImGui.Checkbox("Repeat alarm sound", ref repeatAlarm))
            {
                cfg.OvenRepeatAlarm = repeatAlarm;
                changed = true;
            }

            if (ImGui.Checkbox("Show icon when done", ref showIcon))
            {
                cfg.OvenShowIconWhenDone = showIcon;
                changed = true;
            }

            if (ImGui.SliderInt("Alarm interval (seconds)", ref interval, 1, 60))
            {
                cfg.OvenAlarmIntervalSeconds = interval;
                changed = true;
            }

            if (changed) PersistChanges();
        }

        private static void DrawPotSettings()
        {
            if (!ImGui.CollapsingHeader("Cooking pot", ImGuiTreeNodeFlags.DefaultOpen)) return;

            var cfg = ModConfig.Current;
            bool repeatAlarm = cfg.PotRepeatAlarm;
            bool showIcon = cfg.PotShowIconWhenDone;
            int interval = cfg.PotAlarmIntervalSeconds;
            bool changed = false;

            if (ImGui.Checkbox("Repeat alarm sound", ref repeatAlarm))
            {
                cfg.PotRepeatAlarm = repeatAlarm;
                changed = true;
            }

            if (ImGui.Checkbox("Show icon when done", ref showIcon))
            {
                cfg.PotShowIconWhenDone = showIcon;
                changed = true;
            }

            if (ImGui.SliderInt("Alarm interval (seconds)", ref interval, 1, 60))
            {
                cfg.PotAlarmIntervalSeconds = interval;
                changed = true;
            }

            if (changed) PersistChanges();
        }

        private static void DrawCrucibleSettings()
        {
            if (!ImGui.CollapsingHeader("Crucible", ImGuiTreeNodeFlags.DefaultOpen)) return;

            var cfg = ModConfig.Current;
            bool repeatAlarm = cfg.CrucibleRepeatAlarm;
            bool showIcon = cfg.CrucibleShowIconWhenDone;
            int interval = cfg.CrucibleAlarmIntervalSeconds;
            bool changed = false;

            if (ImGui.Checkbox("Repeat alarm sound", ref repeatAlarm))
            {
                cfg.CrucibleRepeatAlarm = repeatAlarm;
                changed = true;
            }

            if (ImGui.Checkbox("Show icon when done", ref showIcon))
            {
                cfg.CrucibleShowIconWhenDone = showIcon;
                changed = true;
            }

            if (ImGui.SliderInt("Alarm interval (seconds)", ref interval, 1, 60))
            {
                cfg.CrucibleAlarmIntervalSeconds = interval;
                changed = true;
            }

            if (changed) PersistChanges();
        }

        private static void PersistChanges()
        {
            if (_api == null) return;
            ModConfig.SyncFromConfigLib(_api);
        }

        private static void DrawScreenBackground(ImDrawListPtr drawList, Vector2 origin, Vector2 size)
        {
            var bottomRight = origin + size;
            drawList.AddRectFilled(origin, bottomRight, ToColor(0.08f, 0.08f, 0.1f, 1f));
            drawList.AddRect(origin, bottomRight, ToColor(0.45f, 0.45f, 0.5f, 1f));
        }

        private static void DrawStationBand(
            ImDrawListPtr drawList,
            Vector2 canvasOrigin,
            float baseX,
            float y,
            float iconSize,
            float iconGap,
            uint color)
        {
            for (int i = 0; i < 2; i++)
            {
                float x = baseX + i * (iconSize + iconGap);
                var topLeft = canvasOrigin + new Vector2(x, y);
                var bottomRight = topLeft + new Vector2(iconSize, iconSize);
                drawList.AddRectFilled(topLeft, bottomRight, color);
                drawList.AddRect(topLeft, bottomRight, ToColor(1f, 1f, 1f, 0.35f));
            }
        }

        private static uint ToColor(float r, float g, float b, float a) =>
            ImGui.ColorConvertFloat4ToU32(new Vector4(r, g, b, a));
    }
}
