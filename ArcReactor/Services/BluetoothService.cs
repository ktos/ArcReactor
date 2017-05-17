using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

// ########################################################################## //
namespace ArcReactor
{
    class BluetoothService
    {

        DeviceInformationCollection devices;
        public bool IsConnected { get; set; } = false;
        StreamSocket socket = null;
        DataReader _btReader = null;
        DataWriter _btWriter = null;

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