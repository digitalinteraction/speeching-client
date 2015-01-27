using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Android.OS;
using Android.App;
using Android.Content;

namespace Droid_PeopleWithParkinsons
{
    [Service]
    public class UploadService : Android.App.Service
    {
        private bool isRunning = false;

        private ContentUploader contentUploader;

        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            if (isRunning == false)
            {
                isRunning = true;
                
                // Instantiate our uploader object and provide callbacks
                contentUploader = new ContentUploader();
                contentUploader.itemUploadedEvent += ItemUploaded;
                contentUploader.allItemsUploadedEvent += AllFilesUploaded;
                contentUploader.operationFinishedEvent += OperationFinished;

                // Run the uploading in a new thread so we don't block UI.
                new Thread(new ThreadStart(() =>
                {
                    contentUploader.BeginUploadProcess();
                })).Start();               
            }

            // Else we are already running
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            contentUploader.itemUploadedEvent -= ItemUploaded;
            contentUploader.allItemsUploadedEvent -= AllFilesUploaded;
            contentUploader.operationFinishedEvent -= OperationFinished;
            contentUploader = null;
        }

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }


        private void ItemUploaded(string filePath)
        {
            SendNotification(filePath);
        }

        private void AllFilesUploaded()
        {
            SendNotification("All items uploaded");
        }

        private void OperationFinished()
        {
            StopSelf();
        }

        private void SendNotification(string message)
        {
            // TODO: Change this to call shared platform code notification
            // So everything is handled nicely (:


            // Keep this incase we need to target older platforms later.
            // Although we might want to use AppCompat at this point.
            /*var nMgr = (NotificationManager) GetSystemService(NotificationService);
            var notification = new Notification(Resource.Drawable.Icon, "Message from service");
            var pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(RecordSoundRunActivity)), 0);
            notification.SetLatestEventInfo(this, "Upload completed", message, null);
            nMgr.Notify(0, notification);*/

            PendingIntent pIntent = PendingIntent.GetActivity(Android.App.Application.Context.ApplicationContext, 0, new Intent(), 0);

            Notification noti = new Notification.Builder(Android.App.Application.Context)
             .SetContentTitle("Upload Completed")
             .SetContentText(message)
             .SetSmallIcon(Resource.Drawable.Icon)
             .SetContentIntent(pIntent)
             .Build();

            // Get the notification manager:
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            const int notificationId = 0;
            notificationManager.Notify(notificationId, noti);
        }
    }    
}