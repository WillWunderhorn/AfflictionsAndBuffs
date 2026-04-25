using AfflictionComponent.Components;
using AfflictionComponent.Enums;
using AfflictionComponent.Interfaces;
using HarmonyLib;
using Il2Cpp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AfflictionsAndBuffs.Buffs
{
    public class BurningHeart : CustomAffliction, IInstance, IDuration, IBuff
    {
        public InstanceType Type { get; set; } = InstanceType.Single;
        public float Duration { get; set; } = 2f;
        public float EndTime { get; set; }
        public bool Buff { get; set; } = true;
        public bool BuffCold { get; set; }
        public bool BuffFatigue { get; set; }
        public bool BuffHunger { get; set; }
        public bool BuffThirst { get; set; }

        public static bool IsActive { get; private set; } = false;

        public BurningHeart(AfflictionBodyArea bodyArea = AfflictionBodyArea.Chest)
            : base(
                "GAMEPLAY_BurningHeartName",
                "GAMEPLAY_BurningHeartCause",
                "GAMEPLAY_BurningHeartDescription",
                null,
                "AfflictionsAndBuffs.Resources.Icons.BurningHeart.png",
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
            if (existingAffliction is BurningHeart)
            {
                IsActive = true;
            }
        }

        public static bool IsMemoryOfTheFireActive()
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return false;
            for (int i = 0; i < mgr.m_Afflictions.Count; i++)
            {
                if (mgr.m_Afflictions[i] is BurningHeart)
                    return true;
            }
            return false;
        }

        public static void OnSuccessfulFireLit()
        {
            if (GameManager.GetPlayerObject() == null) return;

            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null)
            {
                MelonCoroutines.Start(StartBuffNextFrame());
                return;
            }

            BurningHeart existing = null;
            for (int i = 0; i < mgr.m_Afflictions.Count; i++)
            {
                if (mgr.m_Afflictions[i] is BurningHeart bh)
                {
                    existing = bh;
                    break;
                }
            }

            if (existing != null)
            {
                var tod = GameManager.GetTimeOfDayComponent();
                if (tod != null)
                {
                    float currentHours = tod.GetHoursPlayedNotPaused();
                    existing.EndTime = currentHours + existing.Duration;
                }
            }
            else
            {
                MelonCoroutines.Start(StartBuffNextFrame());
            }
        }

        private static IEnumerator StartBuffNextFrame()
        {
            yield return null;
            if (GameManager.GetPlayerObject() == null) yield break;
            var buff = new BurningHeart(AfflictionBodyArea.Chest);
            buff.Start();
        }

        public static void UpdateBurningHeart() { }
    }

    [HarmonyPatch(typeof(Fire), nameof(Fire.AddFuel))]
    internal static class FireLightPatch
    {
        private static void Postfix(Fire __instance)
        {
            if (__instance == null) return;

            var tempComp = GameManager.GetFreezingComponent();
            if (tempComp == null) return;

            bool isTempFine = tempComp.GetFreezingLevel() == FreezingLevel.Warm ||
                              tempComp.GetFreezingLevel() == FreezingLevel.SlightlyCold;

            if (__instance.m_FuelHeatIncrease >= 18f && isTempFine)
            {
                BurningHeart.OnSuccessfulFireLit();
            }
        }
    }

    [HarmonyPatch(typeof(Weather), nameof(Weather.CalculateCurrentTemperature))]
    public static class ImprovedWeather
    {
        internal static void Postfix(ref Weather __instance)
        {
            if (BurningHeart.IsMemoryOfTheFireActive())
            {
                __instance.m_CurrentTemperature += 4f;
            }
        }
    }
}