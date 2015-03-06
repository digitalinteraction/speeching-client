using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using SpeechingCommon;

namespace Droid_PeopleWithParkinsons
{
    /// <summary>
    /// A fragment which lists the currently available tasks in a grid layout
    /// </summary>
    public class TaskListFragment : Android.Support.V4.App.Fragment
    {
        private ExpandableListView mainList;

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
            mainList.ChildClick += delegate(object sender, ExpandableListView.ChildClickEventArgs args)
            {
                ISpeechingActivityItem thisItem = AppData.session.categories[args.GroupPosition].activities[args.ChildPosition];

                System.Type objectType = thisItem.GetType();
                System.Type targetActivity = typeof(MainActivity);
                
                if(objectType == typeof(Scenario)) targetActivity = typeof(ScenarioActivity);

                Intent intent = new Intent(Activity, targetActivity);
                string scenarioId = AppData.session.categories[args.GroupPosition].activities[args.ChildPosition].Id;
                intent.PutExtra("ActivityId", scenarioId);

                if(AppData.CheckIfScenarioCompleted(scenarioId))
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
            };

            if(AppData.session.categories.Count == 1)
            {
                mainList.ExpandGroup(0, true);
            }

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            // Make sure we're showing the latest available scenarios
            //mainList.Adapter = new ScenarioListAdapter(Activity, Resource.Id.mainActivitiesList, AppData.session.scenarios.ToArray());
        }
    
        public class ScenarioListAdapter : BaseExpandableListAdapter
        {
            Activity context;

            ActivityCategory[] categories;

            /// <summary>
            /// An adapter to be able to display the details on each task in an expandable list
            /// </summary>
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