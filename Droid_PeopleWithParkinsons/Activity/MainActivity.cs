using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Content.PM;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Speeching", MainLauncher = true, Icon = "@drawable/Icon", ScreenOrientation = ScreenOrientation.Portrait)]
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
                Intent recordMenu = new Intent(this, typeof(RecordSoundRunActivity));
                StartActivity(recordMenu);
            };

            // Not yet implemented
            // Probably not even going to exist
            // TODO: Probably delete this.
            analyseButton = FindViewById<Button>(Resource.Id.AnalyseBtn);
            analyseButton.Enabled = false;
            analyseButton.Alpha = 0.0f;

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
    }

   
}

