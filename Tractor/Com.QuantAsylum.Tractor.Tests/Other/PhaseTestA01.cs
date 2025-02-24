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
    public class PhaseTest : AudioTestBase
    {

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue =-100, MaxValue = 6)]
        public float AnalyzerOutputLevel = -30;

        [ObjectEditorAttribute(Index = 220, DisplayText = "Windowing (mS) (0=none)", MinValue = 0, MaxValue = 20)]
        public float WindowingMs = 5;

        [ObjectEditorAttribute(Index = 225, DisplayText = "Smoothing (1/N), N=", MinValue = 1, MaxValue = 96)]
        public int SmoothingDenominator = 12;

        [ObjectEditorAttribute(Index = 230, DisplayText = "Mask File Name", IsFileName = true, MaxLength = 512)]
        public string MaskFileName = "";

        [ObjectEditorAttribute(Index = 240, DisplayText = "Analyzer Input Range")]
        public AudioAnalyzerInputRanges AnalyzerInputRange = new AudioAnalyzerInputRanges() { InputRange = 6 };

        [ObjectEditorAttribute(Index = 250, DisplayText = "Check Phase")]
        public bool CheckPhase = false;

        public PhaseTest() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels
            tr = new TestResult(2);

            Tm.SetToDefaults();
            SetupBaseTests();

            // Get the absolute path of the mask file
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string absolutePath = Path.Combine(appDirectory, MaskFileName);


            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange.InputRange);

            ((IAudioAnalyzer)Tm.TestClass).DoFrAquisition(AnalyzerOutputLevel, WindowingMs/1000, SmoothingDenominator);
            ((IAudioAnalyzer)Tm.TestClass).TestMask(absolutePath, false, false, true, out bool passLeft, out bool passRight, out bool passMath);
            ((IAudioAnalyzer)Tm.TestClass).AddMathToDisplay();

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            bool passPhase = true;
            int passCount = 0;
            if (CheckPhase) 
            {
                if (((IAudioAnalyzer)Tm.TestClass).LRVerifyPhase((int)FftSize * 1024 / 4)) ++passCount;
                if (((IAudioAnalyzer)Tm.TestClass).LRVerifyPhase((int)FftSize * 1024 / 4 + 300)) ++passCount;
                if (passCount != 2)
                    passPhase = false;
            }

            tr.Pass = passMath && passPhase;

            if (passPhase == false)
            {
                tr.OperatorMessage += "PHASE ";
            }

            if (passMath == false)
            {
                tr.OperatorMessage += "MASK";
            }

            return;
        }

        public override string GetTestLimits()
        {
            return "---";
        }

        public override string GetTestDescription()
        {
            return "Compares a reference microphone (left channel) to a test microphone (right channel). The difference (L-R) is " +
                "displayed and compared to a specified mask.";
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
