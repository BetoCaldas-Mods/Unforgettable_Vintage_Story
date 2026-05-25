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
        private CrucibleAlarmSystem? _crucibleAlarmSystem;
        private HudRenderer? _hudRenderer;
        private Harmony? _harmony;

        public override void StartPre(ICoreAPI api)
        {
            _harmony = new Harmony(HarmonyId);
            _harmony.PatchAll();
        }

        public override void Start(ICoreAPI api)
        {
            ModConfig.Load(api);
            ConfigLibBridge.TryIntegrate(api);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            _alarmSystem = new AlarmSystem(api);
            AlarmSystem.Instance = _alarmSystem;

            _firepitAlarmSystem = new FirepitAlarmSystem(api);
            FirepitAlarmSystem.Instance = _firepitAlarmSystem;

            _crucibleAlarmSystem = new CrucibleAlarmSystem(api);
            CrucibleAlarmSystem.Instance = _crucibleAlarmSystem;

            _hudRenderer = new HudRenderer(api);

            api.Logger.Notification("[unforgettable] Unforgettable started");
        }

        public override void Dispose()
        {
            _hudRenderer?.Dispose();
            _alarmSystem?.Dispose();
            _firepitAlarmSystem?.Dispose();
            _crucibleAlarmSystem?.Dispose();
            _harmony?.UnpatchAll(HarmonyId);
            base.Dispose();
        }
    }
}
