using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;
using System.IO.Ports;
using System.Diagnostics.Eventing.Reader;
using System.IO;

namespace Com.QuantAsylum.Tractor.Tests
{
    [Serializable]
    public class SerialSendXL : UiTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Input Connector Select", MaxLength = 128, IsSerial = 3)]
        public string InConnectorSelect = "TRS1";

        [ObjectEditorAttribute(Index = 210, DisplayText = "Output Connector Select", MaxLength = 128, IsSerial = 4)]
        public string OutConnectorSelect = "TRS1";

        [ObjectEditorAttribute(Index = 220, DisplayText = "Input Mode", MaxLength = 128, IsSerial = 5)]
        public string InputModeSelect = "Balanced";

        [ObjectEditorAttribute(Index = 230, DisplayText = "Input Mode", MaxLength = 128, IsSerial = 6)]
        public string OutputModeSelect = "Balanced";

        [ObjectEditorAttribute(Index = 240, DisplayText = "COM Port")]
        public int COMPort = 7;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Baud Rate", MaxLength = 128, IsSerial = 7)]
        public string BaudR = "9600";

        private SerialPort port;

        private bool feedback = false;
        public SerialSendXL() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            tr = new TestResult(2);
            string COM = "COM" + COMPort;
            int baud = CheckBaudRate();
            try
            {
                port = new SerialPort(COM, baud);
                port.NewLine = "\n";
                port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                System.Threading.Thread.Sleep(20);
                port.Open();
            }
            catch (Exception ex)
            {
                port.Close();
                port = new SerialPort(COM, baud);
                port.NewLine = "\n";
                port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                port.Open();
            }
            System.Threading.Thread.Sleep(500);



            port.WriteLine("0");                        // Reset all relays
            port.WriteLine(CheckInput());               // Enable input relay
            port.WriteLine(CheckOutput());
            port.WriteLine(CheckInputMode());           // Enable input mode relay
            port.WriteLine(CheckOutputMode());          // Enable output mode relay

            try
            {
                int input = int.Parse(CheckInput());                // This try catch block acts as a check if the user tries to set the ATPI XL
                int output = int.Parse(CheckOutput());              // input or output to a channel that is not installed
                Console.WriteLine(input);
                Console.WriteLine(output);
                port.WriteLine("getconfig");
                string current_mode = port.ReadLine();
                current_mode = current_mode.Trim();
                Console.WriteLine(current_mode);
                if (current_mode == "2" || current_mode == "0")
                {
                    if (input > 24 && input < 33)
                    {
                        Console.WriteLine("you tried to use an input on exp 1 and its not installed");
                        port.Close();
                        tr.Pass = false;
                        return;
                    }
                    else if (output > 24 && output < 33)
                    {
                        Console.WriteLine("you tried to use an output on exp 1 and its not installed");
                        port.Close();
                        tr.Pass = false;
                        return;
                    }
                }
                else if (current_mode == "1" || current_mode == "0")
                {
                    if (input > 32 && input < 41)
                    {
                        Console.WriteLine("you tried to use an input on exp 2 and its not installed");
                        port.Close();
                        tr.Pass = false;
                        return;
                    }
                    else if (output > 32 && output < 41)
                    {
                        Console.WriteLine("you tried to use an output on exp 2 and its not installed");
                        port.Close();
                        tr.Pass = false;
                        return;
                    }
                }
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Read timed out.");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }


            System.Threading.Thread.Sleep(20);
            port.Close();


            tr.Pass = true;

        }


        // returns corresponding serial output char for selected input to send to ATPI
        string CheckInput()
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
        // returns corresponding serial output char for selected output to send to ATPI
        string CheckOutput()
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

        string CheckInputMode()
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

        string CheckOutputMode()
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

        int CheckBaudRate()
        {
            switch (OutputModeSelect)
            {
                case "4800":
                    return 4800;
                case "9600 ATPI/XL":
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
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            sp.ReadTimeout = 2000;
            //sp.NewLine = "\n";
            //string indata = sp.ReadLine();
            string indata = sp.ReadExisting();
            Console.WriteLine("Data Received:");
            Console.Write(indata);
            //feedback = true;
        }
        public override string GetTestDescription()
        {
            return "Test designed for the ATPI XL that sets connection relays via Arduino. " +
                "Use the Input/Output boxes to select desired signal paths, input COM port and baud rate of Arduino." +
                " Default baud rate is for the ATPI XL is 9600.";
        }
    }
}