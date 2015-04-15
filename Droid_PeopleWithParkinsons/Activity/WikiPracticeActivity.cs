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
using Android.Support.V7.App;
using Android.Media;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;
using Android.Views.Animations;

namespace DroidSpeeching
{
    [Activity(Label = "WikiPaceActivity", ParentActivity = typeof(MainActivity))]
    public class WikiPracticeActivity : ActionBarActivity
    {
        public enum PracticeMode { None, Loudness, Metronome};

        PracticeMode currentMode;
        Dictionary<PracticeMode, LinearLayout> modeLayouts;

        WikipediaResult wiki;
        TextView wikiText;
        ImageView wikiImage;
        Button startBtn;

        // Metronome variables
        LinearLayout metron_controlsLayout;
        TextView metron_bpmText;
        Button metron_downBtn;
        Button metron_upBtn;
        int metron_currentBPM = 80;
        int metron_maxBPM = 140;
        int metron_minBPM = 60;
        AudioTrack metron_audioTrack;
        short[] metron_audioBuffer;
        int metron_buffSize;

        LinearLayout loud_controlsLayout;
        TextView loud_volText;
        Button loud_targetButton;
        int loud_currentVol;
        int loud_targetVol;

        AndroidUtils.RecordAudioManager audioManager;
        bool reading = false;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.WikiPracticeActivity);

            modeLayouts = new Dictionary<PracticeMode, LinearLayout>();

            wikiText = FindViewById<TextView>(Resource.Id.wiki_text);
            wikiImage = FindViewById<ImageView>(Resource.Id.wiki_image);
            startBtn = FindViewById<Button>(Resource.Id.wiki_startBtn);
            startBtn.Click += startBtn_Click;

            // Metronome layout
            metron_bpmText = FindViewById<TextView>(Resource.Id.wiki_bpm);
            metron_downBtn = FindViewById<Button>(Resource.Id.wiki_downBtn);
            metron_downBtn.Click += downBtn_Click;
            metron_upBtn = FindViewById<Button>(Resource.Id.wiki_upBtn);
            metron_upBtn.Click += upBtn_Click;
            metron_controlsLayout = FindViewById<LinearLayout>(Resource.Id.wiki_speedControls);
            modeLayouts.Add(PracticeMode.Metronome, metron_controlsLayout);

            // Loudness layout
            loud_volText = FindViewById<TextView>(Resource.Id.wiki_volume);
            loud_targetButton = FindViewById<Button>(Resource.Id.wiki_measureVolBtn);
            loud_controlsLayout = FindViewById<LinearLayout>(Resource.Id.wiki_volControls);
            modeLayouts.Add(PracticeMode.Loudness, loud_controlsLayout);

            currentMode = PracticeMode.None;

            SetupRecorder();

