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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

// ########################################################################## //
namespace ArcReactor.Services
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

            _btReader.InputStreamOptions = InputStreamOptions.Partial;
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
        public async Task<String> ReadAsync(CancellationToken cancellationToken)
        {
            if (!IsConnected) { return null; }

            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 10;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            _btReader.InputStreamOptions = InputStreamOptions.Partial;

            // Create a task object to wait for data on the serialPort.InputStream

            if (cancellationToken.IsCancellationRequested)
                return null;

            loadAsyncTask = _btReader.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                try
                {
                    string recvdtxt = _btReader.ReadString(bytesRead);
                    return recvdtxt;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("ReadAsync: " + ex.Message);
                }

            }

            return null;

        }

        // ------------------------------------------------------------------ //
        public async void Disconnect()
        {
            try
            {
                await socket.CancelIOAsync();
                socket.Dispose();
                socket = null;
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                IsConnected = false;
            }
        }

        // ------------------------------------------------------------------ //
    }
}