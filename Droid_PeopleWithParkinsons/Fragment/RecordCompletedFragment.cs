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
using Android.Views;

namespace Droid_PeopleWithParkinsons
{
    class RecordCompletedFragment : Android.App.Fragment, ViewTreeObserver.IOnGlobalLayoutListener
    {
        private string _filePath;
        private string filePath { get { return _filePath; } set { _filePath = value; byteData = null; } }

        private ImageView playbackImage;
        private TextView playbackText;
        private Button confirmButton;

        private AudioTrack audioTrackPlayer;
        private byte[] byteData = null;
        private bool isPlaying = false;
        private bool didPlayAudio = false;

        private Animation downAnim;
        private Animation normalAnim;

        private TextView text;

        private View ourView;
        private IOnFinishedPlaybackListener mListener;


        public void OnGlobalLayout()
        {
            Speeching_Utils.FitTextInTextView(text, Activity);
            Speeching_Utils.FitTextInTextView(ourView.FindViewById<TextView>(Resource.Id.RecordSoundHeaderLower), Activity);
            Speeching_Utils.FitTextInTextView(ourView.FindViewById<TextView>(Resource.Id.RecordCompleted_ButtonText), Activity);
            Speeching_Utils.FitTextInTextView(ourView.FindViewById<TextView>(Resource.Id.CheckBoxLowerHeader), Activity);
            //Speeching_Utils.FitTextInTextView(ourView.FindViewById<TextView>(Resource.Id.ButtonText), Activity);

            text.ViewTreeObserver.RemoveGlobalOnLayoutListener(this);
        }


        public interface IOnFinishedPlaybackListener
        {
            void OnFinishedPlaybackListener(string filepath);
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            ourView = inflater.Inflate(Resource.Layout.RecordCompleted, container, false);

            // Load our resources
            normalAnim = AnimationUtils.LoadAnimation(Activity, Resource.Animation.scale_button_normal);
            downAnim = AnimationUtils.LoadAnimation(Activity, Resource.Animation.scale_button_pressed);

            // Get and initiate buttons
            playbackImage = ourView.FindViewById<ImageView>(Resource.Id.PlaybackButtonRoot);
            playbackImage.Click += PlayBackButtonClicked;
            playbackText = ourView.FindViewById<TextView>(Resource.Id.RecordCompleted_ButtonText);

            confirmButton = ourView.FindViewById<Button>(Resource.Id.RecordCompletedConfirmButton);
            confirmButton.Click += ConfirmButtonClicked;

            text = ourView.FindViewById<TextView>(Resource.Id.RecordCompletedHeader);
            ViewTreeObserver vto = text.ViewTreeObserver;
            vto.AddOnGlobalLayoutListener(this);

            return ourView;
        }

        public override void OnStart()
        {
            base.OnStart();

            Bundle extras = Arguments;

            if (extras != null)
            {
                if (extras.ContainsKey("filepath"))
                {
                    filePath = extras.GetString("filepath");
                }
                else
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(Activity);

                    alert.SetTitle("Error");
                    alert.SetMessage("There's been a problem recording your audio. Sorry about that, please try again.");

                    alert.SetPositiveButton("OK", (senderAlert, args) =>
                    {
                        Intent mainMenu = new Intent(Activity, typeof(MainActivity));
                        StartActivity(mainMenu);
                    });

                    alert.Show();
                }
            }
        }


        public override void OnResume()
        {
            base.OnResume();
        }


        public override void OnPause()
        {
            base.OnPause();

            if (didPlayAudio)
            {
                if (isPlaying)
                {
                    audioTrackPlayer.Stop();
                    playbackText.Text = "Play Sound";
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


        public override void OnDestroyView()
        {
            base.OnDestroyView();

            unbindDrawables(ourView);
        }


        /// <summary>
        /// Asks the user for confirmation if they have flagged bad audio. Otherwise instantly submits audio selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfirmButtonClicked(object sender, EventArgs e)
        {
            bool isTicked = ourView.FindViewById<CheckBox>(Resource.Id.FlagAudio).Checked;

            if (isTicked)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(Activity);

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

            // Move file to rootActual audio directory
            // Get string to new directory
            // Call listener with string
            filePath = AudioFileManager.FinaliseAudio(filePath);
            mListener.OnFinishedPlaybackListener(filePath);
        }


        /// <summary>
        /// Gets a reference to our Activity's interface method so we can call it.
        /// </summary>
        /// <param name="activity"></param>
        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            try
            {
                mListener = (IOnFinishedPlaybackListener)activity;
            }
            catch (NotImplementedException e)
            {
                throw new NotImplementedException(activity.ToString() + " must implement OnArticleSelectedListener : " + e.ToString());
            }
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
                    playbackImage.SetImageResource(Resource.Drawable.button_pressed);
                    playbackImage.StartAnimation(downAnim);

                    playbackText.Text = "Stop Sound";
                    isPlaying = true;
                    didPlayAudio = true;

                    ThreadPool.QueueUserWorkItem(o => PlayAudio());
                }
                else
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(Activity);

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
            playbackImage.SetImageResource(Resource.Drawable.button_unpressed);
            playbackImage.StartAnimation(normalAnim);

            isPlaying = false;
            playbackText.Text = "Play Sound";

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
                if (audioTrackPlayer != null) audioTrackPlayer.Play();
                if (audioTrackPlayer != null) audioTrackPlayer.Write(byteData, 0, byteData.Length);

                Activity.RunOnUiThread(() =>
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


        /// <summary>
        /// Taken from Stackoverflow
        /// To ensure that the view fragment is properly disposes of
        /// Not sure if this is neccessary.
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
                for (int i = 0; i < ((ViewGroup)view).ChildCount; i++)
                {
                    unbindDrawables(((ViewGroup)view).GetChildAt(i));
                }
                ((ViewGroup)view).RemoveAllViews();
            }
        }
    }
}