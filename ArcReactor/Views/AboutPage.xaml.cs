using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ArcReactor.Views
{
    public sealed partial class AboutPage : Page
    {
        Template10.Services.SerializationService.ISerializationService _SerializationService;

        public AboutPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            _SerializationService = Template10.Services.SerializationService.SerializationService.Json;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //var index = int.Parse(_SerializationService.Deserialize(e.Parameter?.ToString()).ToString());
            //MyPivot.SelectedIndex = index;
        }
    }
}
