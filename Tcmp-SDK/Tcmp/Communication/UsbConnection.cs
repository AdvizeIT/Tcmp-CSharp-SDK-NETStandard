using System.Collections.Generic;
using System.IO.Ports;
using System.Diagnostics;
using TapTrack.Tcmp.Communication.Exceptions;
using System;

namespace TapTrack.Tcmp.Communication
{
    internal class UsbConnection : Connection
    {
        private SerialPort port;

        public UsbConnection()
        {
            port = new SerialPort();

            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            port.BaudRate = 115200;
            port.RtsEnable = false;
            port.WriteTimeout = 2000; // prevent port.Write blocking forever on non-Tappy ports
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            OnDataReceived(e);
        }

        public override bool Connect(string portName)
        {
            try
            {
                Disconnect();
				port.PortName = portName;
				port.Open();		
				return true;
            }
            catch
            {
				port.PortName= " ";            
				return false;
            }
        }

        public override void Disconnect()
        {
            if (port?.IsOpen == true)
                port.Close();
        }

        public override bool IsOpen()
        {
            return port?.IsOpen == true;
        }

        public override string[] GetAvailableDevices()
        {
            return SerialPort.GetPortNames();
        }

		public override string[] GetAvailableDevices(int timeout)
		{
			return SerialPort.GetPortNames();
		}

		public override string[] GetAvailableDevices(int timeout, bool scanForBlueGiga)
		{
			return SerialPort.GetPortNames();
		}

		public override void Send(byte[] data)
        {
            Debug.Write("   sending: ");
            Debug.Write(BitConverter.ToString(data));
            Debug.WriteLine("");

            try
            {
                port.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                throw new HardwareException("Error writing to serial port for TappyUSB", e);
            }
        }

        public override int Read(List<byte> buffer)
        {
            byte temp;
            int count = 0;

            Debug.Write("   received: ");
            try
            {
                while (port.BytesToRead > 0)
                {
                    temp = (byte)port.ReadByte();
                    buffer.Add(temp);
                    count++;
                    Debug.Write(string.Format("{0:X}", temp).PadLeft(2, '0') + " ");
                }
            }
            catch (Exception e)
            {
                throw new HardwareException("Error reading serial port for TappyUSB", e);
            }

            Debug.WriteLine("");

            return count;
        }

		public override bool getConnectionStatus()
		{
            return IsOpen();			
		}

		public override bool getBlueGigaStatus()
		{
			throw new NotImplementedException();

		}

		public override void setDisconnectCallback(Bluegiga.BLE.Events.Connection.DisconnectedEventHandler disconnectCallback)
		{
			throw new NotImplementedException();
		}

		public override void DisconnectBlueGiga()
		{
			throw new NotImplementedException();
		}

		public override void Flush()
        {
            if (port?.IsOpen != true) return;
            try
            {
                port.DiscardInBuffer();
                port.DiscardOutBuffer();
            }
            catch
            {
                // Ignored.
            }
        }

        public override void Dispose()
        {
            port?.Dispose();
        }
    }
}