            LoadWikiInfo();
        }

        protected override void OnPause()
        {
            base.OnPause();
            if(reading)
            {
                reading = false;
                metron_controlsLayout.Visibility = ViewStates.Gone;
                startBtn.Text = "Start!";
            }
        }

        void startBtn_Click(object sender, EventArgs e)
        {
            reading = !reading;

            if(reading)
            {
                modeLayouts[currentMode].Visibility = ViewStates.Visible;
                startBtn.Text = "Stop!";
                audioManager.StartRecording(AppData.practiceRecording);
                StartModeFunc();
            }
            else
            {
                modeLayouts[currentMode].Visibility = ViewStates.Gone;
                startBtn.Text = "Start!";
                audioManager.StopRecording();
            }
        }

        private void SetupRecorder()
        {
            if (audioManager != null) return;
            audioManager = new AndroidUtils.RecordAudioManager(this, null);
        }

        /// <summary>
        /// Pulls today's featured wikipedia article
        /// </summary>
        private async void LoadWikiInfo()
        {
            ProgressDialog dialog = new ProgressDialog(this);
            dialog.SetTitle("Please Wait...");
            dialog.SetMessage("Downloading today's content!");
            dialog.SetCancelable(false);
            dialog.Show();

            wiki = await AndroidUtils.GetTodaysWiki(this);

            string[] sentences = wiki.content.Split(new string[] { ". " }, StringSplitOptions.RemoveEmptyEntries);

            string finalText = "";
            int charTarget = 400;

            foreach (string sentence in sentences)
            {
                if (finalText.Length < charTarget)
                {
                    finalText += sentence + ". ";
                }
                else
                {
                    break;
                }
            }

            wikiText.Text = finalText;

            // If it's longer than expected, reduce the text size!
            if (finalText.Length > 520 &&
                ((Resources.Configuration.ScreenLayout & Android.Content.Res.ScreenLayout.SizeMask) <= Android.Content.Res.ScreenLayout.SizeNormal))
            {
                wikiText.SetTextSize(Android.Util.ComplexUnitType.Sp, 15);
            }

            if (wiki.imageURL != null)
            {
                wikiImage.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(wiki.imageURL)));
                wikiImage.Visibility = ViewStates.Visible;
            }
            else
            {
                wikiImage.Visibility = ViewStates.Gone;
            }

            SwitchMode(PracticeMode.Loudness);

            dialog.Hide();
        }

        /// <summary>
        /// Switch the functionality of the activity
        /// </summary>
        private void SwitchMode(PracticeMode newMode)
        {
            if (newMode == currentMode) return;

            // Hide the current mode if needed
            if(currentMode != PracticeMode.None)
            {
                RunOnUiThread(() => { modeLayouts[currentMode].Visibility = ViewStates.Gone; });
            }
          
            currentMode = newMode;

            switch(newMode)
            {
                case PracticeMode.Metronome :
                    ChangeBPM(0);
                    metron_buffSize = AudioTrack.GetMinBufferSize(44100, ChannelOut.Mono, Android.Media.Encoding.Pcm16bit);
                    metron_audioBuffer = new short[metron_buffSize];
                    break;
                case PracticeMode.Loudness :
                    break;
            }
        }

        private void StartModeFunc()
        {
            switch(currentMode)
            {
                case PracticeMode.Metronome :
                    ThreadPool.QueueUserWorkItem(o => PlayMetronome());
                    break;
                case PracticeMode.Loudness :
                    ThreadPool.QueueUserWorkItem(o => MeasureVolume());
                    break;
            }
        }

        #region Loudness specific code
        private void MeasureVolume()
        {
            double[] vals = new double[15]; 

            while(reading)
            {
                for (int i = 0; i < vals.Length; i++ )
                {
                    Thread.Sleep(75);
                    vals[i] = audioManager.GetAmplitude();
                }

                double total = 0;
                foreach(double vol in vals)
                {
                    total += vol;
                }

                loud_currentVol = (int)(total / vals.Length);

                RunOnUiThread(() => { loud_volText.Text = loud_currentVol.ToString(); });
            }
        }
        #endregion

        #region Metronome specific code
        void upBtn_Click(object sender, EventArgs e)
        {
            ChangeBPM(10);
        }

        void downBtn_Click(object sender, EventArgs e)
        {
            ChangeBPM(-10);
        }

        private void ChangeBPM(int amount)
        {
            metron_currentBPM += amount;

            metron_currentBPM = Math.Min(metron_maxBPM, metron_currentBPM);
            metron_currentBPM = Math.Max(metron_minBPM, metron_currentBPM);

            metron_bpmText.Text = metron_currentBPM.ToString() + " BPM";
        }

        private void PlayMetronome()
        {
            int amp = 10000;
            double twopi = 8 * Math.Atan(1.0);
            double fr = 440.0;
            double ph = 0.0;

            int lastBPM = metron_currentBPM;

            Animation anim = new AlphaAnimation(0.5f, 1.0f);
            anim.Duration = (60000 / metron_currentBPM) / 2;
            anim.StartOffset = 0;
            anim.RepeatMode = RepeatMode.Reverse;
            anim.RepeatCount = Animation.Infinite;
            RunOnUiThread(() => { metron_bpmText.StartAnimation(anim); });

            metron_audioTrack = new AudioTrack(Stream.Music, 44100, ChannelOut.Mono, Android.Media.Encoding.Pcm16bit, metron_buffSize, AudioTrackMode.Stream);

            metron_audioTrack.Play();

            while (reading)
            {
                Thread.Sleep(60000 / metron_currentBPM);

                if (lastBPM != metron_currentBPM)
                {
                    // The BPM has changed - change the animation speed!
                    lastBPM = metron_currentBPM;
                    anim.Duration = (60000 / metron_currentBPM) / 2;

                    RunOnUiThread(() =>
                    {
                        metron_bpmText.ClearAnimation();
                        metron_bpmText.StartAnimation(anim);
                    });
                }

                for (int i = 0; i < metron_audioBuffer.Length; i++)
                {
                    metron_audioBuffer[i] = (short)(amp * Math.Sin(ph));
                    ph += twopi * fr / 44100;
                }

                metron_audioTrack.Write(metron_audioBuffer, 0, metron_audioBuffer.Length);
            }

            metron_audioTrack.Stop();
            metron_audioTrack.Release();

            RunOnUiThread(() => { metron_bpmText.ClearAnimation(); });
        }
        #endregion
    }
}