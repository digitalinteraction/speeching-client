using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Droid_PeopleWithParkinsons", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private Button recordButton;
        private Button analyseButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            // Get and assign buttons

            recordButton = FindViewById<Button>(Resource.Id.RecordBtn);
            recordButton.Click += delegate {
                Intent recordMenu = new Intent(this, typeof(RecordTitleActivity));
                StartActivity(recordMenu);
            };

            // Not yet implemented
            analyseButton = FindViewById<Button>(Resource.Id.AnalyseBtn);
            analyseButton.Enabled = false;

        }
    }
}

