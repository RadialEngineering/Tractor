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
        [ObjectEditorAttribute(Index = 200, DisplayText = "Input Connector Select", MaxLength = 128, IsSerial = 3)]
        public string InConnectorSelect = "TRS1";

        [ObjectEditorAttribute(Index = 210, DisplayText = "Output Connector Select", MaxLength = 128, IsSerial = 4)]
        public string OutConnectorSelect = "TRS1";

        [ObjectEditorAttribute(Index = 220, DisplayText = "Input Mode", MaxLength = 128, IsSerial = 5)]
        public string InputModeSelect = "Balanced";

        [ObjectEditorAttribute(Index = 230, DisplayText = "Output Mode", MaxLength = 128, IsSerial = 6)]
        public string OutputModeSelect = "Balanced";

        [ObjectEditorAttribute(Index = 240, DisplayText = "Phantom Power Enabled", MaxLength = 128)]
        public bool PhantomPowerEnabled = false;

        [ObjectEditorAttribute(Index = 250, DisplayText = "COM Port")]      // Declaring int creates an editable textbox
        public int COMPort = 7;

        [ObjectEditorAttribute(Index = 260, DisplayText = "Baud Rate", MaxLength = 128, IsSerial = 7)]
        public string BaudR = "9600";

        private SerialPort port;                // Declare serial port

        public SerialSendXL() : base()          // Constructor to initialize base class
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
            
            if (!CheckBoardsInstalled(GetConfig(port), ref port, ref tr))  // Check if there are missing boards
            {
                tr.Pass = false;
                return;                         // Exit early if board not installed
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
            port.WriteLine("0");                        // Reset all relays
            port.WriteLine(CheckInput());               // Enable input relays
            port.WriteLine(CheckOutput());              // Enable output relays
            port.WriteLine(CheckInputMode());           // Enable input mode relays
            port.WriteLine(CheckOutputMode());          // Enable output mode relays
            port.WriteLine(CheckPhantom());             // Phantom relay
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

        // Check if inputs or outputs are routed to expansion boards not currently installed, throw an error if they do
        private bool CheckBoardsInstalled(string currentMode, ref SerialPort port, ref TestResult tr)
        {
            int input = int.Parse(CheckInput());
            int output = int.Parse(CheckOutput());

            Console.WriteLine("Current Mode: "+currentMode+" Input: "+input+" Output"+output);

            if (currentMode == "2" || currentMode == "0")  // Check for EXP 1
            {
                if ((input > 24 && input < 33) || (output > 24 && output < 33))
                {
                    port.Close();
                    tr.StringValue[0] = "MISSING EXP 1";
                    tr.Pass = false;
                    return false; // Validation failed
                }
            }
            if (currentMode == "1" || currentMode == "0")
            {
                if ((input > 32 && input < 41) || (output > 32 && output < 41))  // Check for EXP 2
                {
                    port.Close();
                    tr.StringValue[1] = "MISSING EXP 2";
                    tr.Pass = false;
                    return false; // Validation failed
                }
            }

            return true; // Validation passed
        }

        private string GetConfig(SerialPort port)
        {
            int retries = 0;

            while (retries < maxBoardCheckRetries)
            {
                try
                {
                    if (!port.IsOpen)               // Open port if it is closed
                    {
                        port.Open();
                    }

                    port.WriteLine("getconfig");    // Write the "getconfig" command to the serial port

                    port.ReadTimeout = 500;         // Timeout in milliseconds
                                     
                    string response = port.ReadLine().Trim();   // Read the response
                    Console.WriteLine(response);
                    if (response == "0" ||  response == "1" || response == "2" || response == "3")
                    {
                        return response;                // Return the successful response
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("Read timed out.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"Access failed: {ex.Message}");
                    break;  // No point retrying if the issue is access-related
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"I/O error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                }

                retries++;
                if (retries < maxBoardCheckRetries)
                {
                    Console.WriteLine($"Retrying {retries}/{maxBoardCheckRetries}...");
                }
            }

            // If it fails after retries, return board config corresponding to both boards being installed
            Console.WriteLine("Failed to read response after retries.");
            return "3";
        }

        string CheckInput()                     // returns corresponding serial command for selecting ATPI XL input
        {
            switch (InConnectorSelect)
            {
                case "TRS1":
                    return "21";
                case "TRS2":
                    return "22";
                case "XLR1":
                    return "23";
                case "XLR2":
                    return "24";
                case "CH1A":
                    return "13";
                case "CH2A":
                    return "14";
                case "CH3A":
                    return "15";
                case "CH4A":
                    return "16";
                case "CH1B":
                    return "17";
                case "CH2B":
                    return "18";
                case "CH3B":
                    return "19";
                case "CH4B":
                    return "20";
                case "EXP1 CH1":
                    return "29";
                case "EXP1 CH2":
                    return "30";
                case "EXP1 CH3":
                    return "31";
                case "EXP1 CH4":
                    return "32";
                case "EXP2 CH1":
                    return "37";
                case "EXP2 CH2":
                    return "38";
                case "EXP2 CH3":
                    return "39";
                case "EXP2 CH4":
                    return "40";
            }
            return "0";
        }
        
        string CheckOutput()                // returns corresponding serial command for selecting ATPI XL output
        {
            switch (OutConnectorSelect)
            {
                case "TRS1":
                    return "1";
                case "TRS2":
                    return "2";
                case "XLR1":
                    return "3";
                case "XLR2":
                    return "4";
                case "CH1A":
                    return "5";
                case "CH2A":
                    return "6";
                case "CH3A":
                    return "7";
                case "CH4A":
                    return "8";
                case "CH1B":
                    return "9";
                case "CH2B":
                    return "10";
                case "CH3B":
                    return "11";
                case "CH4B":
                    return "12";
                case "EXP1 CH1":
                    return "25";
                case "EXP1 CH2":
                    return "26";
                case "EXP1 CH3":
                    return "27";
                case "EXP1 CH4":
                    return "28";
                case "EXP2 CH1":
                    return "33";
                case "EXP2 CH2":
                    return "34";
                case "EXP2 CH3":
                    return "35";
                case "EXP2 CH4":
                    return "36";
            }
            return "0";
        }

        string CheckInputMode()        // Return input mode command as string depending on dropdown menu selection
        {
            switch (InputModeSelect)
            {
                case "Balanced":
                    return "ibal";
                case "Unbalanced":
                    return "iunbal";
                case "Stereo":
                    return "istereo";
                case "Stereo Option":
                    return "istereo2";
            }
            return "0";
        }

        string CheckOutputMode()        // Return output mode command as string depending on dropdown menu selection
        {
            switch (OutputModeSelect)
            {
                case "Balanced":
                    return "obal";
                case "Unbalanced":
                    return "ounbal";
                case "Stereo":
                    return "ostereo";
                case "Stereo Option":
                    return "ostereo2";
            }
            return "0";
        }

        string CheckPhantom()
        {
            return PhantomPowerEnabled ? "phantom_on" : "phantom_off";
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