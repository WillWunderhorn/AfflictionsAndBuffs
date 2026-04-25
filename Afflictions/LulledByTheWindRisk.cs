#nullable disable
using AfflictionComponent.Components;
using AfflictionComponent.Enums;
using AfflictionComponent.Interfaces;
using Il2Cpp;
using Il2CppAK;
using System;
using System.Collections;
using UnityEngine;
using MelonLoader;

namespace AfflictionsAndBuffs.Afflictions
{
    public class LulledByTheWindRisk : CustomAffliction, IInstance, IRiskPercentage
    {
        public InstanceType Type { get; set; } = InstanceType.Single;

        private float m_RiskValue = 0f;
        private bool m_WasDecreasing = false;
        private float m_ContinuousIndoorMinutes = 0f;
        private float m_LastUpdateHours = -1f;

        private const float INDOOR_DECREASE_DELAY_MINUTES = 25f;
        private const float SLOW_DECREASE_RATE_PER_INGAME_SECOND = 0.02f;
        private const float OUTDOOR_INCREASE_RATE_PER_INGAME_SECOND = 0.013f;

        private static float s_OutdoorMinutes = 0f;
        private static float s_ContinuousIndoorMinutesForOutdoorTimer = 0f;
        private static float s_LastOutdoorUpdateHours = -1f;
        private const float OUTDOOR_RISK_DELAY_MINUTES = 180f;
        private const float OUTDOOR_TIMER_RESET_AFTER_INDOOR_MINUTES = 120f;

        private float m_LastStatusLogTime = 0f;

        public bool Risk { get; set; } = true;
        public float GetRiskValue() => m_RiskValue;
        public void UpdateRiskValue() { }

        public static void RestoreActiveRiskOnLoad()
        {
            if (IsRiskOrDebuffActive() || IsLulledByTheWindRisk) return;

            var risk = new LulledByTheWindRisk(AfflictionBodyArea.Chest);
            risk.Start();
        }

        private void SyncRiskToSaveData()
        {
            SaveDataManager.LulledByTheWindRiskPercentage = m_RiskValue;
        }

        public static void RestoreOutdoorTimerOnLoad()
        {
            s_OutdoorMinutes = SaveDataManager.LulledByTheWindOutdoorMinutes;
            s_LastOutdoorUpdateHours = -1f;
        }

        internal void RestoreRiskPercentage(float riskPercentage)
        {
            m_RiskValue = Mathf.Clamp(riskPercentage, 0f, 100f);
            m_WasDecreasing = false;
            m_ContinuousIndoorMinutes = SaveDataManager.LulledByTheWindContinuousIndoorMinutes;
            m_LastUpdateHours = -1f;
            SaveDataManager.LulledByTheWindRiskPercentage = m_RiskValue;
        }

        public Tuple<string, int, int>[] RemedyItems { get; set; } = Array.Empty<Tuple<string, int, int>>();
        public Tuple<string, int, int>[] AltRemedyItems { get; set; } = Array.Empty<Tuple<string, int, int>>();

        public bool InstantHeal { get; set; } = true;
        public static bool IsLulledByTheWindRisk { get; private set; } = false;

        public LulledByTheWindRisk(AfflictionBodyArea bodyArea)
            : base(
                "GAMEPLAY_LulledByTheWindRiskName",
                "GAMEPLAY_LulledByTheWindRiskCause",
                "GAMEPLAY_LulledByTheWindRiskDescription",
                null,
                "AfflictionsAndBuffs.Resources.Icons.LulledByTheWind.png",
                bodyArea,
                true)
        {
            m_RiskValue = Mathf.Clamp(SaveDataManager.LulledByTheWindRiskPercentage, 0f, 100f);
            m_WasDecreasing = false;
            m_ContinuousIndoorMinutes = SaveDataManager.LulledByTheWindContinuousIndoorMinutes;
            m_LastUpdateHours = -1f;
            m_LastStatusLogTime = 0f;
            IsLulledByTheWindRisk = true;
        }

