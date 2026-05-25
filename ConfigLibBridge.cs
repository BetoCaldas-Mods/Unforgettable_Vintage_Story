using ConfigLib;
using Vintagestory.API.Common;

namespace Unforgettable
{
    internal static class ConfigLibBridge
    {
        private const string Domain = "unforgettable";

        public static void TryIntegrate(ICoreAPI api)
        {
            if (!api.ModLoader.IsModEnabled("configlib")) return;

            ConfigLibModSystem configLib;
            try
            {
                configLib = api.ModLoader.GetModSystem<ConfigLibModSystem>();
            }
            catch
            {
                api.Logger.Warning("[unforgettable] ConfigLib is enabled but ConfigLibModSystem was not found.");
                return;
            }

            UnforgettableConfigPanel.BindApi(api);
            RegisterConfigPanel(api, configLib);
            SubscribeToConfigEvents(api, configLib);
        }

        private static void RegisterConfigPanel(ICoreAPI api, ConfigLibModSystem configLib)
        {
            if (api.Side == EnumAppSide.Client && api.ModLoader.IsModEnabled("vsimgui"))
            {
                configLib.RegisterCustomConfig(Domain, UnforgettableConfigPanel.Draw);
                return;
            }

            configLib.RegisterCustomManagedConfig(
                Domain,
                ModConfig.Current,
                ModConfig.FileName,
                onConfigSaved: () => ModConfig.SyncFromConfigLib(api));
        }

        private static void SubscribeToConfigEvents(ICoreAPI api, ConfigLibModSystem configLib)
        {
            configLib.SettingChanged += (domain, _, setting) =>
            {
                if (!BelongsToMod(domain)) return;
                setting.AssignSettingValue(ModConfig.Current);
                ModConfig.SyncFromConfigLib(api);
            };

            configLib.ConfigsLoaded += () =>
            {
                configLib.GetConfig(Domain)?.AssignSettingsValues(ModConfig.Current);
                ModConfig.SyncFromConfigLib(api);
            };
        }

        private static bool BelongsToMod(string domain) =>
            domain == Domain || domain.StartsWith(Domain + " - ");
    }
}
