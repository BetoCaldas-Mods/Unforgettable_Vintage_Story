using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Unforgettable
{
    public class HudRenderer : IRenderer, IDisposable
    {
        private const float MaxBlinkFrequency = 2f;

        public double RenderOrder => 1.0;
        public int RenderRange => 0;

        private readonly ICoreClientAPI _api;

        private LoadedTexture? _ovenTexture;
        private LoadedTexture? _firepitTexture;
        private LoadedTexture? _crucibleTexture;
        private bool _ovenLoadAttempted;
        private bool _firepitLoadAttempted;
        private bool _crucibleLoadAttempted;

        private readonly Dictionary<string, float> _ovenBlinkTimers = new();
        private readonly Dictionary<string, float> _firepitBlinkTimers = new();
        private readonly Dictionary<string, float> _crucibleBlinkTimers = new();
        private readonly Dictionary<string, bool> _ovenWasActive = new();
        private readonly Dictionary<string, bool> _firepitWasActive = new();
        private readonly Dictionary<string, bool> _crucibleWasActive = new();

        public HudRenderer(ICoreClientAPI api)
        {
            _api = api;
            api.Event.RegisterRenderer(this, EnumRenderStage.Ortho);
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            EnsureOvenTextureLoaded();
            EnsureFirepitTextureLoaded();
            EnsureCrucibleTextureLoaded();

            float iconSize = ModConfig.Current.HudIconSize;
            float marginLeft = ModConfig.Current.HudMarginLeft;
            float marginTop = ModConfig.Current.HudMarginTop;
            float iconGap = ModConfig.Current.HudIconGap;
            float bandGap = ModConfig.Current.HudBandGap;

            float ovenY = marginTop;
            RenderBand(
                deltaTime,
                AlarmSystem.Instance?.HudStates,
                _ovenTexture,
                _ovenBlinkTimers,
                _ovenWasActive,
                ovenY,
                marginLeft,
                iconSize,
                iconGap);

            float potBandY = ovenY + iconSize + bandGap;
            RenderBand(
                deltaTime,
                FirepitAlarmSystem.Instance?.HudStates,
                _firepitTexture,
                _firepitBlinkTimers,
                _firepitWasActive,
                potBandY,
                marginLeft,
                iconSize,
                iconGap);

            float crucibleBandY = potBandY + iconSize + bandGap;
            RenderBand(
                deltaTime,
                CrucibleAlarmSystem.Instance?.HudStates,
                _crucibleTexture,
                _crucibleBlinkTimers,
                _crucibleWasActive,
                crucibleBandY,
                marginLeft,
                iconSize,
                iconGap);
        }

        private void RenderBand(
            float deltaTime,
            IReadOnlyDictionary<string, HudState>? states,
            LoadedTexture? texture,
            Dictionary<string, float> blinkTimers,
            Dictionary<string, bool> wasActive,
            float y,
            float marginLeft,
            float iconSize,
            float iconGap)
        {
            if (states == null || states.Count == 0)
            {
                blinkTimers.Clear();
                wasActive.Clear();
                return;
            }

            if (texture == null || texture.TextureId <= 0) return;

            var active = states
                .Where(kv => kv.Value.IsActive)
                .OrderBy(kv => kv.Key)
                .ToList();

            var activeKeys = new HashSet<string>(active.Select(kv => kv.Key));
            foreach (string key in blinkTimers.Keys.ToList())
            {
                if (!activeKeys.Contains(key))
                {
                    blinkTimers.Remove(key);
                    wasActive.Remove(key);
                }
            }

            for (int i = 0; i < active.Count; i++)
            {
                string key = active[i].Key;
                HudState state = active[i].Value;
                float x = marginLeft + i * (iconSize + iconGap);
                RenderStationIcon(deltaTime, state, texture, key, x, y, iconSize, blinkTimers, wasActive);
            }
        }

        private void RenderStationIcon(
            float deltaTime,
            HudState state,
            LoadedTexture texture,
            string key,
            float x,
            float y,
            float iconSize,
            Dictionary<string, float> blinkTimers,
            Dictionary<string, bool> wasActive)
        {
            bool isActive = state.IsActive;
            wasActive.TryGetValue(key, out bool prevActive);

            if (!prevActive && isActive)
                blinkTimers[key] = 0f;
            wasActive[key] = isActive;

            if (!isActive) return;

            if (!blinkTimers.TryGetValue(key, out float blinkTimer))
                blinkTimer = blinkTimers[key] = 0f;

            blinkTimer += deltaTime;
            blinkTimers[key] = blinkTimer;

            if (!IsIconVisible(state, blinkTimer)) return;

            _api.Render.Render2DTexture(texture.TextureId, x, y, iconSize, iconSize, 50f);
        }

        private static bool IsIconVisible(HudState state, float timer)
        {
            float frequency = state.IsDone ? MaxBlinkFrequency : state.Progress * MaxBlinkFrequency;
            if (frequency < 0.05f) return true;

            float halfPeriod = 0.5f / frequency;
            return (timer % (halfPeriod * 2f)) < halfPeriod;
        }

        private void EnsureOvenTextureLoaded()
        {
            if (_ovenTexture != null && _ovenTexture.TextureId > 0) return;
            if (_ovenLoadAttempted) return;
            _ovenLoadAttempted = true;
            _ovenTexture = LoadTexture("unforgettable:textures/oven_timer_inverted_transparent.png");
        }

        private void EnsureFirepitTextureLoaded()
        {
            if (_firepitTexture != null && _firepitTexture.TextureId > 0) return;
            if (_firepitLoadAttempted) return;
            _firepitLoadAttempted = true;
            _firepitTexture = LoadTexture("unforgettable:textures/cooking_pot_timer_inverted_transparent.png");
        }

        private void EnsureCrucibleTextureLoaded()
        {
            if (_crucibleTexture != null && _crucibleTexture.TextureId > 0) return;
            if (_crucibleLoadAttempted) return;
            _crucibleLoadAttempted = true;
            _crucibleTexture = LoadTexture("unforgettable:textures/crucible_timer_inverted_transparent.png");
        }

        private LoadedTexture? LoadTexture(string assetPath)
        {
            try
            {
                var asset = _api.Assets.TryGet(new AssetLocation(assetPath));
                if (asset == null)
                {
                    _api.Logger.Error("[unforgettable] HudRenderer: asset '{0}' não encontrado", assetPath);
                    return null;
                }

                var texture = new LoadedTexture(_api);
                BitmapRef bmp = asset.ToBitmap(_api);
                _api.Render.LoadTexture(bmp, ref texture);
                bmp.Dispose();

                _api.Logger.Notification("[unforgettable] HudRenderer: '{0}' carregada — TextureId={1}", assetPath, texture.TextureId);
                return texture;
            }
            catch (Exception ex)
            {
                _api.Logger.Error("[unforgettable] HudRenderer: erro ao carregar '{0}' — {1}", assetPath, ex.Message);
                return null;
            }
        }

        public void Dispose()
        {
            _api.Event.UnregisterRenderer(this, EnumRenderStage.Ortho);
            _ovenTexture?.Dispose();
            _firepitTexture?.Dispose();
            _crucibleTexture?.Dispose();
        }
    }
}
