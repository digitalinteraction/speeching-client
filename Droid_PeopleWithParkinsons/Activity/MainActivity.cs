using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using com.refractored;
using SpeechingShared;

namespace DroidSpeeching
{
    [Activity(Label = "Speeching", Icon = "@drawable/Icon", LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
    public class MainActivity : ActionBarActivity
    {
        private MyPagerAdapter adapter;
        private PagerSlidingTabStrip tabs;
        private RelativeLayout offlineNotice;
        private ViewPager pager;
        private ISharedPreferences prefs;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);

            base.OnCreate(bundle);

            // Register for tabs
            SetContentView(Resource.Layout.Main);

            tabs = FindViewById<PagerSlidingTabStrip>(Resource.Id.tabs);
            pager = FindViewById<ViewPager>(Resource.Id.viewPager);
            offlineNotice = FindViewById<RelativeLayout>(Resource.Id.offlineWarning);

            int pageMargin = (int) TypedValue.ApplyDimension(ComplexUnitType.Dip, 4, Resources.DisplayMetrics);
            pager.PageMargin = pageMargin;
            InitAdapter();

            AndroidUtils.mainActivity = this;
            AndroidUtils.IsConnected();

            SupportActionBar.Show();

            CheckForFirstTime();
        }

        private async void CheckForFirstTime()
        {
            if (!GetPrefs().GetBoolean("FIRSTTIME", true)) return;

            try
            {
                ActivityHelp help = await ServerData.FetchHelp(ServerData.TaskType.None) as ActivityHelp;

                if (help == null) return;

                VideoPlayerFragment helpVidFragment = new VideoPlayerFragment(help.HelpVideo, help.ActivityName, help.ActivityDescription);
                helpVidFragment.Show(SupportFragmentManager, "video_helper");

                if (!string.IsNullOrWhiteSpace(help.HelpVideo))
                {
                    helpVidFragment.StartVideo();
                }
                    
                ISharedPreferencesEditor editor = GetPrefs().Edit();
                editor.PutBoolean("FIRSTTIME", false);
                editor.Apply();
            }
            catch (Exception except)
            {
                ISharedPreferencesEditor editor = GetPrefs().Edit();
                editor.PutBoolean("FIRSTTIME", true);
                editor.Apply();
            }
        }

        private ISharedPreferences GetPrefs()
        {
            if (prefs == null)
            {
                prefs = GetSharedPreferences("APP", FileCreationMode.MultiProcess);
            }

            return prefs;
        }

        public void ShowOfflineWarning(bool show)
        {
            if (offlineNotice == null) return;

            offlineNotice.Visibility = (show) ? ViewStates.Visible : ViewStates.Gone;
        }

        private void InitAdapter()
        {
            pager.Adapter = null;
            var oldAdapter = adapter;
            adapter = new MyPagerAdapter(SupportFragmentManager);
            pager.Adapter = adapter;
            tabs.SetViewPager(pager);

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
            if (item.ItemId == Resource.Id.action_uploads)
            {
                StartActivity(typeof (UploadsActivity));
                return true;
            }
            if (item.ItemId == Resource.Id.action_settings)
            {
                StartActivity(typeof (SettingsActivity));
                return true;
            }
            if (item.ItemId == Resource.Id.action_logOut)
            {
                Android.Support.V7.App.AlertDialog alert = new Android.Support.V7.App.AlertDialog.Builder(this)
                    .SetTitle("Sign out?")
                    .SetMessage("This will erase your current session data!")
                    .SetPositiveButton("Confirm", (arg1, arg2) =>
                    {
                        AppData.Session = null;
                        AppData.SaveCurrentData();

                        Intent intent = new Intent(this, typeof (LoginActivity));
                        intent.PutExtra("signOut", true);
                        StartActivity(intent);

                        Finish();
                    })
                    .SetNegativeButton("Cancel", (arg1, arg2) => { })
                    .SetCancelable(true)
                    .Create();
                alert.Show();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        // Save the selected tab
        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt("selected_tab", SupportActionBar.SelectedNavigationIndex);
            base.OnSaveInstanceState(outState);
        }

        public class MyPagerAdapter : FragmentPagerAdapter
        {
            protected internal static readonly string[] Titles = {"Home", "Practice"};
            protected internal static readonly string[] Titles2 = Titles.Select(s => s + " (Alt)").ToArray();
            private readonly string[] titles;
            private int count;

            public MyPagerAdapter(Android.Support.V4.App.FragmentManager supportFragmentManager)
                : base(supportFragmentManager)
            {
                count = Titles.Length;
                titles = new string[Titles.Length];
                Array.Copy(Titles, titles, Titles.Length);
            }

            public override int Count
            {
                get { return count; }
            }

            public override Android.Support.V4.App.Fragment GetItem(int position)
            {
                switch (position)
                {
                    case 0:
                        return new FeedFragment();
                    case 1:
                        return new TaskListFragment();
                    default:
                        return null;
                }
            }

            public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
            {
                return new Java.Lang.String(titles[position]);
            }

            public void SetCount(int newCount)
            {
                if (newCount < 0 || newCount > Titles.Length)
                    return;

                count = newCount;
                NotifyDataSetChanged();
            }

            public virtual void ChangeTitle(int position)
            {
                if (titles[position] == Titles[position])
                {
                    titles[position] = Titles2[position];
                }
                else
                {
                    titles[position] = Titles[position];
                }
                NotifyDataSetChanged();
            }
        }
    }
}