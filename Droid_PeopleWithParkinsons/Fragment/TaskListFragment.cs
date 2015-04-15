using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using SpeechingCommon;

namespace DroidSpeeching
{
    /// <summary>
    /// A fragment which lists the currently available tasks in a grid layout
    /// </summary>
    public class TaskListFragment : Android.Support.V4.App.Fragment
    {
        private SwipeRefreshLayout refresher;
        private ExpandableListView mainList;
        private Button viewFeedbackBtn;
        private Button practiceBtn;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.MainTaskListFragment, container, false);
                
            View header = Activity.LayoutInflater.Inflate(Resource.Layout.MainTaskListHeader, null);
            mainList = view.FindViewById<ExpandableListView>(Resource.Id.mainActivitiesList);
            mainList.AddHeaderView(header, null, false);
            mainList.SetAdapter(new ScenarioListAdapter(Activity, Resource.Id.mainActivitiesList, AppData.session.categories.ToArray()));
            mainList.ChildClick += mainList_ChildClick;  

            // When the pull to refresh is activated, pull new data from the server and refresh the list with the new data
            refresher = view.FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);
            refresher.Refresh += async delegate
            {
                if (!AppData.CheckNetwork())
                {
                    AndroidUtils.OfflineAlert(Activity);
                    refresher.Refreshing = false;
                    return;
                }

                await ServerData.FetchCategories();
                refresher.Refreshing = false;

                ((ScenarioListAdapter)mainList.ExpandableListAdapter).categories = AppData.session.categories.ToArray();
                Activity.RunOnUiThread(() => ((ScenarioListAdapter)mainList.ExpandableListAdapter).NotifyDataSetChanged());
            };

            // If there's only one category, it makes sense to expand it by default
            if (AppData.session.categories.Count == 1)
            {
                mainList.ExpandGroup(0, true);
            }

            viewFeedbackBtn = header.FindViewById<Button>(Resource.Id.viewSubmittedBtn);
            viewFeedbackBtn.Click += viewFeedbackBtn_Click;

            practiceBtn = header.FindViewById<Button>(Resource.Id.practiceAreaBtn);
            practiceBtn.Click += practiceButton_Click;
            return view;
        }

        /// <summary>
        /// A child in the list (an activity) has been selected. Check to see if existing data for this activity
        /// already exists before launching it.
        /// </summary>
        void mainList_ChildClick(object sender, ExpandableListView.ChildClickEventArgs e)
        {
            ISpeechingActivityItem thisItem = AppData.session.categories[e.GroupPosition].activities[e.ChildPosition];

            System.Type objectType = thisItem.GetType();
            System.Type targetActivity = typeof(MainActivity);

            if (objectType == typeof(Scenario)) targetActivity = typeof(ScenarioActivity);
            else if (objectType == typeof(Guide)) targetActivity = typeof(GuideActivity);

            Intent intent = new Intent(Activity, targetActivity);
            int itemId = AppData.session.categories[e.GroupPosition].activities[e.ChildPosition].Id;
            intent.PutExtra("ActivityId", itemId);

            if (!AppData.CheckNetwork() && !GetActivityPrefs(itemId).GetBoolean("DOWNLOADED", false))
            {
                AndroidUtils.OfflineAlert(Activity, "This activity has not been downloaded yet and requires an Internet connection to prepare!");
                return;
            }

            if (AppData.CheckForActivityResultData(itemId))
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(Activity)
                .SetTitle("Existing results found...")
                .SetMessage("Re-doing this scenario will wipe any progress for it which hasn't been uploaded. Are you sure you want to do this?")
                .SetPositiveButton("Continue", (senderAlert, confArgs) => { StartActivity(intent); })
                .SetNegativeButton("Cancel", (senderAlert, confArgs) => { })
                .SetCancelable(true);
                alert.Show();
            }
            else
            {
                StartActivity(intent);
            }
        }

        void viewFeedbackBtn_Click(object sender, System.EventArgs e)
        {
            if (AppData.CheckNetwork())
                this.Activity.StartActivity(typeof(FeedbackActivity));
            else
                AndroidUtils.OfflineAlert(Activity);
        }

        void practiceButton_Click(object sender, System.EventArgs e)
        {
            if (AppData.CheckNetwork())
                this.Activity.StartActivity(typeof(WikiPaceActivity));
            else
                AndroidUtils.OfflineAlert(Activity);
        }

        private ISharedPreferences GetActivityPrefs(int id)
        {
            return Activity.GetSharedPreferences("ACT_" + id, FileCreationMode.MultiProcess);
        }

        /// <summary>
        /// An expandable list adapter which displays the available categories and the activities under them
        /// </summary>
        public class ScenarioListAdapter : BaseExpandableListAdapter
        {
            Activity context;

            public ActivityCategory[] categories;

            public ScenarioListAdapter(Activity context, int resource, ActivityCategory[] data)
            {
                this.context = context;
                this.categories = data;
            }

            public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
            {
                return null;
            }

            public override long GetChildId(int groupPosition, int childPosition)
            {
                return childPosition;
            }

            public override int GetChildrenCount(int groupPosition)
            {
                return categories[groupPosition].activities.Length;
            }

            public override Java.Lang.Object GetGroup(int groupPosition)
            {
                return null;
            }

            public override int GroupCount
            {
                get { return categories.Length; }
            }

            public override long GetGroupId(int groupPosition)
            {
                return groupPosition;
            }

            public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
            {
                View view = convertView;
                ISpeechingActivityItem scenario = categories[groupPosition].activities[childPosition];
                if (view == null)
                {
                    view = context.LayoutInflater.Inflate(Resource.Layout.MainTaskListChild, null);
                }

                view.FindViewById<TextView>(Resource.Id.tasklist_childTitle).Text = scenario.Title;
                view.FindViewById<ImageView>(Resource.Id.tasklist_childIcon).SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(scenario.Icon)));
                return view;
            }

            public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
            {
                View view = convertView;
                ActivityCategory category = categories[groupPosition];
                if (view == null)
                {
                    view = context.LayoutInflater.Inflate(Resource.Layout.MainTaskListParent, null);
                }

                view.FindViewById<TextView>(Resource.Id.tasklist_parentTitle).Text = category.title;

                if(category.icon != null) view.FindViewById<ImageView>(Resource.Id.tasklist_parentIcon).SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(category.icon)));
                return view;
            }

            public override bool HasStableIds
            {
                get { return false; }
            }

            public override bool IsChildSelectable(int groupPosition, int childPosition)
            {
                return true;
            }
        }
    }
}