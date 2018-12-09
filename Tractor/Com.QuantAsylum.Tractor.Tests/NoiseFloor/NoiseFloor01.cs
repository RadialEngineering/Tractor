﻿using Com.QuantAsylum;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Com.QuantAsylum.Tractor.TestManagers;

namespace Com.QuantAsylum.Tractor.Tests.NoiseFloors
{
    /// <summary>
    /// Measures the noise floor without any weighting applied, from 20 to 20KHz
    /// </summary>
    [Serializable]
    public class NoiseFloor01 : TestBase
    {
        public float MinimumOKNoise = -200;
        public float MaximumOKNoise = -105;

        public NoiseFloor01() : base()
        {
            TestType = TestTypeEnum.LevelGain;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            ((IComposite)Tm.TestClass).SetToDefaults();
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);

            // Disable generators
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(false, -60, 1000);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(false, -60, 1000);
            ((IAudioAnalyzer)Tm.TestClass).RunSingle();

            while (((IAudioAnalyzer)Tm.TestClass).AnalyzerIsBusy())
            {
                Thread.Sleep(20);
            }

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            tr.Value[0] = (float)((IAudioAnalyzer)Tm.TestClass).ComputeRms(((IAudioAnalyzer)Tm.TestClass).GetData(ChannelEnum.Left), 20, 20000);
            tr.Value[1] = (float)((IAudioAnalyzer)Tm.TestClass).ComputeRms(((IAudioAnalyzer)Tm.TestClass).GetData(ChannelEnum.Right), 20, 20000);

            if (LeftChannel)
                tr.StringValue[0] = tr.Value[0].ToString("0.0") + " dB";
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
                tr.StringValue[1] = tr.Value[1].ToString("0.0") + " dB";
            else
                tr.StringValue[1] = "SKIP";

            if (LeftChannel && tr.Value[0] > MinimumOKNoise && tr.Value[0] < MaximumOKNoise && RightChannel && tr.Value[1] > MinimumOKNoise && tr.Value[1] < MaximumOKNoise)
                tr.Pass = true;
            else if (!LeftChannel && RightChannel && tr.Value[1] > MinimumOKNoise && tr.Value[1] < MaximumOKNoise)
                tr.Pass = true;
            else if (!RightChannel && LeftChannel && tr.Value[0] > MinimumOKNoise && tr.Value[0] < MaximumOKNoise)
                tr.Pass = true;

            return;
        }

        public override string GetTestLimitsString()
        {
            return string.Format("{0:N1}...{1:N1} dBV", MinimumOKNoise, MaximumOKNoise);
        }

        public override string GetTestDescription()
        {
            return "Measures the noise floor (residual noise) with A-Weighting applied. If the resulting measurement is " +
                   "within the specified limits, then 'pass = true' is returned.";
        }

        public override bool IsRunnable()
        {
            if (Tm.TestClass is IAudioAnalyzer)
            {
                return true;
            }

            return false;
        }
    }
}
