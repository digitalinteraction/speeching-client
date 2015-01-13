using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Media;
using Android.Content;
using Android.Views.Animations;

using Xamarin.Forms.Platform.Android;


namespace Droid_PeopleWithParkinsons
{
    // TODO: Need to add OnPause and OnResume methods to cleanup playback code.
    // TODO: Do I need to set thread priotity here for playing audio?
    [Activity(Label = "Sound Recorder")]
    class RecordCompletedActivity : Activity
    {
        private string filePath;

        private Button playbackButton;
        private Button confirmButton;

        private AudioTrack audioTrackPlayer;
        private byte[] byteData = null;
        private bool isPlaying = false;
        private bool didPlayAudio = false;        

        private Animation downAnim;
        private Animation normalAnim;        


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.RecordCompleted);

            // Load our resources
            normalAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.scale_button_normal);
            downAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.scale_button_pressed);

            // TODO: Do we need to validate this somehow? Or do we just assume that this is correct?
            // Seems like a dangerous way for Android to handle things? Maybe I'm missing something...Resort to Google.
            filePath = this.Intent.GetStringExtra("filepath");

            // Get and initiate buttons
            playbackButton = FindViewById<Button>(Resource.Id.RoundPlayButton);
            playbackButton.Click += PlayBackButtonClicked;

            confirmButton = FindViewById<Button>(Resource.Id.RecordCompletedConfirmButton);
            confirmButton.Click += ConfirmButtonClicked;
        }


        public override void OnBackPressed()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);

            alert.SetTitle("You can't go back now! You must confirm your audio.");

            alert.SetPositiveButton("OK", (senderAlert, args) =>
            {
            });

            alert.Show();
        }


        private void ConfirmButtonClicked(object sender, EventArgs e)
        {
            bool isTicked = FindViewById<CheckBox>(Resource.Id.FlagAudio).Checked;
            
            if (isTicked)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);

                alert.SetTitle("Warning: You have flagged this as bad audio. Are you sure?");

                alert.SetPositiveButton("Yes", (senderAlert, args) =>
                {
                    SubmitAudio();
                });

                alert.SetNegativeButton("No", (senderAlert, args) =>
                {
                    // Cancel so user can change choice options.
                });

                alert.Show();
            }
            else
            {
                SubmitAudio();
            }
        }

        private void SubmitAudio()
        {
            // TODO: Need to check if we have to stop the audio. Can press play, and then press submit. Naughty.

            AudioFileManager.DeleteAll();
            Intent mainMenu = new Intent(this, typeof(MainActivity));
            StartActivity(mainMenu);
        }


        private void PlayBackButtonClicked(object sender, EventArgs e)
        {
            if (!isPlaying)
            {
                if (AudioFileManager.IsExist(filePath))
                {
                    playbackButton.SetBackgroundDrawable(GetDrawable(Resource.Drawable.round_button_alt));
                    playbackButton.StartAnimation(downAnim);

                    playbackButton.Text = "Stop Sound";
                    isPlaying = true;
                    didPlayAudio = true;

                    ThreadPool.QueueUserWorkItem(o => PlayAudio());                    
                }
                else
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);

                    alert.SetTitle("Error: File does not exist");

                    alert.SetPositiveButton("OK", (senderAlert, args) =>
                    {
                    });

                    alert.Show();
                }
            }
            else
            {
                AudioFinishedPlaying();
            }
        }

        private void AudioFinishedPlaying()
        {
            playbackButton.SetBackgroundDrawable(GetDrawable(Resource.Drawable.round_button));
            playbackButton.StartAnimation(normalAnim);

            isPlaying = false;
            playbackButton.Text = "Play Sound";

            if (audioTrackPlayer != null)
            {
                audioTrackPlayer.Stop();
                audioTrackPlayer.Release();
                audioTrackPlayer.Dispose();
                audioTrackPlayer = null;
            }
        }

        private void PlayAudio()
        {
            // Only get this once - It's always the same file!
            // Woah, wait a second. It's not the same file if we're returning to the activity
            // TODO: Fix that. - filePath set method invalidates byteData?
            if (byteData == null)
            {
                GetByteData();
            }

            // Set and push to audio track..
            int intSize = AudioTrack.GetMinBufferSize(44100, ChannelOut.Mono, Android.Media.Encoding.Pcm16bit);

            audioTrackPlayer = new AudioTrack(Stream.Music, 44100, ChannelOut.Mono, Android.Media.Encoding.Pcm16bit, intSize, AudioTrackMode.Stream);

            if (audioTrackPlayer != null)
            {
                if (audioTrackPlayer != null)  audioTrackPlayer.Play();
                if (audioTrackPlayer != null)  audioTrackPlayer.Write(byteData, 0, byteData.Length);
                
                // Run cleanup on UI thread, as audio can be paused mid-track
                // Also because we need to updated UI controls, and that can only be done on the main thread.
                RunOnUiThread(() =>
                {
                    AudioFinishedPlaying();
                });
            }
        }

        private void GetByteData()
        {
            Java.IO.File file = null;
            file = new Java.IO.File(filePath);
            byteData = new byte[(int)file.Length()];
            Java.IO.FileInputStream inputStream = null;

            try
            {
                inputStream = new Java.IO.FileInputStream(file);
                inputStream.Read(byteData);
                inputStream.Close();
            }
            catch (Java.IO.FileNotFoundException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