        private void ResetAllInternalState()
        {
            m_RiskValue = 0f;
            m_WasDecreasing = false;
            m_ContinuousIndoorMinutes = 0f;
            SaveDataManager.LulledByTheWindContinuousIndoorMinutes = 0f;
            m_LastUpdateHours = -1f;
            m_LastStatusLogTime = 0f;
        }

        private static bool IsCooldownActive()
        {
            float now = GameManager.GetTimeOfDayComponent()?.GetHoursPlayedNotPaused() ?? 0f;
            return now < SaveDataManager.LulledByTheWindCooldownEndHours;
        }

        private static bool IsShortRiskCooldownActive()
        {
            float now = GameManager.GetTimeOfDayComponent()?.GetHoursPlayedNotPaused() ?? 0f;
            return now < SaveDataManager.LulledByTheWindRiskShortCooldownEndHours;
        }

        public static void LulledByTheWindUpdateOutdoorTimer()
        {
            var tod = GameManager.GetTimeOfDayComponent();
            if (tod == null) return;

            float currentHours = tod.GetHoursPlayedNotPaused();
            bool isOutdoors = !IsPlayerIndoorsStatic();

            if (s_LastOutdoorUpdateHours < 0f)
                s_LastOutdoorUpdateHours = currentHours;

            float deltaMinutes = Mathf.Clamp((currentHours - s_LastOutdoorUpdateHours) * 60f, 0f, 0.25f);
            s_LastOutdoorUpdateHours = currentHours;

            if (isOutdoors)
            {
                s_ContinuousIndoorMinutesForOutdoorTimer = 0f;
                s_OutdoorMinutes += deltaMinutes;
                SaveDataManager.LulledByTheWindOutdoorMinutes = s_OutdoorMinutes;
            }
            else
            {
                s_ContinuousIndoorMinutesForOutdoorTimer += deltaMinutes;
                if (s_ContinuousIndoorMinutesForOutdoorTimer >= OUTDOOR_TIMER_RESET_AFTER_INDOOR_MINUTES)
                {
                    s_OutdoorMinutes = 0f;
                    SaveDataManager.LulledByTheWindOutdoorMinutes = 0f;
                }
            }

            if (IsCooldownActive() || IsShortRiskCooldownActive())
                return;

            if (isOutdoors)
            {
                if (IsRiskOrDebuffActive()) return;

                if (s_OutdoorMinutes >= OUTDOOR_RISK_DELAY_MINUTES)
                {
                    StartRisk();
                    s_OutdoorMinutes = 0f;
                    SaveDataManager.LulledByTheWindOutdoorMinutes = 0f;
                }
            }
        }

        private static void StartRisk()
        {
            if (IsRiskOrDebuffActive()) return;

            var risk = new LulledByTheWindRisk(AfflictionBodyArea.Chest);
            risk.Start();
            //MelonLogger.Msg("[LulledByTheWindRisk] Risk started.");
        }

        public static void ResetOutdoorTracking()
        {
            s_OutdoorMinutes = 0f;
            s_ContinuousIndoorMinutesForOutdoorTimer = 0f;
            s_LastOutdoorUpdateHours = -1f;
            SaveDataManager.LulledByTheWindOutdoorMinutes = 0f;
        }

        private static bool IsRiskOrDebuffActive()
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return false;

            for (int i = 0; i < mgr.m_Afflictions.Count; i++)
            {
                var aff = mgr.m_Afflictions[i];
                if (aff is LulledByTheWindRisk || aff is LulledByTheWind)
                    return true;
            }
            return false;
        }

        private static bool IsPlayerIndoorsStatic()
        {
            var weather = GameManager.GetWeatherComponent();
            if (weather == null) return false;
            return weather.IsIndoorScene() ||
                   weather.IsIndoorEnvironment() ||
                   GameManager.GetPlayerInVehicle().IsInside() ||
                   GameManager.GetSnowShelterManager().PlayerInShelter();
        }

