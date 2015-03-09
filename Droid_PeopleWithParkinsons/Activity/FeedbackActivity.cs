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
        private ISpeechingActivityItem currentActivity;
        private IFeedbackItem[] currentFeedback;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.FeedbackActivity);

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

            if(drawer != null)
            {
                // this won't be present on some layouts as the drawer might be a list which is always visible
                drawerToggle = new ActionBarDrawerToggle(this, drawer, Resource.Drawable.ic_drawer, Resource.String.drawer_open, Resource.String.drawer_close);
                drawer.SetDrawerListener(drawerToggle);

                ActionBar.Show();
                ActionBar.SetHomeButtonEnabled(true);
                ActionBar.SetDisplayHomeAsUpEnabled(true);
            }

            currentFeedback = AppData.FetchFeedback("sossie");

            feedbackList = FindViewById<ListView>(Resource.Id.feedback_feedbackList);
            feedbackList.Adapter = new FeedbackAdapter(this, Resource.Id.mainFriendsList, currentFeedback);
            feedbackList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                Toast.MakeText(this, ((PercentageFeedback)currentFeedback[args.Position]).Percentage.ToString(), ToastLength.Short).Show();
            };
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
        Activity context;
        IFeedbackItem[] feedbackItems;

        /// <summary>
        /// An adapter to be able to display the details on each task in a grid or list
        /// </summary>
        public FeedbackAdapter(Activity context, int resource, IFeedbackItem[] data)
        {
            this.context = context;
            this.feedbackItems = data;
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

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            IFeedbackItem thisItem = feedbackItems[position];

            if (view == null)
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.FeedbackListItem, null);
            }

            //view.FindViewById<TextView>(Resource.Id.feedback_progressView).Value = ((PercentageFeedback)feedbackItems[position]).Percentage;

            if(thisItem.GetType() == typeof(PercentageFeedback))
            {
                AnimatePercentage(((PercentageFeedback)feedbackItems[position]).Percentage, 2000, view.FindViewById<RadialProgressView>(Resource.Id.feedback_progressView));
            }

            return view;
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