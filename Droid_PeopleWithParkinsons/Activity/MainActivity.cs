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
    public class MainActivity : Activity, GestureDetector.IOnGestureListener
    {
        private DrawerLayout drawer;
        private ActionBarDrawerToggle drawerToggle;
        private ListView drawerList;
        private Bundle currentBundle;
        private GestureDetector gestureDetector;

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

            gestureDetector = new GestureDetector(this);
        }

        private void GestureLeft()
        {
            if (ActionBar.SelectedTab.Position - 1 < 0)
            {
                ActionBar.SelectTab(ActionBar.GetTabAt(ActionBar.TabCount - 1));
            }
            else
            {
                ActionBar.SelectTab(ActionBar.GetTabAt(ActionBar.SelectedTab.Position - 1));
            }
        }

        private void GestureRight()
        {
            if (ActionBar.SelectedTab.Position + 1 >= ActionBar.TabCount) 
            {
                ActionBar.SelectTab(ActionBar.GetTabAt(0));
            }
            else
            {
                ActionBar.SelectTab(ActionBar.GetTabAt(ActionBar.SelectedTab.Position + 1));
            }
        }

        #region Gestures
        private int SWIPE_MAX_OFF_PATH = 250;
        private int SWIPE_MIN_DISTANCE = 120;
        private int SWIPE_THRESHOLD_VELOCITY = 200;
        public bool OnDown(MotionEvent e)
        {
            return true;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            try
            {
                if (Math.Abs(e1.GetY() - e2.GetY()) > SWIPE_MAX_OFF_PATH)
                    return false;
                // right to left swipe
                if (e1.GetX() - e2.GetX() > SWIPE_MIN_DISTANCE && Math.Abs(velocityX) > SWIPE_THRESHOLD_VELOCITY)
                    GestureLeft();
                else if (e2.GetX() - e1.GetX() > SWIPE_MIN_DISTANCE && Math.Abs(velocityX) > SWIPE_THRESHOLD_VELOCITY)
                    GestureRight();
            }
            catch (Exception e)
            {
                // nothing
            }
            return false;
        }

        public void OnLongPress(MotionEvent e)
        {
        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return true;
        }

        public void OnShowPress(MotionEvent e)
        {
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            return true;
        }

        #endregion

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

