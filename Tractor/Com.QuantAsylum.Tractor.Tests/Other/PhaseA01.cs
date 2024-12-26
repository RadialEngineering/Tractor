using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using Tractor;
using Tractor.Com.QuantAsylum.Tractor.Tests;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

namespace Com.QuantAsylum.Tractor.Tests
{
    /// <summary>
    /// This test will check the phase against a reference signal
    /// </summary>
    [Serializable]
    public class PhaseA01 : AudioTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Test Frequency (Hz)", MinValue = 10, MaxValue = 20000)]
        public float TestFrequency = 1000;

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue = -100, MaxValue = 6)]
        public float AnalyzerOutputLevel = -30;

        [ObjectEditorAttribute(Index = 230, DisplayText = "Minimum Phase to Pass (deg)", MinValue = -180, MaxValue = 360)]
        public float MinimumPhase = -10.5f;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Maximum Phase to Pass (deg)", MinValue = -180, MaxValue = 360, MustBeGreaterThanIndex = 230)]
        public float MaximumPhase = -9.5f;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Analyzer Input Range")]
        public AudioAnalyzerInputRanges AnalyzerInputRange = new AudioAnalyzerInputRanges() { InputRange = 6 };

        public PhaseA01() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        public override string GetTestDescription()
        {
            return "Measures the phase at a specified frequency and amplitude and compares to a reference. Results must be within a given window to pass.";
        }

        public override string GetTestLimits()
        {
            return "---";
        }

        public override bool IsRunnable()
        {
            if (Tm.TestClass is IAudioAnalyzer)
            {
                return true;
            }

            return false;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            tr = new TestResult(2);
            Tm.SetToDefaults();
            SetupBaseTests();

            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange.InputRange);
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);

            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, AnalyzerOutputLevel, TestFrequency);

            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            bool passLeft = true, passRight = true;

            Console.WriteLine("Checking phase");

            PointD[] phaseDataIn = ((IAudioAnalyzer)Tm.TestClass).getPhase(0);
            PointD[] phaseDataIn1 = ((IAudioAnalyzer)Tm.TestClass).getPhase(1);

            PointD[] leftIn = ((IAudioAnalyzer)Tm.TestClass).GetData(0);
            PointD[] rightIn = ((IAudioAnalyzer)Tm.TestClass).GetData(1);
            PointD[] leftOut = ((IAudioAnalyzer)Tm.TestClass).GetData(2);
            PointD[] rightOut = ((IAudioAnalyzer)Tm.TestClass).GetData(3);

            PointD[] leftInTime = ((IAudioAnalyzer)Tm.TestClass).GetTimeData(0);
            PointD[] rightInTime = ((IAudioAnalyzer)Tm.TestClass).GetTimeData(1);
            PointD[] leftOutTime = ((IAudioAnalyzer)Tm.TestClass).GetTimeData(2);
            PointD[] rightOutTime = ((IAudioAnalyzer)Tm.TestClass).GetTimeData(3);

            // find zero crossing index
            int zeroCrossingIndex = 0;
            for (int i = 1000; i < leftInTime.Length; i++)
            {
                if (leftInTime[i].Y < 0.0 && leftInTime[i + 1].Y >= 0.0)
                {
                    zeroCrossingIndex = i;
                    break;
                }
            }

            // find peak index
            int peakIndex = 0;
            for (int i = 1000; i < leftInTime.Length; i++)
            {
                if (leftInTime[i].Y > leftInTime[i-1].Y && leftInTime[i].Y > leftInTime[i+1].Y)
                {
                    peakIndex = i;
                    break;
                }
            }

            // find peak index for leftOutTime
            int peakIndexOut = 0;
            for (int i = 1001; i < leftOutTime.Length; i++)
            {
                if (leftOutTime[i].Y > leftOutTime[i - 1].Y && leftOutTime[i].Y > leftOutTime[i + 1].Y)
                {
                    peakIndexOut = i;
                    break;
                }
            }

            double timeDiff = Math.Abs(leftInTime[peakIndex].X - leftOutTime[peakIndexOut].X);

            double phaseDiff = (timeDiff * TestFrequency * 360) % 360;

            Console.WriteLine("Phase diff: " + phaseDiff);


            if (LeftChannel && RightChannel)
                tr.Pass = passLeft && passRight;
            else if (LeftChannel)
                tr.Pass = passLeft;
            else if (RightChannel)
                tr.Pass = passRight;
        }
    }
}