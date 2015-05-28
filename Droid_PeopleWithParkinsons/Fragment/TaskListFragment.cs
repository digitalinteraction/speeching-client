using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using SpeechingShared;
using System;

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
            mainList.SetAdapter(new ScenarioListAdapter(Activity, Resource.Id.mainActivitiesList, AppData.Session.categories.ToArray()));
            mainList.ChildClick += mainList_ChildClick;  

            // When the pull to refresh is activated, pull new data from the server and refresh the list with the new data
            refresher = view.FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);
            refresher.Refresh += async delegate
            {
                if (!AndroidUtils.IsConnected())
                {
                    AndroidUtils.OfflineAlert(Activity);
                    refresher.Refreshing = false;
                    return;
                }

                await ServerData.FetchCategories();
                refresher.Refreshing = false;

                ((ScenarioListAdapter)mainList.ExpandableListAdapter).categories = AppData.Session.categories.ToArray();
                Activity.RunOnUiThread(() => ((ScenarioListAdapter)mainList.ExpandableListAdapter).NotifyDataSetChanged());
            };

            // If there's only one category, it makes sense to expand it by default
            if (AppData.Session.categories.Count == 1)
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
        /// A child in the list (an practiceActivity) has been selected. Check to see if existing data for this practiceActivity
        /// already exists before launching it.
        /// </summary>
        void mainList_ChildClick(object sender, ExpandableListView.ChildClickEventArgs e)
        {
            ISpeechingPracticeActivity @this = AppData.Session.categories[e.GroupPosition].Activities[e.ChildPosition];

            Type objectType = @this.GetType();
            Type targetActivity = typeof(MainActivity);

            if (objectType == typeof(Scenario)) targetActivity = typeof(ScenarioActivity);
            else if (objectType == typeof(Guide)) targetActivity = typeof(GuideActivity);
            else if (objectType == typeof (Assessment)) targetActivity = typeof (AssessmentActivity);

            Intent intent = new Intent(Activity, targetActivity);
            int itemId = @this.Id;
            intent.PutExtra("ActivityId", itemId);

            if (!AndroidUtils.IsConnected() && !AndroidUtils.IsActivityAvailableOffline(@this.Id, Activity))
            {
                AndroidUtils.OfflineAlert(Activity, "This practiceActivity has not been downloaded yet and requires an Internet connection to prepare!");
                return;
            }

            if (AppData.CheckForActivityResultData(itemId))
            {
                Android.Support.V7.App.AlertDialog.Builder alert = new Android.Support.V7.App.AlertDialog.Builder(Activity)
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
            if (AndroidUtils.IsConnected())
                this.Activity.StartActivity(typeof(AssessmentActivity)); //FeedbackActivity));
            else
                AndroidUtils.OfflineAlert(Activity);
        }

        void practiceButton_Click(object sender, System.EventArgs e)
        {
            if (AndroidUtils.IsConnected())
                this.Activity.StartActivity(typeof(WikiPracticeActivity));
            else
                AndroidUtils.OfflineAlert(Activity);
        }

        private ISharedPreferences GetActivityPrefs(int id)
        {
            return Activity.GetSharedPreferences("ACT_" + id, FileCreationMode.MultiProcess);
        }

        /// <summary>
        /// An expandable list adapter which displays the available categories and the Activities under them
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
                return categories[groupPosition].Activities.Length;
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
                ISpeechingPracticeActivity scenario = categories[groupPosition].Activities[childPosition];
                if (view == null)
                {
                    view = context.LayoutInflater.Inflate(Resource.Layout.MainTaskListChild, null);
                }

                view.FindViewById<TextView>(Resource.Id.tasklist_childTitle).Text = scenario.Title;

                AndroidUtils.PrepareIcon(view.FindViewById<ImageView>(Resource.Id.tasklist_childIcon), scenario);

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

                view.FindViewById<TextView>(Resource.Id.tasklist_parentTitle).Text = category.Title;

                AndroidUtils.PrepareIcon(view.FindViewById<ImageView>(Resource.Id.tasklist_parentIcon), category);

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