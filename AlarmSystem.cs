using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Unforgettable
{
    public enum SlotPhase { None, Baking, Done }

    public class HudState
    {
        public bool IsActive;
        public float Progress;
        public bool IsDone;
    }

    public class AlarmSystem : IDisposable
    {
        private const string AlarmSound = "unforgettable:sounds/oventialarm";
        private const int OvenSlotCount = 4;
        private const int RepeatingAlarmIntervalMs = 5000;

        public static AlarmSystem? Instance;
        public IReadOnlyDictionary<string, HudState> HudStates => _hudStates;

        private readonly ICoreClientAPI _api;
        private readonly Dictionary<string, HudState> _hudStates = new();
        private readonly Dictionary<string, SlotPhase> _phases = new();
        private readonly Dictionary<string, float> _progress = new();
        private readonly Dictionary<string, string?> _prevCodes = new();
        private readonly StationAlarmQueue _alarmQueue;
        private int _syncCount;

        public AlarmSystem(ICoreClientAPI api)
        {
            _api = api;
            _alarmQueue = new StationAlarmQueue(api, AlarmSound, RepeatingAlarmIntervalMs);
        }

        public void OnOvenSynced(BlockEntityOven oven, ITreeAttribute tree)
        {
            if (_api.World.Player?.Entity == null) return;

            _syncCount++;
            bool logThisCycle = _syncCount <= 10 || _syncCount % 20 == 0;

            if (logThisCycle)
            {
                var treeKeys = DumpTree(tree);
                Log($"[Sync #{_syncCount}] Forno {oven.Pos} — tree keys: {treeKeys}");
            }

            for (int i = 0; i < OvenSlotCount; i++)
                UpdateSlot(oven, tree, i, logThisCycle);

            StationHudSync.Refresh(_hudStates, _phases, _progress);

            if (logThisCycle)
                Log($"[Sync #{_syncCount}] forno ativos={_hudStates.Count} done={_phases.Values.Count(p => p == SlotPhase.Done)}");
        }

        private void UpdateSlot(BlockEntityOven oven, ITreeAttribute tree, int slotIndex, bool doLog)
        {
            string key = SlotKey(oven.Pos, slotIndex);
            SlotPhase oldPhase = _phases.GetValueOrDefault(key);
            var slot = oven.Inventory[slotIndex];
            string? currCode = slot?.Itemstack?.Collectible?.Code?.Path;
            _prevCodes.TryGetValue(key, out string? prevCode);

            if (doLog && (currCode != null || oldPhase != SlotPhase.None))
                Log($"  Slot {slotIndex}: currCode={currCode ?? "(vazio)"} prevCode={prevCode ?? "(nenhum)"} phase={oldPhase}");

            if (currCode == null)
            {
                SetPhase(key, SlotPhase.None, doLog, slotIndex, "vazio");
                _progress.Remove(key);
                _prevCodes.Remove(key);
                return;
            }

            bool justBecamePerfect = currCode.EndsWith("-perfect")
                && prevCode?.EndsWith("-partbaked") == true;
            bool wasAlreadyDone = oldPhase == SlotPhase.Done;

            if (justBecamePerfect || wasAlreadyDone)
            {
                SetPhase(key, SlotPhase.Done, doLog, slotIndex, $"Done (justBecamePerfect={justBecamePerfect})");
                _progress.Remove(key);
            }
            else if (currCode.EndsWith("-partbaked"))
            {
                var bp = BakingProperties.ReadFrom(slot!.Itemstack!);
                float bakedLevel = tree.GetFloat("baked" + slotIndex);

                if (doLog)
                    Log($"  Slot {slotIndex}: BakingProperties={bp?.ResultCode ?? "NULL"} bakedLevel={bakedLevel}");

                _phases[key] = SlotPhase.Baking;
                _progress[key] = bp != null && bp.LevelTo > bp.LevelFrom
                    ? CalculateBakingProgress(bakedLevel, bp.LevelFrom, bp.LevelTo)
                    : 0f;

                if (doLog) Log($"  Slot {slotIndex}: → Baking (final) progress={_progress[key]:F2}");
                NotifyIfPhaseChanged(oldPhase, key, SlotPhase.Baking);
            }
            else if (tree.GetFloat("tbake" + slotIndex) > 0)
            {
                _phases[key] = SlotPhase.Baking;
                _progress[key] = 0f;
                if (doLog) Log($"  Slot {slotIndex}: → Baking (inicial) código={currCode}");
                NotifyIfPhaseChanged(oldPhase, key, SlotPhase.Baking);
            }
            else
            {
                SetPhase(key, SlotPhase.None, doLog, slotIndex, $"None (código={currCode})");
                _progress.Remove(key);
            }

            _prevCodes[key] = currCode;
        }

        private void SetPhase(string key, SlotPhase phase, bool doLog, int slotIndex, string reason)
        {
            SlotPhase oldPhase = _phases.GetValueOrDefault(key);
            _phases[key] = phase;
            if (doLog) Log($"  Slot {slotIndex}: → {reason}");
            NotifyIfPhaseChanged(oldPhase, key, phase);
        }

        private void NotifyIfPhaseChanged(SlotPhase oldPhase, string key, SlotPhase newPhase)
        {
            if (oldPhase == newPhase) return;
            StationHudSync.NotifyPhaseChange(_alarmQueue, oldPhase, newPhase, key);
        }

        private static float CalculateBakingProgress(float bakedLevel, float levelFrom, float levelTo)
        {
            float stageRange = levelTo - levelFrom;
            if (stageRange <= 0) return 1f;
            return Math.Clamp((bakedLevel - levelFrom) / stageRange, 0f, 1f);
        }

        private void Log(string msg) =>
            _api.Logger.Notification("[unforgettable] " + msg);

        private static string DumpTree(ITreeAttribute tree)
        {
            var sb = new StringBuilder();
            foreach (var kv in tree)
                sb.Append($"{kv.Key}={kv.Value?.GetValue()} | ");
            return sb.Length > 0 ? sb.ToString() : "(vazio)";
        }

        private static string SlotKey(BlockPos pos, int slot) =>
            $"{pos.X},{pos.Y},{pos.Z},{pos.dimension}:{slot}";

        public void Dispose()
        {
            _alarmQueue.Dispose();
            _hudStates.Clear();
            _phases.Clear();
            _progress.Clear();
            _prevCodes.Clear();
            Instance = null;
        }
    }
}
