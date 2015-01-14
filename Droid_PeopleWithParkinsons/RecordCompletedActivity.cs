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
    [Activity(Label = "Sound Recorder")]
    class RecordCompletedActivity : Activity
    {
        private string _filePath;
        private string filePath { get { return _filePath; } set { _filePath = value; byteData = null;} }

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

            // Get and initiate buttons
            playbackButton = FindViewById<Button>(Resource.Id.RoundPlayButton);
            playbackButton.Click += PlayBackButtonClicked;

            confirmButton = FindViewById<Button>(Resource.Id.RecordCompletedConfirmButton);
            confirmButton.Click += ConfirmButtonClicked;
        }

        protected override void OnStart()
        {
            base.OnStart();

            // Stackoverflow recommends this format to avoid null pointer exceptions
            Intent intent = this.Intent;
            Bundle extras = intent.Extras;

            if (extras != null)
            {
                if (extras.ContainsKey("filepath"))
                {
                    filePath = extras.GetString("filepath");
                }
                else
                {
                    throw new NotImplementedException();
                    // TODO: Manage exception where there isn't valid data.
                }
            }
        }


        protected override void OnResume()
        {
            base.OnResume();
        }


        protected override void OnPause()
        {
            base.OnPause();

            if (didPlayAudio)
            {
                if (isPlaying)
                {
                    audioTrackPlayer.Stop();
                    isPlaying = false;
                }

                if (audioTrackPlayer != null)
                {
                    audioTrackPlayer.Release();
                    audioTrackPlayer.Dispose();
                    audioTrackPlayer = null;
                }

                didPlayAudio = false;                
            }
        }           


        protected override void OnRestart()
        {
            base.OnRestart();

            
        }


        /// <summary>
        /// Disables back button support. We want users to commit to the audio they have selected.
        /// </summary>
        public override void OnBackPressed()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);

            alert.SetTitle("You can't go back now! You must record and confirm your audio.");

            alert.SetPositiveButton("OK", (senderAlert, args) =>
            {
            });

            alert.Show();
        }


        /// <summary>
        /// Asks the user for confirmation if they have flagged bad audio. Otherwise instantly submits audio selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        /// <summary>
        /// When user finishes confirming audio
        /// </summary>
        private void SubmitAudio()
        {
            if (didPlayAudio)
            {
                if (isPlaying)
                {
                    audioTrackPlayer.Stop();
                    isPlaying = false;
                }

                if (audioTrackPlayer != null)
                {
                    audioTrackPlayer.Release();
                    audioTrackPlayer.Dispose();
                    audioTrackPlayer = null;
                }

                didPlayAudio = false;
            }

            AudioFileManager.DeleteAll();
            Intent mainMenu = new Intent(this, typeof(MainActivity));
            StartActivity(mainMenu);
        }


        /// <summary>
        /// Cycles through playing and stopping the selected audio at filePath
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        /// <summary>
        /// To be called when audio has finished playing via stop or reached end of track.
        /// </summary>
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


        /// <summary>
        /// Begins playback of recorded audio. Will automatically update UI thread when audio has finished playing.
        /// </summary>
        private void PlayAudio()
        {
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
                
                RunOnUiThread(() =>
                {
                    AudioFinishedPlaying();
                });
            }
        }

        /// <summary>
        /// Reads in byte data to byteData from current set filePath
        /// </summary>
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
