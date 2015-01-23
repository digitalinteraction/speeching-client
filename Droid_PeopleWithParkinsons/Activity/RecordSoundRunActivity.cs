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
    public class RecordSoundRunActivity : Activity, RecordSoundFragment.IOnFinishedRecordingListener, RecordCompletedFragment.IOnFinishedPlaybackListener
    {
        public bool isBound = false;
        public UploadService.UploadServiceBinder binder;
        public UploadServiceConnection uploadServiceConnection;

        private bool isRecordFragment = true;
        private Bundle currentBundle;
        private bool hasFragment;

        private string serviceParameter;


        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
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
                if (currentBundle == null)
                {
                    currentBundle = Intent.Extras;
                }

                if (isRecordFragment)
                {
                    LoadFragment(new RecordSoundFragment(), currentBundle, "RecordSoundFragment");
                }
                else
                {
                    LoadFragment(new RecordCompletedFragment(), currentBundle, "RecordCompletedFragment");
                }
            };

            ActionBar.AddTab(tab);

            tab = ActionBar.NewTab();
            tab.SetText("Results");
            //tab.SetIcon(Resource.Drawable.tab1_icon);
            tab.TabSelected += (sender, args) =>
            {
                Bundle arguments = Intent.Extras;
                LoadFragment(new PlaceholderFragment(), arguments, "PlaceholderFragment");
            };

            ActionBar.AddTab(tab);
        }


        /// <summary>
        /// Clears up our references. Unbinds from upload service.
        /// </summary>
        protected override void OnPause()
        {
            base.OnPause();

            if (isBound)
            {
                UnbindService(uploadServiceConnection);
            }
        }


        /// <summary>
        /// Inflates our action bar menu for this screen.
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.RecordMenu, menu);
            return base.OnPrepareOptionsMenu(menu);
        }


        /// <summary>
        /// Handles case scenario of pressing 'help' button on the action bar.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Adds the most recent recorded item filepath to the upload service queue.
        /// </summary>
        public void OnBoundToService()
        {
            binder.GetUploadService().AddFile(serviceParameter);
            Toast.MakeText(this, "Added sound to upload queue.", ToastLength.Short).Show();

            Intent mainMenu = new Intent(this, typeof(MainActivity));
            StartActivity(mainMenu);
        }


        /// <summary>
        /// Event listener for used fragment.
        /// </summary>
        public void OnFinishedPlaybackListener(string filepath)
        {
            serviceParameter = filepath;
            isRecordFragment = true;
            currentBundle = null;

            // Starts a service (Ensures it carries on after activity ends)
            Intent uploadServiceIntent = new Intent(this, typeof(UploadService));
            StartService(uploadServiceIntent);

            uploadServiceConnection = new UploadServiceConnection(this);
            BindService(uploadServiceIntent, uploadServiceConnection, Bind.AutoCreate);
        }


        /// <summary>
        /// Event listener for used fragment.
        /// </summary>
        /// <param name="filepath">Filepath to recorded audio</param>
        public void OnFinishedRecordingListener(string filepath)
        {
            currentBundle.PutString("filepath", filepath);

            LoadFragment(new RecordCompletedFragment(), currentBundle, "RecordCompletedFragment");
            isRecordFragment = false;
        }


        /// <summary>
        /// Generic method to load any new or replacing fragment into Resource.Id.RecordSoundRunFragment
        /// </summary>
        /// <typeparam name="T">Fragment to load</typeparam>
        /// <param name="args">Bundle will set T.Arguments param</param>
        /// <returns></returns>
        private Fragment LoadFragment<T>(T _fragment, Bundle args, string tag) where T : Fragment, new()
        {
            var newFragment = new T();

            newFragment.Arguments = args;

            var ft = FragmentManager.BeginTransaction();

            Fragment _frag = FragmentManager.FindFragmentByTag(tag);

            if (_frag != null)
            {
                ft.Detach(_frag);
            }

            if (!hasFragment)
            {
                ft.Add(Resource.Id.RecordSoundRunFragment, newFragment, tag);
                hasFragment = true;
            }
            else
            {
                ft.Replace(Resource.Id.RecordSoundRunFragment, newFragment, tag);
            }

            ft.Commit();

            return newFragment;
        }


        /// <summary>
        /// Class for handling a connection to the Upload Service.
        /// Calls Activity.OnBoundToService() when successfully bound.
        /// </summary>
        public class UploadServiceConnection : Java.Lang.Object, IServiceConnection
        {
            RecordSoundRunActivity activity;

            public UploadServiceConnection(RecordSoundRunActivity activity)
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
