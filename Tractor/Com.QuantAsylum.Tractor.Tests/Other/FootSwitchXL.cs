using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;
using System.IO.Ports;
using System.Diagnostics.Eventing.Reader;
using System.IO;

namespace Com.QuantAsylum.Tractor.Tests
{
    [Serializable]                                                           // Dropdown menus for inputs and outputs
    public class FootSwitchXL : UiTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "FootSwitch - A or B", MaxLength = 128, IsSerial = 8)]
        public string fs_a_b = "A";

        [ObjectEditorAttribute(Index = 210, DisplayText = "Footswitch enabled", MaxLength = 128)]
        public bool fs = false;

        [ObjectEditorAttribute(Index = 220, DisplayText = "Mute Footswitch enabled", MaxLength = 128)]
        public bool fs_mute = false;

        [ObjectEditorAttribute(Index = 230, DisplayText = "COM Port")]      // Declaring int creates an editable textbox
        public int COMPort = 7;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Baud Rate", MaxLength = 128, IsSerial = 7)]
        public string BaudR = "9600";

        private SerialPort port;                // Declare serial port

        public FootSwitchXL() : base()          // Constructor to initialize base class
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        private const int maxCOMRetries = 3;        // Max number of times program will attempt to open COM port before failing
        private const int maxBoardCheckRetries = 5; // Max number of times program will attempt to check for missing boards,
                                                    // sometimes arduino sends leftover junk instead of the board config 

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

            tr.Pass = true;                     // Set test to pass

        }

        private void ConfigureRelays(SerialPort port)
        {
            port.WriteLine(Check_fs_a_b());         // Enable relay for fs_a or fs_b
            port.WriteLine(Check_fs());             // Enable relay for fs
            port.WriteLine(Check_fs_mute());        // Enable relay for fs_mute
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

        string Check_fs_a_b()                     // returns corresponding serial command for selecting ATPI XL output
        {
            switch (fs_a_b)
            {
                case "A":
                    return "fs_a";
                case "B":
                    return "fs_b";
            }
            return "0";
        }
        
        string Check_fs()                // returns corresponding serial command for selecting ATPI XL output
        {
            return fs ? "fs_on" : "fs_off";
        }

        string Check_fs_mute()        // Return input mode command as string depending on dropdown menu selection
        {
            return fs_mute ? "fs_mute_on" : "fs_mute_off";
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
            return "Test designed for the ATPI XL that sets connection relays via Arduino. " +
                   "Use the Input/Output boxes to select desired signal paths, input COM port and baud rate of Arduino. " +
                   "Default baud rate is for the ATPI XL is 9600.";
        }
    }
}