using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using Tractor;
using Tractor.Com.QuantAsylum.Tractor.Tests;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Dialogs;
using System.Drawing;

namespace Com.QuantAsylum.Tractor.Tests
{
    /// <summary>
    /// This test will check the phase against a reference signal
    /// </summary>
    [Serializable]
    public class PhaseTest180A01 : PhaseTestBase
    {
        [ObjectEditorAttribute(Index = 100, DisplayText = "FFT Size (k)", MustBePowerOfTwo = true, MinValue = 2, MaxValue = 64)]
        public uint FftSize = 8;

        [ObjectEditorAttribute(Index = 102, DisplayText = "Retry Count")]
        public int RetryCount = 2;

        [ObjectEditorAttribute(Index = 104, DisplayText = "Measure Left Channel")]
        public bool LeftChannel = true;

        //[ObjectEditorAttribute(Index = 105, DisplayText = "Left Reference", MaxLength = 128, IsPhase = 1)]
        //public string LeftReference = "Left QA Output";

        [ObjectEditorAttribute(Index = 106, DisplayText = "Measure Right Channel")]
        public bool RightChannel = true;

        //[ObjectEditorAttribute(Index = 108, DisplayText = "Right Reference", MaxLength = 128, IsPhase = 2)]
        //public string RightReference = "Right QA Output";

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

        [ObjectEditorAttribute(Index = 220, DisplayText = "Prompt Message", MaxLength = 512)]
        public string PromptMessage = "";

        [ObjectEditorAttribute(Index = 230, DisplayText = "Minimum Phase to Pass (deg)", MinValue = -180, MaxValue = 360)]
        public float MinimumPhase = 175f;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Maximum Phase to Pass (deg)", MinValue = -180, MaxValue = 360, MustBeGreaterThanIndex = 230)]
        public float MaximumPhase = 185f;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Analyzer Input Range")]
        public AudioAnalyzerInputRanges AnalyzerInputRange = new AudioAnalyzerInputRanges() { InputRange = 6 };

        public PhaseTest180A01() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        public override string GetTestDescription()
        {
            return "Measures the phase at a specified frequency and amplitude and compares to a chosen reference. Results must be within a given window to pass." +
                "Phase values using QA Output as reference reflect the values shown in the QA401 software. This test prompts the user after taking a reference measurement to engage " +
                "a phase invert switch then measures again to calculate the difference.";
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

            double[] phaseLeft = new double[2];
            double[] phaseRight = new double[2];

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

            bool passLeft = true, passRight = true;

            // measure phase twice with prompt in between
            for (int i = 0;i < 2; i++)
            {
                ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();
                TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

                if (LeftChannel)
                {
                    // calculate phase for Left Channel Out with Left Input Reference
                    phaseLeft[i] = ((IAudioAnalyzer)Tm.TestClass).ComputePhase(leftOut, leftIn, true, TestFrequency);

                    //Console.WriteLine("Left Phase: " + phaseLeft);



                    //if (phaseLeft < MinimumPhase || phaseLeft > MaximumPhase)
                    //    passLeft = false;
                }
                else
                {
                    tr.StringValue[0] = "SKIP";
                }

                if (RightChannel)
                {
                    // calculate phase for Right Channel Out with Right Input Reference
                    phaseRight[i] = ((IAudioAnalyzer)Tm.TestClass).ComputePhase(rightOut, rightIn, true, TestFrequency);

                    //Console.WriteLine("Right Phase: " + phaseRight);

                    //tr.Value[1] = phaseRight;
                    //tr.StringValue[1] = tr.Value[1].ToString("0.00") + " deg";

                    //if (phaseRight < MinimumPhase || phaseRight > MaximumPhase)
                    //    passRight = false;
                }
                else
                {
                    tr.StringValue[1] = "SKIP";
                }

                if (i == 0)
                {
                    ShowPrompt(PromptMessage, false, null);
                }
                    
            }

            if (LeftChannel)
            {
                tr.Value[0] = Math.Abs(phaseLeft[1] - phaseLeft[0]);
                tr.StringValue[0] = tr.Value[0].ToString("0.00") + " deg";

                if (tr.Value[0] < MinimumPhase || tr.Value[0] > MaximumPhase)
                    passLeft = false;
            }

            if (RightChannel)
            {
                tr.Value[1] = Math.Abs(phaseRight[1] - phaseRight[0]);
                tr.StringValue[1] = tr.Value[1].ToString("0.00") + " deg";

                if (tr.Value[1] < MinimumPhase || tr.Value[1] > MaximumPhase)
                    passRight = false;
            }


            if (LeftChannel && RightChannel)
                tr.Pass = passLeft && passRight;
            else if (LeftChannel)
                tr.Pass = passLeft;
            else if (RightChannel)
                tr.Pass = passRight;
        }

        void ShowPrompt(string instruction, bool showFailButton, Bitmap image)
        {
            using (var prompt = new DlgPrompt(instruction, showFailButton, image))
            {
                prompt.ShowDialog();
            }
        }
    }
}