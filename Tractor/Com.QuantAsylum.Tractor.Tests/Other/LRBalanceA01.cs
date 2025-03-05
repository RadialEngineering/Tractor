using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    /// <summary>
    /// This test will check the gain
    /// </summary>
    [Serializable]
    public class LRBalanceA01 : UiTestBase
    {
        [ObjectEditorAttribute(Index = 100, DisplayText = "FFT Size (k)", MustBePowerOfTwo = true, MinValue = 2, MaxValue = 64)]
        public uint FftSize = 8;

        [ObjectEditorAttribute(Index = 102, DisplayText = "Retry Count")]
        public int RetryCount = 2;

        [ObjectEditorAttribute(Index = 110, DisplayText = "Display Y Max", MinValue = -200, MaxValue = 200, MustBeGreaterThanIndex = 120)]
        public int YMax = 10;

        [ObjectEditorAttribute(Index = 120, DisplayText = "Display Y Min", MinValue = -200, MaxValue = 200)]
        public int YMin = -180;

        [ObjectEditorAttribute(Index = 130, DisplayText = "Pre-analyzer Input Gain (dB)", MinValue = -100, MaxValue = 100)]
        public int PreAnalyzerInputGain = 0;

        [ObjectEditorAttribute(Index = 200, DisplayText = "Test Frequency (Hz)", MinValue = 10, MaxValue = 20000)]
        public float TestFrequency = 1000;

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue =-100, MaxValue = 6)]
        public float AnalyzerOutputLevel = -30;

        //[ObjectEditorAttribute(Index = 220, DisplayText = "Pre-analyzer Input Gain (dB)", MinValue = -100, MaxValue = 100)]
        //public float ExternalAnalyzerInputGain = 0;

        //[ObjectEditorAttribute(Index = 230, DisplayText = "Min Gain Differential to Pass (dB)", MinValue = -150, MaxValue = 100)]
        //public float MinimumPassGain = -10.5f;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Max Gain Differential to Pass (dB)", MinValue = -150, MaxValue = 100)]
        public float MaximumPassGain = 3;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Analyzer Input Range")]
        public AudioAnalyzerInputRanges AnalyzerInputRange = new AudioAnalyzerInputRanges() { InputRange = 6 };

        [ObjectEditorAttribute(Index = 260, DisplayText = "Number of Measurements ", MinValue = 1, MaxValue = 50)]
        public int NumMeasurements = 1;

        public LRBalanceA01() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels
            tr = new TestResult(2);

            Tm.SetToDefaults();
            ((IAudioAnalyzer)Tm.TestClass).SetFftLength(FftSize * 1024);
            ((IAudioAnalyzer)Tm.TestClass).SetYLimits(YMax, YMin);
            ((IAudioAnalyzer)Tm.TestClass).SetOffsets(PreAnalyzerInputGain, 0);
            ((IAudioAnalyzer)Tm.TestClass).SetMuting(false, false);

            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange.InputRange);
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);

            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, AnalyzerOutputLevel, TestFrequency);

            bool passLeft = true;
            tr.StringValue[1] = "SKIP";

            for (int i = 0; i < NumMeasurements; i++)
            {
                ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();
                ((IAudioAnalyzer)Tm.TestClass).ComputePeakDb(TestFrequency * 0.90f, TestFrequency * 1.10f, out tr.Value[0], out tr.Value[1]);
                tr.Value[0] = Math.Abs(tr.Value[1] - tr.Value[0]);
                if (tr.Value[0] > MaximumPassGain)
                {
                    passLeft = false;
                    break;
                }
            }

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            tr.StringValue[0] = tr.Value[0].ToString("0.00") + " dB";

            tr.StringValue[1] = "SKIP";

            tr.Pass = passLeft;

            return;
        }

        public override string GetTestLimits()
        {
            return string.Format("{0:N1}...{1:N1} dB", MaximumPassGain);
        }

        public override string GetTestDescription()
        {
            return "Measures the gain differential between the Left and Right Channels, using the Right Channel as reference. Results must be within a given window to pass." +
                "Use the \"Number of Measurements\" input to perform multiple acquisitions for doing potentiometer sweeps.";
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
