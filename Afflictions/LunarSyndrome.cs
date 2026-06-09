#nullable disable
using AfflictionComponent.Components;
using AfflictionComponent.Enums;
using AfflictionComponent.Interfaces;
using HarmonyLib;
using Il2Cpp;
using System.Collections.Generic;
using UnityEngine;
using MelonLoader;

namespace AfflictionsAndBuffs.Afflictions
{
    public class LunarSyndrome : CustomAffliction, IInstance
    {
        public InstanceType Type { get; set; } = InstanceType.Single;

        public static bool IsActive { get; private set; } = false;

        private static float s_LastCheckHours = -1f;
        private const float CHECK_INTERVAL_MINUTES = 10f;

        private float m_IndoorTimer = 0f;
        private float m_LastIndoorHours = -1f;
        private float m_TargetCureDelay = -1f;

        private static readonly Dictionary<BaseAi, OriginalAiValues> OriginalValues = new Dictionary<BaseAi, OriginalAiValues>();

        public LunarSyndrome(AfflictionBodyArea bodyArea = AfflictionBodyArea.Chest)
            : base(
                "GAMEPLAY_LunarSyndromeName",
                "GAMEPLAY_LunarSyndromeCause",
                "GAMEPLAY_LunarSyndromeDescription",
                null,
                "AfflictionsAndBuffs.Resources.Icons.LunarSyndrome.png",
                bodyArea,
                true)
        {
            IsActive = true;
            MelonLogger.Msg("[LunarSyndrome] Activated - Wolves are more alert under the moon");
            InitializeExistingAnimals();
        }

        public override void OnUpdate()
        {
            IsActive = true;
            var tod = GameManager.GetTimeOfDayComponent();
            if (tod == null) return;

            float currentHours = tod.GetHoursPlayedNotPaused();

            if (m_LastIndoorHours < 0f)
                m_LastIndoorHours = currentHours;

            float deltaMinutes = (currentHours - m_LastIndoorHours) * 60f;
            m_LastIndoorHours = currentHours;

            bool conditionsMet = IsNightTime() && IsValidWeather() && IsPlayerOutdoors();

            if (conditionsMet)
            {
                m_IndoorTimer = 0f;
                m_TargetCureDelay = -1f;
                return;
            }

            if (m_TargetCureDelay < 0f)
            {
                m_TargetCureDelay = UnityEngine.Random.Range(4f, 9f);
                m_IndoorTimer = 0f;
            }

            m_IndoorTimer += deltaMinutes;

            if (m_IndoorTimer >= m_TargetCureDelay)
            {
                Cure();
            }
        }

        public void OnFoundExistingInstance(CustomAffliction existingAffliction)
        {
            IsActive = true;
            InitializeExistingAnimals();
        }

        public void OnCure()
        {
            IsActive = false;
            m_IndoorTimer = 0f;
            m_TargetCureDelay = -1f;
            m_LastIndoorHours = -1f;
            MelonLogger.Msg("[LunarSyndrome] Affliction cured");
        }

        public static void UpdateLunarSyndrome()
        {
            var tod = GameManager.GetTimeOfDayComponent();
            var weather = GameManager.GetWeatherComponent();
            if (tod == null || weather == null) return;

            float currentHours = tod.GetHoursPlayedNotPaused();

            if (s_LastCheckHours < 0f)
                s_LastCheckHours = currentHours;

            float minutesSinceLastCheck = (currentHours - s_LastCheckHours) * 60f;

            if (minutesSinceLastCheck < CHECK_INTERVAL_MINUTES)
                return;

            s_LastCheckHours = currentHours;

            bool isNight = IsNightTime();
            bool isValidWeather = IsValidWeather();
            bool isOutdoors = IsPlayerOutdoors();
            bool alreadyActive = IsLunarSyndromeActive();

            if (isNight && isValidWeather && isOutdoors && !alreadyActive)
            {
                float roll = UnityEngine.Random.Range(0f, 100f);
                if (roll <= 20f)
                {
                    MelonLogger.Msg("[LunarSyndrome] Lunar influence triggered!");
                    new LunarSyndrome(AfflictionBodyArea.Chest).Start();
                }
            }
        }

        private static bool IsLunarSyndromeActive()
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return false;

            for (int i = 0; i < mgr.m_Afflictions.Count; i++)
            {
                if (mgr.m_Afflictions[i] is LunarSyndrome)
                    return true;
            }
            return false;
        }

