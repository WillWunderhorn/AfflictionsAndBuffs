#nullable disable
using AfflictionComponent.Components;
using AfflictionComponent.Enums;
using AfflictionComponent.Interfaces;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace AfflictionsAndBuffs.Afflictions
{
    public class Starving : CustomAffliction, IInstance
    {
        public InstanceType Type { get; set; } = InstanceType.Single;
        public static bool IsStarving { get; private set; } = false;

        private static float s_LowHungerStartHours = -1f;
        private const float REQUIRED_HOURS = 6f;

        private static float s_OriginalSprintMax;
        private static float s_OriginalSprintMin;
        private static float s_OriginalWalk;
        private static float s_OriginalMaxFatigue;
        private static bool s_ModifiersApplied = false;

        public Starving(AfflictionBodyArea bodyArea)
            : base(
                  "GAMEPLAY_StarvingName",
                  "GAMEPLAY_StarvingCause",
                  "GAMEPLAY_StarvingDescription",
                  null,
                  "AfflictionsAndBuffs.Resources.Icons.Starving.png",
                  bodyArea,
                  true)
        {
            IsStarving = true;
            ApplyFatigueModifiers();
        }

        public override void OnUpdate()
        {
            IsStarving = true;

            var hungerComp = GameManager.GetHungerComponent();
            if (hungerComp != null)
            {
                var hungerState = hungerComp.GetHungerLevel();
                bool isRecovered = hungerState == HungerLevel.Full ||
                                   hungerState == HungerLevel.SlightlyHungry;

                if (isRecovered)
                {
                    OnCure();
                    Cure();
                    return;
                }
            }
        }

        public void OnCure()
        {
            IsStarving = false;
            ResetTracking();
            RestoreFatigueModifiers();

            float now = GameManager.GetTimeOfDayComponent()?.GetHoursPlayedNotPaused() ?? 0f;
            SaveDataManager.StarvingCooldownEndHours = now + (24f * 5f);
        }

        public void OnFoundExistingInstance(CustomAffliction existingAffliction)
        {
            if (existingAffliction is Starving)
            {
                IsStarving = true;
                ApplyFatigueModifiers();
            }
        }

        private static void ApplyFatigueModifiers()
        {
            if (s_ModifiersApplied) return;

            var fatigue = GameManager.GetFatigueComponent();
            if (fatigue == null) return;

            s_OriginalSprintMax = fatigue.m_FatigueIncreasePerHourSprintingMax;
            s_OriginalSprintMin = fatigue.m_FatigueIncreasePerHourSprintingMin;
            s_OriginalWalk = fatigue.m_FatigueIncreasePerHourWalking;
            s_OriginalMaxFatigue = fatigue.m_MaxFatigue;

            fatigue.m_FatigueIncreasePerHourSprintingMax *= 1.9f;
            fatigue.m_FatigueIncreasePerHourSprintingMin *= 1.3f;
            fatigue.m_FatigueIncreasePerHourWalking *= 2f;
            fatigue.m_MaxFatigue = 90f;

            s_ModifiersApplied = true;
        }

        private static void RestoreFatigueModifiers()
        {
            if (!s_ModifiersApplied) return;

            var fatigue = GameManager.GetFatigueComponent();
            if (fatigue == null) return;

            fatigue.m_FatigueIncreasePerHourSprintingMax = s_OriginalSprintMax;
            fatigue.m_FatigueIncreasePerHourSprintingMin = s_OriginalSprintMin;
            fatigue.m_FatigueIncreasePerHourWalking = s_OriginalWalk;
            fatigue.m_MaxFatigue = s_OriginalMaxFatigue;

            s_ModifiersApplied = false;
        }

        public static void UpdateStarving()
        {
            var tod = GameManager.GetTimeOfDayComponent();
            var hungerComp = GameManager.GetHungerComponent();
            if (tod == null || hungerComp == null) return;

            float currentHours = tod.GetHoursPlayedNotPaused();
            var hungerState = hungerComp.GetHungerLevel();

            if (IsCooldownActive())
            {
                ResetTracking();
                return;
            }

            bool isLowHunger = hungerState == HungerLevel.Starving ||
                               hungerState == HungerLevel.VeryHungry ||
                               hungerState == HungerLevel.Hungry;

            if (isLowHunger)
            {
                if (s_LowHungerStartHours < 0f)
                    s_LowHungerStartHours = currentHours;

                float hoursPassed = currentHours - s_LowHungerStartHours;
                if (hoursPassed >= REQUIRED_HOURS)
                    StartStarving();
            }
            else
            {
                ResetTracking();
            }
        }

        private static void StartStarving()
        {
            if (IsStarvingActive() || IsCooldownActive())
                return;

            var aff = new Starving(AfflictionBodyArea.Stomach);
            aff.Start();
        }

        private static bool IsStarvingActive()
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return false;

            for (int i = 0; i < mgr.m_Afflictions.Count; i++)
            {
                if (mgr.m_Afflictions[i] is Starving)
                    return true;
            }
            return false;
        }

        private static bool IsCooldownActive()
        {
            float now = GameManager.GetTimeOfDayComponent()?.GetHoursPlayedNotPaused() ?? 0f;
            return now < SaveDataManager.StarvingCooldownEndHours;
        }

        private static void ResetTracking()
        {
            s_LowHungerStartHours = -1f;
        }
    }

    [HarmonyPatch(typeof(vp_FPSController), nameof(vp_FPSController.GetSlopeMultiplier))]
    public class StarvingMovementSpeedModifier
    {
        public static void Postfix(ref float __result)
        {
            if (!Starving.IsStarving) return;
            __result *= 0.7f;
        }
    }
}