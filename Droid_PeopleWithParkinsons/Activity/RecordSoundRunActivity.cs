using System;
using System.Collections.Generic;
using System.Text;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;
using Android.Views;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Speeching")]
    class RecordSoundRunActivity : Activity, RecordSoundFragment.IOnFinishedRecordingListener
    {
        public void OnFinishedRecordingListener(string filepath)
        {
            // TODO: Change this to use an individual bundle for the Fragment, rather than the activity.
            // Switch fragments here.
            var newFragment = new RecordCompletedFragment();
            Intent.PutExtra("filepath", filepath);

            Bundle arguments = Intent.Extras;
            newFragment.Arguments = arguments;

            var ft = FragmentManager.BeginTransaction();

            ft.Replace(Resource.Id.RecordSoundRunFragment, newFragment);
            ft.Commit();
        }


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.RecordMenu, menu);
            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.help:
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);

                    alert.SetTitle("Help");
                    alert.SetMessage("Custom help text/images can go here.");
            
                    alert.SetPositiveButton("OK", (senderAlert, args) =>
                    {
                    });

                    alert.Show();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            // Register for tabs
            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.RecordSoundRun);

            // Do tab setup - Each tab is a fragment
            ActionBar.Tab tab = ActionBar.NewTab();
            tab.SetText("Record");
            //tab.SetIcon(Resource.Drawable.tab1_icon);
            tab.TabSelected += (sender, args) =>
            {
                // Do something when tab is selected
            };

            ActionBar.AddTab(tab);

            tab = ActionBar.NewTab();
            tab.SetText("Results");
            //tab.SetIcon(Resource.Drawable.tab1_icon);
            tab.TabSelected += (sender, args) =>
            {
                // Do something when tab is selected
            };

            ActionBar.AddTab(tab);

            var newFragment = new RecordSoundFragment();
            Bundle arguments = Intent.Extras;
            newFragment.Arguments = arguments;

            var ft = FragmentManager.BeginTransaction();
            
            ft.Add(Resource.Id.RecordSoundRunFragment, newFragment);
            ft.Commit();
        }

        public void TestFunc()
        {

        }
    }
}
