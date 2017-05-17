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

using ArcReactor.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.Devices.Enumeration;
using Windows.UI.Popups;

namespace ArcReactor.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private BluetoothService bs;
        public ObservableCollection<DeviceInformation> BluetoothSerialDevices { get; set; }

        private DeviceInformation selectedDevice;

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

        private bool isConnected;

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
                    ConnectButtonDescription = (isConnected) ? "Disconnect" : "Connect";

                    RaisePropertyChanged(nameof(IsConnected));
                }
            }
        }

        private DelegateCommand _connectCommand;

        public DelegateCommand ConnectCommand
            => _connectCommand ?? (_connectCommand = new DelegateCommand(async () =>
            {
                if (bs.IsConnected)
                {
                    bs.Disconnect();
                }
                else
                {
                    var result = await bs.ConnectAsync(SelectedDevice);
                    if (!result)
                    {
                        await MessageBox("Connection failed");
                    }
                    else
                    {
                    }
                }

                IsConnected = bs.IsConnected;
            }, () => true));

        private DelegateCommand _sendPulseCommand;

        public DelegateCommand SendPulseCommand
            => _sendPulseCommand ?? (_sendPulseCommand = new DelegateCommand(async () =>
            {
                await bs.WriteAsync("pulse");
            }, () => true));

        private DelegateCommand _sendStartupCommand;

        public DelegateCommand SendStartupCommand
            => _sendStartupCommand ?? (_sendStartupCommand = new DelegateCommand(async () =>
            {
                await bs.WriteAsync("startup");
            }, () => true));

        private DelegateCommand _turnOffCommand;

        public DelegateCommand TurnOffCommand
            => _turnOffCommand ?? (_turnOffCommand = new DelegateCommand(async () =>
            {
                await bs.WriteAsync("black");
            }, () => true));

        private async Task MessageBox(string text)
        {
            var m = new MessageDialog(text);
            await m.ShowAsync();
        }

        private string connectButtonDescription = "Connect";

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

        public MainPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                return;

            bs = new BluetoothService();
            PopulateDevicesList();
        }

        public async Task PopulateDevicesList()
        {
            BluetoothSerialDevices = new ObservableCollection<DeviceInformation>();

            var pairedDevices = await bs.FindPairedDevicesAsync();
            foreach (var item in pairedDevices)
            {
                BluetoothSerialDevices.Add(item);
            }
        }
    }
}