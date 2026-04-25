using AfflictionComponent.Components;
using AfflictionComponent.Enums;
using AfflictionComponent.Interfaces;
using Il2Cpp;

namespace AfflictionsAndBuffs.Afflictions
{
    public class HowDidYouDoThat : CustomAffliction, IInstance, IDuration
    {
        private static float s_NextStartAttemptHours = 0f;
        private const float StartRetryDelayHours = 0.05f;

        public InstanceType Type { get; set; } = InstanceType.Single;
        public float Duration { get; set; } = 10f;
        public float EndTime { get; set; }

        public static bool IsActive { get; private set; } = false;

        public HowDidYouDoThat(AfflictionBodyArea bodyArea = AfflictionBodyArea.Chest)
            : base(
                "GAMEPLAY_HowDidYouDoThatName",
                "GAMEPLAY_HowDidYouDoThatCause",
                "GAMEPLAY_HowDidYouDoThatDescription",
                null,
                "AfflictionsAndBuffs.Resources.Icons.HowDidYouDoThat.png",
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
            if (existingAffliction is HowDidYouDoThat)
                IsActive = true;
        }

        public static void UpdateHowDidYouDoThat()
        {
            if (SaveDataManager.HowDidYouDoThatHasAppeared)
                return;

            var tod = GameManager.GetTimeOfDayComponent();
            if (tod == null) return;

            float currentHours = tod.GetHoursPlayedNotPaused();
            if (currentHours < s_NextStartAttemptHours)
                return;

            if (tod.GetDayNumber() >= 1500 && !IsHowDidYouDoThatActive())
            {
                TryStartHowDidYouDoThat(currentHours);
            }
        }

        private static void TryStartHowDidYouDoThat(float currentHours)
        {
            if (AfflictionManager.GetAfflictionManagerInstance() == null)
            {
                s_NextStartAttemptHours = currentHours + StartRetryDelayHours;
                return;
            }

            try
            {
                new HowDidYouDoThat(AfflictionBodyArea.Chest).Start();
                SaveDataManager.HowDidYouDoThatHasAppeared = true;
            }
            catch (System.NullReferenceException)
            {
                s_NextStartAttemptHours = currentHours + StartRetryDelayHours;
            }
        }

        private static bool IsHowDidYouDoThatActive()
        {
            if (IsActive)
                return true;

            var manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null || manager.m_Afflictions == null)
                return false;

            for (int i = 0; i < manager.m_Afflictions.Count; i++)
            {
                if (manager.m_Afflictions[i] is HowDidYouDoThat)
                    return true;
            }

            return false;
        }
    }
}