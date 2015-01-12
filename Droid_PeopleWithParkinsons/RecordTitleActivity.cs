using System;
using System.Collections.Generic;
using System.Text;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Record")]
    class RecordTitleActivity : Activity
    {
        private Button recordSoundButton;
        private Button viewResultsButton;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.RecordTitle);

            // Get and assign buttons

            recordSoundButton = FindViewById<Button>(Resource.Id.RecordSoundBtn);
            recordSoundButton.Click += delegate {
                Intent recordSound = new Intent(this, typeof(RecordSoundActivity));
                StartActivity(recordSound);
            };

            // Not yet implemented
            viewResultsButton = FindViewById<Button>(Resource.Id.ResultsBtn);
            viewResultsButton.Enabled = false;
        }
    }
}
