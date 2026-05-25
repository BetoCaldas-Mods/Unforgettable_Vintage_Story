using System;
using Vintagestory.API.Common;

namespace Unforgettable
{
    internal static class ModConfig
    {
        private const string ConfigFileName = "unforgettable.json";

        public static UnforgettableConfig Current { get; private set; } = new();

        public static void Load(ICoreAPI api)
        {
            try
            {
                var loaded = api.LoadModConfig<UnforgettableConfig>(ConfigFileName);
                Current = loaded ?? new UnforgettableConfig();
                api.StoreModConfig(Current, ConfigFileName);
            }
            catch (Exception ex)
            {
                api.Logger.Error("[unforgettable] Could not load config, using defaults. {0}", ex);
                Current = new UnforgettableConfig();
            }
        }
    }
}
