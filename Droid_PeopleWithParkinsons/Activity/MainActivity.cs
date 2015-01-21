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
            // TODO: Start and attacj to upload service
            // Add all currently stored non-uploaded files
            // Unbind from service.
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);
            
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

        public void OnBoundToService()
        {
            // TODO: Add files to the service for upload
            // Get list of files from audio file manager?
            // Iterate over and add to the list?
            // Then unbind
            List<string> list = AudioFileManager.GetAllFiles(false);

            if (list.Count > 0)
            {
                foreach (string item in list)
                {
                    binder.GetUploadService().AddFile(item);
                }

                Toast.MakeText(this, "Added item(s) to upload queue", ToastLength.Short).Show();

                list.Clear();
                list = null;
            }
        }

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

