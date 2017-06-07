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

using ArcReactor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace ArcReactor.Services
{
    /// <summary>
    /// Handles a situation when battery level message from ArcReactor
    /// was received
    /// </summary>
    /// <param name="value">New battery level</param>
    public delegate void BatteryLevelChangedHandler(float value);

    /// <summary>
    /// The class for interaction with Arc Reactor 01 device
    /// </summary>
    public class ArcReactorService
    {
        private BluetoothService bs;

        /// <summary>
        /// Occurs when new battery status message is received from the device
        /// </summary>
        public event BatteryLevelChangedHandler BatteryLevelChanged;

        /// <summary>
        /// Occurs when device is disconnected
        /// </summary>
        public event DisconnectedEventHandler Disconnected;

        /// <summary>
        /// Initializes a new instance of ArcReactorService
        /// </summary>
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

        /// <summary>
        /// Describes if the device is currently connected
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Connects with the selected Arc Reactor device
        /// </summary>
        /// <param name="selectedDevice">
        /// The selected device to be connected with
        /// </param>
        /// <returns>If the connection was successful</returns>
        public async Task<bool> ConnectAsync(DeviceInformation selectedDevice)
        {
            if (bs.IsConnected)
                return false;

            var result = await bs.ConnectAsync(selectedDevice);
            IsConnected = bs.IsConnected;

            return result;
        }

        /// <summary>
        /// Disconnects from the device
        /// </summary>
        public void Disconnect()
        {
            if (bs.IsConnected)
            {
                bs.Disconnect();
                IsConnected = false;
            }
        }

        /// <summary>
        /// Sends the message to the device to start up the "pulse"
        /// lighting sequence
        /// </summary>
        public async void SetPulseSequence()
        {
            await bs.WriteAsync("pulse");
        }

        /// <summary>
        /// Sends the message to the device to start up the "startup"
        /// lighting sequence
        /// </summary>
        public async void SetStartupSequence()
        {
            await bs.WriteAsync("startup");
        }

        /// <summary>
        /// Sends the message to the device to turn off all LEDs
        /// </summary>
        public async void SetAllBlack()
        {
            await bs.WriteAsync("black");
        }

        /// <summary>
        /// Sends the message to the device to set all LEDs colors in
        /// one message
        /// </summary>
        /// <param name="leds">
        /// A list of colors and index values for LEDs
        /// </param>
        public async void SetLedsBatchAsync(IList<ColoredLed> leds)
        {
            byte[] sb = new byte[(leds.Count * 3) + 1];
            sb[0] = (byte)'i';

            for (int i = 0; i < leds.Count; i++)
            {
                sb[(i * 3) + 2] = (byte)leds[i].G;
                sb[(i * 3) + 3] = (byte)leds[i].B;
                sb[(i * 3) + 1] = (byte)leds[i].R;
            }

            await bs.WriteBytesAsync(sb);
        }

        /// <summary>
        /// Sets the single LED color by sending the respective message
        /// </summary>
        /// <param name="led">Index value and desired color</param>
        public async void SetSingleLedAsync(ColoredLed led)
        {
            await bs.WriteBytesAsync(led.ToDeviceCommand());
        }

        /// <summary>
        /// Searches for a list of all devices which may be Arc Reactor device
        /// </summary>
        /// <returns>
        /// List of all Arc Reactor devices paired with the system
        /// </returns>
        public async Task<DeviceInformationCollection> FindPairedDevicesAsync()
        {
            var pairedDevices = await bs.FindPairedDevicesAsync();

            pairedDevices.Where(x => x.Name.StartsWith("Arc Reactor") || x.Name.Contains("Dev B"));

            return pairedDevices;
        }

        /// <summary>
        /// Sends the message to request new battery level immediatelly
        /// </summary>
        public async void RequestBatteryLevel()
        {
            await bs.WriteAsync("batt");
        }
    }
}