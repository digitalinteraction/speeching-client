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

        private const float MAX_ANIM_SCALE = 2.0f;

        private Button roundSoundRecorderButtton;

        private AudioRecorder audioRecorder;
        private string outputPath;

        private TextView backgroundNoiseDisplay;
        private AudioRecorder backgroundAudioRecorder;
        private bool bgRunning = false;
        private bool bgShouldToggle = false;

        private Animation downAnim;
        private Animation normalAnim;

        private ImageView circleWaveForm;
        private Animation glowAnimation;

        private Bitmap glowBitMap;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.RecordSound);

            // Load animations
            ThreadPool.QueueUserWorkItem(o =>
            {
                    normalAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.scale_button_normal);
                    downAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.scale_button_pressed);
                    glowBitMap = GetGlow(Resource.Drawable.circle);
                    RunOnUiThread(() => circleWaveForm.SetImageBitmap(glowBitMap) );
            });
            

            // Get and assign buttons
            roundSoundRecorderButtton = FindViewById<Button>(Resource.Id.RoundSoundRecorderBtn);
            roundSoundRecorderButtton.Click += SoundRecorderButtonClicked;                        

            backgroundNoiseDisplay = FindViewById<TextView>(Resource.Id.BackgroundAudioDisplay);

            circleWaveForm = FindViewById<ImageView>(Resource.Id.CircleWaveForm);

            // Initiate 'glowing' effect and hide for later use
            // Takes 'time', doing this here improves responsiveness.
            
            circleWaveForm.Alpha = 0.0f;            
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
                EndGlowAnimation();
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
                    AnimateOuterGlow(1.15f, 1.3f, 400);
                    roundSoundRecorderButtton.SetBackgroundDrawable(GetDrawable(Resource.Drawable.round_button_alt));

                    if (downAnim != null)
                    {
                        roundSoundRecorderButtton.StartAnimation(downAnim);
                    }
                    roundSoundRecorderButtton.Text = "Stop Recording";   
                }
            }
            else
            {
                if (audioRecorder.StopAudio())
                {
                    EndGlowAnimation();
                    roundSoundRecorderButtton.SetBackgroundDrawable(GetDrawable(Resource.Drawable.round_button));

                    if (normalAnim != null)
                    {
                        roundSoundRecorderButtton.StartAnimation(normalAnim);
                    }
                    roundSoundRecorderButtton.Text = "Begin Recording";

                    Intent recordCompleted = new Intent(this, typeof(RecordCompletedActivity));
                    recordCompleted.PutExtra("filepath", outputPath);
                    StartActivity(recordCompleted);
                }
            }
        }


        public Bitmap GetGlow(int resourceId)
        {
            Bitmap bmp = null;

            try
            {
                int margin = 500;
                int halfMargin = margin / 2;
                int glowRadius = 500;
                int glowColor = Color.Rgb(0, 192, 200);

                Bitmap src = BitmapFactory.DecodeResource(Resources, resourceId);

                Bitmap alpha = src.ExtractAlpha();

                bmp = Bitmap.CreateBitmap(src.Width + margin, src.Height
                        + margin, Bitmap.Config.Argb8888);

                Canvas canvas = new Canvas(bmp);

                Paint paint = new Paint();
                paint.Color = Color.Blue;

                paint.SetMaskFilter(new BlurMaskFilter(glowRadius, BlurMaskFilter.Blur.Outer));
                canvas.DrawBitmap(alpha, halfMargin, halfMargin, paint);

                canvas.DrawBitmap(src, halfMargin, halfMargin, null);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return bmp;
        }

        public void AnimateOuterGlow(float from, float to, long duration)
        {
            if (glowAnimation == null)
            {
                glowAnimation = new ScaleAnimation(from, to, from, to, Dimension.RelativeToSelf, 0.5f, Dimension.RelativeToSelf, 0.5f);
                glowAnimation.RepeatMode = RepeatMode.Reverse;
                glowAnimation.RepeatCount = Animation.Infinite;

                glowAnimation.FillAfter = false;
                glowAnimation.FillEnabled = false;
            }
            
            glowAnimation.Duration = duration;

            circleWaveForm.Alpha = 1.0f;

            circleWaveForm.StartAnimation(glowAnimation);            
        }

        void EndGlowAnimation()
        {
            glowAnimation.Cancel();

            circleWaveForm.Alpha = 0.0f;
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
