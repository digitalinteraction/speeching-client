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
using Android.Support.V4.View;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Speeching", Icon = "@drawable/Icon")]
    public class MainActivity : FragmentActivity
    {
        private DrawerLayout drawer;
        private ActionBarDrawerToggle drawerToggle;
        private ListView drawerList;
        private ViewPager pager;
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Register for tabs
            SetContentView(Resource.Layout.Main);

            pager = FindViewById<ViewPager>(Resource.Id.fragmentContainer);
            var adaptor = new AndroidUtils.PagerAdapter(SupportFragmentManager);
            pager.Adapter = adaptor;

            string[] fakeOptions = new string[]{ "these are", "some fake", "options to fill", "up sidebar space"};

            // Set up drawer
            drawer = FindViewById<DrawerLayout>(Resource.Id.mainMenuDrawerLayout);
            drawerList = FindViewById<ListView>(Resource.Id.mainDrawerList);
            drawerList.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, fakeOptions);
            drawerToggle = new ActionBarDrawerToggle(this, drawer, Resource.Drawable.ic_drawer, Resource.String.drawer_open, Resource.String.drawer_close);
            drawer.SetDrawerListener(drawerToggle);
            drawerList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                Toast.MakeText(this, "Option selected!", ToastLength.Short).Show();
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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.mainActivityActions, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if(item.ItemId == Resource.Id.action_uploads)
            {
                StartActivity(typeof(UploadsActivity));
            }
            return base.OnOptionsItemSelected(item);
        }

        // Save the selected tab
        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt("selected_tab", this.ActionBar.SelectedNavigationIndex);
            base.OnSaveInstanceState(outState);
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
        #endregion

    }
}

