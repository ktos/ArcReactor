﻿#region License

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
using System.Collections.Generic;
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

        public List<LedColor> LedColors { get; set; }

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

        public async Task Connect()
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
        }

        public void CopyFirstLed()
        {
            for (int i = 1; i < LED_COUNTER; i++)
            {
                LedColors[i].SetRgb(LedColors[0].R, LedColors[0].G, LedColors[0].B);
            }
        }

        public void ApplyAll()
        {
            throw new NotImplementedException();
        }

        public async void SendPulse()
        {
            await bs.WriteAsync("pulse");
        }

        public async void SendStartup()
        {
            await bs.WriteAsync("startup");
        }

        public async void SendBlack()
        {
            await bs.WriteAsync("black");
        }

        private async Task MessageBox(string text)
        {
            var m = new MessageDialog(text);
            await m.ShowAsync();
        }

        private string connectButtonDescription = "Connect";
        private const int LED_COUNTER = 20;

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
            RefreshDevicesList();

            CreateLedList();
        }

        private DelegateCommand<LedColor> _applyColorCommand;

        public DelegateCommand<LedColor> ApplyColorCommand
            => _applyColorCommand ?? (_applyColorCommand = new DelegateCommand<LedColor>(ApplyColorCommandExecute, ApplyColorCommandCanExecute));

        private bool ApplyColorCommandCanExecute(LedColor param) => true;

        private async void ApplyColorCommandExecute(LedColor param)
        {
            await bs.WriteAsync(param.ToDeviceCommand());
        }

        public void CreateLedList()
        {
            LedColors = new List<LedColor>();
            for (int i = 0; i < LED_COUNTER; i++)
            {
                LedColors.Add(new LedColor { Index = i, R = 190, G = 100, B = 20 });
            }
        }

        public async Task RefreshDevicesList()
        {
            BluetoothSerialDevices = new ObservableCollection<DeviceInformation>();

            var pairedDevices = await bs.FindPairedDevicesAsync();
            foreach (var item in pairedDevices)
            {
                BluetoothSerialDevices.Add(item);
            }
        }

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.AboutPage));
    }
}