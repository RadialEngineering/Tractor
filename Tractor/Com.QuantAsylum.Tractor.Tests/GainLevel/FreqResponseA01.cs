﻿using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.IO;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    /// <summary>
    /// Performs a swept test of gain using an expo sweep and compares the result to a specified mask file.
    /// </summary>
    [Serializable]
    public class FreqResponseA01 : AudioTestBase
    {

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue =-100, MaxValue = 6)]
        public float AnalyzerOutputLevel = -30;

        [ObjectEditorAttribute(Index = 212, DisplayText = "Smoothing (1/N), N=", MinValue = 1, MaxValue = 96)]
        public int SmoothingDenominator = 12;

        [ObjectEditorAttribute(Index = 215, DisplayText = "Mask File Name", IsFileName = true, MaxLength = 512)]
        public string MaskFileName = "";

        [ObjectEditorAttribute(Index = 250, DisplayText = "Analyzer Input Range")]
        public AudioAnalyzerInputRanges AnalyzerInputRange = new AudioAnalyzerInputRanges() { InputRange = 6 };

        public FreqResponseA01() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.LevelGain;
            FftSize = 32;  // Override and set 32K as default
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels
            tr = new TestResult(2);

            // Get the absolute path of the mask file
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string absolutePath = Path.Combine(appDirectory, MaskFileName);

            Tm.SetToDefaults();
            SetupBaseTests();

            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange.InputRange);
            ((IAudioAnalyzer)Tm.TestClass).DoFrAquisition(AnalyzerOutputLevel, 0, SmoothingDenominator);
            ((IAudioAnalyzer)Tm.TestClass).TestMask(absolutePath, LeftChannel, RightChannel, false, out bool passLeft, out bool passRight, out _);

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            if (LeftChannel)
            {
                tr.StringValue[0] = passLeft.ToString();
            }
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
            {
                tr.StringValue[1] = passRight.ToString();
            }
            else
                tr.StringValue[1] = "SKIP";


            if (LeftChannel && RightChannel)
                tr.Pass = passLeft && passRight;
            else if (LeftChannel)
                tr.Pass = passLeft;
            else if (RightChannel)
                tr.Pass = passRight;

            return;
        }

        public override string GetTestLimits()
        {
            return "---";
        }

        public override string GetTestDescription()
        {
            return "Measures the frequency response using a chirp and compares to a mask. NOTE: FFT should be >=32768.";
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
