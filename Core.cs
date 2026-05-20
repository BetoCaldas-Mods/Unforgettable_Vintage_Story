using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Unforgettable
{
    public class Core : ModSystem
    {
        private const string HarmonyId = "unforgettable";

        private AlarmSystem? _alarmSystem;
        private FirepitAlarmSystem? _firepitAlarmSystem;
        private HudRenderer? _hudRenderer;
        private Harmony? _harmony;

        public override void StartPre(ICoreAPI api)
        {
            _harmony = new Harmony(HarmonyId);
            _harmony.PatchAll();
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            _alarmSystem = new AlarmSystem(api);
            AlarmSystem.Instance = _alarmSystem;

            _firepitAlarmSystem = new FirepitAlarmSystem(api);
            FirepitAlarmSystem.Instance = _firepitAlarmSystem;

            _hudRenderer = new HudRenderer(api);

            api.Logger.Notification("[unforgettable] Unforgettable started");
        }

        public override void Dispose()
        {
            _hudRenderer?.Dispose();
            _alarmSystem?.Dispose();
            _firepitAlarmSystem?.Dispose();
            _harmony?.UnpatchAll(HarmonyId);
            base.Dispose();
        }
    }
}
