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
    public class GainLRLimitsA01 : TestBase
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

        [ObjectEditorAttribute(Index = 230, DisplayText = "Minimum Left Gain to Pass (dB)", MinValue = -150, MaxValue = 100)]
        public float MinimumPassGainL = -10.5f;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Maximum Left Gain to Pass (dB)", MinValue = -150, MaxValue = 100, MustBeGreaterThanIndex = 230)]
        public float MaximumPassGainL = -9.5f;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Minimum Right Gain to Pass (dB)", MinValue = -150, MaxValue = 100)]
        public float MinimumPassGainR = -10.5f;

        [ObjectEditorAttribute(Index = 260, DisplayText = "Maximum Right Gain to Pass (dB)", MinValue = -150, MaxValue = 100, MustBeGreaterThanIndex = 250)]
        public float MaximumPassGainR = -9.5f;

        [ObjectEditorAttribute(Index = 270, DisplayText = "Analyzer Input Range")]
        public AudioAnalyzerInputRanges AnalyzerInputRange = new AudioAnalyzerInputRanges() { InputRange = 6 };

        public GainLRLimitsA01() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.LevelGain;
        }

        public void SetupBaseTests()
        {
            ((IAudioAnalyzer)Tm.TestClass).SetFftLength(FftSize * 1024);
            ((IAudioAnalyzer)Tm.TestClass).SetYLimits(YMax, YMin);
            ((IAudioAnalyzer)Tm.TestClass).SetOffsets(PreAnalyzerInputGain, 0);
            ((IAudioAnalyzer)Tm.TestClass).SetMuting(false, false);

        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels
            tr = new TestResult(2);

            Tm.SetToDefaults();
            SetupBaseTests();

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
            if ((tr.Value[0] < MinimumPassGainL) || (tr.Value[0] > MaximumPassGainL))
                passLeft = false;

            tr.StringValue[1] = tr.Value[1].ToString("0.00") + " dB";
            if ((tr.Value[1] < MinimumPassGainR) || (tr.Value[1] > MaximumPassGainR))
                passRight = false;

            tr.Pass = passLeft && passRight;


            return;
        }

        public override string GetTestLimits()
        {
            return string.Format("L{0:N1}...{1:N1} dB, R{2:N1}...{3:N1} dB", MinimumPassGainL, MaximumPassGainL, MinimumPassGainR, MaximumPassGainR);
        }

        public override string GetTestDescription()
        {
            return "Measures the gain at a specified frequency and amplitude. Results must be within a given window to pass.";
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
