using Android.App;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using SpeechingCommon;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DroidSpeeching
{
    [Activity(Label = "Your Feedback", ParentActivity = typeof(MainActivity))]
    public class FeedbackActivity : ActionBarActivity
    {
        private DrawerLayout drawer;
        private Android.Support.V4.App.ActionBarDrawerToggle drawerToggle;
        private ListView drawerList;

        private RecyclerView feedbackList;
        private TextView teaseText;
        private TextView activityTitle;
        private TextView completionDate;

        private IResultItem[] submissions;
        private ISpeechingActivityItem thisActivity;
        private int selectedIndex;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);
            base.OnCreate(bundle);

            // Restore the user's last selection if it exists
            selectedIndex = (bundle == null)? 0 : bundle.GetInt("SelectedIndex", 0);

            SetContentView(Resource.Layout.FeedbackActivity);

            teaseText = FindViewById<TextView>(Resource.Id.feedback_tease);
            teaseText.Visibility = ViewStates.Invisible;
            activityTitle = FindViewById<TextView>(Resource.Id.feedback_title);
            activityTitle.Text = "Loading...";
            completionDate = FindViewById<TextView>(Resource.Id.feedback_completionDate);
            completionDate.Text = "";

            FetchFeedbackDataInit();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutInt("SelectedIndex", selectedIndex);
        }

        private async Task FetchFeedbackDataInit()
        {
            submissions = await ServerData.FetchSubmittedList();

            if(submissions == null || submissions.Length == 0)
            {
                AlertDialog alert = new AlertDialog.Builder(this)
                    .SetTitle("No feedback to display")
                    .SetMessage("You haven't submitted any results yet! Come back here once you have completed some activities and uploaded your results to the server!")
                    .SetCancelable(false)
                    .SetPositiveButton("Ok", (arg1, arg2) => { this.Finish(); })
                    .Create();
                alert.Show();

                return;
            }

            teaseText.Visibility = ViewStates.Visible;

            feedbackList = FindViewById<RecyclerView>(Resource.Id.feedback_feedbackList);
            feedbackList.HasFixedSize = true;
            LinearLayoutManager llm = new LinearLayoutManager(this);
            llm.Orientation = LinearLayoutManager.Vertical;
            feedbackList.SetLayoutManager(llm);

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

            // Add button to front of list
            //data.feedback.Insert(0, new FeedbackSubmissionButton());

            thisActivity = data.activity;
            string iconAddress = await Utils.FetchLocalCopy(thisActivity.Icon);

            FindViewById<ImageView>(Resource.Id.feedback_icon).SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(iconAddress)));
            activityTitle.Text = thisActivity.Title;
            completionDate.Text = "Completed on " + data.submission.CompletionDate.ToShortDateString();

            if (feedbackList.GetAdapter() == null)
            {
                //FeedCardAdapter adapter = new FeedCardAdapter(data.feedback, this);
                //feedbackList.SetAdapter(adapter);
            }
            else
            {
                RunOnUiThread(() =>
                {
                    //((FeedCardAdapter)feedbackList.GetAdapter()).data = data.feedback;
                    ((FeedCardAdapter)feedbackList.GetAdapter()).NotifyDataSetChanged();
                    feedbackList.ScrollToPosition(0);
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
        public List<IFeedItem> feedback;
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
    
}