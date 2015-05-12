using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows_Speeching.Data;
using Windows_Speeching.Common;
using SpeechingShared;
using Windows.Networking.Connectivity;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace Windows_Speeching
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private ObservableCollection<ISpeechingActivityItem> activities = new ObservableCollection<ISpeechingActivityItem>();
        private ObservableCollection<IFeedItem> feedItems = new ObservableCollection<IFeedItem>();

        public HubPage()
        {
            this.InitializeComponent();

            SpeechingCategories.Source = activities;
            MainFeed.Source = feedItems;

            Prepare();
        }

        public async void Prepare()
        {
            AppData.checkForConnection = () =>
            {
                ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
                bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
                return internet;
            };

            AppData.onConnectionSuccess = () =>
            {
                
            };

            AppData.IO = new Win8PCLHelper();

            await AppData.AssignCacheLocations();
            await AppData.InitializeIfNeeded();

            LoadMainFeed();
            LoadActivities();
        }

        private async void LoadMainFeed()
        {
            List<IFeedItem> loaded = await ServerData.FetchMainFeed();

            foreach(IFeedItem item in loaded)
            {
                feedItems.Add(item);
            }
        }

        private async void LoadActivities()
        {
            await ServerData.FetchCategories();
            foreach (ActivityCategory cat in AppData.session.categories)
            {
                foreach (ISpeechingActivityItem act in cat.activities)
                {
                    activities.Add(act);
                }
            }
        }

        private void Feed_ItemClick(object sender, ItemClickEventArgs e)
        {
            AppData.IO.PrintToConsole("Click from " + (e.ClickedItem as ISpeechingActivityItem).Title);
        }
    }
}
