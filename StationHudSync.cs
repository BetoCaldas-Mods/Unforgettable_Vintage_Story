using System.Collections.Generic;
using System.Linq;

namespace Unforgettable
{
    internal static class StationHudSync
    {
        public static void Refresh(
            Dictionary<string, HudState> hudStates,
            Dictionary<string, SlotPhase> phases,
            Dictionary<string, float> progress)
        {
            var activeKeys = new HashSet<string>();
            foreach (var kv in phases)
            {
                if (kv.Value == SlotPhase.None) continue;
                activeKeys.Add(kv.Key);
                if (!hudStates.TryGetValue(kv.Key, out HudState? hs))
                    hudStates[kv.Key] = hs = new HudState();
                hs.IsActive = true;
                hs.IsDone = kv.Value == SlotPhase.Done;
                hs.Progress = kv.Value == SlotPhase.Baking && progress.TryGetValue(kv.Key, out float p) ? p : 0f;
            }

            foreach (string key in hudStates.Keys.ToList())
            {
                if (!activeKeys.Contains(key))
                    hudStates.Remove(key);
            }
        }

        public static void NotifyPhaseChange(
            StationAlarmQueue queue,
            SlotPhase oldPhase,
            SlotPhase newPhase,
            string key)
        {
            if (oldPhase != SlotPhase.Done && newPhase == SlotPhase.Done)
                queue.NotifyDone(key);
            else if (oldPhase == SlotPhase.Done && newPhase != SlotPhase.Done)
                queue.NotifyNotDone(key);
        }
    }
}
