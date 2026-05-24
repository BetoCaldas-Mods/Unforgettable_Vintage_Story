using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Unforgettable
{
    public class CrucibleAlarmSystem : IDisposable
    {
        private const string AlarmSound = "unforgettable:sounds/crucible_sound";
        private const int RepeatingAlarmIntervalMs = 5000;
        private const float CompletionThresholdFraction = 0.88f;

        public static CrucibleAlarmSystem? Instance;
        public IReadOnlyDictionary<string, HudState> HudStates => _hudStates;

        private readonly ICoreClientAPI _api;
        private readonly Dictionary<string, HudState> _hudStates = new();
        private readonly Dictionary<string, SlotPhase> _phases = new();
        private readonly Dictionary<string, float> _progress = new();
        private readonly Dictionary<string, float> _prevOreTimes = new();
        private readonly Dictionary<string, float> _prevMaxTimes = new();
        private readonly StationAlarmQueue _alarmQueue;
        private int _syncCount;

        public CrucibleAlarmSystem(ICoreClientAPI api)
        {
            _api = api;
            _alarmQueue = new StationAlarmQueue(api, AlarmSound, RepeatingAlarmIntervalMs);
        }

        public void OnFirepitSynced(BlockEntityFirepit firepit, ITreeAttribute tree)
        {
            if (_api.World.Player?.Entity == null) return;

            _syncCount++;
            bool doLog = _syncCount <= 10 || _syncCount % 30 == 0;
            string key = PosKey(firepit.Pos);
            SlotPhase oldPhase = _phases.GetValueOrDefault(key);

            bool hasCrucibleInput = IsSmeltingContainerInInput(firepit);
            bool hasSmeltedOutput = IsSmeltedContainerInOutput(firepit);

            if (IsCookingPotFamilyInInput(firepit) || (!hasCrucibleInput && !hasSmeltedOutput))
            {
                IgnoreCrucibleFirepit(key, doLog);
                return;
            }

            float currOreTime = tree.GetFloat("oreCookingTime", 0f);
            float maxTime = firepit.maxCookingTime();
            bool hasIngredients = HasSmeltingIngredients(firepit);
            bool fireLit = firepit.IsBurning;

            _prevOreTimes.TryGetValue(key, out float prevOreTime);
            _prevMaxTimes.TryGetValue(key, out float prevMaxTime);

            bool wasAlreadyDone = oldPhase == SlotPhase.Done;
            bool wasBaking = oldPhase == SlotPhase.Baking;

            bool justSmelted = prevMaxTime > 0.01f
                && prevOreTime >= prevMaxTime * CompletionThresholdFraction
                && currOreTime < 0.01f
                && prevOreTime > 1f
                && (hasCrucibleInput || wasBaking);

            if (doLog)
                Log($"[Sync #{_syncCount}] Cadinho {firepit.Pos} — oreTime={currOreTime:F1}/{maxTime:F1} fireLit={fireLit} inCrucible={hasCrucibleInput} outSmelted={hasSmeltedOutput} ingredients={hasIngredients} phase={oldPhase}");

            ApplyPhase(key, hasCrucibleInput, hasSmeltedOutput, hasIngredients, fireLit,
                currOreTime, maxTime, wasAlreadyDone, wasBaking, justSmelted, doLog);

            _prevOreTimes[key] = currOreTime;
            _prevMaxTimes[key] = maxTime;
            FinishCrucibleSync(key, oldPhase);
        }

        private void ApplyPhase(
            string key,
            bool hasCrucibleInput,
            bool hasSmeltedOutput,
            bool hasIngredients,
            bool fireLit,
            float currOreTime,
            float maxTime,
            bool wasAlreadyDone,
            bool wasBaking,
            bool justSmelted,
            bool doLog)
        {
            bool shouldBeDone = hasSmeltedOutput
                && (justSmelted || wasAlreadyDone || wasBaking);

            if (!hasCrucibleInput && !hasSmeltedOutput)
            {
                ClearCrucibleSlot(key);
                if (doLog) Log("  → None (sem cadinho no fogão)");
            }
            else if (shouldBeDone)
            {
                _phases[key] = SlotPhase.Done;
                _progress.Remove(key);
                if (doLog) Log($"  → Done (justSmelted={justSmelted})");
            }
            else if (hasCrucibleInput && !hasIngredients)
            {
                ClearCrucibleSlot(key);
                if (doLog) Log("  → None (cadinho vazio)");
            }
            else if (hasCrucibleInput && fireLit && !hasSmeltedOutput)
            {
                _phases[key] = SlotPhase.Baking;
                _progress[key] = maxTime > 0f ? Math.Clamp(currOreTime / maxTime, 0f, 1f) : 0f;
                if (doLog) Log($"  → Baking progress={_progress[key]:F2}");
            }
            else if (hasCrucibleInput && !fireLit && wasBaking)
            {
                _phases[key] = SlotPhase.Baking;
                if (!_progress.ContainsKey(key))
                    _progress[key] = maxTime > 0f ? Math.Clamp(currOreTime / maxTime, 0f, 1f) : 0f;
                if (doLog) Log($"  → Baking (fogo apagado) progress={_progress.GetValueOrDefault(key):F2}");
            }
            else
            {
                ClearCrucibleSlot(key);
            }
        }

        private static bool HasSmeltingIngredients(BlockEntityFirepit firepit) =>
            firepit.otherCookingSlots.Any(s => s.Itemstack != null);

        private void ClearCrucibleSlot(string key)
        {
            _phases[key] = SlotPhase.None;
            _progress.Remove(key);
            _prevOreTimes.Remove(key);
            _prevMaxTimes.Remove(key);
        }

        private void IgnoreCrucibleFirepit(string key, bool doLog)
        {
            SlotPhase oldPhase = _phases.GetValueOrDefault(key);
            bool hadState = _phases.ContainsKey(key) && _phases[key] != SlotPhase.None
                || _progress.ContainsKey(key);

            ClearCrucibleSlot(key);
            FinishCrucibleSync(key, oldPhase);

            if (doLog && hadState)
                Log($"Fogão {key}: cadinho ignorado (panela ou vazio)");
        }

        private void FinishCrucibleSync(string key, SlotPhase oldPhase)
        {
            SlotPhase newPhase = _phases.GetValueOrDefault(key);
            StationHudSync.NotifyPhaseChange(_alarmQueue, oldPhase, newPhase, key);
            StationHudSync.Refresh(_hudStates, _phases, _progress);
        }

        private static bool IsSmeltingContainerInInput(BlockEntityFirepit firepit) =>
            firepit.inputSlot?.Itemstack?.Collectible is BlockSmeltingContainer;

        private static bool IsSmeltedContainerInOutput(BlockEntityFirepit firepit) =>
            firepit.outputSlot?.Itemstack?.Collectible is BlockSmeltedContainer;

        private static bool IsCookingPotFamilyInInput(BlockEntityFirepit firepit)
        {
            var input = firepit.inputSlot?.Itemstack?.Collectible;
            if (input == null) return false;
            return input is BlockCookingContainer or BlockCookedContainer;
        }

        private void Log(string msg) =>
            _api.Logger.Notification("[unforgettable] " + msg);

        private static string PosKey(BlockPos pos) =>
            $"{pos.X},{pos.Y},{pos.Z},{pos.dimension}";

        public void Dispose()
        {
            _alarmQueue.Dispose();
            _hudStates.Clear();
            _phases.Clear();
            _progress.Clear();
            _prevOreTimes.Clear();
            _prevMaxTimes.Clear();
            Instance = null;
        }
    }
}
