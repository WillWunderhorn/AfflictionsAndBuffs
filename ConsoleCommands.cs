using System;
using AfflictionComponent.Components;
using AfflictionsAndBuffs.Afflictions;
using AfflictionsAndBuffs.Buffs;

internal static class ConsoleCommands
{
    internal static void Register()
    {
        // ==================== LULLED BY THE WIND RISK AND AFFLICTION ====================
        uConsole.RegisterCommand("lullrisk", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null) return;
            foreach (var aff in mgr.m_Afflictions)
                if (aff is LulledByTheWindRisk) return;
            var affliction = new LulledByTheWindRisk(AfflictionBodyArea.Chest);
            affliction.Start();
        }));

        uConsole.RegisterCommand("lulldebuff", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null) return;
            foreach (var aff in mgr.m_Afflictions)
                if (aff is LulledByTheWind) return;
            var debuff = new LulledByTheWind(AfflictionBodyArea.Chest);
            debuff.Start();
        }));

        uConsole.RegisterCommand("lullrisk_cure", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return;
            for (int i = mgr.m_Afflictions.Count - 1; i >= 0; i--)
            {
                if (mgr.m_Afflictions[i] is LulledByTheWindRisk)
                    mgr.m_Afflictions[i].Cure();
            }
        }));

        uConsole.RegisterCommand("lulldebuff_cure", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return;
            for (int i = mgr.m_Afflictions.Count - 1; i >= 0; i--)
            {
                if (mgr.m_Afflictions[i] is LulledByTheWind)
                    mgr.m_Afflictions[i].Cure();
            }
        }));

        // ==================== DETERMINATION ====================
        uConsole.RegisterCommand("determination", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null) return;
            foreach (var aff in mgr.m_Afflictions)
                if (aff is Determination) return;
            var buff = new Determination(AfflictionBodyArea.Chest);
            buff.Start();
        }));

        uConsole.RegisterCommand("determination_cure", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return;
            for (int i = mgr.m_Afflictions.Count - 1; i >= 0; i--)
            {
                if (mgr.m_Afflictions[i] is Determination)
                    mgr.m_Afflictions[i].Cure();
            }
        }));

        // ==================== STARVING ====================
        uConsole.RegisterCommand("starving", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null) return;
            foreach (var aff in mgr.m_Afflictions)
                if (aff is Starving) return;
            var affliction = new Starving(AfflictionBodyArea.Stomach);
            affliction.Start();
        }));

        uConsole.RegisterCommand("starving_cure", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return;
            for (int i = mgr.m_Afflictions.Count - 1; i >= 0; i--)
            {
                if (mgr.m_Afflictions[i] is Starving)
                    mgr.m_Afflictions[i].Cure();
            }
        }));

        // ==================== LITTLE HEART ====================
        uConsole.RegisterCommand("littleheart", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null) return;
            foreach (var aff in mgr.m_Afflictions)
                if (aff is LittleHeart) return;
            var buff = new LittleHeart(AfflictionBodyArea.Chest);
            buff.Start();
        }));

        uConsole.RegisterCommand("littleheart_cure", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return;
            for (int i = mgr.m_Afflictions.Count - 1; i >= 0; i--)
            {
                if (mgr.m_Afflictions[i] is LittleHeart)
                    mgr.m_Afflictions[i].Cure();
            }
        }));

        // ==================== FOG'S EMBRACE ====================
        uConsole.RegisterCommand("fogembrace", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null) return;
            foreach (var aff in mgr.m_Afflictions)
                if (aff is FogsEmbrace) return;
            var buff = new FogsEmbrace(AfflictionBodyArea.Chest);
            buff.Start();
        }));

        uConsole.RegisterCommand("fogembrace_cure", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return;
            for (int i = mgr.m_Afflictions.Count - 1; i >= 0; i--)
            {
                if (mgr.m_Afflictions[i] is FogsEmbrace)
                    mgr.m_Afflictions[i].Cure();
            }
        }));

        // ==================== BURNING HEART ====================
        uConsole.RegisterCommand("burningheart", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null) return;
            foreach (var aff in mgr.m_Afflictions)
                if (aff is BurningHeart) return;
            var buff = new BurningHeart(AfflictionBodyArea.Chest);
            buff.Start();
        }));

        uConsole.RegisterCommand("burningheart_cure", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return;
            for (int i = mgr.m_Afflictions.Count - 1; i >= 0; i--)
            {
                if (mgr.m_Afflictions[i] is BurningHeart)
                    mgr.m_Afflictions[i].Cure();
            }
        }));

        // ==================== HOW DID YOU DO THAT ====================
        uConsole.RegisterCommand("hdyd", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null) return;

            foreach (var aff in mgr.m_Afflictions)
                if (aff is HowDidYouDoThat) return;

            var affliction = new HowDidYouDoThat(AfflictionBodyArea.Chest);
            affliction.Start();
        }));

        uConsole.RegisterCommand("hdyd_cure", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return;

            for (int i = mgr.m_Afflictions.Count - 1; i >= 0; i--)
            {
                if (mgr.m_Afflictions[i] is HowDidYouDoThat)
                    mgr.m_Afflictions[i].Cure();
            }
        }));

        // ==================== LUNAR SYNDROME ====================
        uConsole.RegisterCommand("lunar", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null) return;

            foreach (var aff in mgr.m_Afflictions)
                if (aff is LunarSyndrome) return;

            var affliction = new LunarSyndrome(AfflictionBodyArea.Chest);
            affliction.Start();
        }));

        uConsole.RegisterCommand("lunar_cure", new Action(() =>
        {
            var mgr = AfflictionManager.GetAfflictionManagerInstance();
            if (mgr == null || mgr.m_Afflictions == null) return;

            for (int i = mgr.m_Afflictions.Count - 1; i >= 0; i--)
            {
                if (mgr.m_Afflictions[i] is LunarSyndrome)
                    mgr.m_Afflictions[i].Cure();
            }
        }));

        uConsole.RegisterCommand("lullrisk", new Action(() => uConsole.RunCommand("lullrisk")));
        uConsole.RegisterCommand("lulldebuff", new Action(() => uConsole.RunCommand("lulldebuff")));

        uConsole.RegisterCommand("lullriskcure", new Action(() => uConsole.RunCommand("lullrisk_cure")));
        uConsole.RegisterCommand("lulldebuffcure", new Action(() => uConsole.RunCommand("lulldebuff_cure")));

        uConsole.RegisterCommand("determination", new Action(() => uConsole.RunCommand("determination")));
        uConsole.RegisterCommand("determinationcure", new Action(() => uConsole.RunCommand("determination_cure")));

        uConsole.RegisterCommand("starving", new Action(() => uConsole.RunCommand("starving")));
        uConsole.RegisterCommand("starvingcure", new Action(() => uConsole.RunCommand("starving_cure")));

        uConsole.RegisterCommand("littleheart", new Action(() => uConsole.RunCommand("littleheart")));
        uConsole.RegisterCommand("littleheartcure", new Action(() => uConsole.RunCommand("littleheart_cure")));

        uConsole.RegisterCommand("fogembrace", new Action(() => uConsole.RunCommand("fogembrace")));
        uConsole.RegisterCommand("fogembracecure", new Action(() => uConsole.RunCommand("fogembrace_cure")));

        uConsole.RegisterCommand("hdyd", new Action(() => uConsole.RunCommand("hdyd")));
        uConsole.RegisterCommand("hdyd_cure", new Action(() => uConsole.RunCommand("hdyd_cure")));

        uConsole.RegisterCommand("lunar", new Action(() => uConsole.RunCommand("lunar")));
        uConsole.RegisterCommand("lunarcure", new Action(() => uConsole.RunCommand("lunar_cure")));
    }
}