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
using Android.Support.V4.Graphics.Drawable;
using Android.Support.V7.App;

namespace DroidSpeeching
{
    [Activity(Label = "Your Feedback", ParentActivity = typeof(MainActivity))]
    public class FeedbackActivity : ActionBarActivity
    {
        private DrawerLayout drawer;
        private Android.Support.V4.App.ActionBarDrawerToggle drawerToggle;
        private ListView drawerList;

        private ListView feedbackList;
        private List<FeedbackData> feedbackData;

        private IResultItem[] submissions;
        private IFeedbackItem[] currentFeedback;
        private ISpeechingActivityItem thisActivity;
        private int selectedIndex;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);
            base.OnCreate(bundle);

            // Restore the user's last selection if it exists
            selectedIndex = (bundle == null)? 0 : bundle.GetInt("SelectedIndex", 0);

            FetchFeedbackDataInit();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutInt("SelectedIndex", selectedIndex);
        }

        private async Task FetchFeedbackDataInit()
        {
            SetContentView(Resource.Layout.FeedbackActivity);

            submissions = await ServerData.FetchSubmittedList();

            feedbackList = FindViewById<ListView>(Resource.Id.feedback_feedbackList);
            feedbackList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
            };

            PrepareDrawer();
        }

        private async Task PrepareDrawer()
        {
            FeedbackAdapter adapter = new FeedbackAdapter(this, new List<FeedbackData>());

                // Set up drawer
            drawer = FindViewById<DrawerLayout>(Resource.Id.feedback_drawerLayout);
            drawerList = FindViewById<ListView>(Resource.Id.feedback_itemsList);
            drawerList.Adapter = adapter;
            drawerList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                FeedbackData data = AndroidUtils.Cast<FeedbackData>(adapter.GetItem(args.Position));
                LoadFeedbackForActivity(data);
                selectedIndex = args.Position;
                if(drawer != null) drawer.CloseDrawers();
            };

            if (drawer != null)
            {
                // this won't be present on some layouts as the drawer might be a list which is always visible
                drawerToggle = new Android.Support.V4.App.ActionBarDrawerToggle(this, drawer, Resource.Drawable.ic_drawer, Resource.String.drawer_open, Resource.String.drawer_close);
                drawer.SetDrawerListener(drawerToggle);

                SupportActionBar.Show();
                SupportActionBar.SetHomeButtonEnabled(true);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            }

            ProgressDialog prog = new ProgressDialog(this);
            prog.SetTitle("Communicating with server...");
            prog.SetMessage("Please wait...");
            prog.Show();

            // Fetch the data after making the drawer
            for(int i = 0; i < submissions.Length; i++)
            {
                ISpeechingActivityItem act = await AppData.session.FetchActivityWithId(submissions[i].ParticipantActivityId);
                FeedbackData newData = new FeedbackData();
                newData.activity = act;
                newData.submission = submissions[i];
                adapter.Add(newData);

                if (i == 0)
                {
                    // Load the first item!
                    prog.Hide();
                    LoadFeedbackForActivity(newData); 
                }
            }
        }

        /// <summary>
        /// Make the main view about the given activity
        /// </summary>
        /// <param name="actId">The ID fo the activity to show feedback for</param>
        /// <returns></returns>
        private async Task LoadFeedbackForActivity(FeedbackData data)
        {
            ProgressDialog prog = new ProgressDialog(this);
            prog.SetTitle("Fetching Feedback");
            prog.SetMessage("Please wait...");
            prog.Show();

            if (data.feedback == null)
            {
                data.feedback = await ServerData.FetchFeedbackFor(data.submission.Id);
            }

            thisActivity = data.activity;
            string iconAddress = await Utils.FetchLocalCopy(thisActivity.Icon);

            FindViewById<ImageView>(Resource.Id.feedback_icon).SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(iconAddress)));
            FindViewById<TextView>(Resource.Id.feedback_title).Text = thisActivity.Title;
            FindViewById<TextView>(Resource.Id.feedback_completionDate).Text = "Completed on " + data.submission.CompletionDate.ToShortDateString();

            if (feedbackList.Adapter == null)
            {
                feedbackList.Adapter = new FeedbackTypesAdapter(this, Resource.Id.mainFriendsList, data.feedback);
            }
            else
            {
                RunOnUiThread(() =>
                {
                    ((FeedbackTypesAdapter)feedbackList.Adapter).NotifyDataSetChanged();
                    ((FeedbackTypesAdapter)feedbackList.Adapter).feedbackItems = data.feedback;
                    feedbackList.SetSelection(0);
                });
            }

            prog.Hide();
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

    /// <summary>
    /// A simple container holding all needed information
    /// </summary>
    public class FeedbackData
    {
        public ISpeechingActivityItem activity;
        public IResultItem submission;
        public IFeedbackItem[] feedback;
    }

    public class FeedbackAdapter : BaseAdapter<FeedbackData>
    {
        private Activity context;
        public List<FeedbackData> data;

        public FeedbackAdapter(Activity context, List<FeedbackData> data)
        {
            this.context = context;
            this.data = data;
        }

        public override FeedbackData this[int position]
        {
            get { return data[position]; }
        }

        public override int Count
        {
            get { return data.Count; }
        }

        public void Add(FeedbackData newData)
        {
            data.Add(newData);
            this.NotifyDataSetChanged();
        }

        public override long GetItemId(int position)
        {
            return data[position].submission.Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            FeedbackData thisItem = data[position];

            if (convertView == null)
            {
                convertView = context.LayoutInflater.Inflate(Resource.Layout.FeedbackListItem, null);
            }
            convertView.FindViewById<TextView>(Resource.Id.feedbackList_title).Text = thisItem.activity.Title;
            convertView.FindViewById<TextView>(Resource.Id.feedbackList_date).Text = "Submitted on " + thisItem.submission.CompletionDate.ToShortDateString();

            return convertView;
        }
    }

    public class FeedbackTypesAdapter : BaseAdapter<IFeedbackItem>
    {
        private Activity context;
        private List<int> seen;
        public IFeedbackItem[] feedbackItems;
        private Dictionary<Type, int> viewTypes;

        /// <summary>
        /// Lists feedback in multiple layout and object types
        /// </summary>
        public FeedbackTypesAdapter(Activity context, int resource, IFeedbackItem[] data)
        {
            this.context = context;
            this.feedbackItems = data;

            viewTypes = new Dictionary<Type, int>();
            viewTypes.Add(typeof(PercentageFeedback), 0);
            viewTypes.Add(typeof(StarRatingFeedback), 1);
            viewTypes.Add(typeof(CommentFeedback), 2);
            seen = new List<int>();
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

        public override void NotifyDataSetChanged()
        {
            base.NotifyDataSetChanged();
            seen.Clear();
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

                // Keep track of which items have been seen so that we don't animate them multiple times!
                if (!seen.Contains(feedbackItems[position].Id))
                {
                    AnimatePercentage(((PercentageFeedback)feedbackItems[position]).Percentage, 1500, convertView.FindViewById<RadialProgressView>(Resource.Id.feedback_progressView));
                    seen.Add(feedbackItems[position].Id);
                }
                else
                {
                    convertView.FindViewById<RadialProgressView>(Resource.Id.feedback_progressView).Value = ((PercentageFeedback)feedbackItems[position]).Percentage;
                }
            }
            else if (thisItem.GetType() == typeof(StarRatingFeedback))
            {
                if (convertView == null)
                {
                    convertView = context.LayoutInflater.Inflate(Resource.Layout.FeedbackRatingItem, null);
                }
                convertView.FindViewById<RatingBar>(Resource.Id.feedback_ratingBar).Rating = ((StarRatingFeedback)thisItem).Rating;
            }
            else if(thisItem.GetType() == typeof(CommentFeedback))
            {
                if (convertView == null)
                {
                    convertView = context.LayoutInflater.Inflate(Resource.Layout.FeedbackCommentItem, null);
                }
                LoadUserAvatar(((CommentFeedback)thisItem).Commenter, convertView.FindViewById<ImageView>(Resource.Id.feedback_commentAvatar));
                convertView.FindViewById<TextView>(Resource.Id.feedback_comment_Username).Text = ((CommentFeedback)thisItem).Commenter.name;
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
        private async Task AnimatePercentage(float toVal, float millis, RadialProgressView progressView)
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

        /// <summary>
        /// Load the given user's avatar into the ImageView
        /// </summary>
        private async Task LoadUserAvatar(User user, ImageView view)
        {
            string imageLoc = await Utils.FetchLocalCopy(user.avatar, typeof(User));

            RoundedBitmapDrawable img = RoundedBitmapDrawableFactory.Create(view.Resources, imageLoc);
            img.SetAntiAlias(true);
            img.CornerRadius = 120;
            view.SetImageDrawable(img);
        }
    }
    
}