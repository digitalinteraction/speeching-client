using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Widget;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Speeching", Icon = "@drawable/Icon")]
    public class MainActivity : FragmentActivity
    {
        private DrawerLayout drawer;
        private ActionBarDrawerToggle drawerToggle;
        private ListView drawerList;
        private ViewPager pager;
        private PagerSlidingTabStrip.PagerSlidingTabStrip _tabs;
        private MyPagerAdapter _adapter;
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Register for tabs
            SetContentView(Resource.Layout.Main);

            _tabs = FindViewById<PagerSlidingTabStrip.PagerSlidingTabStrip>(Resource.Id.tabs);
            pager = FindViewById<ViewPager>(Resource.Id.viewPager);

            int pageMargin = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 4, Resources.DisplayMetrics);
            pager.PageMargin = pageMargin;
            InitAdapter();

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

        private void InitAdapter()
        {
            pager.Adapter = null;
            var oldAdapter = _adapter;
            _adapter = new MyPagerAdapter(SupportFragmentManager);
            pager.Adapter = _adapter;
            _tabs.SetViewPager(pager);
            //have to dispose it after we've set the view pager, otherwise an error occurs because we've dumped out
            //the Java Reference.
            if (oldAdapter != null)
            {
                oldAdapter.Dispose();
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


        public class MyPagerAdapter : FragmentPagerAdapter
        {
            private Android.Support.V4.App.FragmentManager SupportFragmentManager;

            public MyPagerAdapter(Android.Support.V4.App.FragmentManager SupportFragmentManager)
                : base(SupportFragmentManager)
            {
                // TODO: Complete member initialization
                this.SupportFragmentManager = SupportFragmentManager;
                _count =  Titles.Length;
                _titles = new string[Titles.Length];
                Array.Copy(Titles, _titles, Titles.Length);
            }

            protected internal static readonly string[] Titles = { "Scenarios", "Friends", "Submitted" };

            protected internal static readonly string[] Titles2 = Titles.Select(s => s + " (Alt)").ToArray();

            protected internal readonly string[] _titles;

            public override Android.Support.V4.App.Fragment GetItem(int position)
            {
                switch (position)
                {
                    case 0:
                        return new TaskListFragment();
                    case 1:
                        return new FriendListFragment();
                    case 2:
                        return new UploadededFragment();
                    default:
                        return null;
                }
            }

            void toReturn_ChangeTitleRequested(object sender, int e)
            {
                ChangeTitle(e);
            }

            private int _count;
            public override int Count
            {
                get { return _count; }
            }

            public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
            {
                return new Java.Lang.String(_titles[position]);
            }

            /// <summary>
            /// used to demonstrate how the control can respond to tabs being added and removed.
            /// </summary>
            /// <param name="count"></param>
            public void SetCount(int count)
            {
                if (count < 0 || count > Titles.Length)
                    return;

                _count = count;
                NotifyDataSetChanged();
            }

            public virtual void ChangeTitle(int position)
            {
                if (_titles[position] == Titles[position])
                {
                    _titles[position] = Titles2[position];
                }
                else
                {
                    _titles[position] = Titles[position];
                }
                //this one has to do it this way because 
                NotifyDataSetChanged();
            }
        } 

    }
}

