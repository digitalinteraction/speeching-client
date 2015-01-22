using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Speeching", MainLauncher = true, Icon = "@drawable/Icon")]
    public class MainActivity : Activity
    {
        public UploadServiceConnection uploadServiceConnection;
        public UploadService.UploadServiceBinder binder;
        public bool isBound = false;

        private Button recordButton;
        private Button analyseButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            Intent.SetFlags(ActivityFlags.ReorderToFront);

            // Get and assign buttons

            recordButton = FindViewById<Button>(Resource.Id.RecordBtn);
            recordButton.Click += delegate {
                Intent recordMenu = new Intent(this, typeof(RecordSoundRunActivity));
                recordMenu.PutExtra("text", PlaceholderStrings.GetRandomSentence());
                StartActivity(recordMenu);
            };

            // Not yet implemented
            analyseButton = FindViewById<Button>(Resource.Id.AnalyseBtn);
            analyseButton.Enabled = false;

            Intent uploadServiceIntent = new Intent(this, typeof(UploadService));
            StartService(uploadServiceIntent);

            uploadServiceConnection = new UploadServiceConnection(this);
            BindService(uploadServiceIntent, uploadServiceConnection, Bind.AutoCreate);

            AudioFileManager.DeleteAllTemp();
        }


        /// <summary>
        /// Checks for any unuploaded files and adds them to the upload queue.
        /// Useful on first-start to make sure the user doesn't leave any files hanging around
        /// Unbinds from the Upload Service when finished adding any files to queue.
        /// Displays toast message if files were added.
        /// </summary>
        public void OnBoundToService()
        {
            List<string> list = AudioFileManager.GetAllFiles(false);

            if (list.Count > 0)
            {
                bool makeToast = false;

                foreach (string item in list)
                {
                    bool success = binder.GetUploadService().AddFile(item);

                    makeToast = success ? success : makeToast;
                }

                if (makeToast)
                {
                    Toast.MakeText(this, "Added item(s) to upload queue", ToastLength.Short).Show();
                }

                list.Clear();
                list = null;
            }

            UnbindService(uploadServiceConnection);
        }


        /// <summary>
        /// Handles connection to allow class to bind to the UploadService.
        /// Calls Activity.OnBoundToService() when successfully bound.
        /// </summary>
        public class UploadServiceConnection : Java.Lang.Object, IServiceConnection
        {
            MainActivity activity;

            public UploadServiceConnection(MainActivity activity)
            {
                this.activity = activity;
            }

            public void OnServiceConnected(ComponentName name, IBinder service)
            {
                var demoServiceBinder = service as UploadService.UploadServiceBinder;
                if (demoServiceBinder != null)
                {
                    activity.binder = demoServiceBinder;
                    activity.isBound = true;
                    activity.OnBoundToService();
                }
            }

            public void OnServiceDisconnected(ComponentName name)
            {
                activity.isBound = false;
            }
        }
    }

   
}

