using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;
using System.IO.Ports;
using System.Threading;

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
        public int COMPort = 6;

        [ObjectEditorAttribute(Index = 230, DisplayText = "Baud Rate", ValidInts = new int[] { 4800, 9600, 14400, 38400, 28800, 57600, 115200 })]
        public int BaudR = 9600;

        private ManualResetEvent _echoReceivedEvent = new ManualResetEvent(false);
        private string _receivedData = string.Empty;

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
                using (var port = new SerialPort(COM, BaudR))
                {
                    port.NewLine = "\n";
                    port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    port.Open();

                    // Send input selection command and wait for echo
                    SendCommandAndWaitForEcho(port, CheckInput());

                    // Send output selection command and wait for echo
                    SendCommandAndWaitForEcho(port, CheckOutput());

                    tr.Pass = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                tr.Pass = false;
            }



        }
        // returns corresponding serial output char for selected input to send to ATPI
        byte CheckInput()
        {
            switch (InConnectorSelect)
            {
                case "TRS1":
                    return 0x01;
                case "TRS2":
                    return 0x02;
                case "RCA1":
                    return 0x03;
                case "RCA2":
                    return 0x04;
                case "3.5mm":
                    return 0x05;
                case "XLR":
                    return 0x06;
            }
            return 0x00;
        }
        // returns corresponding serial output char for selected output to send to ATPI
        byte CheckOutput()
        {
            switch (OutConnectorSelect)
            {
                case "TRS":
                    return 0x01;
                case "RCA":
                    return 0x02;
                case "3.5mm":
                    return 0x03;
                case "XLR1":
                    return 0x04;
                case "XLR2":
                    return 0x05;
            }
            return 0x00;
        }

        private void SendCommandAndWaitForEcho(SerialPort port, byte command)
        {
            _echoReceivedEvent.Reset();
            _receivedData = string.Empty;

            port.Write(new byte[] { command }, 0, 1);

            if (!_echoReceivedEvent.WaitOne(2000)) // Wait for up to 2 seconds for the echo
            {
                throw new TimeoutException("No echo received from the slave device.");
            }

            if (_receivedData != command.ToString())
            {
                throw new InvalidOperationException("Received echo does not match the sent command.");
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            try
            {
                sp.ReadTimeout = 2000;
                _receivedData = sp.ReadExisting();
                _echoReceivedEvent.Set();
                // write received data to console
                Console.WriteLine($"Received data: {_receivedData}");
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Read timeout occurred.");
            }
        }
        public override string GetTestDescription()
        {
            return "Sets up connector selection relays via Arduino. Note this test is only applicable when using the custom ATPI." +
                "Use the Input/Output boxes to select desired signal paths. Input whichever COM port the Arduino is connected to and," +
                " the baud rate used (COM4 and 9600 by default)";
        }
    }
}