using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Unforgettable
{
    public class HudRenderer : IRenderer, IDisposable
    {
        private const float IconSize = 80f;
        private const float IconMarginLeft = 20f;
        private const float MaxBlinkFrequency = 2f;

        public double RenderOrder => 1.0;
        public int RenderRange => 0;

        private readonly ICoreClientAPI _api;
        private LoadedTexture? _iconTexture;
        private float _blinkTimer;
        private bool _textureLoaded;
        private bool _wasActive;

        public HudRenderer(ICoreClientAPI api)
        {
            _api = api;
            api.Event.RegisterRenderer(this, EnumRenderStage.Ortho);
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            EnsureTextureLoaded();

            var state = AlarmSystem.Instance?.HudState;
            bool isActive = state != null && state.IsActive;

            if (!_wasActive && isActive)
            {
                _blinkTimer = 0f;
                _api.Logger.Notification("[unforgettable] HudRenderer: estado ativo, iniciando renderização do ícone");
            }
            _wasActive = isActive;

            if (!isActive) return;

            if (_iconTexture == null || _iconTexture.TextureId <= 0)
            {
                _api.Logger.Warning("[unforgettable] HudRenderer: textura inválida (TextureId={0})", _iconTexture?.TextureId ?? -1);
                return;
            }

            _blinkTimer += deltaTime;

            if (!IsIconVisible(state!)) return;

            float x = IconMarginLeft;
            float y = (_api.Render.FrameHeight - IconSize) / 2f;

            _api.Render.Render2DTexture(_iconTexture.TextureId, x, y, IconSize, IconSize, 50f);
        }

        private bool IsIconVisible(HudState state)
        {
            float frequency = state.IsDone ? MaxBlinkFrequency : state.Progress * MaxBlinkFrequency;
            if (frequency < 0.05f) return true;

            float halfPeriod = 0.5f / frequency;
            return (_blinkTimer % (halfPeriod * 2f)) < halfPeriod;
        }

        private void EnsureTextureLoaded()
        {
            if (_textureLoaded) return;
            _textureLoaded = true;

            _iconTexture = new LoadedTexture(_api);
            _api.Render.GetOrLoadTexture(
                new AssetLocation("unforgettable:textures/oventimer"),
                ref _iconTexture
            );

            _api.Logger.Notification("[unforgettable] HudRenderer: textura carregada — TextureId={0}", _iconTexture.TextureId);
        }

        public void Dispose()
        {
            _api.Event.UnregisterRenderer(this, EnumRenderStage.Ortho);
            _iconTexture?.Dispose();
        }
    }
}
