using AfflictionComponent.Components;
using AfflictionComponent.Enums;
using AfflictionComponent.Interfaces;
using Il2CppAK;

namespace AfflictionsAndBuffs.Afflictions
{
    public class LulledByTheWind : CustomAffliction, IDuration, IRemedies, IInstance
    {
        public InstanceType Type { get; set; } = InstanceType.Single;
        public float Duration { get; set; } = 3f;
        public float EndTime { get; set; }
        public bool InstantHeal { get; set; } = false;
        public Tuple<string, int, int>[] RemedyItems { get; set; } = Array.Empty<Tuple<string, int, int>>();
        public Tuple<string, int, int>[] AltRemedyItems { get; set; } = Array.Empty<Tuple<string, int, int>>();

        public static bool IsLulledByTheWind { get; private set; } = false;

        public LulledByTheWind(AfflictionBodyArea bodyArea)
            : base(
                  "GAMEPLAY_LulledByTheWindName",
                  "GAMEPLAY_LulledByTheWindCause",
                  "GAMEPLAY_LulledByTheWindDescription",
                  null, 
                  "AfflictionsAndBuffs.Resources.Icons.LulledByTheWind.png",
                  bodyArea,
                  true)
        {
            float now = GameManager.GetTimeOfDayComponent()?.GetHoursPlayedNotPaused() ?? 0f;

            if (GameManager.GetPlayerObject() != null)
                GameAudioManager.PlaySound(EVENTS.PLAY_FATIGUEHIGH, GameManager.GetPlayerObject());

            IsLulledByTheWind = true;

        }

        public override void OnUpdate()
        {
            IsLulledByTheWind = true;
        }

        public void CureSymptoms() { }

        public void OnCure()
        {
            IsLulledByTheWind = false;

            float now = GameManager.GetTimeOfDayComponent()?.GetHoursPlayedNotPaused() ?? 0f;

            SaveDataManager.LulledByTheWindCooldownEndHours = now + 48f;

            //MelonLogger.Msg($"48 cooldown started! Next at hour {SaveDataManager.LulledByTheWindCooldownEndHours:F1}");
            
        }

        public void OnFoundExistingInstance(CustomAffliction existingAffliction)
        {
            if (existingAffliction is LulledByTheWind existing)
            {
                existing.ResetAffliction(resetRemedies: false);
                float now = GameManager.GetTimeOfDayComponent()?.GetHoursPlayedNotPaused() ?? 0f;
                existing.EndTime = now + existing.Duration;
            }
        }
    }

    [HarmonyPatch(typeof(Fatigue), "CalculateFatigueIncrease")]
    internal static class LulledByTheWindFatiguePatch
    {
        private static void Postfix(Fatigue __instance, float realtimeSeconds, ref float __result)
        {
            if (LulledByTheWind.IsLulledByTheWind)
            {
                __result *= 5f;
            }
        }
    }
}