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
using Android.Graphics;
using Android.Speech.Tts;

namespace DroidSpeeching
{
    [Activity(Label = "WikiPaceActivity", ParentActivity = typeof(MainActivity))]
    public class WikiPracticeActivity : ActionBarActivity, Android.Content.IDialogInterfaceOnClickListener
    {
        public enum PracticeMode { None, Loudness, Metronome};

        PracticeMode currentMode;
        Dictionary<PracticeMode, LinearLayout> modeLayouts;
        BiDictionary<PracticeMode, string> modeNames;
        List<string> names;

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
        TextView loud_targetText;
        Button loud_targetButton;
        int loud_currentVol;
        int loud_targetVol = 45;

        Button ttsBtn;
        TTSManager tts;
        AndroidUtils.RecordAudioManager audioManager;
        bool reading = false;

        Action<string> onSpeechComplete;
        string speechWaiting = "Start Speech Preview";
        string speechSpeaking = "Stop Speech Preview";

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.WikiPracticeActivity);

            modeLayouts = new Dictionary<PracticeMode, LinearLayout>();
            modeNames = new BiDictionary<PracticeMode, string>();

            wikiText = FindViewById<TextView>(Resource.Id.wiki_text);
            wikiImage = FindViewById<ImageView>(Resource.Id.wiki_image);
            startBtn = FindViewById<Button>(Resource.Id.wiki_startBtn);
            startBtn.Click += startBtn_Click;
            ttsBtn = FindViewById<Button>(Resource.Id.ttsBtn);
            ttsBtn.Click += ttsBtn_Click;

            // Metronome layout
            metron_bpmText = FindViewById<TextView>(Resource.Id.wiki_bpm);
            metron_downBtn = FindViewById<Button>(Resource.Id.wiki_downBtn);
            metron_downBtn.Click += downBtn_Click;
            metron_upBtn = FindViewById<Button>(Resource.Id.wiki_upBtn);
            metron_upBtn.Click += upBtn_Click;
            metron_controlsLayout = FindViewById<LinearLayout>(Resource.Id.wiki_speedControls);
            modeLayouts.Add(PracticeMode.Metronome, metron_controlsLayout);
            modeNames.Add(PracticeMode.Metronome, "Speech Pacing");

            // Loudness layout
            loud_volText = FindViewById<TextView>(Resource.Id.wiki_volume);
            loud_targetText = FindViewById<TextView>(Resource.Id.wiki_Targetvolume);
            loud_targetButton = FindViewById<Button>(Resource.Id.wiki_measureVolBtn);
            loud_targetButton.Click += loud_targetButton_Click;
            loud_controlsLayout = FindViewById<LinearLayout>(Resource.Id.wiki_volControls);
            modeLayouts.Add(PracticeMode.Loudness, loud_controlsLayout);
            modeNames.Add(PracticeMode.Loudness, "Loudness of Speech");

            names = new List<string>();

            foreach (KeyValuePair<PracticeMode, string> entry in (Dictionary<PracticeMode, string>)modeNames.firstToSecond)
            {
                names.Add(entry.Value);
            }

            currentMode = PracticeMode.None;

            onSpeechComplete = (string id) => 
            {
                RunOnUiThread(()=> ttsBtn.Text = speechWaiting);
            };

            tts = new TTSManager(this, onSpeechComplete);

            SetupRecorder();

