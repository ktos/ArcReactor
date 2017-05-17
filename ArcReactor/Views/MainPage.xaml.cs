using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcReactor.Views
{
    public sealed partial class MainPage : Page
    {
        private BluetoothService bs;

        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            bs = new BluetoothService();
            PopulateDevicesList();
        }

        public async Task PopulateDevicesList()
        {
            var pairedDevices = await bs.FindPairedDevicesAsync();
            cbDevices.ItemsSource = pairedDevices;
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (bs.IsConnected)
            {
                bs.Disconnect();
                btnConnect.Content = "Connect";
                spBasic.Visibility = Visibility.Collapsed;
            }
            else
            {
                var result = await bs.ConnectAsync(cbDevices.SelectedItem as DeviceInformation);
                if (!result)
                {
                    await MessageBox("Connection Failed");
                }
                else
                {
                    btnConnect.Content = "Disconnect";
                    spBasic.Visibility = Visibility.Visible;
                }
            }
        }

        private async Task MessageBox(string text)
        {
            var m = new MessageDialog(text);
            await m.ShowAsync();
        }

        private async void SendPulse(object sender, RoutedEventArgs e)
        {
            await bs.WriteAsync("pulse");
        }

        private async void SendStartup(object sender, RoutedEventArgs e)
        {
            await bs.WriteAsync("startup");
        }
    }
}