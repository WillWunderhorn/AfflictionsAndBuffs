using AfflictionComponent.Components;
using AfflictionsAndBuffs.Afflictions;
using AfflictionsAndBuffs.Buffs;
using LocalizationUtilities;
using MelonLoader;

namespace AfflictionsAndBuffs
{
    public class Core : MelonMod
    {
        public static Core Instance { get; private set; }

        public override void OnInitializeMelon()
        {
            Instance = this;

            string json = LoadEmbeddedLocalization();

            if (json != null)
                LocalizationManager.LoadJsonLocalization(json);

            ConsoleCommands.Register();
        }

        public override void OnUpdate()
        {
            if (GameManager.GetPlayerObject() == null)
                return;

            var tod = GameManager.GetTimeOfDayComponent();

            if (tod == null)
                return;

            LulledByTheWindRisk.LulledByTheWindUpdateOutdoorTimer();
            Determination.UpdateWeatherBuff();
            Starving.UpdateStarving();
            LittleHeart.UpdateLittleHeart();
            FogsEmbrace.UpdateFogBuff();
            HowDidYouDoThat.UpdateHowDidYouDoThat();
            BurningHeart.UpdateBurningHeart();
            LunarSyndrome.UpdateLunarSyndrome();
        }

        private static string LoadEmbeddedLocalization()
        {
            try
            {
                const string resourceName =
                    "AfflictionsAndBuffs.Resources.Localization.Localization.json";

                using var stream =
                    System.Reflection.Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream(resourceName);

                if (stream == null)
                    return null;

                using var reader = new System.IO.StreamReader(stream);

                return reader.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }
    }
}