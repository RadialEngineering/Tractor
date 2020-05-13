﻿using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.NoiseFloors
{
    /// <summary>
    /// Measures the noise floor without any weighting applied, from 20 to 20KHz
    /// </summary>
    [Serializable]
    public class RmsLevelA03 : AudioTestBase
    {

        [ObjectEditorAttribute(Index = 230, DisplayText = "Maximum Level to Pass (dBV)", MinValue = -150, MaxValue = 0, MustBeGreaterThanIndex = 240)]
        public float MaximumPassLevel = -9.5f;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Minimum Level to Pass (dBV)", MinValue = -150, MaxValue = 0)]
        public float MinimumPassLevel = -10.5f;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Load Impedance (ohms)", ValidInts = new int[] { 8, 4 })]
        public int ProgrammableLoadImpedance = 8;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Analyzer Input Range", ValidInts = new int[] { 6, 26 })]
        public int AnalyzerInputRange = 6;

        [ObjectEditorAttribute(Index = 260, DisplayText = "RMS Measurement Start (Hz)", MinValue = 10, MaxValue = 20000)]
        public float StartFreq = 20;

        [ObjectEditorAttribute(Index = 270, DisplayText = "RMS Measurement Stop (Hz)", MinValue = 10, MaxValue = 20000, MustBeGreaterThanIndex = 260)]
        public float EndFreq = 20000;

        public RmsLevelA03() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.LevelGain;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetToDefaults();
            SetupBaseTests();

            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange);
            ((IProgrammableLoad)Tm.TestClass).SetImpedance(ProgrammableLoadImpedance);

            // Disable generators
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(false, -60, 1000);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(false, -60, 1000);
            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();


            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            ((IAudioAnalyzer)Tm.TestClass).ComputeRms(StartFreq, EndFreq, out tr.Value[0], out tr.Value[1]);

            if (LeftChannel)
                tr.StringValue[0] = tr.Value[0].ToString("0.0") + " dBV";
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
                tr.StringValue[1] = tr.Value[1].ToString("0.0") + " dBV";
            else
                tr.StringValue[1] = "SKIP";

            if (LeftChannel && tr.Value[0] > MinimumPassLevel && tr.Value[0] < MaximumPassLevel && RightChannel && tr.Value[1] > MinimumPassLevel && tr.Value[1] < MaximumPassLevel)
                tr.Pass = true;
            else if (!LeftChannel && RightChannel && tr.Value[1] > MinimumPassLevel && tr.Value[1] < MaximumPassLevel)
                tr.Pass = true;
            else if (!RightChannel && LeftChannel && tr.Value[0] > MinimumPassLevel && tr.Value[0] < MaximumPassLevel)
                tr.Pass = true;

            return;
        }

        public override string GetTestLimits()
        {
            return string.Format("{0:N1}...{1:N1} dBV", MinimumPassLevel, MaximumPassLevel);
        }

        public override string GetTestDescription()
        {
            return "Measures the RMS level in the specified bandwidth with the indicated load applied. If the resulting measurement is " +
                   "within the specified limits, then 'pass = true' is returned.";
        }

        internal override int HardwareMask
        {
            get
            {
                return (int)HardwareTypes.AudioAnalyzer | (int)HardwareTypes.ProgrammableLoad;
            }
        }
    }
}
