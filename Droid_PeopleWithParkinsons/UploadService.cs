using System;
using System.Collections.Generic;
using System.Text;

using Android.OS;
using Android.App;
using Android.Widget;
using Android.Content;
using Android.Media;
using Android.Views;
using Android.Graphics.Drawables;
using Android.Views.Animations;
using Android.Graphics;

using System.Threading;

namespace Droid_PeopleWithParkinsons
{
    [Service]
    public class UploadService : Android.App.Service
    {
        // TODO: If uploading files, create a toast to inform the user this is happening.
        private UploadServiceBinder binder;
        private bool isRunning = false;

        private List<string> filesToUpload = new List<string>();

        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            if (isRunning == false)
            {
                isRunning = true;

                new Thread(new ThreadStart(() =>
                {
                    Thread.Sleep(5 * 1000);

                    while (filesToUpload.Count > 0)
                    {
                        // TODO: Get index 0 and check if it exists before performing operation.
                        // Complete via rework of actual upload process.
                        SendNotification(filesToUpload[0]);
                        string fPath = filesToUpload[0];
                        AudioFileManager.DeleteFile(fPath);
                        filesToUpload.RemoveAt(0);
                        Thread.Sleep(5 * 1000);
                    }

                    StopSelf();

                })).Start();
            }

            // Else we are already running

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        
        public override IBinder OnBind(Intent intent)
        {
            binder = new UploadServiceBinder(this);
            return binder;
        }


        void SendNotification(string message)
        {
            var nMgr = (NotificationManager) GetSystemService(NotificationService);
            var notification = new Notification(Resource.Drawable.Icon, "Message from service");
            var pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(RecordSoundRunActivity)), 0);
            // TODO: Use non obsolete (Whatever that is. This needs heavily reworking anyway).
            notification.SetLatestEventInfo(this, message, message, null);
            nMgr.Notify(0, notification);            
        }

        public void AddFile(string filePath)
        {
            if (filesToUpload.Contains(filePath))
            {
                return;
            }
            else
            {
                filesToUpload.Add(filePath);
            }
        }

        public class UploadServiceBinder : Binder
        {
            UploadService service;

            public UploadServiceBinder(UploadService service)
            {
                this.service = service;
            }

            public UploadService GetUploadService()
            {
                return service;
            }
        }



    }
}