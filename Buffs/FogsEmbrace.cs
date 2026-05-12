using AfflictionComponent.Components;
using AfflictionComponent.Enums;
using AfflictionComponent.Interfaces;
using HarmonyLib;
using Il2Cpp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MelonLoader;

namespace AfflictionsAndBuffs.Buffs
{
    public class FogsEmbrace : CustomAffliction, IInstance, IBuff
    {
        public InstanceType Type { get; set; } = InstanceType.Single;
        public float Duration { get; set; } = 1f;
        public float EndTime { get; set; }
        public bool Buff { get; set; } = true;
        public bool BuffCold { get; set; }
        public bool BuffFatigue { get; set; }
        public bool BuffHunger { get; set; }
        public bool BuffThirst { get; set; }
        private static float s_LastCheckHours = -1f;
        private const float CHECK_INTERVAL_MINUTES = 5f;
        private static float s_RemoveTime = -1f;
        private const float REMOVE_DELAY_MINUTES = 10f;

        private static readonly Dictionary<BaseAi, OriginalAiValues> OriginalValues = new Dictionary<BaseAi, OriginalAiValues>();

        public FogsEmbrace(AfflictionBodyArea bodyArea = AfflictionBodyArea.Chest)
            : base(
                "GAMEPLAY_FogsEmbraceName",
                "GAMEPLAY_FogsEmbraceCause",
                "GAMEPLAY_FogsEmbraceDescription",
                null,
                "AfflictionsAndBuffs.Resources.Icons.FogsEmbrace.png",
                bodyArea,
                true)
        {
        }

        public override void OnUpdate() { }

        public void OnCure()
        {
            s_RemoveTime = -1f;
        }

        public void OnFoundExistingInstance(CustomAffliction existingAffliction) { }

        public static bool IsFogsEmbraceActive()
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return false;

            for (int i = 0; i < mgr.m_Afflictions.Count; i++)
            {
                if (mgr.m_Afflictions[i] is FogsEmbrace)
                    return true;
            }
            return false;
        }

        private static void StartBuff()
        {
            if (IsFogsEmbraceActive()) return;
            if (GameManager.GetPlayerObject() == null) return;

            MelonCoroutines.Start(DoStartBuffNextFrame());
        }

        private static IEnumerator DoStartBuffNextFrame()
        {
            yield return null;
            if (GameManager.GetPlayerObject() == null) yield break;

            var buff = new FogsEmbrace(AfflictionBodyArea.Chest);
            buff.Start();
        }

        public static void UpdateFogBuff()
        {
            if (GameManager.GetPlayerObject() == null) return;
            if (GameManager.GetWeatherComponent() == null) return;
            if (GameManager.GetTimeOfDayComponent() == null) return;

            bool isOutdoors = !IsPlayerIndoorsStatic();
            var tod = GameManager.GetTimeOfDayComponent();
            float currentHours = tod.GetHoursPlayedNotPaused();

            if (s_LastCheckHours < 0f)
                s_LastCheckHours = currentHours;

            float minutesSinceLastCheck = (currentHours - s_LastCheckHours) * 60f;
            if (minutesSinceLastCheck < CHECK_INTERVAL_MINUTES) return;

            s_LastCheckHours = currentHours;

            var weatherComp = GameManager.GetWeatherComponent();
            string currentWeather = weatherComp.GetWeatherStage().ToString();
            bool isFoggy = IsFoggyWeather(currentWeather);

            if (isOutdoors && isFoggy)
            {
                if (!IsFogsEmbraceActive())
                    StartBuff();

                s_RemoveTime = -1f;
            }
            else
            {
                if (IsFogsEmbraceActive() && s_RemoveTime < 0f)
                {
                    s_RemoveTime = currentHours + (REMOVE_DELAY_MINUTES / 60f);
                }

                if (s_RemoveTime > 0f && currentHours >= s_RemoveTime)
                {
                    RemoveBuff();
                    s_RemoveTime = -1f;
                }
            }
        }

        private static bool IsPlayerIndoorsStatic()
        {
            var weather = GameManager.GetWeatherComponent();
            if (weather == null) return false;

            return weather.IsIndoorScene() ||
                   weather.IsIndoorEnvironment() ||
                   (GameManager.GetPlayerInVehicle() != null && GameManager.GetPlayerInVehicle().IsInside()) ||
                   GameManager.GetSnowShelterManager().PlayerInShelter();
        }

        private static bool IsFoggyWeather(string weatherStage)
        {
            return weatherStage.Contains("Fog") ||
                   weatherStage == "DenseFog" ||
                   weatherStage == "HeavyFog" ||
                   weatherStage == "Blizzard" ||
                   weatherStage == "LightFog";
        }

