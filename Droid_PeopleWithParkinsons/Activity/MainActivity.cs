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
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V4.View;
using Android.Support.V4.App;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Speeching", Icon = "@drawable/Icon", LaunchMode=Android.Content.PM.LaunchMode.SingleTop)]
    public class MainActivity : ActionBarActivity //FragmentActivity
    {
        private ListView drawerList;
        private ViewPager pager;
        private PagerSlidingTabStrip.PagerSlidingTabStrip _tabs;
        private MyPagerAdapter _adapter;
        
        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);

            base.OnCreate(bundle);

            // Register for tabs
            SetContentView(Resource.Layout.Main);

            string packageName = ApplicationContext.PackageName;

            _tabs = FindViewById<PagerSlidingTabStrip.PagerSlidingTabStrip>(Resource.Id.tabs);
            pager = FindViewById<ViewPager>(Resource.Id.viewPager);

            int pageMargin = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 4, Resources.DisplayMetrics);
            pager.PageMargin = pageMargin;
            InitAdapter();

            this.SupportActionBar.Show();
        }

        private void InitAdapter()
        {
            pager.Adapter = null;
            var oldAdapter = _adapter;
            _adapter = new MyPagerAdapter(SupportFragmentManager);
            pager.Adapter = _adapter;
            _tabs.SetViewPager(pager);

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
            outState.PutInt("selected_tab", this.SupportActionBar.SelectedNavigationIndex);
            base.OnSaveInstanceState(outState);
        }


        public class MyPagerAdapter : FragmentPagerAdapter
        {
            private Android.Support.V4.App.FragmentManager SupportFragmentManager;

            public MyPagerAdapter(Android.Support.V4.App.FragmentManager SupportFragmentManager)
                : base(SupportFragmentManager)
            {
                this.SupportFragmentManager = SupportFragmentManager;
                _count =  Titles.Length;
                _titles = new string[Titles.Length];
                Array.Copy(Titles, _titles, Titles.Length);
            }

            protected internal static readonly string[] Titles = { "Practice", "Friends", "Submitted" };

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
                        return new SubmittedListFragment();
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

