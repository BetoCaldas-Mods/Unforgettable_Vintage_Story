using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Unforgettable
{
    internal class StationAlarmQueue : IDisposable
    {
        private readonly ICoreClientAPI _api;
        private readonly AssetLocation _sound;
        private readonly Func<bool> _repeatAlarm;
        private readonly Func<int> _intervalMs;
        private readonly List<string> _order = new();
        private long _handle = -1;

        public StationAlarmQueue(
            ICoreClientAPI api,
            string soundPath,
            Func<bool> repeatAlarm,
            Func<int> intervalMs)
        {
            _api = api;
            _sound = new AssetLocation(soundPath);
            _repeatAlarm = repeatAlarm;
            _intervalMs = intervalMs;
        }

        public void NotifyDone(string key)
        {
            if (_order.Contains(key)) return;
            _order.Add(key);
            if (_repeatAlarm())
            {
                if (_order.Count == 1) StartRepeating();
            }
            else
            {
                Play();
            }
        }

        public void NotifyNotDone(string key)
        {
            if (!_order.Remove(key)) return;
            if (_order.Count == 0) Stop();
        }

        private void StartRepeating()
        {
            Play();
            _handle = _api.Event.RegisterGameTickListener(_ => Play(), _intervalMs());
        }

        private void Stop()
        {
            if (_handle < 0) return;
            _api.Event.UnregisterGameTickListener(_handle);
            _handle = -1;
        }

        private void Play()
        {
            if (_order.Count == 0) return;
            var entity = _api.World.Player?.Entity;
            if (entity == null) return;
            _api.World.PlaySoundAt(_sound, entity, null, false, 8f, 1f);
        }

        public void Dispose() => Stop();
    }
}
