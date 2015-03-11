using System;
using System.Collections.Generic;
using System.Text;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;
using Android.Views;
using Android.Content.PM;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Speeching")]
    public class RecordSoundRunActivity : Activity, RecordSoundFragment.IOnFinishedRecordingListener, RecordCompletedFragment.IOnFinishedPlaybackListener
    {
        private bool isRecordFragment = true;
        private Bundle currentBundle;
        private bool hasFragment;

        private string serviceParameter;
        private string sentence;


        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
        }


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            // Register for tabs
            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

            SetContentView(Resource.Layout.RecordSoundRun);

            // Do tab setup - Each tab is a fragment
            ActionBar.Tab tab = ActionBar.NewTab();
            tab.SetText("Record");
            //tab.SetIcon(Resource.Drawable.tab1_icon);
            tab.TabSelected += (sender, args) =>
            {
                if (currentBundle == null)
                {
                    currentBundle = Intent.Extras == null ? new Bundle() : Intent.Extras;
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
                //LoadFragment(new PlaceholderFragment(), arguments, "PlaceholderFragment");
            };

            ActionBar.AddTab(tab);
        }


        /// <summary>
        /// Clears up our references. Unbinds from upload service.
        /// </summary>
        protected override void OnPause()
        {
            base.OnPause();
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
        /// Event listener for used fragment.
        /// </summary>
        public void OnFinishedPlaybackListener(string filepath)
        {
            serviceParameter = filepath;
            isRecordFragment = true;
            currentBundle = null;

            SentenceModel model = new SentenceModel();

            model.path = serviceParameter;
            model.sentence = sentence;

            //ModelManager.AddModel(model);

            //uploadServiceBinder.GetUploadService().AddFile(serviceParameter, sentence);
            Toast.MakeText(this, "Added sound to upload queue.", ToastLength.Short).Show();

            SentenceManager.DeleteQuestion(sentence);

            // Starts a service (Ensures it carries on after activity ends)
            //Intent uploadServiceIntent = new Intent(this, typeof(UploadService));
            //StartService(uploadServiceIntent);

            Intent mainMenu = new Intent(this, typeof(MainActivity));
            StartActivity(mainMenu);           
        }


        /// <summary>
        /// Event listener for used fragment.
        /// </summary>
        /// <param name="filepath">Filepath to recorded audio</param>
        public void OnFinishedRecordingListener(string filepath, string _sentence)
        {
            sentence = _sentence;
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
    }    
}
