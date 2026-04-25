using AfflictionComponent.Components;
using AfflictionComponent.Enums;
using AfflictionComponent.Interfaces;

namespace AfflictionsAndBuffs.Buffs
{
    public class LittleHeart : CustomAffliction, IInstance, IDuration, IBuff
    {
        public InstanceType Type { get; set; } = InstanceType.Single;
        public float Duration { get; set; } = 1f;
        public float EndTime { get; set; }

        public bool Buff { get; set; } = true;
        public bool BuffCold { get; set; }
        public bool BuffFatigue { get; set; }
        public bool BuffHunger { get; set; }
        public bool BuffThirst { get; set; }

        public static bool IsActive { get; private set; } = false;

        private float m_StartHours = -1f;

        public LittleHeart(AfflictionBodyArea bodyArea = AfflictionBodyArea.Chest)
            : base(
                  "GAMEPLAY_LittleHeartName",
                  "GAMEPLAY_LittleHeartCause",
                  "GAMEPLAY_LittleHeartDescription",
                  null,
                  "AfflictionsAndBuffs.Resources.Icons.LittleHeart.png",
                  bodyArea,
                  true)
        {
            IsActive = true;
            m_StartHours = GameManager.GetTimeOfDayComponent()?.GetHoursPlayedNotPaused() ?? 0f;

            var stamina = GameManager.GetFatigueComponent();
            var health = GameManager.GetConditionComponent();

            if (stamina != null)
            {
                stamina.m_CurrentFatigue = Mathf.Max(0f, stamina.m_CurrentFatigue - 30f);
            }
            if (health != null)
            {
                health.m_CurrentHP = Mathf.Min(health.m_MaxHP, health.m_CurrentHP + 8f);
            }

        }

        public override void OnUpdate()
        {
            IsActive = true;

            var tod = GameManager.GetTimeOfDayComponent();
            if (tod == null) return;

            float currentHours = tod.GetHoursPlayedNotPaused();
            if (m_StartHours < 0f) m_StartHours = currentHours;

            if (currentHours - m_StartHours >= Duration)
            {
                OnCure();
            }
        }

        public void OnCure()
        {
            IsActive = false;

            float now = GameManager.GetTimeOfDayComponent()?.GetHoursPlayedNotPaused() ?? 0f;
            SaveDataManager.LittleHeartCooldownEndHours = now + (24f * 15f);

        }

        public void OnFoundExistingInstance(CustomAffliction existingAffliction)
        {
            if (existingAffliction is LittleHeart)
                IsActive = true;
        }

        public static void UpdateLittleHeart()
        {
            var condition = GameManager.GetConditionComponent();
            var tod = GameManager.GetTimeOfDayComponent();
            if (condition == null || tod == null) return;

            float now = tod.GetHoursPlayedNotPaused();
            bool lowCondition = condition.m_CurrentHP <= 20f;
            bool cooldownActive = now < SaveDataManager.LittleHeartCooldownEndHours;

            if (lowCondition && !IsActive && !cooldownActive && tod.GetDayNumber() >= 30)
            {
                new LittleHeart(AfflictionBodyArea.Chest).Start();
            }
        }

        public static bool IsLittleHeartBuffActive()
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return false;

            for (int i = 0; i < mgr.m_Afflictions.Count; i++)
            {
                if (mgr.m_Afflictions[i] is LittleHeart)
                    return true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Fatigue), "CalculateFatigueIncrease")]
    internal static class LittleHeartFatiguePatch
    {
        private static void Postfix(Fatigue __instance, float realtimeSeconds, ref float __result)
        {
            if (LittleHeart.IsLittleHeartBuffActive())
                __result *= 0.2f;
        }
    }
}