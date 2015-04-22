using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using SpeechingCommon;
using System.Collections.Generic;

namespace DroidSpeeching
{
    public class FeedFragment : Android.Support.V4.App.Fragment
    {
        RecyclerView feedList;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            return inflater.Inflate(Resource.Layout.MainResultsFragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            feedList = view.FindViewById<RecyclerView>(Resource.Id.mainResults_recyclerView);

            feedList.HasFixedSize = true;

            LinearLayoutManager llm = new LinearLayoutManager(Activity);
            llm.Orientation = LinearLayoutManager.Vertical;
            feedList.SetLayoutManager(llm);
        }

        public override void OnResume()
        {
            base.OnResume();
            InsertData();
        }

        private async void InsertData()
        {
            List<IFeedItem> items = await ServerData.FetchMainFeed();

            FeedCardAdapter adapter = new FeedCardAdapter(items, Activity);
            feedList.SetAdapter(adapter);
        }
    }
}