        private static void RemoveBuff()
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null) return;

            for (int i = 0; i < mgr.m_Afflictions.Count; i++)
            {
                if (mgr.m_Afflictions[i] is FogsEmbrace fog)
                {
                    fog.Cure();
                    return;
                }
            }
        }

        private struct OriginalAiValues
        {
            public float DetectionRange;
            public float DetectionFOV;
            public float MaxPlayerApproachDistanceToInvestigateFood;
            public float SmellRange;
            public float HearFootstepsRange;
            public float HearFootstepsRangeWhileFeeding;
            public float MaxSurvivorDistanceToPlayerForTargetting;
            public float MinSmellDistance;
        }

        [HarmonyPatch(typeof(BaseAi), nameof(BaseAi.Awake))]
        public static class SaveOriginalAiValues
        {
            private static void Postfix(BaseAi __instance)
            {
                if (__instance == null) return;

                AiSubType type = __instance.m_AiSubType;
                if (type != AiSubType.Wolf && type != AiSubType.Bear && type != AiSubType.Stag &&
                    type != AiSubType.Rabbit && type != AiSubType.Moose && type != AiSubType.Cougar)
                    return;

                if (!OriginalValues.ContainsKey(__instance))
                {
                    OriginalValues[__instance] = new OriginalAiValues
                    {
                        DetectionRange = __instance.m_DetectionRange,
                        DetectionFOV = __instance.m_DetectionFOV,
                        MaxPlayerApproachDistanceToInvestigateFood = __instance.m_MaxPlayerApproachDistanceToInvestigateFood,
                        SmellRange = __instance.m_SmellRange,
                        HearFootstepsRange = __instance.m_HearFootstepsRange,
                        HearFootstepsRangeWhileFeeding = __instance.m_HearFootstepsRangeWhileFeeding,
                        MaxSurvivorDistanceToPlayerForTargetting = __instance.m_MaxSurvivorDistanceToPlayerForTargetting,
                        MinSmellDistance = __instance.m_MinSmellDistance
                    };
                }
            }
        }

        [HarmonyPatch(typeof(BaseAi), nameof(BaseAi.Update))]
        public static class DynamicFogDetectionReduction
        {
            private static void Postfix(BaseAi __instance)
            {
                if (__instance == null) return;

                if (!OriginalValues.TryGetValue(__instance, out var original))
                {
                    original = new OriginalAiValues
                    {
                        DetectionRange = __instance.m_DetectionRange,
                        DetectionFOV = __instance.m_DetectionFOV,
                        MaxPlayerApproachDistanceToInvestigateFood = __instance.m_MaxPlayerApproachDistanceToInvestigateFood,
                        SmellRange = __instance.m_SmellRange,
                        HearFootstepsRange = __instance.m_HearFootstepsRange,
                        HearFootstepsRangeWhileFeeding = __instance.m_HearFootstepsRangeWhileFeeding,
                        MaxSurvivorDistanceToPlayerForTargetting = __instance.m_MaxSurvivorDistanceToPlayerForTargetting,
                        MinSmellDistance = __instance.m_MinSmellDistance
                    };
                    OriginalValues[__instance] = original;
                }

                bool shouldReduce = IsFogsEmbraceActive();

                if (shouldReduce)
                {
                    __instance.m_DetectionRange = 0.7f;
                    __instance.m_DetectionFOV = 0.7f;
                    __instance.m_MaxPlayerApproachDistanceToInvestigateFood = 0.7f;
                    __instance.m_SmellRange = 0.7f;
                    __instance.m_HearFootstepsRange = 0.7f;
                    __instance.m_HearFootstepsRangeWhileFeeding = 0.7f;
                    __instance.m_MaxSurvivorDistanceToPlayerForTargetting = 0.7f;
                    __instance.m_MinSmellDistance = 0.7f;
                }
                else
                {
                    __instance.m_DetectionRange = original.DetectionRange;
                    __instance.m_DetectionFOV = original.DetectionFOV;
                    __instance.m_MaxPlayerApproachDistanceToInvestigateFood = original.MaxPlayerApproachDistanceToInvestigateFood;
                    __instance.m_SmellRange = original.SmellRange;
                    __instance.m_HearFootstepsRange = original.HearFootstepsRange;
                    __instance.m_HearFootstepsRangeWhileFeeding = original.HearFootstepsRangeWhileFeeding;
                    __instance.m_MaxSurvivorDistanceToPlayerForTargetting = original.MaxSurvivorDistanceToPlayerForTargetting;
                    __instance.m_MinSmellDistance = original.MinSmellDistance;
                }
            }
        }
    }
}