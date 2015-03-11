using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Widget;
using Android.Support.V4.App;
using SpeechingCommon;
using RadialProgress;
using System.Threading.Tasks;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Your Feedback")]
    public class FeedbackActivity : Activity
    {
        private DrawerLayout drawer;
        private ActionBarDrawerToggle drawerToggle;
        private ListView drawerList;

        private ListView feedbackList;
        private ResultItem[] submissions;
        private IFeedbackItem[] currentFeedback;
        private ISpeechingActivityItem thisActivity;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            FetchFeedbackDataInit();
        }

        private async Task FetchFeedbackDataInit()
        {

            SetContentView(Resource.Layout.FeedbackActivity);

            submissions = await ServerData.FetchSubmittedList();

            //TODO
            string[] fakeOptions = new string[] { "these are", "some fake", "options to fill", "up sidebar space" };

            // Set up drawer
            drawer = FindViewById<DrawerLayout>(Resource.Id.feedback_drawerLayout);
            drawerList = FindViewById<ListView>(Resource.Id.feedback_itemsList);
            drawerList.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, fakeOptions);
            drawerList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                Toast.MakeText(this, "Option selected!", ToastLength.Short).Show();
            };

            if (drawer != null)
            {
                // this won't be present on some layouts as the drawer might be a list which is always visible
                drawerToggle = new ActionBarDrawerToggle(this, drawer, Resource.Drawable.ic_drawer, Resource.String.drawer_open, Resource.String.drawer_close);
                drawer.SetDrawerListener(drawerToggle);

                ActionBar.Show();
                ActionBar.SetHomeButtonEnabled(true);
                ActionBar.SetDisplayHomeAsUpEnabled(true);
            }

            feedbackList = FindViewById<ListView>(Resource.Id.feedback_feedbackList);
            feedbackList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                Toast.MakeText(this, ((PercentageFeedback)currentFeedback[args.Position]).Percentage.ToString(), ToastLength.Short).Show();
            };
            LoadFeedbackForActivity(submissions[0]);
        }

        /// <summary>
        /// Make the main view about the given activity
        /// </summary>
        /// <param name="actId">The ID fo the activity to show feedback for</param>
        /// <returns></returns>
        private async Task LoadFeedbackForActivity(ResultItem result)
        {
            currentFeedback = await ServerData.FetchFeedbackFor(result.id);

            thisActivity = await AppData.session.FetchActivityWithId(result.activityId);
            string iconAddress = await Utils.FetchLocalCopy(thisActivity.Icon);

            FindViewById<ImageView>(Resource.Id.tasklist_childIcon).SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(iconAddress)));

            if(feedbackList.Adapter == null)
            {
                feedbackList.Adapter = new FeedbackAdapter(this, Resource.Id.mainFriendsList, currentFeedback);
            }
            else
            {
                ((FeedbackAdapter)feedbackList.Adapter).feedbackItems = currentFeedback;
                RunOnUiThread(() => ((FeedbackAdapter)feedbackList.Adapter).NotifyDataSetChanged());
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (drawer != null && drawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #region DrawerLayout management
        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            if(drawer != null) drawerToggle.SyncState();
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            if (drawer != null) drawerToggle.OnConfigurationChanged(newConfig);
        }
        #endregion
    }


    public class FeedbackAdapter : BaseAdapter<IFeedbackItem>
    {
        private Activity context;
        public IFeedbackItem[] feedbackItems;
        private Dictionary<Type, int> viewTypes;

        /// <summary>
        /// Lists feedback in multiple layout and object types
        /// </summary>
        public FeedbackAdapter(Activity context, int resource, IFeedbackItem[] data)
        {
            this.context = context;
            this.feedbackItems = data;

            viewTypes = new Dictionary<Type, int>();
            viewTypes.Add(typeof(PercentageFeedback), 0);
            viewTypes.Add(typeof(StarRatingFeedback), 1);
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override IFeedbackItem this[int position]
        {
            get { return feedbackItems[position]; }
        }

        public override int Count
        {
            get { return feedbackItems.Length; }
        }

        public override int ViewTypeCount
        {
            get { return viewTypes.Count; }
        }

        // Helps decide which layout to use, based on object type
        public override int GetItemViewType(int position)
        {
            return viewTypes[feedbackItems[position].GetType()];
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            IFeedbackItem thisItem = feedbackItems[position];

            if(thisItem.GetType() == typeof(PercentageFeedback))
            {
                if (convertView == null)
                {
                     convertView = context.LayoutInflater.Inflate(Resource.Layout.FeedbackPercentItem, null);
                }
                AnimatePercentage(((PercentageFeedback)feedbackItems[position]).Percentage, 1500, convertView.FindViewById<RadialProgressView>(Resource.Id.feedback_progressView));
            }

            if (thisItem.GetType() == typeof(StarRatingFeedback))
            {
                if (convertView == null)
                {
                    convertView = context.LayoutInflater.Inflate(Resource.Layout.FeedbackRatingItem, null);
                }
                convertView.FindViewById<RatingBar>(Resource.Id.feedback_ratingBar).Rating = ((StarRatingFeedback)thisItem).Rating;
            }

            // If this list is going to be really big, it might be worth setting up a view holder? Don't think it will be
            convertView.FindViewById<TextView>(Resource.Id.feedback_itemTitle).Text = thisItem.Title;
            convertView.FindViewById<TextView>(Resource.Id.feedback_itemCaption).Text = thisItem.Caption;

            return convertView;
        }

        /// <summary>
        /// Make the progress view gradually count up to the given value
        /// </summary>
        /// <param name="toVal">The eventual target value</param>
        /// <param name="millis">The total time for the animation</param>
        /// <param name="progressView">The view to affect</param>
        /// <returns>Awaitable</returns>
        public async Task AnimatePercentage(float toVal, float millis, RadialProgressView progressView)
        {
            int waitTime = (int)(millis / toVal);
            float current = 0;
            while(current < toVal)
            {
                current++;
                progressView.Value = current;
                await Task.Delay(waitTime);
            }
        }
    }
    
}