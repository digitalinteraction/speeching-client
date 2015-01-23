using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Android.OS;
using Android.App;
using Android.Widget;
using Android.Content;
using Android.Media;
using Android.Views;
using Android.Graphics.Drawables;
using Android.Views.Animations;
using Android.Graphics;

namespace Droid_PeopleWithParkinsons
{
    class DownloadService : Android.App.Service
    {
        private DownloadServiceBinder binder;
        private bool isRunning = false;

        private List<string> filesToUpload = new List<string>();

        private QuestionDownloader questionDownloader;

        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            if (isRunning == false)
            {
                isRunning = true;

                // Instantiate our downloader object and provide callbacks
                questionDownloader = new QuestionDownloader();
                questionDownloader.questionsDownloadedEvent += QuestionsDownloaded;

                // Run the downloader in a new thread so we don't block UI.
                new Thread(new ThreadStart(() =>
                {
                    questionDownloader.BeginDownloadProcess();

                })).Start();
            }

            // Else we are already running
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            questionDownloader.questionsDownloadedEvent -= QuestionsDownloaded;
            questionDownloader = null;
        }


        public override IBinder OnBind(Intent intent)
        {
            binder = new DownloadServiceBinder(this);
            return binder;
        }


        private void QuestionsDownloaded()
        {
            StopSelf();
        }


        public class DownloadServiceBinder : Binder
        {
            DownloadService service;

            public DownloadServiceBinder(DownloadService service)
            {
                this.service = service;
            }

            public DownloadService GetDownloadService()
            {
                return service;
            }
        }
    }
}
