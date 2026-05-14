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
        private const float DefaultBrowningPoint = 160f;
        private const float DefaultTimeToBake = 150f;
        private const int RepeatingAlarmIntervalMs = 5000;

        public static AlarmSystem? Instance;
        public HudState HudState { get; } = new();

        private readonly ICoreClientAPI _api;
        private readonly Dictionary<string, SlotPhase> _phases = new();
        private readonly Dictionary<string, float> _progress = new();
        private readonly Dictionary<string, string?> _prevCodes = new();
        private long _repeatingAlarmHandle = -1;
        private int _syncCount;

        public AlarmSystem(ICoreClientAPI api)
        {
            _api = api;
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

            bool wasAnyDone = _phases.Values.Any(p => p == SlotPhase.Done);

            for (int i = 0; i < OvenSlotCount; i++)
                UpdateSlot(oven, tree, i, logThisCycle);

            bool isAnyDone = _phases.Values.Any(p => p == SlotPhase.Done);

            if (!wasAnyDone && isAnyDone) StartRepeatingAlarm();
            else if (wasAnyDone && !isAnyDone) StopRepeatingAlarm();

            RefreshHudState();

            if (logThisCycle)
                Log($"[Sync #{_syncCount}] HudState → IsActive={HudState.IsActive} IsDone={HudState.IsDone} Progress={HudState.Progress:F2}");
        }

        private void UpdateSlot(BlockEntityOven oven, ITreeAttribute tree, int slotIndex, bool doLog)
        {
            string key = SlotKey(oven.Pos, slotIndex);
            var slot = oven.Inventory[slotIndex];
            string? currCode = slot?.Itemstack?.Collectible?.Code?.Path;
            _prevCodes.TryGetValue(key, out string? prevCode);

            if (doLog && (currCode != null || _phases.GetValueOrDefault(key) != SlotPhase.None))
                Log($"  Slot {slotIndex}: currCode={currCode ?? "(vazio)"} prevCode={prevCode ?? "(nenhum)"} phase={_phases.GetValueOrDefault(key)}");

            if (currCode == null)
            {
                _phases[key] = SlotPhase.None;
                _progress.Remove(key);
                _prevCodes.Remove(key);
                return;
            }

            bool justBecamePerfect = currCode.EndsWith("-perfect")
                && prevCode?.EndsWith("-partbaked") == true;

            bool wasAlreadyDone = _phases.GetValueOrDefault(key) == SlotPhase.Done;

            if (justBecamePerfect || wasAlreadyDone)
            {
                _phases[key] = SlotPhase.Done;
                _progress.Remove(key);
                if (doLog) Log($"  Slot {slotIndex}: → Done (justBecamePerfect={justBecamePerfect})");
            }
            else if (currCode.EndsWith("-partbaked"))
            {
                // Item está na fase final de assamento (partbaked → perfect).
                // Tentamos ler BakingProperties para calcular progresso, mas exibimos o ícone mesmo se falhar.
                var bp = BakingProperties.ReadFrom(slot!.Itemstack!);
                if (doLog) Log($"  Slot {slotIndex}: BakingProperties={bp?.ResultCode ?? "NULL"} LevelFrom={bp?.LevelFrom} LevelTo={bp?.LevelTo}");

                float bakedLevel = tree.GetFloat("baked" + slotIndex);
                float itemTemp   = tree.GetFloat("temp"  + slotIndex);
                float browning   = ResolveOrDefault(tree.GetFloat("brown" + slotIndex), DefaultBrowningPoint);

                if (doLog) Log($"  Slot {slotIndex}: bakedLevel={bakedLevel} itemTemp={itemTemp} browning={browning}");

                _phases[key] = SlotPhase.Baking;

                if (bp != null && bp.LevelTo > bp.LevelFrom)
                    _progress[key] = CalculateBakingProgress(bakedLevel, bp.LevelFrom, bp.LevelTo);
                else
                    _progress[key] = 0f;

                if (doLog) Log($"  Slot {slotIndex}: → Baking progress={_progress[key]:F2}");
            }
            else
            {
                _phases[key] = SlotPhase.None;
                _progress.Remove(key);
                if (doLog && currCode != null) Log($"  Slot {slotIndex}: → None (código={currCode})");
            }

            _prevCodes[key] = currCode;
        }

        private static float CalculateBakingProgress(float bakedLevel, float levelFrom, float levelTo)
        {
            float stageRange = levelTo - levelFrom;
            if (stageRange <= 0) return 1f;
            return Math.Clamp((bakedLevel - levelFrom) / stageRange, 0f, 1f);
        }

        private void RefreshHudState()
        {
            bool anyBaking = _phases.Values.Any(p => p == SlotPhase.Baking);
            bool anyDone   = _phases.Values.Any(p => p == SlotPhase.Done);

            bool wasActive = HudState.IsActive;
            HudState.IsActive  = anyBaking || anyDone;
            HudState.IsDone    = anyDone;
            HudState.Progress  = anyBaking
                ? _progress.Values.DefaultIfEmpty(0f).Max()
                : 0f;

            if (!wasActive && HudState.IsActive)
                Log("HudState ficou ATIVO — ícone deve aparecer");
            else if (wasActive && !HudState.IsActive)
                Log("HudState ficou INATIVO — ícone deve sumir");
        }

        private void StartRepeatingAlarm()
        {
            Log("Alarme INICIADO");
            PlayAlarm();
            _repeatingAlarmHandle = _api.Event.RegisterGameTickListener(_ => PlayAlarm(), RepeatingAlarmIntervalMs);
        }

        private void StopRepeatingAlarm()
        {
            if (_repeatingAlarmHandle < 0) return;
            Log("Alarme PARADO");
            _api.Event.UnregisterGameTickListener(_repeatingAlarmHandle);
            _repeatingAlarmHandle = -1;
        }

        private void PlayAlarm()
        {
            var entity = _api.World.Player?.Entity;
            if (entity == null) return;
            _api.World.PlaySoundAt(new AssetLocation(AlarmSound), entity, null, false, 8f, 1f);
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

        private static float ResolveOrDefault(float value, float fallback) =>
            value > 0 ? value : fallback;

        private static string SlotKey(BlockPos pos, int slot) =>
            $"{pos.X},{pos.Y},{pos.Z},{pos.dimension}:{slot}";

        public void Dispose()
        {
            StopRepeatingAlarm();
            _phases.Clear();
            _progress.Clear();
            _prevCodes.Clear();
            Instance = null;
        }
    }
}
