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
using ArcReactor.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.Devices.Enumeration;
using Windows.UI.Popups;

namespace ArcReactor.ViewModels
{
    /// <summary>
    /// ViewModel for the MainPage
    /// </summary>
    public class MainPageViewModel : ViewModelBase
    {
        private const int LEDCOUNTER = 25;

        private ArcReactorService reactor;

        /// <summary>
        /// The collection of Arc Reactor devices to connect with
        /// </summary>
        public ObservableCollection<DeviceInformation> ArcReactorDevices { get; set; }

        /// <summary>
        /// The collection of RGB LEDs available at the Arc Reactor device
        /// </summary>
        public ObservableCollection<ColoredLed> RgbLeds { get; set; }

        private DeviceInformation selectedDevice;

        /// <summary>
        /// Selected device to connect to
        /// </summary>
        public DeviceInformation SelectedDevice
        {
            get
            {
                return selectedDevice;
            }

            set
            {
                if (this.selectedDevice != value)
                {
                    selectedDevice = value;
                    RaisePropertyChanged();
                }
            }
        }

        private float batteryLevel;

        /// <summary>
        /// Battery level of the connected device
        /// </summary>
        public float BatteryLevel
        {
            get
            {
                return batteryLevel;
            }

            set
            {
                if (this.batteryLevel != value)
                {
                    batteryLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool isConnected;

        /// <summary>
        /// Is the device connected or not
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }

            set
            {
                if (this.isConnected != value)
                {
                    isConnected = value;
                    ConnectButtonDescription = isConnected ? "Disconnect" : "Connect";

                    RaisePropertyChanged(nameof(IsConnected));
                }
            }
        }

        /// <summary>
        /// Connects with a selected device
        /// </summary>
        public async void Connect()
        {
            if (reactor.IsConnected)
            {
                reactor.Disconnect();
            }
            else
            {
                Views.Busy.SetBusy(true, "Connecting...");
                var result = await reactor.ConnectAsync(SelectedDevice);
                reactor.RequestBatteryLevel();
                Views.Busy.SetBusy(false);
                if (!result)
                {
                    await MessageBox("Connection failed");
                }
                else
                {
                }
            }

            IsConnected = reactor.IsConnected;
        }

        /// <summary>
        /// Copy color from the first LED to every another
        /// </summary>
        public void CopyFirstLed()
        {
            for (int i = 1; i < LEDCOUNTER; i++)
            {
                RgbLeds[i].SetRgb(RgbLeds[0].R, RgbLeds[0].G, RgbLeds[0].B);
            }
        }

        /// <summary>
        /// Asks the device about the battery level
        /// </summary>
        public void GetBatteryLevel()
        {
            reactor.RequestBatteryLevel();
        }

        private async Task MessageBox(string text)
        {
            var m = new MessageDialog(text);
            await m.ShowAsync();
        }

        private string connectButtonDescription = "Connect";

        /// <summary>
        /// Description of the connect/disconnect button
        /// </summary>
        public string ConnectButtonDescription
        {
            get
            {
                return connectButtonDescription;
            }

            set
            {
                if (this.connectButtonDescription != value)
                {
                    connectButtonDescription = value;
                    RaisePropertyChanged(nameof(ConnectButtonDescription));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of MainPageViewModel, and starts
        /// searching for Arc Reactor devices
        /// </summary>
        public MainPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                return;

            reactor = new ArcReactorService();
            RefreshDevicesList();

            CreateLedList();

            reactor.BatteryLevelChanged += Reactor_BatteryLevelChanged;
            reactor.Disconnected += Reactor_Disconnected;
        }

        private void Reactor_Disconnected()
        {
            IsConnected = reactor.IsConnected;
        }

        private void Reactor_BatteryLevelChanged(float value)
        {
            BatteryLevel = value;
        }

        /// <summary>
        /// Changes device mode to pulsing
        /// </summary>
        public void SendPulse()
        {
            reactor.SetPulseSequence();
        }

        /// <summary>
        /// Turns off all device LEDs
        /// </summary>
        public void SendBlack()
        {
            reactor.SetAllBlack();
        }

        /// <summary>
        /// Turns on "startup" mode for device
        /// </summary>
        public void SendStartup()
        {
            reactor.SetStartupSequence();
        }

        /// <summary>
        /// Sends to the device desired LEDs color for every diode
        /// </summary>
        public void SendLeds()
        {
            reactor.SetLedsBatchAsync(RgbLeds);
        }

        private DelegateCommand<ColoredLed> _applyColorCommand;

        /// <summary>
        /// Sends to the device desired LED color for a single diode
        /// </summary>
        public DelegateCommand<ColoredLed> ApplyColor
            => _applyColorCommand ?? (_applyColorCommand = new DelegateCommand<ColoredLed>(ApplyColorCommandExecute, ApplyColorCommandCanExecute));

        private bool ApplyColorCommandCanExecute(ColoredLed param) => true;

        private void ApplyColorCommandExecute(ColoredLed param)
        {
            reactor.SetSingleLedAsync(param);
        }

        private void CreateLedList()
        {
            RgbLeds = new ObservableCollection<ColoredLed>();
            for (int i = 0; i < LEDCOUNTER; i++)
            {
                RgbLeds.Add(new ColoredLed { Index = i, R = 0, G = 0, B = 0 });
            }
        }

        /// <summary>
        /// Refreshes the list of devices available for connection
        /// </summary>
        public async void RefreshDevicesList()
        {
            ArcReactorDevices = new ObservableCollection<DeviceInformation>();

            var pairedDevices = await reactor.FindPairedDevicesAsync();
            foreach (var item in pairedDevices)
            {
                ArcReactorDevices.Add(item);
            }
        }

        /// <summary>
        /// Navigates to About page
        /// </summary>
        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.AboutPage));
    }
}