        private static bool IsNightTime()
        {
            var tod = GameManager.GetTimeOfDayComponent();
            if (tod == null) return false;
            float hour = tod.GetHour();
            return hour >= 21.5f || hour < 6.5f;
        }

        private static bool IsValidWeather()
        {
            var weather = GameManager.GetWeatherComponent();
            if (weather == null) return false;
            string stage = weather.GetWeatherStage().ToString();
            return stage.Equals("Clear", System.StringComparison.OrdinalIgnoreCase) ||
                   stage.Equals("ClearAurora", System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPlayerOutdoors()
        {
            var weather = GameManager.GetWeatherComponent();
            if (weather == null) return false;
            return !(weather.IsIndoorScene() ||
                     weather.IsIndoorEnvironment() ||
                     (GameManager.GetPlayerInVehicle() != null && GameManager.GetPlayerInVehicle().IsInside()) ||
                     GameManager.GetSnowShelterManager().PlayerInShelter());
        }

        private struct OriginalAiValues
        {
            public float DetectionRange;
            public float DetectionFOV;
            public float MaxPlayerApproachDistanceToInvestigateFood;
            public float NextAllowedAttackDamageTime;
        }

        private static void InitializeExistingAnimals()
        {
            var allAis = UnityEngine.Object.FindObjectsOfType<BaseAi>(true);
            foreach (var ai in allAis)
            {
                if (ai == null) continue;
                if (ai.m_AiSubType != AiSubType.Wolf) continue;

                if (!OriginalValues.ContainsKey(ai))
                {
                    OriginalValues[ai] = new OriginalAiValues
                    {
                        DetectionRange = ai.m_DetectionRange,
                        DetectionFOV = ai.m_DetectionFOV,
                        MaxPlayerApproachDistanceToInvestigateFood = ai.m_MaxPlayerApproachDistanceToInvestigateFood,
                        NextAllowedAttackDamageTime = ai.m_NextAllowedAttackDamageTime
                    };
                }
            }
        }

        [HarmonyPatch(typeof(BaseAi), nameof(BaseAi.Awake))]
        public static class SaveOriginalAiValues
        {
            private static void Postfix(BaseAi __instance)
            {
                if (__instance == null || __instance.m_AiSubType != AiSubType.Wolf) return;

                if (!OriginalValues.ContainsKey(__instance))
                {
                    OriginalValues[__instance] = new OriginalAiValues
                    {
                        DetectionRange = __instance.m_DetectionRange,
                        DetectionFOV = __instance.m_DetectionFOV,
                        MaxPlayerApproachDistanceToInvestigateFood = __instance.m_MaxPlayerApproachDistanceToInvestigateFood,
                        NextAllowedAttackDamageTime = __instance.m_NextAllowedAttackDamageTime
                    };
                }
            }
        }

        [HarmonyPatch(typeof(BaseAi), nameof(BaseAi.Update))]
        public static class LunarDetectionIncrease
        {
            private static void Postfix(BaseAi __instance)
            {
                if (__instance == null || __instance.m_AiSubType != AiSubType.Wolf) return;

                if (!OriginalValues.TryGetValue(__instance, out var original))
                {
                    original = new OriginalAiValues
                    {
                        DetectionRange = __instance.m_DetectionRange,
                        DetectionFOV = __instance.m_DetectionFOV,
                        MaxPlayerApproachDistanceToInvestigateFood = __instance.m_MaxPlayerApproachDistanceToInvestigateFood,
                        NextAllowedAttackDamageTime = __instance.m_NextAllowedAttackDamageTime
                    };
                    OriginalValues[__instance] = original;
                }

                bool lunarActive = IsActive;

                if (lunarActive)
                {
                    __instance.m_DetectionRange = original.DetectionRange * 1.2f;
                    __instance.m_DetectionFOV = original.DetectionFOV * 1.25f;
                    __instance.m_MaxPlayerApproachDistanceToInvestigateFood = original.MaxPlayerApproachDistanceToInvestigateFood * 1.2f;
                    __instance.m_NextAllowedAttackDamageTime = original.NextAllowedAttackDamageTime * 1.3f;
                }
                else
                {
                    __instance.m_DetectionRange = original.DetectionRange;
                    __instance.m_DetectionFOV = original.DetectionFOV;
                    __instance.m_MaxPlayerApproachDistanceToInvestigateFood = original.MaxPlayerApproachDistanceToInvestigateFood;
                    __instance.m_NextAllowedAttackDamageTime = original.NextAllowedAttackDamageTime;
                }
            }
        }
    }
}