using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows_Speeching.Common;
using SpeechingShared;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace Windows_Speeching
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private readonly ObservableCollection<ISpeechingPracticeActivity> activities =
            new ObservableCollection<ISpeechingPracticeActivity>();

        private readonly ObservableCollection<IFeedItem> feedItems = new ObservableCollection<IFeedItem>();

        public HubPage()
        {
            InitializeComponent();

            SpeechingCategories.Source = activities;
            MainFeed.Source = feedItems;

            Prepare();
        }

        public async void Prepare()
        {
            await WindowsUtils.PrepareApp();

            LoadMainFeed();
            LoadActivities();
        }

        private async void LoadMainFeed()
        {
            List<IFeedItem> loaded = await ServerData.FetchMainFeed();

            foreach (IFeedItem item in loaded)
            {
                feedItems.Add(item);
            }
        }

        private async void LoadActivities()
        {
            await ServerData.FetchCategories();
            foreach (ActivityCategory cat in AppData.Session.Categories)
            {
                foreach (ISpeechingPracticeActivity act in cat.Activities)
                {
                    activities.Add(act);
                }
            }
        }

        private void Feed_ItemClick(object sender, ItemClickEventArgs e)
        {
            AppData.Io.PrintToConsole("Click from " + (e.ClickedItem as ISpeechingPracticeActivity).Title);
        }
    }
}