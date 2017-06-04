using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcReactor.Models;
using Windows.Devices.Enumeration;
using System.Threading;

namespace ArcReactor.Services
{
    class ArcReactorService
    {
        private BluetoothService bs;

        public ArcReactorService()
        {
            bs = new BluetoothService();
        }

        public bool IsConnected { get; internal set; }

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
            throw new NotImplementedException();
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

        public async Task<float> GetBatteryLevelAsync()
        {
            await bs.WriteAsync("batt");
            await Task.Delay(2000);
            var cts = new CancellationTokenSource(5000);

            try
            {
                var result = await bs.ReadAsync(cts.Token);

                while (string.IsNullOrEmpty(result) || result.Length != 4)
                {
                    result = await bs.ReadAsync(cts.Token);
                }

                return float.Parse(result);
            }
            catch (TaskCanceledException)
            {
                return float.NaN;
            }
            catch (ObjectDisposedException)
            {
                return float.NaN;
            }
        }
    }
}
