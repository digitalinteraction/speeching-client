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
        private UploadServiceBinder binder;
        private bool isRunning = false;

        private List<string> filesToUpload = new List<string>();

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
                    // Wait a little while to make sure we've added all our files
                    Thread.Sleep(5 * 1000);
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
            binder = new UploadServiceBinder(this);
            return binder;
        }


        private void ItemUploaded(string filePath)
        {
            SendNotification(filePath);
            AudioFileManager.DeleteFile(filePath);            
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
            var nMgr = (NotificationManager) GetSystemService(NotificationService);
            var notification = new Notification(Resource.Drawable.Icon, "Message from service");
            var pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(RecordSoundRunActivity)), 0);
            // TODO: Use non obsolete (Whatever that is. This needs heavily reworking anyway).
            notification.SetLatestEventInfo(this, "Upload completed", message, null);
            nMgr.Notify(0, notification);            
        }

        public bool AddFile(string filePath)
        {
            return contentUploader.AddFileToUploadQueue(filePath);
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