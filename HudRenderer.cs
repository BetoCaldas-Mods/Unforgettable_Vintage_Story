using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Unforgettable
{
    public class HudRenderer : IRenderer, IDisposable
    {
        private const float IconSize      = 80f;
        private const float IconMarginLeft = 20f;
        private const float IconGap        = 10f;
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

        private float _ovenBlinkTimer;
        private float _firepitBlinkTimer;
        private float _crucibleBlinkTimer;
        private bool _ovenWasActive;
        private bool _firepitWasActive;
        private bool _crucibleWasActive;

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

            RenderIcon(
                deltaTime,
                AlarmSystem.Instance?.HudState,
                _ovenTexture,
                ref _ovenBlinkTimer,
                ref _ovenWasActive,
                iconIndex: 0
            );

            RenderIcon(
                deltaTime,
                FirepitAlarmSystem.Instance?.HudState,
                _firepitTexture,
                ref _firepitBlinkTimer,
                ref _firepitWasActive,
                iconIndex: 1
            );

            RenderIcon(
                deltaTime,
                CrucibleAlarmSystem.Instance?.HudState,
                _crucibleTexture,
                ref _crucibleBlinkTimer,
                ref _crucibleWasActive,
                iconIndex: 2
            );
        }

        private void RenderIcon(
            float deltaTime,
            HudState? state,
            LoadedTexture? texture,
            ref float blinkTimer,
            ref bool wasActive,
            int iconIndex)
        {
            bool isActive = state != null && state.IsActive;

            if (!wasActive && isActive)
                blinkTimer = 0f;
            wasActive = isActive;

            if (!isActive) return;

            if (texture == null || texture.TextureId <= 0) return;

            blinkTimer += deltaTime;

            if (!IsIconVisible(state!, blinkTimer)) return;

            float x = IconMarginLeft;
            float totalHeight = (_api.Render.FrameHeight - IconSize) / 2f;
            float y = totalHeight + iconIndex * (IconSize + IconGap);

            _api.Render.Render2DTexture(texture.TextureId, x, y, IconSize, IconSize, 50f);
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
