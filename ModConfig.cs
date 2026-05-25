using System;
using Vintagestory.API.Common;

namespace Unforgettable
{
    internal static class ModConfig
    {
        public const string FileName = "unforgettable.json";

        public static UnforgettableConfig Current { get; private set; } = new();

        public static void Load(ICoreAPI api)
        {
            try
            {
                var loaded = api.LoadModConfig<UnforgettableConfig>(FileName);
                Current = loaded ?? new UnforgettableConfig();
                api.StoreModConfig(Current, FileName);
            }
            catch (Exception ex)
            {
                api.Logger.Error("[unforgettable] Could not load config, using defaults. {0}", ex);
                Current = new UnforgettableConfig();
            }
        }

        public static void Save(ICoreAPI api) =>
            api.StoreModConfig(Current, FileName);

        public static void SyncFromConfigLib(ICoreAPI api) =>
            Save(api);
    }
}
