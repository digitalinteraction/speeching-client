using System.Collections.Generic;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using com.dbeattie;
using SpeechingShared;

namespace DroidSpeeching
{
    public class FeedFragment : Android.Support.V4.App.Fragment, ISwipeListener, IActionClickListener
    {
        private FeedCardAdapter adapter;
        private IFeedItem[] backup;
        private int[] changed;
        private RecyclerView feedList;
        private List<IFeedItem> items;
        private CustomSwipeToRefresh refresher;

        public void OnActionClicked(Snackbar snackbar)
        {
            foreach (int position in changed)
            {
                items.Insert(position, backup[position]);
                adapter.NotifyItemInserted(position);
            }

            adapter.NotifyDataSetChanged();
            adapter.NotifyItemRangeChanged(0, items.Count);
        }

        public bool CanSwipe(int position)
        {
            return items[position].Dismissable;
        }

        public void OnDismissedBySwipeLeft(RecyclerView recyclerView, int[] reverseSortedPositions)
        {
            backup = items.ToArray();
            changed = reverseSortedPositions;
            foreach (int position in reverseSortedPositions)
            {
                items.RemoveAt(position);
                adapter.NotifyItemRemoved(position);
            }
            adapter.NotifyDataSetChanged();
            adapter.NotifyItemRangeChanged(0, items.Count);

            SnackbarManager.Show(
                Snackbar.With(Activity).Text("Removed item from feed")
                    .ActionLabel("Undo")
                    .Color(Color.DarkRed)
                    .ActionColor(Color.White)
                    .TextColor(Color.White)
                    .ActionListener(this));
        }

        public void OnDismissedBySwipeRight(RecyclerView recyclerView, int[] reverseSortedPositions)
        {
            OnDismissedBySwipeLeft(recyclerView, reverseSortedPositions);
        }

        public bool CanSwipeLeft
        {
            get { return false; }
        }

        public bool CanSwipeRight
        {
            get { return true; }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            return inflater.Inflate(Resource.Layout.MainResultsFragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            refresher = view.FindViewById<CustomSwipeToRefresh>(Resource.Id.refresher);
            refresher.SetSlop(Activity);
            refresher.Refresh += async delegate
            {
                if (!AndroidUtils.IsConnected() || adapter == null)
                {
                    AndroidUtils.OfflineAlert(Activity);
                    refresher.Refreshing = false;
                    return;
                }

                items = await ServerData.FetchMainFeed();
                adapter.Data = items;
                refresher.Refreshing = false;

                Activity.RunOnUiThread(() => adapter.NotifyDataSetChanged());
            };

            feedList = view.FindViewById<RecyclerView>(Resource.Id.mainResults_recyclerView);

            feedList.HasFixedSize = true;

            LinearLayoutManager llm = new LinearLayoutManager(Activity) {Orientation = LinearLayoutManager.Vertical};
            feedList.SetLayoutManager(llm);
        }

        public override void OnResume()
        {
            base.OnResume();
            InsertData();
        }

        private async void InsertData()
        {
            Activity.RunOnUiThread(() => { refresher.Post(() => { refresher.Refreshing = true; }); });

            items = await ServerData.FetchMainFeed();

            adapter = new FeedCardAdapter(items, Activity);
            feedList.SetAdapter(adapter);

            SwipeableRecyclerViewTouchListener swipeTouchListener = new SwipeableRecyclerViewTouchListener(feedList,
                this, Activity);
            feedList.AddOnItemTouchListener(swipeTouchListener);

            Activity.RunOnUiThread(() => { refresher.Post(() => { refresher.Refreshing = false; }); });
        }
    }
}