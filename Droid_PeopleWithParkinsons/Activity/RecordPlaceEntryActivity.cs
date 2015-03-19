using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SpeechingCommon;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Make a new recording")]
    public class RecordPlaceEntryActivity : Activity
    {
        string placeId;
        string placeName;
        string imageLoc;

        Button recordButton;
        AndroidUtils.RecordAudioManager audioManager;
        bool recording = false;
        string recordTo;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            placeId = Intent.GetStringExtra("PlaceID");
            placeName = Intent.GetStringExtra("PlaceName");
            imageLoc = Intent.GetStringExtra("PlaceImage");

            SetContentView(Resource.Layout.PlacesRecordEntry);

            recordTo = System.IO.Path.Combine(AppData.cacheDir + AppData.placesCache, placeId + ".mp4");

            if(imageLoc != null)
            {
                // Load the location's photo if there is one (already downloaded)
                FindViewById<ImageView>(Resource.Id.placesRecord_photo).SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(imageLoc)));
            }
            else
            {
                // TODO do something which is less disappointing, because the image does look pretty nice
                FindViewById<ImageView>(Resource.Id.placesRecord_photo).Visibility = ViewStates.Gone;
            }
            FindViewById<TextView>(Resource.Id.placesRecord_title).Text = "Create an entry about " + placeName;

            recordButton = FindViewById<Button>(Resource.Id.placesRecord_button);
            recordButton.Click += recordButton_Click;
        }

        void recordButton_Click(object sender, EventArgs e)
        {
            if(!recording)
            {
                recordButton.Text = "Stop recording!";
                audioManager.StartRecording(recordTo);
                recordButton.SetBackgroundResource(Resource.Drawable.recordButtonRed);
            }
            else
            {
                recordButton.Text = "Recording complete!";
                audioManager.StopRecording();
                recordButton.SetBackgroundResource(Resource.Drawable.recordButtonBlue);

                AlertDialog alert = new AlertDialog.Builder(this)
                    .SetTitle("Recording complete!")
                    .SetMessage("You completed a new voice entry about " + placeName + ". Would you like to try again or export this recording?")
                    .SetNegativeButton("Restart", (EventHandler<DialogClickEventArgs>)null)
                    .SetPositiveButton("Export", (s, args) => { /* TODO */ })
                    .Create();

                alert.Show();

                // A second alert dialogue, confirming the decision to restart
                Button negative = alert.GetButton((int)DialogButtonType.Negative);
                negative.Click += delegate(object s, EventArgs ev)
                {
                    AlertDialog.Builder confirm = new AlertDialog.Builder(this);
                    confirm.SetTitle("Are you sure?");
                    confirm.SetMessage("Restarting will wipe your current progress. Restart the scenario?");
                    confirm.SetPositiveButton("Restart", (senderAlert, confArgs) =>
                    {
                        recordButton.Text = "Start recording!";
                        alert.Dismiss();
                    });
                    confirm.SetNegativeButton("Cancel", (senderAlert, confArgs) => { });
                    confirm.Show();
                };      
            }

            recording = !recording;
        }

        protected override void OnResume()
        {
            base.OnResume();
            audioManager = new AndroidUtils.RecordAudioManager(this, null);
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (audioManager != null)
            {
                audioManager.CleanUp();
                audioManager = null;
            }
        }
    }
}