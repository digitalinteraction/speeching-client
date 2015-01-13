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
    // TODO: Do we need to do a destructor thing for the audio players?
    // TODO: When recording a second audio, the background noise recorder doesn't work anymore?
    // Note: OnCreate is only -ever- called once. When returning through the loop, it goes through OnResume. Interesting.
    // TODO: Do I need to set thread priority here for recording of user-interaction audio?
    [Activity(Label = "Sound Recorder")]
    class RecordSoundActivity : Activity
    {
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

            // Initiate main recorder
            outputPath = AudioFileManager.GetNewAudioFilePath();

            audioRecorder = new AudioRecorder();
            audioRecorder.PrepareAudioRecorder(outputPath, true);

            // Initiate background recorder
            backgroundAudioRecorder = new AudioRecorder();
            backgroundNoiseDisplay = FindViewById<TextView>(Resource.Id.BackgroundAudioDisplay);

            backgroundAudioRecorder.PrepareAudioRecorder(AudioFileManager.RootBackgroundAudioPath, false);
            bgRunning = true;
            bgShouldToggle = true;

            ThreadPool.QueueUserWorkItem(o => DoBackgroundNoiseChecker());
            ThreadPool.QueueUserWorkItem(o => DoBackgroundNoiseCycler());

            // Set text
            string text = this.Intent.GetStringExtra("text");
            Console.WriteLine(text);
            FindViewById<TextView>(Resource.Id.textView1).Text = text;
        }


        protected override void OnPause()
        {
            base.OnPause();

            // TODO: Do you have to clean up the suprressor?

            // Clean up access to recorder/player
            // and delete audio if currently recording to avoid incomplete files
            bool deleteFile = audioRecorder.isRecording;
            audioRecorder.Dispose();
            audioRecorder = null;
        
            if (deleteFile)
            {
                AudioFileManager.DeleteFile(outputPath);
                roundSoundRecorderButtton.Text = "Begin Recording";
            }

            bgRunning = false;
            bgShouldToggle = false;
            backgroundAudioRecorder.Dispose();
            backgroundAudioRecorder = null;
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

                    // TODO: What we actually want to do is make sure that the file saved properly and is ready to go to the next screen
                    // How do we do this? - Some sort of validation. After this point, user cannot cancel their recording.

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
                Thread.Sleep(500);

                if (backgroundAudioRecorder != null)
                {
                    int? amplitude = backgroundAudioRecorder.soundLevel;

                    if (amplitude != null)
                    {
                        RunOnUiThread(() => backgroundNoiseDisplay.Text = string.Concat("Background noise level: ", amplitude.ToString()));
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
