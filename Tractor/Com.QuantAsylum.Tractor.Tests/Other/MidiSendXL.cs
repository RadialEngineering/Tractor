using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;
using System.IO.Ports;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Collections.Generic;

namespace Com.QuantAsylum.Tractor.Tests
{
    [Serializable]                                                           // Dropdown menus for inputs and outputs
    public class MidiSendXL : UiTestBase
    {

        [ObjectEditorAttribute(Index = 300, DisplayText = "Midi Data (Hex)", IsDataField = true)]      // editable data field text box
        public string MidiData = "Enter midi data here.";

        [ObjectEditorAttribute(Index = 320, DisplayText = "COM Port")]      // Declaring int creates an editable textbox
        public int COMPort = 7;

        [ObjectEditorAttribute(Index = 330, DisplayText = "Baud Rate", MaxLength = 128, IsSerial = 7)]
        public string BaudR = "9600";

        private SerialPort port;                // Declare serial port

        public MidiSendXL() : base()          // Constructor to initialize base class
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

            List<string> hexValues = ParseMidiData(MidiData);   // create a list of all valid hex values from text box


            port.WriteLine("midi");                 // Start midi transfer
            foreach (string hexValue in hexValues)  // Loop through all hex values, sending as strings
            {
                Console.WriteLine(hexValue);
                port.WriteLine(hexValue);
            }
            port.WriteLine("data_end");             // end midi transfer


            if (port.IsOpen)                    // Close port if it is open
            {
                port.Close();
            }
            
            tr.Pass = true;                     // Set test to pass

        }

        private static List<string> ParseMidiData(string hexString)
        {
            List<string> hexList = new List<string>();          // Create new list

            hexString = hexString.Replace(" ", string.Empty);   // Remove all whitespace

            for (int i = 0; i < hexString.Length; i += 2)       // Loop through the string in pairs of two characters at a time
            {
                                                                
                string hexByte = hexString.Substring(i, 2);     // Extract the hex byte pair


                if (IsHex(hexByte))                             // If it's a valid hex string, add it to the list
                {
                    hexList.Add(hexByte);
                }
                else
                {
                    Console.WriteLine($"Invalid hex byte: {hexByte}");
                }
            }

            return hexList;
        }

        public static bool IsHex(string hex)       // determines if a string is a valid hex
        {
            foreach (char c in hex)
            {
                if (!Uri.IsHexDigit(c))
                {
                    return false;
                }
            }
            return true;
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

            
            port.Dispose();     
            port = null;
            return false;       // clean up and return false
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
            return "This test is designed to enable the ATPI XL to send MIDI data over its dedicated MIDI output. " +
                   "Enter the MIDI data (in hex) into the text box and configure the port number and baud rate. " +
                   "Default baud rate is for the ATPI XL is 9600.";
        }
    }
}