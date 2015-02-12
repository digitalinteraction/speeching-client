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
    [Activity(Label = "Speeching", Icon = "@drawable/Icon")]
    public class MainActivity : Activity
    {
        private DrawerLayout drawer;
        private ActionBarDrawerToggle drawerToggle;
        private ListView drawerList;
        private Bundle currentBundle;

        bool hasFragment;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Register for tabs
            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

            SetContentView(Resource.Layout.Main);

            // Do actionbar tab setup - Each tab is a fragment
            Android.App.Fragment tasks = new TaskListFragment();
            Android.App.Fragment friends = new FriendListFragment();
            
            AndroidUtils.AddTab("Tasks", currentBundle, this, Resource.Id.fragmentContainer, tasks);
            AndroidUtils.AddTab("Friends", currentBundle, this, Resource.Id.fragmentContainer, friends);     

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

        /// <summary>
        /// Generic method to load any new or replacing fragment into Resource.Id.RecordSoundRunFragment
        /// </summary>
        /// <typeparam name="T">Fragment to load</typeparam>
        /// <param name="args">Bundle will set T.Arguments param</param>
        /// <returns></returns>
        private Android.App.Fragment LoadFragment<T>(T _fragment, Bundle args, string tag) where T : Android.App.Fragment, new()
        {
            var newFragment = new T();

            newFragment.Arguments = args;

            var ft = FragmentManager.BeginTransaction();

            Android.App.Fragment _frag = FragmentManager.FindFragmentByTag(tag);

            if (_frag != null)
            {
                ft.Detach(_frag);
            }

            if (!hasFragment)
            {
                ft.Add(Resource.Id.RecordSoundRunFragment, newFragment, tag);
                hasFragment = true;
            }
            else
            {
                ft.Replace(Resource.Id.RecordSoundRunFragment, newFragment, tag);
            }

            ft.Commit();

            return newFragment;
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
}

