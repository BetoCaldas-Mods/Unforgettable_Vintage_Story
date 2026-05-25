using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Unforgettable
{
    public class FirepitAlarmSystem : IDisposable
    {
        private const string AlarmSound = "unforgettable:sounds/oventialarm";
        private const int RepeatingAlarmIntervalMs = 5000;
        private const float CompletionThresholdFraction = 0.88f;
        private const float TransitionCompleteLevel = 0.99f;

        public static FirepitAlarmSystem? Instance;
        public IReadOnlyDictionary<string, HudState> HudStates => _hudStates;

        private readonly ICoreClientAPI _api;
        private readonly Dictionary<string, HudState> _hudStates = new();
        private readonly Dictionary<string, SlotPhase> _phases = new();
        private readonly Dictionary<string, float> _progress = new();
        private readonly Dictionary<string, float> _prevOreTimes = new();
        private readonly Dictionary<string, float> _prevMaxTimes = new();
        private readonly Dictionary<string, string?> _trackedCooksIntoRecipe = new();
        private readonly StationAlarmQueue _alarmQueue;
        private int _syncCount;

        public FirepitAlarmSystem(ICoreClientAPI api)
        {
            _api = api;
            _alarmQueue = new StationAlarmQueue(api, AlarmSound, RepeatingAlarmIntervalMs, repeat: false);
        }

        public void OnFirepitSynced(BlockEntityFirepit firepit, ITreeAttribute tree)
        {
            if (_api.World.Player?.Entity == null) return;

            _syncCount++;
            bool doLog = _syncCount <= 10 || _syncCount % 30 == 0;

            string key = PosKey(firepit.Pos);
            SlotPhase oldPhase = _phases.GetValueOrDefault(key);

            if (!ShouldTrackPotFirepit(firepit))
            {
                IgnoreFirepitNotCookingPot(key, doLog);
                return;
            }

            float currOreTime = tree.GetFloat("oreCookingTime", 0f);
            float maxTime = firepit.maxCookingTime();

            if (IsFinishedMealWaiting(firepit))
            {
                _phases[key] = SlotPhase.Done;
                _progress.Remove(key);
                _trackedCooksIntoRecipe.Remove(key);
                _prevOreTimes[key] = currOreTime;
                _prevMaxTimes[key] = maxTime;
                if (doLog) Log($"  → Done (refeição pronta no fogão {firepit.Pos})");
                FinishFirepitSync(key, oldPhase);
                return;
            }

            if (firepit.inputSlot?.Itemstack?.Collectible is not BlockCookingContainer cookingPot)
            {
                FinishFirepitSync(key, oldPhase);
                return;
            }

            ItemStack[] stacks = GetCookingStacks(firepit);
            CookingRecipe? matchingRecipe = TryGetMatchingRecipe(cookingPot, stacks, out _);

            _trackedCooksIntoRecipe.TryGetValue(key, out string? trackedCode);
            CookingRecipe? trackedRecipe = ResolveRecipe(trackedCode);
            CookingRecipe? activeCooksIntoRecipe = matchingRecipe?.CooksInto != null
                ? matchingRecipe
                : trackedRecipe;

            bool hasCookingItems = stacks.Length > 0;
            bool fireLit = firepit.IsBurning;

            bool hasCooksIntoOutput = activeCooksIntoRecipe != null
                && HasCooksIntoOutput(_api.World, stacks, activeCooksIntoRecipe);

            _prevOreTimes.TryGetValue(key, out float prevOreTime);
            _prevMaxTimes.TryGetValue(key, out float prevMaxTime);

            bool wasAlreadyDone = _phases.GetValueOrDefault(key) == SlotPhase.Done;
            bool wasBaking = _phases.GetValueOrDefault(key) == SlotPhase.Baking;
            bool wasTrackingCooksInto = !string.IsNullOrEmpty(trackedCode);

            bool mealFinishedByTimer = matchingRecipe?.CooksInto == null
                && prevMaxTime > 0.01f
                && prevOreTime >= prevMaxTime * CompletionThresholdFraction
                && currOreTime < 0.01f
                && prevOreTime > 1f;

            bool cooksIntoFinished = wasTrackingCooksInto
                && activeCooksIntoRecipe != null
                && (hasCooksIntoOutput
                    || IsTransitionComplete(firepit.otherCookingSlots, activeCooksIntoRecipe));

            bool justCooked = mealFinishedByTimer || cooksIntoFinished;

            if (doLog)
            {
                string recipeCode = matchingRecipe?.Code ?? trackedCode ?? "(nenhuma)";
                EnumTransitionType? transType = activeCooksIntoRecipe?.PerishableProps?.Type;
                Log($"[Sync #{_syncCount}] Fogão {firepit.Pos} — recipe={recipeCode} trans={transType} oreTime={currOreTime:F1}/{maxTime:F1} fireLit={fireLit} hasOutput={hasCooksIntoOutput} mealDone={mealFinishedByTimer} cooksIntoDone={cooksIntoFinished} phase={_phases.GetValueOrDefault(key)}");
            }

            bool wasAnyDone = _phases.GetValueOrDefault(key) == SlotPhase.Done;
            bool isMealFlow = activeCooksIntoRecipe == null && matchingRecipe?.CooksInto == null;
            bool shouldBeDoneMeal = hasCookingItems
                && isMealFlow
                && (mealFinishedByTimer || wasAlreadyDone);
            bool shouldBeDoneCooksInto = hasCookingItems
                && activeCooksIntoRecipe != null
                && (cooksIntoFinished || wasAlreadyDone
                    || (hasCooksIntoOutput && (wasBaking || wasTrackingCooksInto)));
            bool shouldBeDone = shouldBeDoneMeal || shouldBeDoneCooksInto;

            if (!hasCookingItems)
            {
                ClearFirepitSlot(key);
                if (doLog && wasAnyDone) Log("  → None (conteúdo removido da panela)");
            }
            else if (shouldBeDone)
            {
                _phases[key] = SlotPhase.Done;
                _progress.Remove(key);
                _trackedCooksIntoRecipe.Remove(key);
                if (doLog) Log($"  → Done (justCooked={justCooked})");
            }
            else if (fireLit && matchingRecipe?.CooksInto != null && !hasCooksIntoOutput)
            {
                _trackedCooksIntoRecipe[key] = matchingRecipe.Code;
                _phases[key] = SlotPhase.Baking;
                _progress[key] = CalculateCooksIntoProgress(
                    firepit, matchingRecipe, currOreTime, maxTime);
                if (doLog) Log($"  → Baking (CooksInto) progress={_progress[key]:F2}");
            }
            else if (fireLit && matchingRecipe?.CooksInto == null && currOreTime > 0f)
            {
                _trackedCooksIntoRecipe.Remove(key);
                _phases[key] = SlotPhase.Baking;
                _progress[key] = maxTime > 0f ? Math.Clamp(currOreTime / maxTime, 0f, 1f) : 0f;
                if (doLog) Log($"  → Baking (refeição) progress={_progress[key]:F2}");
            }
            else if (fireLit && matchingRecipe?.CooksInto == null)
            {
                _trackedCooksIntoRecipe.Remove(key);
                _phases[key] = SlotPhase.Baking;
                _progress[key] = 0f;
                if (doLog) Log("  → Baking (refeição, aquecendo) progress=0");
            }
            else if (wasBaking || wasTrackingCooksInto)
            {
                _phases[key] = SlotPhase.Baking;
                if (activeCooksIntoRecipe != null)
                    _progress[key] = CalculateCooksIntoProgress(
                        firepit, activeCooksIntoRecipe, currOreTime, maxTime);
                else if (!_progress.ContainsKey(key))
                    _progress[key] = maxTime > 0f ? Math.Clamp(currOreTime / maxTime, 0f, 1f) : 0f;
                if (doLog) Log($"  → Baking (fogo apagado, aguardando) progress={_progress.GetValueOrDefault(key):F2}");
            }
            else
            {
                ClearFirepitSlot(key);
            }

            _prevOreTimes[key] = currOreTime;
            _prevMaxTimes[key] = maxTime;
            FinishFirepitSync(key, oldPhase);
        }

        private static ItemStack[] GetCookingStacks(BlockEntityFirepit firepit)
        {
            return firepit.otherCookingSlots
                .Where(s => s.Itemstack != null)
                .Select(s => s.Itemstack!)
                .ToArray();
        }

        private CookingRecipe? TryGetMatchingRecipe(
            BlockCookingContainer? pot,
            ItemStack[] stacks,
            out int quantityServings)
        {
            quantityServings = 0;
            if (pot == null || stacks.Length == 0) return null;
            return pot.GetMatchingCookingRecipe(_api.World, stacks, out quantityServings);
        }

        private CookingRecipe? ResolveRecipe(string? code) =>
            string.IsNullOrEmpty(code) ? null : _api.GetCookingRecipe(code);

        private float CalculateCooksIntoProgress(
            BlockEntityFirepit firepit,
            CookingRecipe recipe,
            float currOreTime,
            float maxTime)
        {
            float fromTimer = maxTime > 0f ? Math.Clamp(currOreTime / maxTime, 0f, 1f) : 0f;
            float fromTransition = GetRecipeTransitionProgress(firepit.otherCookingSlots, recipe);
            return Math.Max(fromTimer, fromTransition);
        }

        private float GetRecipeTransitionProgress(ItemSlot[] slots, CookingRecipe recipe)
        {
            EnumTransitionType transitionType = recipe.PerishableProps?.Type ?? EnumTransitionType.None;
            float max = 0f;

            foreach (var slot in slots)
            {
                if (slot.Itemstack == null) continue;

                TransitionState[]? states = slot.Itemstack.Collectible.UpdateAndGetTransitionStates(
                    _api.World, slot);
                if (states == null) continue;

                foreach (var state in states)
                {
                    if (state.Props.Type != transitionType) continue;
                    max = Math.Max(max, state.TransitionLevel);
                }
            }

            return max;
        }

        private bool IsTransitionComplete(ItemSlot[] slots, CookingRecipe recipe) =>
            GetRecipeTransitionProgress(slots, recipe) >= TransitionCompleteLevel;

        private static bool HasCooksIntoOutput(IWorldAccessor world, ItemStack[] stacks, CookingRecipe recipe)
        {
            ItemStack? target = recipe.CooksInto?.ResolvedItemstack;
            if (target == null) return false;

            return stacks.Any(s =>
                s != null && s.Equals(world, target, GlobalConstants.IgnoredStackAttributes));
        }

        private void ClearFirepitSlot(string key)
        {
            _phases[key] = SlotPhase.None;
            _progress.Remove(key);
            _prevOreTimes.Remove(key);
            _prevMaxTimes.Remove(key);
            _trackedCooksIntoRecipe.Remove(key);
        }

        private void IgnoreFirepitNotCookingPot(string key, bool doLog)
        {
            SlotPhase oldPhase = _phases.GetValueOrDefault(key);
            bool hadState = _phases.ContainsKey(key) && _phases[key] != SlotPhase.None
                || _progress.ContainsKey(key);

            ClearFirepitSlot(key);
            FinishFirepitSync(key, oldPhase);

            if (doLog && hadState)
                Log($"Fogão {key}: ignorado (não é panela de cozinha)");
        }

        private void FinishFirepitSync(string key, SlotPhase oldPhase)
        {
            SlotPhase newPhase = _phases.GetValueOrDefault(key);
            StationHudSync.NotifyPhaseChange(_alarmQueue, oldPhase, newPhase, key);
            StationHudSync.Refresh(_hudStates, _phases, _progress, showHudWhenDone: false);
        }

        private static bool ShouldTrackPotFirepit(BlockEntityFirepit firepit)
        {
            var input = firepit.inputSlot?.Itemstack?.Collectible;
            var output = firepit.outputSlot?.Itemstack?.Collectible;
            return input is BlockCookingContainer or BlockCookedContainer
                || output is BlockCookedContainer;
        }

        private static bool IsFinishedMealWaiting(BlockEntityFirepit firepit)
        {
            if (firepit.outputSlot?.Itemstack?.Collectible is BlockCookedContainer)
                return true;

            return firepit.inputSlot?.Itemstack?.Collectible is BlockCookedContainer;
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
            _trackedCooksIntoRecipe.Clear();
            Instance = null;
        }
    }
}
