#region License

/*
 * ArcReactor
 *
 * Copyright (C) Marcin Badurowicz <m at badurowicz dot net> 2017
 *
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files
 * (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
 * BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
 * ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#endregion License

using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace ArcReactor.Services
{
    /// <summary>
    /// Handles the event when new string data was sent by Bluetooth device
    /// </summary>
    /// <param name="value">Value received from the device</param>
    public delegate void StringReceivedEventHandler(string value);

    /// <summary>
    /// Handles the situation when the device was disconnected
    /// </summary>
    public delegate void DisconnectedEventHandler();

    /// <summary>
    /// Class for communication with Serial Bluetooth Devices (SPP profile)
    /// </summary>
    public class BluetoothService
    {
        private DeviceInformationCollection devices;
        public bool IsConnected { get; set; } = false;
        private StreamSocket socket = null;
        private DataWriter _btWriter = null;

        /// <summary>
        /// Connected device sent new string
        /// </summary>
        public event StringReceivedEventHandler StringReceived;

        /// <summary>
        /// The device was disconnected
        /// </summary>
        public event DisconnectedEventHandler Disconnected;

        /// <summary>
        /// Initializes a new instance of Bluetooth Service
        /// </summary>
        public BluetoothService()
        {

        }

        /// <summary>
        /// Searches for every paired device supporting the SPP profile and returns the collection of information about such devices
        /// </summary>
        /// <returns>A collection of device information for every device supporting SPP</returns>
        public async Task<DeviceInformationCollection> FindPairedDevicesAsync()
        {
            var aqsDevices = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
            this.devices = await DeviceInformation.FindAllAsync(aqsDevices);
            return this.devices;
        }

        /// <summary>
        /// Connects with a desired SPP device
        /// </summary>
        /// <param name="device">A device to connect with</param>
        /// <returns>Returns if operation was successful</returns>
        public async Task<bool> ConnectAsync(DeviceInformation device)
        {
            var service = await RfcommDeviceService.FromIdAsync(device.Id);
            if (service == null)
            {
                return false;
            }

            socket = new StreamSocket();
            try
            {
                await socket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName, SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);
            }
            catch (Exception ex)
            {
                return false;
            }

            IsConnected = true;

            DataReader btReader = new DataReader(socket.InputStream);
            ReadIncomingDataAsync(btReader);

            _btWriter = new DataWriter(socket.OutputStream);

            return true;
        }

        /// <summary>
        /// Sends a string value ended with \n to the device. The \n is added automatically.
        /// </summary>
        /// <param name="str">Value to be sent to the device</param>
        /// <returns>Returns if operation was successful</returns>
        public async Task<bool> WriteAsync(string str)
        {
            if (!IsConnected) { return false; }
            try
            {
                var n = _btWriter.WriteString(str);
                _btWriter.WriteByte(10);
                await _btWriter.StoreAsync();
                return n > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Sends a stream of bytes to the device, ending with \n. The \n is added automatically.
        /// </summary>
        /// <param name="data">Data to be sent to the device</param>
        /// <returns>Returns if the operation was successful</returns>
        public async Task<bool> WriteBytesAsync(byte[] data)
        {
            if (!IsConnected) { return false; }
            try
            {
                _btWriter.WriteBytes(data);
                _btWriter.WriteByte(10);
                await _btWriter.StoreAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async void ReadIncomingDataAsync(DataReader reader)
        {
            try
            {
                uint size = await reader.LoadAsync(sizeof(byte));
                if (size < sizeof(byte))
                {
                    // remote device terminated conetion
                    return;
                }

                uint stringLength = reader.ReadByte();
                uint actualStringLength = await reader.LoadAsync(stringLength);
                if (actualStringLength != stringLength)
                {
                    // the underlying socket was closed before we were
                    // able to read the whole data
                    return;
                }
                else
                {
                    var result = reader.ReadString(stringLength);
                    StringReceived?.Invoke(result);
                }

                ReadIncomingDataAsync(reader);
            }
            catch (Exception ex)
            {
                lock (this)
                {
                    if (socket == null)
                    {
                        if ((uint)ex.HResult == 0x80072745)
                        {
                            Disconnect();
                        }
                        else if ((uint)ex.HResult == 0x800703E3)
                        {
                            // application exit requested
                        }
                    }
                    else
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Disconnects from the Bluetooth device
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (_btWriter != null)
                {
                    _btWriter.DetachStream();
                    _btWriter = null;
                }

                if (socket != null)
                {
                    socket.Dispose();
                    socket = null;
                }
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                IsConnected = false;
                Disconnected?.Invoke();
            }
        }
    }
}