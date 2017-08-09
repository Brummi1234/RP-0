﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalConstructionTime;

namespace RP0
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION })]
    public class KCTBinderModule : ScenarioModule
    {
        // FIXME if we change the min build rate, FIX THIS.
        protected const double BuildRateOffset = -0.05001d;

        protected double nextTime = -1d;
        protected double checkInterval = 0.5d;
        protected int[] padCounts = new int[10];

        protected bool skipOne = true;
        protected bool skipTwo = true;

        protected void Update()
        {
            if (HighLogic.CurrentGame == null || KerbalConstructionTime.KerbalConstructionTime.instance == null)
                return;

            if (skipOne)
            {
                skipOne = false;
                return;
            }

            if (skipTwo)
            {
                skipTwo = false;
                return;
            }

            if (MaintenanceHandler.Instance == null)
                return;

            double time = Planetarium.GetUniversalTime();
            if (nextTime > time)
                return;

            nextTime = time + checkInterval;

            for (int i = padCounts.Length; i-- > 0;)
                padCounts[i] = 0;

            foreach (KCT_KSC ksc in KCT_GameStates.KSCs)
            {
                double buildRate = 0d;
                for (int i = ksc.VABRates.Count; i-- > 0;)
                    buildRate += Math.Max(0d, ksc.VABRates[i] + BuildRateOffset);
                for (int i = ksc.SPHRates.Count; i-- > 0;)
                    buildRate += Math.Max(0d, ksc.SPHRates[i] + BuildRateOffset);
                if (buildRate > 0.001d)
                    MaintenanceHandler.Instance.kctBuildRates[ksc.KSCName] = buildRate;
                for (int i = ksc.LaunchPads.Count; i-- > 0;)
                    ++padCounts[ksc.LaunchPads[i].level];
            }
            double RDRate = KCT_MathParsing.ParseNodeRateFormula(10, 0, false);

            MaintenanceHandler.Instance.kctResearchRate = RDRate;
            MaintenanceHandler.Instance.kctPadCounts = padCounts;
        }
    }
}
