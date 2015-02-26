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
        private GridView mainList;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.MainTaskListFragment, container, false);

            mainList = view.FindViewById<GridView>(Resource.Id.mainActivitiesList);
            mainList.Adapter = new ScenarioListAdapter(Activity, Resource.Id.mainActivitiesList, AppData.session.scenarios.ToArray());
            mainList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                Intent intent = new Intent(Activity, typeof(ScenarioActivity));
                intent.PutExtra("ScenarioId", AppData.session.scenarios[args.Position].id);

                if(AppData.CheckIfScenarioCompleted(AppData.session.scenarios[args.Position].id))
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

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            // Make sure we're showing the latest available scenarios
            mainList.Adapter = new ScenarioListAdapter(Activity, Resource.Id.mainActivitiesList, AppData.session.scenarios.ToArray());
        }
    
        public class ScenarioListAdapter : BaseAdapter<Scenario>
        {
            Activity context;
            Scenario[] tasks;

            /// <summary>
            /// An adapter to be able to display the details on each task in a grid or list
            /// </summary>
            public ScenarioListAdapter(Activity context, int resource, Scenario[] data)
            {
                this.context = context;
                this.tasks = data;
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override Scenario this[int position]
            {
                get { return tasks[position]; }
            }

            public override int Count
            {
                get { return tasks.Length; }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View view = convertView;

                if (view == null)
                {
                    view = context.LayoutInflater.Inflate(Resource.Layout.MainMenuListItem, null);
                }

                view.FindViewById<TextView>(Resource.Id.mainListActivityTitle).Text = tasks[position].title;
                view.FindViewById<ImageView>(Resource.Id.mainListActivityIcon).SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(tasks[position].icon)));
                return view;
            }
        }
    }
}