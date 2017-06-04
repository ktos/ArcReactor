using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcReactor.Models;
using Windows.Devices.Enumeration;
using System.Threading;
using System.Runtime.InteropServices;

namespace ArcReactor.Services
{
    delegate void BatteryLevelChangedHandler(float value);    

    class ArcReactorService
    {
        private BluetoothService bs;

        public event BatteryLevelChangedHandler BatteryLevelChanged;
        public event DisconnectedEventHandler Disconnected;

        public ArcReactorService()
        {
            bs = new BluetoothService();
            bs.StringReceived += Bs_OnStringReceived;
            bs.Disconnected += Bs_Disconnected;
        }

        private void Bs_Disconnected()
        {
            IsConnected = bs.IsConnected;
            Disconnected?.Invoke();
        }

        private void Bs_OnStringReceived(string value)
        {
            try
            {
                BatteryLevelChanged?.Invoke(float.Parse(value));
            }
            catch (FormatException)
            {

            }
        }

        public bool IsConnected { get; private set; }

        public async Task<bool> ConnectAsync(DeviceInformation selectedDevice)
        {
            if (bs.IsConnected)
                return false;

            var result = await bs.ConnectAsync(selectedDevice);
            IsConnected = bs.IsConnected;

            return result;
        }

        public void DisconnectAsync()
        {
            if (bs.IsConnected)
            {
                bs.Disconnect();
                IsConnected = false;
            }
        }

        public async void SetPulseSequence()
        {
            await bs.WriteAsync("pulse");
        }

        public async void SetStartupSequence()
        {
            await bs.WriteAsync("startup");
        }

        public async void SetAllBlack()
        {
            await bs.WriteAsync("black");
        }

        public async void SetLedsBatchAsync(IList<LedColor> leds)
        {
            byte[] sb = new byte[leds.Count * 3 + 1];
            sb[0] = (byte)'i';

            for (int i = 0; i < leds.Count; i++)
            {
                sb[i * 3 + 1] = (byte)leds[i].R;
                sb[i * 3 + 2] = (byte)leds[i].G;
                sb[i * 3 + 3] = (byte)leds[i].B;
            }

            await bs.WriteBytesAsync(sb);
        }

        public async void SetSingleLedAsync(LedColor led)
        {
            await bs.WriteBytesAsync(led.ToDeviceCommand());
        }

        public async Task<DeviceInformationCollection> FindPairedDevicesAsync()
        {
            var pairedDevices = await bs.FindPairedDevicesAsync();
            return pairedDevices;
        }

        public async void RequestBatteryLevel()
        {
            await bs.WriteAsync("batt");            
        }
    }
}
