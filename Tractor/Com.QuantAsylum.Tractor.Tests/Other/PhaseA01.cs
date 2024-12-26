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
    public class PhaseA01 : PhaseTestBase
    {
        [ObjectEditorAttribute(Index = 100, DisplayText = "FFT Size (k)", MustBePowerOfTwo = true, MinValue = 2, MaxValue = 64)]
        public uint FftSize = 8;

        [ObjectEditorAttribute(Index = 102, DisplayText = "Retry Count")]
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

        public PhaseA01() : base()
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

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            bool passLeft = true, passRight = true;

            //Console.WriteLine("Checking phase");

            //PointD[] leftInTime = ((IAudioAnalyzer)Tm.TestClass).GetTimeData(0);
            //PointD[] rightInTime = ((IAudioAnalyzer)Tm.TestClass).GetTimeData(1);
            //PointD[] leftOutTime = ((IAudioAnalyzer)Tm.TestClass).GetTimeData(2);
            //PointD[] rightOutTime = ((IAudioAnalyzer)Tm.TestClass).GetTimeData(3);

            //int peakIndexIn = 0;
            //int peakIndexOut = 0;
            //double phaseSum = 0;
            //double phaseAcc = 0;
            //const int avg = 25;

            //// calculate phase average for Left Channel Out with Left Input Reference
            //for (int sumNum = 0; sumNum < avg; sumNum++)
            //{
            //    // find peak index for leftInTime 
            //    for (int i = 500 + peakIndexOut; i < leftInTime.Length; i++)
            //    {
            //        if (leftInTime[i].Y > leftInTime[i - 1].Y && leftInTime[i].Y > leftInTime[i + 1].Y)
            //        {
            //            peakIndexIn = i;
            //            break;
            //        }
            //    }

            //    // find peak index for leftOutTime
            //    for (int j = 500 + peakIndexOut; j < leftOutTime.Length; j++)
            //    {
            //        if (leftOutTime[j].Y > leftOutTime[j - 1].Y && leftOutTime[j].Y > leftOutTime[j + 1].Y)
            //        {
            //            peakIndexOut = j;
            //            break;
            //        }
            //    }

            //    double timeDiff = Math.Abs(leftOutTime[peakIndexOut].X - leftInTime[peakIndexIn].X);

            //    phaseAcc = (timeDiff * TestFrequency * 360) % 360;

            //    Console.WriteLine("Phase diff: " + phaseAcc);

            //    phaseSum += phaseAcc;
            //}

            //double phaseLeft = phaseSum / avg;

            //// adjust phaseLeft to be between -180 and 180 to mimic QA401 software reading
            //if (phaseLeft >180)
            //    phaseLeft -= 360;
            //else if (phaseLeft < -180)
            //    phaseLeft += 360;

            //Console.WriteLine("Phase diff: " + phaseLeft);

            //tr.Value[0] = phaseLeft;
            //tr.StringValue[0] = tr.Value[0].ToString("0.00") + " deg";

            //if (phaseLeft < MinimumPhase || phaseLeft > MaximumPhase)
            //    passLeft = false;

            if (LeftChannel) 
            {
            // calculate phase for Left Channel Out with Left Input Reference
            double phaseLeft = ((IAudioAnalyzer)Tm.TestClass).ComputePhase(LeftReference == "Left QA Output" ? leftOut : rightIn, leftIn, true, TestFrequency);


            Console.WriteLine("Left Phase: " + phaseLeft);

            tr.Value[0] = phaseLeft;
            tr.StringValue[0] = tr.Value[0].ToString("0.00") + " deg";

            if (phaseLeft < MinimumPhase || phaseLeft > MaximumPhase)
                passLeft = false;
            }
            else
            {
                tr.StringValue[0] = "SKIP";
            }

            if (RightChannel)
            {
                // calculate phase for Right Channel Out with Right Input Reference
                double phaseRight = ((IAudioAnalyzer)Tm.TestClass).ComputePhase(RightReference == "Right QA Output" ? rightOut : leftIn, rightIn, true, TestFrequency);

                Console.WriteLine("Right Phase: " + phaseRight);

                tr.Value[1] = phaseRight;
                tr.StringValue[1] = tr.Value[1].ToString("0.00") + " deg";

                if (phaseRight < MinimumPhase || phaseRight > MaximumPhase)
                    passRight = false;
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
    }
}