        public override void OnUpdate()
        {
            if (GameManager.GetPlayerObject() == null) return;
            var tod = GameManager.GetTimeOfDayComponent();
            if (tod == null) return;

            float currentHours = tod.GetHoursPlayedNotPaused();
            if (m_LastUpdateHours < 0f)
                m_LastUpdateHours = currentHours;

            float deltaMinutes = Mathf.Clamp((currentHours - m_LastUpdateHours) * 60f, 0f, 0.25f);
            m_LastUpdateHours = currentHours;

            bool isOutdoors = !IsPlayerIndoorsStatic();

            if (isOutdoors)
            {
                m_ContinuousIndoorMinutes = 0f;
                SaveDataManager.LulledByTheWindContinuousIndoorMinutes = 0f;

                if (Risk)
                {
                    m_RiskValue += deltaMinutes * 60f * OUTDOOR_INCREASE_RATE_PER_INGAME_SECOND;
                    m_RiskValue = Mathf.Clamp(m_RiskValue, 0f, 100f);
                }
                m_WasDecreasing = false;
            }
            else
            {
                m_ContinuousIndoorMinutes += deltaMinutes;
                SaveDataManager.LulledByTheWindContinuousIndoorMinutes = m_ContinuousIndoorMinutes;

                bool shouldDecrease = m_ContinuousIndoorMinutes >= INDOOR_DECREASE_DELAY_MINUTES && m_RiskValue > 0f;
                if (shouldDecrease)
                {
                    m_RiskValue -= deltaMinutes * 60f * SLOW_DECREASE_RATE_PER_INGAME_SECOND;
                    m_RiskValue = Mathf.Clamp(m_RiskValue, 0f, 100f);
                    m_WasDecreasing = true;
                }
                else
                {
                    m_WasDecreasing = false;
                }
            }

            if (m_RiskValue <= 0.1f && m_WasDecreasing)
            {
                SaveDataManager.LulledByTheWindRiskPercentage = 0f;
                Cure(true);
                return;
            }

            if (m_RiskValue >= 100f)
            {
                MelonCoroutines.Start(TransitionToDebuffNextFrame());
            }

            SaveDataManager.LulledByTheWindRiskPercentage = m_RiskValue;
        }

        private IEnumerator TransitionToDebuffNextFrame()
        {
            yield return null;
            Cure(false);
            var debuff = new LulledByTheWind(AfflictionBodyArea.Chest);
            debuff.Start();

            if (GameManager.GetPlayerObject() != null)
                GameAudioManager.PlaySound(EVENTS.PLAY_FATIGUEHIGH, GameManager.GetPlayerObject());

            m_RiskValue = 0f;
            m_ContinuousIndoorMinutes = 0f;
            SaveDataManager.LulledByTheWindContinuousIndoorMinutes = 0f;
            m_LastUpdateHours = -1f;
            IsLulledByTheWindRisk = false;
            SaveDataManager.LulledByTheWindRiskPercentage = 0f;
        }

        public void OnFoundExistingInstance(CustomAffliction existingAffliction)
        {
            if (existingAffliction is LulledByTheWindRisk existing)
            {
                existing.ResetAffliction(false);
                existing.m_RiskValue = Mathf.Clamp(Mathf.Max(existing.m_RiskValue, SaveDataManager.LulledByTheWindRiskPercentage), 0f, 100f);
                existing.m_WasDecreasing = false;
                existing.m_ContinuousIndoorMinutes = SaveDataManager.LulledByTheWindContinuousIndoorMinutes;
                existing.m_LastUpdateHours = -1f;
                existing.m_LastStatusLogTime = 0f;
                SaveDataManager.LulledByTheWindRiskPercentage = existing.m_RiskValue;
                IsLulledByTheWindRisk = true;
            }
        }

        public void OnCure()
        {
            ResetAllInternalState();
            IsLulledByTheWindRisk = false;
            SaveDataManager.LulledByTheWindRiskPercentage = 0f;
            ResetOutdoorTracking();
        }
    }
}