using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Support.V4.Widget;
using Android.Support.V4.App;
using Android.Views;
using Droid_PeopleWithParkinsons.Shared;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Speeching", MainLauncher = true, Icon = "@drawable/Icon")]
    public class MainActivity : Activity
    {
        private DrawerLayout drawer;
        private ActionBarDrawerToggle drawerToggle;
        private ListView drawerList;
        private GridView mainList;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            string[] fakeOptions = new string[]{ "these are", "some fake", "options to fill", "up sidebar space"};

            // Set up drawer
            drawer = FindViewById<DrawerLayout>(Resource.Id.mainMenuDrawerLayout);
            drawerList = FindViewById<ListView>(Resource.Id.mainDrawerList);
            drawerList.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, fakeOptions);
            drawerToggle = new ActionBarDrawerToggle(this, drawer, Resource.Drawable.ic_drawer, Resource.String.drawer_open, Resource.String.drawer_close);
            drawer.SetDrawerListener(drawerToggle);
            drawerList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                Toast.MakeText(this, "Selected: " + args.Position, ToastLength.Short).Show();
            };

            ActionBar.Show();
            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            UserTask[] sampleTasks = new UserTask[12];

            for (int i = 0; i < sampleTasks.Length; i++ )
            {
                sampleTasks[i] = new UserTask();
                sampleTasks[i].title = "Task " + i;
            }

            mainList = FindViewById<GridView>(Resource.Id.mainActivitiesList);
            mainList.Adapter = new UserTaskListAdapter(this, Resource.Id.mainActivitiesList, sampleTasks);
            mainList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                StartActivity(typeof(RecordSoundRunActivity));
            };

            Intent uploadServiceIntent = new Intent(this, typeof(UploadService));
            StartService(uploadServiceIntent);

            AudioFileManager.DeleteAllTemp();

            if (SentenceManager.sentences.Count > 0)
            {
                if (SentenceManager.sentences.Count <= SentenceManager.MIN_STORED_SENTENCES)
                {
                    Intent downloadServiceIntent = new Intent(this, typeof(DownloadService));
                    StartService(downloadServiceIntent);
                }
            }
            else
            {
                Intent downloadServiceIntent = new Intent(this, typeof(DownloadService));
                StartService(downloadServiceIntent);
            }
        }

        #region DrawerLayout management
        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            drawerToggle.SyncState();
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            drawerToggle.OnConfigurationChanged(newConfig);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (this.drawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
        #endregion

    }

    public class UserTaskListAdapter : BaseAdapter<UserTask>
    {
        Activity context;
        UserTask[] tasks;

        public UserTaskListAdapter(Activity context, int resource, UserTask[] data)
        {
            this.context = context;
            this.tasks = data;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override UserTask this[int position]
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
            return view;
        }
    }

   
}

