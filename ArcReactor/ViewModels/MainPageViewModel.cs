using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
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


        DelegateCommand _connectCommand;
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

        DelegateCommand _sendPulseCommand;
        public DelegateCommand SendPulseCommand
            => _sendPulseCommand ?? (_sendPulseCommand = new DelegateCommand(async () =>
            {
                await bs.WriteAsync("pulse");
            }, () => true));

        DelegateCommand _sendStartupCommand;
        public DelegateCommand SendStartupCommand
            => _sendStartupCommand ?? (_sendStartupCommand = new DelegateCommand(async () =>
            {
                await bs.WriteAsync("startup");
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
