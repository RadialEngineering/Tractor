using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;
using System.IO.Ports;
using System.Diagnostics.Eventing.Reader;
using System.IO;

namespace Com.QuantAsylum.Tractor.Tests
{
    [Serializable]                                                           // Dropdown menus for inputs and outputs
    public class SerialSendXL : UiTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Channel", MinValue = 0, MaxValue = 15, FormatString = "0.000")]
        public int ChannelIndex;
        
        [ObjectEditorAttribute(Index = 200, DisplayText = "Phantom Power", MaxLength = 128, IsSerial = 3)]
        public string PhantomPower = "OFF";

        [ObjectEditorAttribute(Index = 210, DisplayText = "Minimum Voltage to Pass (V)", MinValue = -100, MaxValue = 100, FormatString = "0.000")]
        public float MinimumPassVoltage = 44.0f;

        [ObjectEditorAttribute(Index = 220, DisplayText = "Maximum Voltage to Pass (V)", MinValue = -100, MaxValue = 100, FormatString = "0.000", MustBeGreaterThanIndex = 210)]
        public float MaximumPassVoltage = 52.0f;

        [ObjectEditorAttribute(Index = 230, DisplayText = "COM Port")]      // Declaring int creates an editable textbox
        public int COMPort = 7;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Baud Rate", MaxLength = 128, IsSerial = 7)]
        public string BaudR = "9600";

        private SerialPort port;                // Declare serial port

        public PhantomXL() : base()          // Constructor to initialize base class
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        private const int maxCOMRetries = 3;        // Max number of times program will attempt to open COM port before failing

        public override void DoTest(string title, out TestResult tr)
        {
            tr = new TestResult(2);             // Test results and pass / fail condition

                                                // Initalize serial port, throw an error and fail test if it can't open

            if (!InitializeSerialPort("COM" + COMPort, CheckBaudRate(), out port))     
            {
                tr.StringValue[1] = $"Unable to open serial port {"COM" + COMPort}.";
                tr.Pass = false;
                return;
            }

            ConfigureRelays(port);              // Write commands to arduino to configure relays

            if (port.IsOpen)                    // Close port if it is open
            {
                port.Close();
            }

            if (PhantomPower == "OFF")
            {
                tr.Pass = true;
                tr.Value[0] = 0.0f;
                tr.StringValue[0] = "Phantom OFF - Test Skipped";
                tr.StringValue[1] = "PASS";
            }
            // Check voltage level parameter (48V +/-4V)
            float voltage = ((IVoltMeter)Tm.TestClass).GetVoltage(ChannelIndex);

            if ((voltage > MinimumPassVoltage) && (voltage < MaximumPassVoltage))
                tr.Pass = true;

            tr.Value[0] = voltage;
            tr.StringValue[0] = voltage.ToString("0.000") + "A";
            tr.StringValue[1] = "SKIP";

        }

        private void ConfigureRelays(SerialPort port)
        {
            port.WriteLine(CheckPhantom());               // Phantom relay
        }

        private bool InitializeSerialPort(string com, int baud, out SerialPort port)
        {
            port = new SerialPort(com, baud)
            {
                NewLine = "\n"
            };
            int retries = 0;
            while (retries < maxCOMRetries)
            {
                try                         // Try to open port
                {
                    port.Open();
                    Console.WriteLine($"Successfully opened port {com} at {baud} baud.");
                    return true;            // Initialization succeeded
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"Port {com} is already in use. Error: {ex.Message}");
                    break;                  // No point in retrying if port is in use
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"I/O error while accessing port {com}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to open port {com} at {baud}: {ex.Message}");
                }

                retries++;
                if (retries < maxCOMRetries)
                {
                    Console.WriteLine($"Retrying {retries}/{maxCOMRetries}...");
                    System.Threading.Thread.Sleep(1000);            // Sleep before retrying
                }
            }

                                // If it still fails, clean up and return false
            port.Dispose();     // Cleanup port resources
            port = null;
            return false;       // Initialization failed after retries
        }

        string CheckPhantom()                     // returns corresponding serial command for selecting ATPI XL input
        {
            switch (PhantomPower)
            {
                case "OFF":
                    return "phantom_off";
                case "ON":
                    return "phantom_on";
            }
            return "0";
        }
        
        int CheckBaudRate()     // Return baud rate as integer depending on dropdown menu selection
        {
            switch (BaudR)
            {
                case "4800":
                    return 4800;
                case "9600 (ATPI/XL)":
                    return 9600;
                case "14400":
                    return 14400;
                case "28800":
                    return 28800;
                case "38400":
                    return 38400;
                case "57600":
                    return 57600;
                case "115200":
                    return 115200;
            }
            return 9600;
        }

        public override string GetTestDescription()     // Return test description message
        {
            return "Test designed for the ATPI XL that sets connection for phantom power relay via Arduino. " +
                   "Use the Input/Output boxes to select desired signal paths, input COM port and baud rate of Arduino. " +
                   "Default baud rate is for the ATPI XL is 9600.";
        }
    }
}