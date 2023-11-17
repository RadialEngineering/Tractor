using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;
using System.IO.Ports;

namespace Com.QuantAsylum.Tractor.Tests
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SerialSend : UiTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Input Connector Select", MaxLength = 128, IsSerial = 1)]
        public string InConnectorSelect = "";

        [ObjectEditorAttribute(Index = 210, DisplayText = "Output Connector Select", MaxLength = 128, IsSerial = 2)]
        public string OutConnectorSelect = "";

        [ObjectEditorAttribute(Index = 220, DisplayText = "COM Port")]
        public int COMPort = 4;

        [ObjectEditorAttribute(Index = 230, DisplayText = "Baud Rate", ValidInts = new int[] { 4800, 9600, 14400, 38400, 28800, 57600, 115200 })]
        public int BaudR = 9600;

        private SerialPort port;

        private bool feedback = false;
        public SerialSend() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            tr = new TestResult(2);
            string COM = "COM" + COMPort;

            try
            {
                port = new SerialPort(COM, 9600);
                port.NewLine = "\n";
                port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                System.Threading.Thread.Sleep(20);
                port.Open();
            }
            catch (Exception ex)
            {
                port.Close();
                port = new SerialPort(COM, 9600);
                port.NewLine = "\n";
                port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                port.Open();
            }
            //System.Threading.Thread.Sleep(500);

            port.Write(BitConverter.GetBytes(CheckInput()), 0, 1);

            //while (!feedback) { }
            //feedback = false;

            port.Write(BitConverter.GetBytes(CheckOutput()), 0, 1);
            //while (!feedback){}
            //feedback = false;

            System.Threading.Thread.Sleep(20);
            port.Close();
            tr.Pass = true;

        }
        // returns corresponding serial output char for selected input to send to ATPI
        char CheckInput()
        {
            switch (InConnectorSelect)
            {
                case "TRS1":
                    return '1';
                case "TRS2":
                    return '2';
                case "RCA1":
                    return '3';
                case "RCA2":
                    return '4';
                case "3.5mm":
                    return '5';
                case "XLR":
                    return '6';
            }
            return '0';
        }
        // returns corresponding serial output char for selected output to send to ATPI
        char CheckOutput()
        {
            switch (OutConnectorSelect)
            {
                case "TRS":
                    return '1';
                case "RCA":
                    return '2';
                case "3.5mm":
                    return '3';
                case "XLR1":
                    return '4';
                case "XLR2":
                    return '5';
            }
            return '0';
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
            return "Sets up connector selection relays via Arduino. Note this test is only applicable when using the custom ATPI." +
                "Use the Input/Output boxes to select desired signal paths. Input whichever COM port the Arduino is connected to and," +
                " the baud rate used (COM4 and 9600 by default)";
        }
    }
}