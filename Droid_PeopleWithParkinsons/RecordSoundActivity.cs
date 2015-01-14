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

using System.Threading;

namespace Droid_PeopleWithParkinsons
{
    // TODO: Save audio to a temp path. Move audio when finished recording. Clear temp path.
    // That way we can always use the same temp file path name.
    // When we confirm our audio, we then move the audio to the correct folder.
    [Activity(Label = "Sound Recorder")]
    class RecordSoundActivity : Activity
    {
        private const int LOW_BACKGROUND_NOISE = 25;
        private const int MEDIUM_BACKGROUND_NOISE = 50;
        private const int HIGH_BACKGROUND_NOISE = 75;

        private const string LOW_BACKGROUND_STRING = "It's quiet here. This is a good time to record.";
        private const string MEDIUM_BACKGROUND_STRING = "It's a little loud here.";
        private const string HIGH_BACKGROUND_STRING = "It's loud here. Try moving somewhere quieter before recording.";

        private Button roundSoundRecorderButtton;

        private AudioRecorder audioRecorder;
        private string outputPath;

        private TextView backgroundNoiseDisplay;
        private AudioRecorder backgroundAudioRecorder;
        private bool bgRunning = false;
        private bool bgShouldToggle = false;

        private Animation downAnim;
        private Animation normalAnim;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.RecordSound);

            // Load animations
            normalAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.scale_button_normal);
            downAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.scale_button_pressed);

            // Get and assign buttons
            roundSoundRecorderButtton = FindViewById<Button>(Resource.Id.RoundSoundRecorderBtn);
            roundSoundRecorderButtton.Click += SoundRecorderButtonClicked;                        

            backgroundNoiseDisplay = FindViewById<TextView>(Resource.Id.BackgroundAudioDisplay);              
        }

        protected override void OnStart()
        {
            base.OnStart();

            // Initiate main recorder
            outputPath = AudioFileManager.GetNewAudioFilePath();

            // Set text
            Intent intent = this.Intent;
            Bundle extras = intent.Extras;
            string text = "";

            if (extras != null)
            {
                if (extras.ContainsKey("text"))
                {
                    text = extras.GetString("text");
                }
                else
                {
                    throw new NotImplementedException();
                    // TODO: Manage exception where there isn't valid data.
                }
            }

            FindViewById<TextView>(Resource.Id.textView1).Text = text;
        }


        protected override void OnResume()
        {
            base.OnResume();

            // Re instantiate players
            audioRecorder = new AudioRecorder();
            audioRecorder.PrepareAudioRecorder(outputPath, true);

            backgroundAudioRecorder = new AudioRecorder();
            backgroundAudioRecorder.PrepareAudioRecorder(AudioFileManager.RootBackgroundAudioPath, false);

            bgRunning = true;
            bgShouldToggle = true;

            ThreadPool.QueueUserWorkItem(o => DoBackgroundNoiseChecker());
            ThreadPool.QueueUserWorkItem(o => DoBackgroundNoiseCycler());
        }


        protected override void OnPause()
        {
            base.OnPause();

            // Clean up access to recorder/player
            // and delete audio if currently recording to avoid incomplete files
            bool deleteFile = audioRecorder.isRecording;
            audioRecorder.Dispose();
            audioRecorder = null;
        
            if (deleteFile)
            {
                AudioFileManager.DeleteFile(outputPath);
                roundSoundRecorderButtton.Text = "Begin Recording";
                // Deleting audio means we need to reset the drawable as it will be in 'active' state when resuming.
                roundSoundRecorderButtton.SetBackgroundDrawable(GetDrawable(Resource.Drawable.round_button));
            }

            bgRunning = false;
            bgShouldToggle = false;
            backgroundAudioRecorder.Dispose();
            backgroundAudioRecorder = null;

            // Deletes background noise audio. Probably a better way to do this
            // Probably shouldn't be saving it on disk in the first place.
            // TODO: Extend to solve issue if time.
            AudioFileManager.DeleteFile(AudioFileManager.RootBackgroundAudioPath);
        }


        private void SoundRecorderButtonClicked(object sender, EventArgs e)
        {
            // Toggles between recording or stopping the recording
            // File is now saved alongside recording.
            if (!audioRecorder.isRecording)
            {                
                backgroundAudioRecorder.StopAudio();
                bgShouldToggle = false;

                if (audioRecorder.StartAudio())
                {
                    roundSoundRecorderButtton.SetBackgroundDrawable(GetDrawable(Resource.Drawable.round_button_alt));
                    roundSoundRecorderButtton.StartAnimation(downAnim);
                    roundSoundRecorderButtton.Text = "Stop Recording";   
                }
            }
            else
            {
                if (audioRecorder.StopAudio())
                {
                    roundSoundRecorderButtton.SetBackgroundDrawable(GetDrawable(Resource.Drawable.round_button));
                    roundSoundRecorderButtton.StartAnimation(normalAnim);
                    roundSoundRecorderButtton.Text = "Begin Recording";

                    Intent recordCompleted = new Intent(this, typeof(RecordCompletedActivity));
                    recordCompleted.PutExtra("filepath", outputPath);
                    StartActivity(recordCompleted);
                }
            }
        }       


        private void DoBackgroundNoiseCycler()
        {
            while (bgRunning)
            {
                if (bgShouldToggle)
                {
                    if (backgroundAudioRecorder.isRecording)
                    {
                        backgroundAudioRecorder.StopAudio();
                        backgroundAudioRecorder.StartAudio();
                        int? temp = backgroundAudioRecorder.soundLevel;
                    }
                    else
                    {
                        backgroundAudioRecorder.StartAudio();
                    }
                }

                Thread.Sleep(10000);
            }
        }


        private void DoBackgroundNoiseChecker()
        {
            while (bgRunning)
            {
                Thread.Sleep(1500);

                if (backgroundAudioRecorder != null)
                {
                    int? soundLevel = backgroundAudioRecorder.soundLevel;

                    if (soundLevel != null)
                    {
                        string displayString = HIGH_BACKGROUND_STRING;

                        if (soundLevel < MEDIUM_BACKGROUND_NOISE)
                        {
                            displayString = MEDIUM_BACKGROUND_STRING;
                        }
                        if (soundLevel < LOW_BACKGROUND_NOISE)
                        {
                            displayString = LOW_BACKGROUND_STRING;
                        }

                        RunOnUiThread(() => backgroundNoiseDisplay.Text = string.Concat(displayString, "\n", "Background noise level: ", soundLevel.ToString()));
                    }
                    else
                    {
                        RunOnUiThread(() => backgroundNoiseDisplay.Text = "Background noise information not available");
                    }
                }
            }
        }
    }
}