            LoadWikiInfo();
        }

        void ttsBtn_Click(object sender, EventArgs e)
        {
            if(tts.IsSpeaking())
            {
                tts.StopSpeaking();
                ttsBtn.Text = speechWaiting;
            }
            else
            {
                tts.SayLine(wikiText.Text, "SpeechingWiki");
                ttsBtn.Text = speechSpeaking;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            tts = new TTSManager(this, onSpeechComplete);
            ttsBtn.Text = speechWaiting;
            audioManager = new AndroidUtils.RecordAudioManager(this, OnRecordingFull);

        }

        protected override void OnPause()
        {
            base.OnPause();
            if(reading)
            {
                reading = false;
                audioManager.StopRecording();
                modeLayouts[currentMode].Visibility = ViewStates.Gone;
                startBtn.Text = "Start!";
            }

            if(tts != null)
            {
                tts.Clean();
                tts = null;
            }

            if(audioManager != null)
            {
                audioManager.CleanUp();
                audioManager = null;
            }
            
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.practiceActivityActions, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_practiceMode)
            {
                ShowChooseModeDialog();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        // Called by manager if it can't record anymore
        private void OnRecordingFull()
        {
            reading = false;
            StopAction(false);

            Toast.MakeText(this, "Recording full!", ToastLength.Long).Show();
        }

        private void StopAction(bool stopRec = true, bool popup = false)
        {
            RunOnUiThread(() =>
            {
                modeLayouts[currentMode].Visibility = ViewStates.Gone;
                startBtn.Text = "Start!";
            });

            if(stopRec)
                audioManager.StopRecording();

            if(popup)
            {
                Android.Net.Uri passedUri = Android.Net.Uri.FromFile(new Java.IO.File(AppData.practiceRecording));

                AlertDialog alert = new AlertDialog.Builder(this)
                    .SetTitle("Session complete!")
                    .SetMessage("Would you like to listen to your speech?")
                    .SetPositiveButton("Listen", (EventHandler<DialogClickEventArgs>)null)
                    .SetNeutralButton("Share this recording", (EventHandler<DialogClickEventArgs>)null)
                    .SetNegativeButton("Close", (arg1, arg2) => { })
                    .Create();
                alert.Show();

                alert.GetButton((int)DialogButtonType.Positive).Click += (object sender, EventArgs e)=> {
                    Intent intent = new Intent();
                    intent.SetAction(Intent.ActionView);
                    intent.SetDataAndType(passedUri, "audio/*");
                    StartActivity(intent);
                };

                alert.GetButton((int)DialogButtonType.Neutral).Click += (object sender, EventArgs e) =>
                {
                    Intent intent = new Intent();
                    intent.SetAction(Intent.ActionSend);
                    intent.SetType("audio/*");
                    intent.PutExtra(Intent.ExtraStream, passedUri);
                    StartActivity(intent);
                };
            }
        }

        private void StartAction()
        {
            modeLayouts[currentMode].Visibility = ViewStates.Visible;
            startBtn.Text = "Stop!";

            SetupRecorder();
            audioManager.StartRecording(AppData.practiceRecording, 300);

            StartModeFunc();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            reading = !reading;

            if(reading)
            {
                StartAction();
            }
            else
            {
                StopAction(true, true);
            }
        }

        private void SetupRecorder()
        {
            if (audioManager != null) return;
            audioManager = new AndroidUtils.RecordAudioManager(this, OnRecordingFull);
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
                wikiText.SetTextSize(Android.Util.ComplexUnitType.Sp, 16);
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

        private void ShowChooseModeDialog()
        {
            if (reading)
            {
                reading = false;
                StopAction(true);
            }

            AlertDialog choice = new AlertDialog.Builder(this)
                .SetTitle("Choose a mode!")
                .SetItems(names.ToArray(), this)
                .Create();
            choice.Show();
        }

        public void OnClick(IDialogInterface dialog, int which)
        {
            string choice = names[which];

            PracticeMode mode;
            modeNames.TryGetBySecond(choice, out mode);

            SwitchMode(mode);
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
            string modeName = "";

            modeNames.TryGetByFirst(newMode, out modeName);

            this.Title = modeName;

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
                    loud_targetText.Text = loud_targetVol.ToString();
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
                    if (audioManager == null) return;
                    vals[i] = audioManager.GetAmplitude();
                }

                double total = 0;
                foreach(double vol in vals)
                {
                    total += vol;
                }

                loud_currentVol = (int)(total / vals.Length);

                RunOnUiThread(() => { 
                    loud_volText.Text = loud_currentVol.ToString(); 

                    if(loud_currentVol >= loud_targetVol)
                    {
                        loud_volText.SetTextColor(Color.Green);
                    }
                    else
                    {
                        loud_volText.SetTextColor(Color.Red);
                    }
                });
            }
        }

        private void loud_targetButton_Click(object sender, EventArgs e)
        {
            AlertDialog alert = new AlertDialog.Builder(this)
                .SetTitle("Set new target volume?")
                .SetMessage("Press start and then talk as loud as you can until the timer finishes to set a new target volume!")
                .SetNegativeButton("Cancel", (arg1, arg2) => { })
                .SetPositiveButton("Start!", (arg1, arg2) =>
                {

                    ThreadPool.QueueUserWorkItem(o => StartVolumeCountdown());

                })
                .Create();
            alert.Show();
        }

        private void StartVolumeCountdown()
        {
            reading = false;
            StopAction(true);

            string message = " seconds remaining...\nTalk as loudly as possible!";
            int startNum = 10;
            int remaining = startNum;

            ProgressDialog countDialog = null;

            RunOnUiThread(() =>
            {
                countDialog = new ProgressDialog(this);
                countDialog.SetMessage(remaining.ToString() + message);
                countDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                countDialog.SetCancelable(false);
                countDialog.Indeterminate = false;
                countDialog.Progress = 0;
                countDialog.Max = startNum;
                countDialog.Show();
            });

            audioManager.StartRecording(AppData.practiceRecording, 300);
            double[] vols = new double[startNum * 5];

            while(remaining > 0)
            {
                if(countDialog == null)
                {
                    // Wait until the GUI thread has created the dialog window
                    Thread.Sleep(10);
                    continue;
                }

                RunOnUiThread(() =>
                {
                    countDialog.SetMessage(remaining.ToString() + message);
                    countDialog.IncrementProgressBy(1);
                });

                int countStart = (startNum - remaining) * vols.Length/startNum;

                for (int i = countStart; i < countStart + vols.Length / startNum; i++)
                {
                    Thread.Sleep(200);
                    vols[i] = audioManager.GetAmplitude();
                }

                remaining--;
            }

            audioManager.StopRecording();

            double total = 0;

            foreach(double vol in vols)
            {
                total += vol;
            }

            loud_targetVol = (int)(total / vols.Length);

            RunOnUiThread(() =>{ countDialog.Hide(); });
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