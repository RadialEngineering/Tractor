﻿using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    /// <summary>
    /// This test will check the gain
    /// </summary>
    [Serializable]
    public class SplitGainA01 : SumSplitBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Test Frequency (Hz)", MinValue = 10, MaxValue = 20000)]
        public float TestFrequency = 1000;

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue =-100, MaxValue = 6)]
        public float AnalyzerOutputLevel = -30;

        //[ObjectEditorAttribute(Index = 220, DisplayText = "Pre-analyzer Input Gain (dB)", MinValue = -100, MaxValue = 100)]
        //public float ExternalAnalyzerInputGain = 0;

        //add radio button for left or right channel
        [ObjectEditorAttribute(Index = 104, DisplayText = "Generator Channel", IsRadio = true)]
        public string Channel = "Left";

        [ObjectEditorAttribute(Index = 230, DisplayText = "Minimum Gain to Pass (dB)", MinValue = -150, MaxValue = 100)]
        public float MinimumPassGain = -10.5f;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Maximum Gain to Pass (dB)", MinValue = -150, MaxValue = 100, MustBeGreaterThanIndex = 230)]
        public float MaximumPassGain = -9.5f;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Analyzer Input Range")]
        public AudioAnalyzerInputRanges AnalyzerInputRange = new AudioAnalyzerInputRanges() { InputRange = 6 };

        public SplitGainA01() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.LevelGain;
        }

       

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels
            tr = new TestResult(2);
            bool leftChannel = false;
            bool rightChannel = false;

            if (Channel == "Left")
                leftChannel = true;
            if (Channel == "Right")
                rightChannel = true;

            Tm.SetToDefaults();
            SetupBaseTests();
            ((IAudioAnalyzer)Tm.TestClass).SetMuting(!leftChannel, !rightChannel);

            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange.InputRange);
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);

            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, AnalyzerOutputLevel, TestFrequency);

            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            // Compute the total RMS around the freq of interest
            ((IAudioAnalyzer)Tm.TestClass).ComputePeakDb(TestFrequency * 0.90f, TestFrequency * 1.10f, out tr.Value[0], out tr.Value[1]);
            tr.Value[0] = tr.Value[0] - AnalyzerOutputLevel;
            tr.Value[1] = tr.Value[1] - AnalyzerOutputLevel;

            bool passLeft = true, passRight = true;

            tr.StringValue[0] = tr.Value[0].ToString("0.00") + " dB";
            if ((tr.Value[0] < MinimumPassGain) || (tr.Value[0] > MaximumPassGain))
                passLeft = false;

            tr.StringValue[1] = tr.Value[1].ToString("0.00") + " dB";
            if ((tr.Value[1] < MinimumPassGain) || (tr.Value[1] > MaximumPassGain))
                passRight = false;

            tr.Pass = passLeft && passRight;

            return;
        }

        public override string GetTestLimits()
        {
            return string.Format("{0:N1}...{1:N1} dB", MinimumPassGain, MaximumPassGain);
        }

        public override string GetTestDescription()
        {
            return "Measures the gain of Left and Right channels at a specified frequency and amplitude and uses only one generator output: Left or Right QA401 output. " +
                "User chooses either Left or Right output channel for generator source. Results must be within a given window to pass.";
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
