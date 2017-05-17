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

// ########################################################################## //
namespace ArcReactor
{
    internal class BluetoothService
    {
        private DeviceInformationCollection devices;
        public bool IsConnected { get; set; } = false;
        private StreamSocket socket = null;
        private DataReader _btReader = null;
        private DataWriter _btWriter = null;

        // ------------------------------------------------------------------ //
        public BluetoothService() { }

        // ------------------------------------------------------------------ //
        public async Task<DeviceInformationCollection> FindPairedDevicesAsync()
        {
            var aqsDevices = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
            this.devices = await DeviceInformation.FindAllAsync(aqsDevices);
            return this.devices;
        }

        // ------------------------------------------------------------------ //
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
            _btReader = new DataReader(socket.InputStream);
            _btWriter = new DataWriter(socket.OutputStream);
            _btReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            _btWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            return true;
        }

        // ------------------------------------------------------------------ //
        public async Task<bool> WriteAsync(string str)
        {
            if (!IsConnected) { return false; }
            try
            {
                var n = _btWriter.WriteString(str);
                await _btWriter.StoreAsync();
                return n > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // ------------------------------------------------------------------ //
        public async Task<String> ReadAsync()
        {
            if (!IsConnected) { return null; }
            var receivedStrings = "";
            try
            {
                uint size = await _btReader.LoadAsync(sizeof(uint));
                if (size < sizeof(uint))
                {
                    Disconnect();
                    return null;
                }

                while (_btReader.UnconsumedBufferLength > 0)
                {
                    uint bytesToRead = _btReader.ReadUInt32();
                    receivedStrings += _btReader.ReadString(bytesToRead) + "\n";
                }

                return receivedStrings;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // ------------------------------------------------------------------ //
        public async void Disconnect()
        {
            await socket.CancelIOAsync();
            socket.Dispose();
            socket = null;

            IsConnected = false;
        }

        // ------------------------------------------------------------------ //
    }
}