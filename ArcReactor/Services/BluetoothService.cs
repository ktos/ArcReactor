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
    internal delegate void StringReceivedEventHandler(string value);
    internal delegate void DisconnectedEventHandler();

    internal class BluetoothService
    {
        private DeviceInformationCollection devices;
        public bool IsConnected { get; set; } = false;
        private StreamSocket socket = null;
        private DataReader _btReader = null;
        private DataWriter _btWriter = null;

        public event StringReceivedEventHandler StringReceived;
        public event DisconnectedEventHandler Disconnected;

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

            DataReader btReader = new DataReader(socket.InputStream);
            ReadIncomingDataAsync(btReader);

            _btWriter = new DataWriter(socket.OutputStream);            
            
            return true;
        }

        // ------------------------------------------------------------------ //
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
                    //Disconnect("Remote device terminated connection - make sure only one instance of server is running on remote device");
                    return;
                }

                uint stringLength = reader.ReadByte();
                uint actualStringLength = await reader.LoadAsync(stringLength);
                if (actualStringLength != stringLength)
                {
                    // The underlying socket was closed before we were able to read the whole data
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

        // ------------------------------------------------------------------ //
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

        // ------------------------------------------------------------------ //
    }
}