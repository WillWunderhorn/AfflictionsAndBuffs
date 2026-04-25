using ModData;
using Newtonsoft.Json;
using AfflictionComponent.Components;
using AfflictionsAndBuffs.Afflictions;

namespace AfflictionsAndBuffs
{
    internal static class SaveDataManager
    {
        private static readonly ModDataManager ModData = new ModDataManager(nameof(AfflictionsAndBuffs));
        private const string Suffix = "afflictionsandbuffs_data";

        internal static float LulledByTheWindRiskPercentage;
        internal static bool PendingRiskRestore;

        public static float LulledByTheWindOutdoorMinutes = 0f;
        public static float LulledByTheWindContinuousIndoorMinutes = 0f;
        public static float LulledByTheWindCooldownEndHours = 0f;
        public static float LulledByTheWindRiskShortCooldownEndHours = 0f;
        public static float StarvingCooldownEndHours = 0f;
        public static float LittleHeartCooldownEndHours = 0f;
        public static bool HowDidYouDoThatHasAppeared = false;
        internal static void OnSaveGame()
        {
            try
            {
                LulledByTheWindRiskPercentage = SnapshotRiskPercentageFromActiveAffliction();

                var data = new ModSaveData
                {
                    LulledByTheWindRiskPercentage = LulledByTheWindRiskPercentage,
                    LulledByTheWindOutdoorMinutes = LulledByTheWindOutdoorMinutes,
                    LulledByTheWindContinuousIndoorMinutes = LulledByTheWindContinuousIndoorMinutes,
                    LulledByTheWindCooldownEndHours = LulledByTheWindCooldownEndHours,
                    LulledByTheWindRiskShortCooldownEndHours = LulledByTheWindRiskShortCooldownEndHours,
                    StarvingCooldownEndHours = StarvingCooldownEndHours,
                    LittleHeartCooldownEndHours = LittleHeartCooldownEndHours,
                    HowDidYouDoThatHasAppeared = HowDidYouDoThatHasAppeared
                };

                string json = JsonConvert.SerializeObject(data);
                ModData.Save(json, Suffix);

                PendingRiskRestore = LulledByTheWindRiskPercentage > 0f;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[SaveData] Save failed: {ex.Message}");
                OnNewGame();
            }
        }

        internal static void OnLoadGame()
        {
            try
            {
                string json = ModData.Load(Suffix);
                if (string.IsNullOrEmpty(json))
                {
                    OnNewGame();
                    return;
                }

                ModSaveData data = JsonConvert.DeserializeObject<ModSaveData>(json);

                LulledByTheWindRiskPercentage = data?.LulledByTheWindRiskPercentage ?? 0f;
                LulledByTheWindOutdoorMinutes = data?.LulledByTheWindOutdoorMinutes ?? 0f;
                LulledByTheWindContinuousIndoorMinutes = data?.LulledByTheWindContinuousIndoorMinutes ?? 0f;
                LulledByTheWindCooldownEndHours = data?.LulledByTheWindCooldownEndHours ?? 0f;
                LulledByTheWindRiskShortCooldownEndHours = data?.LulledByTheWindRiskShortCooldownEndHours ?? 0f;
                StarvingCooldownEndHours = data?.StarvingCooldownEndHours ?? 0f;
                LittleHeartCooldownEndHours = data?.LittleHeartCooldownEndHours ?? 0f;
                HowDidYouDoThatHasAppeared = data?.HowDidYouDoThatHasAppeared ?? false;

                PendingRiskRestore = LulledByTheWindRiskPercentage > 0f;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[SaveData] Load failed: {ex.Message}");
                OnNewGame();
            }
        }

    internal static void OnNewGame()
        {
            LulledByTheWindRiskPercentage = 0f;
            LulledByTheWindOutdoorMinutes = 0f;
            LulledByTheWindContinuousIndoorMinutes = 0f;
            LulledByTheWindCooldownEndHours = 0f;
            LulledByTheWindRiskShortCooldownEndHours = 0f;
            StarvingCooldownEndHours = 0f;
            LittleHeartCooldownEndHours = 0f;
            HowDidYouDoThatHasAppeared = false;
            PendingRiskRestore = false;
        }

        private static float SnapshotRiskPercentageFromActiveAffliction()
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null)
                return LulledByTheWindRiskPercentage;

            for (int i = 0; i < mgr.m_Afflictions.Count; i++)
            {
                if (mgr.m_Afflictions[i] is LulledByTheWindRisk risk)
                    return risk.GetRiskValue();
            }

            return LulledByTheWindRiskPercentage;
        }
    }

    [HarmonyPatch(typeof(SaveGameSlots), nameof(SaveGameSlots.WriteSlotToDisk), new Type[] { typeof(SlotData), typeof(SaveGameSlots.Timestamp) })]
    internal class SaveDataWritePatch
    {
        private static void Prefix() => SaveDataManager.OnSaveGame();
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.LoadSaveGameSlot), new Type[] { typeof(string), typeof(int) })]
    internal class SaveDataLoadPatch
    {
        private static void Postfix()
        {
            SaveDataManager.OnLoadGame();
            LulledByTheWindRisk.RestoreOutdoorTimerOnLoad();
        }
    }

    [HarmonyPatch(typeof(SaveGameSlots), nameof(SaveGameSlots.CreateSlot), new Type[] { typeof(string), typeof(SaveSlotType), typeof(uint), typeof(Episode) })]
    internal class SaveDataNewGamePatch
    {
        private static void Postfix()
        {
            SaveDataManager.OnNewGame();
            LulledByTheWindRisk.ResetOutdoorTracking();
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.DoExitToMainMenu))]
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.LoadMainMenu))]
    internal class SaveDataMainMenuPatch
    {
        private static void Prefix() => SaveDataManager.OnSaveGame();
        private static void Postfix() => SaveDataManager.OnNewGame();
    }

    [HarmonyPatch(typeof(Application), nameof(Application.Quit), new Type[] { })]
    internal class SaveOnQuitPatch
    {
        private static void Prefix()
        {
            SaveDataManager.OnSaveGame();
        }
    }
}