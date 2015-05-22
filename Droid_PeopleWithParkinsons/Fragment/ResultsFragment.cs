using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using SpeechingShared;
using System.Collections.Generic;

namespace DroidSpeeching
{
    public class ResultsFragment : Android.Support.V4.App.Fragment
    {
        RecyclerView recList;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            return inflater.Inflate(Resource.Layout.MainResultsFragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            recList = view.FindViewById<RecyclerView>(Resource.Id.mainResults_recyclerView);

            recList.HasFixedSize = true;

            LinearLayoutManager llm = new LinearLayoutManager(Activity);
            llm.Orientation = LinearLayoutManager.Vertical;
            recList.SetLayoutManager(llm);
        }

        public override void OnResume()
        {
            base.OnResume();
            InsertData();
        }

        private async void InsertData()
        {
            List<IResultItem> uploads = AppData.Session.resultsToUpload;

            if (uploads == null || uploads.Count == 0) return;

            List<IFeedItem> feedback = await ServerData.FetchFeedbackFor(uploads[0].Id);

            FeedCardAdapter adapter = new FeedCardAdapter(feedback, Activity);
            recList.SetAdapter(adapter);
        }
    }
}