using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;
using System.IO.Ports;
using System.Threading;
using NAudio.Midi;


namespace Com.QuantAsylum.Tractor.Tests
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class KL8MIDI : MIDITestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "MIDI Input Device Name", MaxLength = 512)]
        public string inDevName = "";

        [ObjectEditorAttribute(Index = 200, DisplayText = "MIDI Output Device Name", MaxLength = 512)]
        public string outDevName = "";

        private readonly ManualResetEvent _echoReceivedEvent = new ManualResetEvent(false);
        private int[] _receivedData = new int[2];

        const int noteNumber = 60; // Middle C
        const int velocity = 100; // Velocity value (0-127)

        public KL8MIDI() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            tr = new TestResult(2);
            bool pass = false;

            // Find output device
            int outDeviceIndex = -1;
            for (int i = 0; i < MidiOut.NumberOfDevices; i++)
            {
                var info = MidiOut.DeviceInfo(i);
                if (info.ProductName != null && info.ProductName.Contains(outDevName))
                {
                    outDeviceIndex = i;
                    break;
                }
            }
           
            if (outDeviceIndex == -1)
            {
                tr.StringValue[0] = "Requested MIDI output device not found."; 
                tr.Pass = false;
                return;
            }

            // Find input deviceq
            int inDeviceIndex = -1;
            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                var info = MidiIn.DeviceInfo(i);
                if (info.ProductName != null && info.ProductName.Contains(inDevName))
                {
                    inDeviceIndex = i;
                    break;
                }
            }

            if (inDeviceIndex == -1)
            {
                tr.StringValue[0] = "Requested MIDI input device not found.";
                tr.Pass = false;
                return;
            }

            // Create a new MIDI input device
            using (var midiOut = new MidiOut(outDeviceIndex))
            using (var midiIn = new MidiIn(inDeviceIndex))
            {
                midiIn.MessageReceived += (sender, e) =>
                {
                    // Handle incoming MIDI messages here
                    var message = e.MidiEvent;
                    if (message.CommandCode == MidiCommandCode.NoteOn)
                    {
                        // Process Note On message
                        var noteOnMessage = (NoteOnEvent)message;
                        _receivedData[0] = noteOnMessage.NoteNumber;
                        _receivedData[1] = noteOnMessage.Velocity;
                        _echoReceivedEvent.Set();
                    }
                };
                midiIn.Start();

                // Send a MIDI message to the output device
                var noteOn = new NoteOnEvent(0, 1, noteNumber, velocity, 0);
                midiOut.Send(noteOn.GetAsShortMessage());
                tr.StringValue[0] = "MIDI Sent";

                // Wait for the echo received event
                if (!_echoReceivedEvent.WaitOne(5000))
                {
                    tr.StringValue[1] = "No response from device";
                    tr.Pass = false;
                    return;
                }
                // Check if the received data matches the expected data
                if ((_receivedData[0] == noteNumber) && (_receivedData[1] == velocity))
                {
                    //Console.WriteLine("Echo received: " + _receivedData);
                    _echoReceivedEvent.Reset();
                    tr.StringValue[1] = "MIDI Received";
                    pass = true;
                }

            }
            

            tr.Pass = pass;

        }
   
        public override string GetTestDescription()
        {
            return "Opens a user specified MIDI input and output device, sends test data from MIDI out and verifies the same data at the MIDI input device.)";
        }
    }
}