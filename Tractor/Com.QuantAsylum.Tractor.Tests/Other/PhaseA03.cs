using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using Tractor;
using Tractor.Com.QuantAsylum.Tractor.Tests;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;
using System.Linq;

namespace Com.QuantAsylum.Tractor.Tests
{
    /// <summary>
    /// This test will check the phase against a reference signal
    /// </summary>
    [Serializable]
    public class PhaseA03 : PhaseTestBase
    {
        [ObjectEditorAttribute(Index = 100, DisplayText = "FFT Size (k)", MustBePowerOfTwo = true, MinValue = 2, MaxValue = 64)]
        public uint FftSize = 8;

        [ObjectEditorAttribute(Index = 100, DisplayText = "Retry Count")]
        public int RetryCount = 2;

        [ObjectEditorAttribute(Index = 104, DisplayText = "Measure Left Channel")]
        public bool LeftChannel = true;

        [ObjectEditorAttribute(Index = 105, DisplayText = "Left Reference", MaxLength = 128, IsPhase = 1)]
        public string LeftReference = "Left QA Output";

        [ObjectEditorAttribute(Index = 106, DisplayText = "Measure Right Channel")]
        public bool RightChannel = true;

        [ObjectEditorAttribute(Index = 108, DisplayText = "Right Reference", MaxLength = 128, IsPhase = 2)]
        public string RightReference = "Right QA Output";

        //[ObjectEditorAttribute(Index = 110, DisplayText = "Display Y Max", MinValue = -200, MaxValue = 200, MustBeGreaterThanIndex = 120)]
        //public int YMax = 10;

        //[ObjectEditorAttribute(Index = 120, DisplayText = "Display Y Min", MinValue = -200, MaxValue = 200)]
        //public int YMin = -180;

        [ObjectEditorAttribute(Index = 130, DisplayText = "Pre-analyzer Input Gain (dB)", MinValue = -100, MaxValue = 100)]
        public int PreAnalyzerInputGain = 0;

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

        public PhaseA03() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        public override string GetTestDescription()
        {
            return "Measures the phase at a specified frequency and amplitude and compares to a chosen reference. Results must be within a given window to pass." +
                "Phase values using QA Output as reference reflect the values shown in the QA401 software.";
        }

        public override string GetTestLimits()
        {
            return string.Format("{0:N1}...{1:N1} deg", MinimumPhase, MaximumPhase);
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
            const int leftIn = 0;
            const int rightIn = 1;
            const int leftOut = 2;
            const int rightOut = 3;

            tr = new TestResult(2);
            Tm.SetToDefaults();

            ((IAudioAnalyzer)Tm.TestClass).SetFftLength(FftSize * 1024);
            ((IAudioAnalyzer)Tm.TestClass).SetYLimits(10, -180);
            ((IAudioAnalyzer)Tm.TestClass).SetOffsets(PreAnalyzerInputGain, 0);
            ((IAudioAnalyzer)Tm.TestClass).SetMuting(!LeftChannel, !RightChannel);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange.InputRange);
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, AnalyzerOutputLevel, TestFrequency);

            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();

            while (((IAudioAnalyzer)Tm.TestClass).AnalyzerIsBusy())
            {
                float current = ((ICurrentMeter)Tm.TestClass).GetDutCurrent();
                Log.WriteLine(LogType.General, "Current: " + current);
            }

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            bool passLeft = true, passRight = true;

            double sampleRate;

            if (LeftChannel) 
            {
                // calculate phase for Left Channel Out with Left Input Reference
                PointD[] timeDataLeftIn = ((IAudioAnalyzer)Tm.TestClass).GetTimeData(leftIn);
                PointD[] timeDataLeftOut = ((IAudioAnalyzer)Tm.TestClass).GetTimeData(LeftReference == "Left QA Output"?leftOut:rightIn);

                sampleRate = timeDataLeftIn[1].X - timeDataLeftIn[0].X;

                double[] yValues1 = ExtractYValues(timeDataLeftIn);
                double[] yValues2 = ExtractYValues(timeDataLeftOut);

                double[] crossCorrelation = CrossCorrelate(yValues1, yValues2);
                int maxIndex = FindMaxIndex(crossCorrelation);

                int lag = maxIndex - (yValues1.Length - 1);
                double leftPhase = CalculatePhaseDifference(lag, sampleRate, TestFrequency, LeftReference == "Left QA Output" ? true : false);

                tr.StringValue[0] = leftPhase.ToString("0.0") + " deg";

                if (leftPhase < MinimumPhase || leftPhase > MaximumPhase)
                {
                    passLeft = false;
                }
            }
            else
            {
                tr.StringValue[0] = "SKIP";
            }

            if (RightChannel)
            {
                // calculate phase for Right Channel Out with Right Input Reference
                PointD[] timeDataRightIn = ((IAudioAnalyzer)Tm.TestClass).GetTimeData(rightIn);
                PointD[] timeDataRightOut = ((IAudioAnalyzer)Tm.TestClass).GetTimeData(RightReference == "Right QA Output" ? rightOut : leftIn);

                sampleRate = timeDataRightIn[1].X - timeDataRightIn[0].X;

                double[] yValues1 = ExtractYValues(timeDataRightIn);
                double[] yValues2 = ExtractYValues(timeDataRightOut);

                double[] crossCorrelation = CrossCorrelate(yValues1, yValues2);
                int maxIndex = FindMaxIndex(crossCorrelation);

                int lag = maxIndex - (yValues1.Length - 1);
                double rightPhase = CalculatePhaseDifference(lag, sampleRate, TestFrequency, RightReference == "Right QA Output" ? true : false);

                tr.StringValue[1] = rightPhase.ToString("0.0") + " deg";

                if (rightPhase < MinimumPhase || rightPhase > MaximumPhase)
                {
                    passRight = false;
                }

            }
            else
            {
                tr.StringValue[1] = "SKIP";
            }   


            if (LeftChannel && RightChannel)
                tr.Pass = passLeft && passRight;
            else if (LeftChannel)
                tr.Pass = passLeft;
            else if (RightChannel)
                tr.Pass = passRight;
        }

        public static double[] ExtractYValues(PointD[] points)
        {
            return points.Select(p => p.Y).ToArray();
        }

        public static double[] CrossCorrelate(double[] signal1, double[] signal2)
        {
            int length = signal1.Length + signal2.Length - 1;
            double[] result = new double[length];

            for (int i = 0; i < length; i++)
            {
                double sum = 0;
                for (int j = 0; j < signal1.Length; j++)
                {
                    int k = i - j;
                    if (k >= 0 && k < signal2.Length)
                    {
                        sum += signal1[j] * signal2[k];
                    }
                    //else break;
                }
                result[i] = sum;
            }

            return result;
        }

        public static int FindMaxIndex(double[] array)
        {
            int maxIndex = 0;
            double maxValue = array[0];

            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] > maxValue)
                {
                    maxValue = array[i];
                    maxIndex = i;
                }
            }

            return maxIndex;
        }

        public static double CalculatePhaseDifference(int lag, double samplingRate, double frequency, bool offset)
        {
            double offsetVal = offset ? 2.08402E-05 : 0.0;
            double timeDifference = lag * samplingRate - offsetVal;
            double phaseDifference = (timeDifference * frequency * 360) % 360;

            if (phaseDifference > 180)
            {
                phaseDifference -= 360;
            }
            else if (phaseDifference < -180)
            {
                phaseDifference += 360;
            }

            return phaseDifference;
        }
    }
}