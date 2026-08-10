using Com.QuantAsylum;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tractor.Com.QuantAsylum.Tractor.Tests.THDs
{

    [Serializable]
    public class ThdNA01 : AudioTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Test Frequency (Hz)", MinValue = 10, MaxValue = 20000)]
        public float Freq = 1000;

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue = -100, MaxValue = 6)]
        public float OutputLevel = -30;

        [ObjectEditorAttribute(Index = 215, DisplayText = "Start Frequency (Hz)", MinValue = 10, MaxValue = 20000)]
        public float StartFreq = 20;

        [ObjectEditorAttribute(Index = 220, DisplayText = "Stop Frequency (Hz)", MinValue = 10, MaxValue = 20000, MustBeGreaterThanIndex = 215)]
        public float StopFreq = 20000;

        [ObjectEditorAttribute(Index = 230, DisplayText = "Minimum THD+N to Pass (dB)", MinValue = -130, MaxValue = 100)]
        public float MinimumOkThdN = -110;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Maximum THD+N to Pass (dB)", MinValue = -130, MaxValue = 100, MustBeGreaterThanIndex = 230)]
        public float MaximumOkThdN = -100;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Analyzer Input Range")]
        public AudioAnalyzerInputRanges AnalyzerInputRange = new AudioAnalyzerInputRanges() { InputRange = 6 };

        [ObjectEditorAttribute(Index = 260, DisplayText = "Settings Filename", MaxLength = 512)]
        public string settingsFilename = "";

        public ThdNA01() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Distortion;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            if (string.IsNullOrEmpty(settingsFilename))
            {
                Tm.SetToDefaults();
            }
            else
            {
                if (!System.IO.File.Exists(settingsFilename))
                {
                    tr.Pass = false;
                    Console.WriteLine($"Settings file '{settingsFilename}' does not exist.");
                    return;
                }

                // copy the settings file to the QA401 path
                string qa401Path = Constants.QA401Path;
                string settingsFileName = Path.GetFileName(settingsFilename);
                string destFileName = Path.Combine(qa401Path, settingsFileName);

                try
                {
                    // Copy the settings file to the QA401 path
                    File.Copy(settingsFilename, destFileName, true);
                    Console.WriteLine($"Settings file '{settingsFilename}' copied to '{destFileName}'.");
                }
                catch (Exception ex)
                {
                    tr.Pass = false;
                    Console.WriteLine($"Error copying settings file: {ex.Message}");
                    tr.OperatorMessage = $"Error copying settings file: {ex.Message}";
                    return;
                }

                Tm.SetToDefaults(settingsFilename);
            }

            SetupBaseTests();

            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange.InputRange);

            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, OutputLevel, Freq);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, OutputLevel, Freq);
            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            ((IAudioAnalyzer)Tm.TestClass).ComputeThdnPct(Freq, StartFreq, StopFreq, out tr.Value[0], out tr.Value[1]);

            // Convert to db
            tr.Value[0] = 20 * (float)Math.Log10(tr.Value[0] / 100);
            tr.Value[1] = 20 * (float)Math.Log10(tr.Value[1] / 100);

            bool passLeft = true, passRight = true;

            if (LeftChannel)
            {
                tr.StringValue[0] = tr.Value[0].ToString("0.0") + " dB";
                if ((tr.Value[0] < MinimumOkThdN) || (tr.Value[0] > MaximumOkThdN))
                    passLeft = false;
            }
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
            {
                tr.StringValue[1] = tr.Value[1].ToString("0.0") + " dB";
                if ((tr.Value[1] < MinimumOkThdN) || (tr.Value[1] > MaximumOkThdN))
                    passRight = false;
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
            return string.Format("{0:N1}...{1:N1} dB", MinimumOkThdN, MaximumOkThdN);
        }

        public override string GetTestDescription()
        {
            return "Measures THD+N at a given frequency and amplitude over the specified bandwidth. Results must be within a given window to pass.";
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
