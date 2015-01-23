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
    class RecordSoundFragment : Android.App.Fragment, ViewTreeObserver.IOnGlobalLayoutListener
    {
        private const int LOW_BACKGROUND_NOISE = 25;
        private const int MEDIUM_BACKGROUND_NOISE = 50;
        private const int HIGH_BACKGROUND_NOISE = 75;

        private const string LOW_BACKGROUND_STRING = "It's quiet here. This is a good time to record.";
        private const string MEDIUM_BACKGROUND_STRING = "It's a little loud here.";
        private const string HIGH_BACKGROUND_STRING = "It's loud here. Try moving somewhere quieter before recording.";

        private const float MAX_ANIM_SCALE = 2.0f;

        private TextView textToSpeak;
        private ImageView roundSoundImageView;
        private TextView roundSoundText;

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

        private View ourView;
        private IOnFinishedRecordingListener mListener;


        private void FitTextInTextView(TextView view, string overrideString = null)
        {
            int width = view.MeasuredWidth;
            int height = view.MeasuredHeight;

            int widthPading = view.PaddingLeft + view.PaddingRight;
            int heightPadding = view.PaddingTop + view.PaddingBottom;

            width -= widthPading;
            height -= heightPadding;

            string toUse = overrideString == null ? view.Text : overrideString;
            int textSize = Speeching_Utils.GenerateTextSize(toUse, 200, height, width, Speeching_Utils.DISPLAY_UNIT.DP, Activity);

            view.TextSize = textSize;
        }

        public void OnGlobalLayout()
        {
            FitTextInTextView(textToSpeak);
            FitTextInTextView(ourView.FindViewById<TextView>(Resource.Id.Instructions));
            FitTextInTextView(ourView.FindViewById<TextView>(Resource.Id.StoredSoundsValue));
            FitTextInTextView(ourView.FindViewById<TextView>(Resource.Id.BackgroundAudioDisplay), HIGH_BACKGROUND_STRING);
            FitTextInTextView(ourView.FindViewById<TextView>(Resource.Id.ButtonText));
            textToSpeak.ViewTreeObserver.RemoveGlobalOnLayoutListener(this);
        }

        public interface IOnFinishedRecordingListener
        {
            void OnFinishedRecordingListener(string filepath);
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            try
            {
                mListener = (IOnFinishedRecordingListener)activity;
            }
            catch (NotImplementedException e)
            {
                throw new NotImplementedException(activity.ToString() + " must implement OnArticleSelectedListener : " + e.ToString());
            }
       
        }


        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            ourView = inflater.Inflate(Resource.Layout.RecordSound, container, false);

            // Load animations
            // Initiate 'glowing' effect and hide for later use
            // Takes 'time', doing this here improves responsiveness.
            ThreadPool.QueueUserWorkItem(o =>
            {
                normalAnim = AnimationUtils.LoadAnimation(Activity, Resource.Animation.scale_button_normal);
                downAnim = AnimationUtils.LoadAnimation(Activity, Resource.Animation.scale_button_pressed);
                Activity.RunOnUiThread(() => DestroyImageViewDrawable());
                SetCircleWaveFormGlow();
                Activity.RunOnUiThread(() => circleWaveForm.SetImageBitmap(glowBitMap));
            });


            // Get and assign 'buttons'
            roundSoundImageView = ourView.FindViewById<ImageView>(Resource.Id.RecordButtonRoot);
            roundSoundImageView.Click += SoundRecorderButtonClicked;
            roundSoundText = ourView.FindViewById<TextView>(Resource.Id.ButtonText);

            backgroundNoiseDisplay = ourView.FindViewById<TextView>(Resource.Id.BackgroundAudioDisplay);

            circleWaveForm = ourView.FindViewById<ImageView>(Resource.Id.CircleWaveForm);

            // We don't want to show this until the user starts recording.
            circleWaveForm.Alpha = 0.0f;        

            textToSpeak = ourView.FindViewById<TextView>(Resource.Id.TextToSpeak);
            ViewTreeObserver vto = textToSpeak.ViewTreeObserver;
            vto.AddOnGlobalLayoutListener(this);

            return ourView;
        }
        
        public override void OnCreate(Bundle savedInstanceState)
        {
 	        base.OnCreate(savedInstanceState);
        }

        public override void OnStart()
        {
            base.OnStart();

            // Initiate main recorder
            outputPath = AudioFileManager.GetNewAudioFilePath();

            // Set text
            Intent intent = Activity.Intent;
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
                    //throw new NotImplementedException();
                    // TODO: Manage exception where there isn't valid data.
                    // Although, there should never be 'invalid data'.
                }
            }

            ourView.FindViewById<TextView>(Resource.Id.TextToSpeak).Text = text;
        }


        public override void OnResume()
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


        public override void OnPause()
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
                roundSoundText.Text = "Begin Recording";
                // Deleting audio means we need to reset the drawable as it will be in 'active' state when resuming.
                roundSoundImageView.SetImageResource(Resource.Drawable.button_unpressed);
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
                    roundSoundImageView.SetImageResource(Resource.Drawable.button_pressed);
                    roundSoundText.Text = "Stop Recording";

                    if (downAnim != null)
                    {
                        roundSoundImageView.StartAnimation(downAnim);
                    } 
                }
            }
            else
            {
                if (audioRecorder.StopAudio())
                {
                    EndGlowAnimation();
                    roundSoundImageView.SetImageResource(Resource.Drawable.button_unpressed);
                    roundSoundText.Text = "Begin Recording";

                    if (normalAnim != null)
                    {
                        roundSoundImageView.StartAnimation(normalAnim);
                    }

                    mListener.OnFinishedRecordingListener(outputPath);
                }
            }
        }


        /// <summary>
        /// Not the nicest, or most 'component-y'est, but had an issue with a memory leak
        /// And now it's best to just not touch it.
        /// Takes the circle drawable, adds a fade around the outside and re-draws the bitmap to glowBitMap
        /// </summary>
        public void SetCircleWaveFormGlow()
        {
            try
            {
                int margin = 500;
                int halfMargin = margin / 2;
                int glowRadius = 500;
                int glowColor = Color.Rgb(0, 192, 200);

                Bitmap src = BitmapFactory.DecodeResource(Resources, Resource.Drawable.circle);

                Bitmap alpha = src.ExtractAlpha();

                glowBitMap = Bitmap.CreateBitmap(src.Width + margin, src.Height
                        + margin, Bitmap.Config.Argb8888);

                Canvas canvas = new Canvas(glowBitMap);

                Paint paint = new Paint();
                paint.Color = Color.Blue;

                paint.SetMaskFilter(new BlurMaskFilter(glowRadius, BlurMaskFilter.Blur.Outer));
                canvas.DrawBitmap(alpha, halfMargin, halfMargin, paint);

                canvas.DrawBitmap(src, halfMargin, halfMargin, null);

                src.Recycle();
                alpha.Recycle();
                canvas.Dispose();
                paint.Dispose();

                src = null;
                alpha = null;
                canvas = null;
                paint = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        /// <summary>
        /// Adds a glow to the circleWaveForm that is behind the record button.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
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


        /// <summary>
        /// Cycles between 10 second audio recordings of background noise
        /// </summary>
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


        /// <summary>
        /// Polls for the background noise level in a loop and displays it to the user.
        /// </summary>
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

                        
                       Activity.RunOnUiThread(() => backgroundNoiseDisplay.Text = string.Concat(displayString, "\n", "Background noise level: ", soundLevel.ToString()));
                    }
                    else
                    {
                        Activity.RunOnUiThread(() => backgroundNoiseDisplay.Text = "Background noise information not available");
                    }
                }
            }  
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            DestroyImageViewDrawable();
            glowBitMap.Recycle();
            glowBitMap.Dispose();
            glowBitMap = null;
            unbindDrawables(ourView);
        }


        /// <summary>
        /// Bitmap memory is handled differently, so we signal the GC
        /// That it's allowed to free the memory.
        /// Call this before changing the drawable on circleWaveForm
        /// </summary>
        public void DestroyImageViewDrawable()
        {
            circleWaveForm.ClearAnimation();

            if (circleWaveForm != null && circleWaveForm.Drawable is BitmapDrawable)
            {
                // get the drawable
                using (var d = circleWaveForm.Drawable)
                {
                    // remove the reference the imageView has
                    circleWaveForm.SetImageBitmap(null);
                    // recycle it
                    if (((BitmapDrawable)d).Bitmap != null)
                    {
                        ((BitmapDrawable)d).Bitmap.Recycle();
                    }
                }
            }
        }


        /// <summary>
        /// Taken from stackoverflow.
        /// To clean up fragment from memory leaks
        /// Not sure if neccessary.
        /// </summary>
        /// <param name="view"></param>
        private void unbindDrawables(View view) 
        {
            if (view.Background != null) 
            {
               view.Background.SetCallback(null);
            }
            if (view is ViewGroup) 
            {
               for (int i = 0; i < ((ViewGroup) view).ChildCount; i++) 
               {
                  unbindDrawables(((ViewGroup) view).GetChildAt(i));
               }
               ((ViewGroup) view).RemoveAllViews();
            }
       }
    }
}
