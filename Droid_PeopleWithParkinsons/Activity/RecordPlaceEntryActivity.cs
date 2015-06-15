using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Support.V7.App;
using Android.Support.V7.Graphics;
using Android.Views;
using Android.Widget;
using ICSharpCode.SharpZipLib.Zip;
using SpeechingShared;

namespace DroidSpeeching
{
    [Activity(Label = "Make a new recording", ParentActivity = typeof (LocationActivity))]
    public class RecordPlaceEntryActivity : ActionBarActivity, Palette.IPaletteAsyncListener
    {
        private AndroidUtils.RecordAudioManager audioManager;
        private string imageLoc;
        private long lat;
        private long lng;
        private string placeId;
        private string placeName;
        private string recFile;
        private string recFolder;
        private Button recordButton;
        private bool recording;
        private string resultsZipPath;
        private TextView titleText;

        public void OnGenerated(Palette palette)
        {
            Color vibrantDark = new Color(palette.GetDarkVibrantColor(Resource.Color.appMain));
            Color dullDark = new Color(palette.GetDarkMutedColor(Resource.Color.appDark));

            SupportActionBar.SetBackgroundDrawable(new ColorDrawable(vibrantDark));
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            Window.SetStatusBarColor(dullDark);
        }

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);
            base.OnCreate(bundle);

            placeId = Intent.GetStringExtra("PlaceID");
            placeName = Intent.GetStringExtra("PlaceName");
            imageLoc = Intent.GetStringExtra("PlaceImage");
            lat = Intent.GetLongExtra("PlaceLat", (long) 54.9787659);
            lng = Intent.GetLongExtra("PlaceLng", (long) -1.6140803);

            SetContentView(Resource.Layout.PlacesRecordEntry);

            recFolder = System.IO.Path.Combine(AppData.Cache.Path, placeId);

            if (!Directory.Exists(recFolder))
            {
                Directory.CreateDirectory(recFolder);
            }

            recFile = System.IO.Path.Combine(recFolder, "entry.mp4");
            resultsZipPath = System.IO.Path.Combine(AppData.Exports.Path, placeId + ".zip");

            if (imageLoc != null)
            {
                // Load the location's photo if there is one (already downloaded)
                Android.Net.Uri imageUri = Android.Net.Uri.FromFile(new Java.IO.File(imageLoc));
                Bitmap bitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, imageUri);

                ImageView headerImage = FindViewById<ImageView>(Resource.Id.placesRecord_photo);
                headerImage.SetImageBitmap(bitmap);

                Palette.GenerateAsync(bitmap, this);
            }
            else
            {
                // TODO do something which is less disappointing, because the image does look pretty nice
                FindViewById<ImageView>(Resource.Id.placesRecord_photo).Visibility = ViewStates.Gone;
            }

            titleText = FindViewById<TextView>(Resource.Id.placesRecord_title);
            titleText.Text = "Create an entry about " + placeName;

            recordButton = FindViewById<Button>(Resource.Id.placesRecord_button);
            recordButton.Click += recordButton_Click;

            recording = false;

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            if (!recording)
            {
                recordButton.Text = "Stop recording!";
                audioManager.StartRecording(recFile);
                recordButton.SetBackgroundResource(Resource.Drawable.recordButtonRed);
            }
            else
            {
                recordButton.Text = "Recording complete!";
                audioManager.StopRecording();
                recordButton.SetBackgroundResource(Resource.Drawable.recordButtonBlue);

                Android.Support.V7.App.AlertDialog alert = new Android.Support.V7.App.AlertDialog.Builder(this)
                    .SetTitle("Recording complete!")
                    .SetMessage("You completed a new voice entry about " + placeName +
                                ". Would you like to try again or export this recording?")
                    .SetNegativeButton("Restart", (EventHandler<DialogClickEventArgs>) null)
                    .SetPositiveButton("Export", (s, args) => { ExportRecordings(); })
                    .Create();

                alert.Show();

                // A second alert dialogue, confirming the decision to restart
                Button negative = alert.GetButton((int) DialogButtonType.Negative);
                negative.Click += delegate
                {
                    Android.Support.V7.App.AlertDialog.Builder confirm =
                        new Android.Support.V7.App.AlertDialog.Builder(this);
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

        /// <summary>
        /// Exports the recording into a zip and marks it as being ready to upload
        /// </summary>
        private void ExportRecordings()
        {
            // Compress exported recordings into zip (Delete existing zip first)
            File.Delete(resultsZipPath);

            try
            {
                FastZip fastZip = new FastZip();
                fastZip.CreateZip(resultsZipPath, recFolder, true, "");

                LocationRecordingResult results = new LocationRecordingResult
                {
                    CompletionDate = DateTime.Now,
                    ParticipantActivityId = 8675309,
                    ResourceUrl = resultsZipPath,
                    UploadState = Utils.UploadStage.Ready,
                    GooglePlaceId = placeId,
                    GooglePlaceName = placeName,
                    Lat = lat,
                    Lng = lng
                };

                AppData.Session.ResultsToUpload.Add(results);
                AppData.SaveCurrentData();

                Directory.Delete(recFolder, true);

                StartActivity(typeof (UploadsActivity));
                Finish();
            }
            catch (Exception except)
            {
                Console.Write(except.Message);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            audioManager = new AndroidUtils.RecordAudioManager(this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (audioManager == null) return;

            audioManager.CleanUp();
            audioManager = null;
        }
    }
}