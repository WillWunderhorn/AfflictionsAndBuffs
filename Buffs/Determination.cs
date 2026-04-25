using AfflictionComponent.Components;
using AfflictionComponent.Enums;
using AfflictionComponent.Interfaces;
using System.Collections;

namespace AfflictionsAndBuffs.Buffs
{
    public class Determination : CustomAffliction, IInstance, IBuff
    {
        public InstanceType Type { get; set; } = InstanceType.Single;

        public bool Buff { get; set; } = true;
        public bool BuffCold { get; set; }
        public bool BuffFatigue { get; set; } = true;
        public bool BuffHunger { get; set; }
        public bool BuffThirst { get; set; }

        public static bool IsActive { get; private set; } = false;

        private static float s_LastCheckHours = -1f;
        private const float CHECK_INTERVAL_MINUTES = 10f;

        public Determination(AfflictionBodyArea bodyArea = AfflictionBodyArea.Chest)
            : base(
                "GAMEPLAY_DeterminationName",
                "GAMEPLAY_DeterminationCause",
                "GAMEPLAY_DeterminationDescription",
                null,
                "AfflictionsAndBuffs.Resources.Icons.Determination.png",
                bodyArea,
                true)
        {
            IsActive = true;
        }

        public override void OnUpdate()
        {
            IsActive = true;
        }

        public void OnCure()
        {
            IsActive = false;
        }

        public void OnFoundExistingInstance(CustomAffliction existingAffliction)
        {
            if (existingAffliction is Determination)
                IsActive = true;
        }

        public static bool IsDeterminationBuffActive()
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return false;

            for (int i = 0; i < mgr.m_Afflictions.Count; i++)
            {
                if (mgr.m_Afflictions[i] is Determination)
                    return true;
            }
            return false;
        }

        private static bool IsBuffActive() => IsDeterminationBuffActive();

        private static void StartBuff()
        {
            if (IsBuffActive()) return;
            if (GameManager.GetPlayerObject() == null) return;

            MelonCoroutines.Start(DoStartBuffNextFrame());
        }

        private static IEnumerator DoStartBuffNextFrame()
        {
            yield return null;

            if (GameManager.GetPlayerObject() == null) yield break;

            var buff = new Determination(AfflictionBodyArea.Chest);
            buff.Start();
            IsActive = true;
        }

        public static void UpdateWeatherBuff()
        {
            if (GameManager.GetPlayerObject() == null) return;
            if (GameManager.GetWeatherComponent() == null) return;
            if (GameManager.GetTimeOfDayComponent() == null) return;

            var tod = GameManager.GetTimeOfDayComponent();
            float currentHours = tod.GetHoursPlayedNotPaused();

            if (s_LastCheckHours < 0f)
                s_LastCheckHours = currentHours;

            float minutesSinceLastCheck = (currentHours - s_LastCheckHours) * 60f;
            if (minutesSinceLastCheck < CHECK_INTERVAL_MINUTES) return;

            s_LastCheckHours = currentHours;

            var weatherComp = GameManager.GetWeatherComponent();
            string currentWeather = weatherComp != null
                ? weatherComp.GetWeatherStage().ToString()
                : "Unknown";

            bool shouldBeActive =
                currentWeather == "Clear" ||
                currentWeather == "PartlyCloudy" ||
                currentWeather == "ClearAurora";

            if (shouldBeActive)
            {
                StartBuff();
            }
            else if (!shouldBeActive && IsActive)
            {
                RemoveBuff();
            }
        }

        private static void RemoveBuff()
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null) return;

            for (int i = 0; i < mgr.m_Afflictions.Count; i++)
            {
                if (mgr.m_Afflictions[i] is Determination det)
                {
                    det.Cure();
                    return;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Fatigue), "CalculateFatigueIncrease")]
    internal static class DeterminationFatiguePatch
    {
        private static void Postfix(Fatigue __instance, float realtimeSeconds, ref float __result)
        {
            if (Determination.IsDeterminationBuffActive())
                __result *= 0.5f;
        }
    }

    [HarmonyPatch(typeof(vp_FPSController), nameof(vp_FPSController.GetSlopeMultiplier))]
    public class MovementSpeedModifier
    {
        public static void Postfix(ref float __result, vp_FPSController __instance)
        {
            if (!Determination.IsDeterminationBuffActive()) return;

            var player = GameManager.GetPlayerManagerComponent();
            if (player == null) return;

            if (player.PlayerIsWalking() || player.PlayerIsSprinting())
            {
                __result *= 1.2f;
            }
        }
    }
}