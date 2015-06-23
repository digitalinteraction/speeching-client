using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using SpeechingShared;

namespace DroidSpeeching
{
    [Activity(Label = "Wikipedia Practice", ParentActivity = typeof (MainActivity), ScreenOrientation = ScreenOrientation.Portrait)]
    public class WikiPracticeActivity : ActionBarActivity, IDialogInterfaceOnClickListener
    {
        private const int MetronMaxBpm = 140;
        private const int MetronMinBpm = 60;
        private AndroidUtils.RecordAudioManager audioManager;
        private ServerData.TaskType currentMode;
        private LinearLayout loudControlsLayout;
        private int loudCurrentVol;
        private Button loudTargetButton;
        private TextView loudTargetText;
        private int loudTargetVol = 45;
        private TextView loudVolText;
        private short[] metronAudioBuffer;
        private AudioTrack metronAudioTrack;
        private TextView metronBpmText;
        private int metronBuffSize;
        private LinearLayout metronControlsLayout;
        private int metronCurrentBpm = 80;
        private Button metronDownBtn;
        private Button metronUpBtn;
        private Dictionary<ServerData.TaskType, LinearLayout> modeLayouts;
        private BiDictionary<ServerData.TaskType, string> modeNames;
        private List<string> names;
        private Action<string> onSpeechComplete;
        private bool reading;
        private Button startBtn;
        private TTSManager tts;
        private WikipediaResult wiki;
        private ImageView wikiImage;
        private TextView wikiText;

        public void OnClick(IDialogInterface dialog, int which)
        {
            string choice = names[which];

            ServerData.TaskType mode;
            modeNames.TryGetBySecond(choice, out mode);

            SwitchMode(mode);
        }

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.WikiPracticeActivity);

            modeLayouts = new Dictionary<ServerData.TaskType, LinearLayout>();
            modeNames = new BiDictionary<ServerData.TaskType, string>();

            wikiText = FindViewById<TextView>(Resource.Id.wiki_text);
            wikiImage = FindViewById<ImageView>(Resource.Id.wiki_image);
            startBtn = FindViewById<Button>(Resource.Id.wiki_startBtn);
            startBtn.Click += startBtn_Click;
            wikiText.Click += ttsBtn_Click;

            // Pacing layout
            metronBpmText = FindViewById<TextView>(Resource.Id.wiki_bpm);
            metronDownBtn = FindViewById<Button>(Resource.Id.wiki_downBtn);
            metronDownBtn.Click += downBtn_Click;
            metronUpBtn = FindViewById<Button>(Resource.Id.wiki_upBtn);
            metronUpBtn.Click += upBtn_Click;
            metronControlsLayout = FindViewById<LinearLayout>(Resource.Id.wiki_speedControls);
            modeLayouts.Add(ServerData.TaskType.Pacing, metronControlsLayout);
            modeNames.Add(ServerData.TaskType.Pacing, "Speech Pacing");

            // Loudness layout
            loudVolText = FindViewById<TextView>(Resource.Id.wiki_volume);
            loudTargetText = FindViewById<TextView>(Resource.Id.wiki_Targetvolume);
            loudTargetButton = FindViewById<Button>(Resource.Id.wiki_measureVolBtn);
            loudTargetButton.Click += loud_targetButton_Click;
            loudControlsLayout = FindViewById<LinearLayout>(Resource.Id.wiki_volControls);
            modeLayouts.Add(ServerData.TaskType.Loudness, loudControlsLayout);
            modeNames.Add(ServerData.TaskType.Loudness, "Loudness of Speech");

            names = new List<string>();

            foreach (
                KeyValuePair<ServerData.TaskType, string> entry in (Dictionary<ServerData.TaskType, string>)modeNames.firstToSecond)
            {
                names.Add(entry.Value);
            }

            currentMode = ServerData.TaskType.Loudness;

            tts = new TTSManager(this, onSpeechComplete);

            SetupRecorder();

            LoadWikiInfo();
        }

        private void ttsBtn_Click(object sender, EventArgs e)
        {
            if (tts.IsSpeaking())
            {
                tts.StopSpeaking();
            }
            else
            {
                tts.SayLine(wikiText.Text, "SpeechingWiki");
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            tts = new TTSManager(this, onSpeechComplete);
            audioManager = new AndroidUtils.RecordAudioManager(this, OnRecordingFull);
        }

        protected override void OnPause()
        {
            FinishReading();
            base.OnPause();
        }

        private void FinishReading()
        {
            if (reading)
            {
                reading = false;
                audioManager.StopRecording();
                modeLayouts[currentMode].Visibility = ViewStates.Gone;
                startBtn.Text = "Start!";
            }

            if (tts != null)
            {
                tts.Clean();
                tts = null;
            }

            if (audioManager != null)
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

        private async void ShowHelpDialog()
        {
            FinishReading();

            ActivityHelp help = await ServerData.FetchHelp(currentMode);

            VideoPlayerFragment helpVidFragment = new VideoPlayerFragment(help.HelpVideo, help.ActivityName, help.ActivityDescription);
            helpVidFragment.Show(SupportFragmentManager, "video_helper");

            helpVidFragment.StartVideo();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_practiceMode)
            {
                ShowChooseModeDialog();
                return true;
                
            }
            if (item.ItemId == Resource.Id.action_info)
            {
                ShowHelpDialog();
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

            if (stopRec)
                audioManager.StopRecording();

            if (!popup) return;

            Android.Net.Uri passedUri = Android.Net.Uri.FromFile(new Java.IO.File(AppData.TempRecording.Path));

            Android.Support.V7.App.AlertDialog alert = new Android.Support.V7.App.AlertDialog.Builder(this)
                .SetTitle("Session complete!")
                .SetMessage("Would you like to listen to your speech?")
                .SetPositiveButton("Listen", (EventHandler<DialogClickEventArgs>) null)
                .SetNeutralButton("Share recording", (EventHandler<DialogClickEventArgs>) null)
                .SetNegativeButton("Close", (arg1, arg2) => { })
                .Create();
            alert.Show();

            alert.GetButton((int) DialogButtonType.Positive).Click += (sender, e) =>
            {
                Intent intent = new Intent();
                intent.SetAction(Intent.ActionView);
                intent.SetDataAndType(passedUri, "audio/*");
                StartActivity(intent);
            };

            alert.GetButton((int) DialogButtonType.Neutral).Click += (sender, e) =>
            {
                Intent intent = new Intent();
                intent.SetAction(Intent.ActionSend);
                intent.SetType("audio/*");
                intent.PutExtra(Intent.ExtraStream, passedUri);
                StartActivity(intent);
            };
        }

        private void StartAction()
        {
            modeLayouts[currentMode].Visibility = ViewStates.Visible;
            startBtn.Text = "Stop!";

            SetupRecorder();
            audioManager.StartRecording(AppData.TempRecording.Path, 300);

            StartModeFunc();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            reading = !reading;

            if (reading)
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

            if (wiki.imageURL != null)
            {
                if (!File.Exists(wiki.imageURL))
                {
                    // Force update, file has been deleted somehow
                    wiki = await ServerData.FetchWikiData(AndroidUtils.DecodeHTML);
                }
                wikiImage.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(wiki.imageURL)));
                wikiImage.Visibility = ViewStates.Visible;
            }
            else
            {
                wikiImage.Visibility = ViewStates.Gone;
            }


            string[] sentences = wiki.content.Split(new[] {". "}, StringSplitOptions.RemoveEmptyEntries);

            string finalText = "";
            int charTarget = 400;

            foreach (string sentence in sentences)
            {
                if (finalText.Length < charTarget && finalText.Length + sentence.Length < 570)
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
            if (finalText.Length > 520)
            {
                if ((Resources.Configuration.ScreenLayout & Android.Content.Res.ScreenLayout.SizeMask) <=
                    Android.Content.Res.ScreenLayout.SizeNormal)
                {
                    wikiText.SetTextSize(Android.Util.ComplexUnitType.Sp, 15);
                }
                else
                {
                    wikiText.SetTextSize(Android.Util.ComplexUnitType.Sp, 19);
                }
            }


            SwitchMode(ServerData.TaskType.Loudness);

            dialog.Hide();
        }

        private void ShowChooseModeDialog()
        {
            if (reading)
            {
                reading = false;
                StopAction();
            }

            Android.Support.V7.App.AlertDialog choice = new Android.Support.V7.App.AlertDialog.Builder(this)
                .SetTitle("Choose a mode!")
                .SetItems(names.ToArray(), this)
                .Create();
            choice.Show();
        }

        /// <summary>
        /// Switch the functionality of the practiceActivity
        /// </summary>
        private void SwitchMode(ServerData.TaskType newMode)
        {
            if (newMode == currentMode) return;

            // Hide the current mode if needed
            if (currentMode != ServerData.TaskType.None)
            {
                RunOnUiThread(() => { modeLayouts[currentMode].Visibility = ViewStates.Gone; });
            }

            currentMode = newMode;
            string modeName;

            modeNames.TryGetByFirst(newMode, out modeName);

            Title = modeName;

            switch (newMode)
            {
                case ServerData.TaskType.Pacing:
                    ChangeBpm(0);
                    metronBuffSize = AudioTrack.GetMinBufferSize(44100, ChannelOut.Mono, Encoding.Pcm16bit);
                    metronAudioBuffer = new short[metronBuffSize];
                    break;
                case ServerData.TaskType.Loudness:
                    break;
            }
        }

        private void StartModeFunc()
        {
            switch (currentMode)
            {
                case ServerData.TaskType.Pacing:
                    ThreadPool.QueueUserWorkItem(o => PlayMetronome());
                    break;
                case ServerData.TaskType.Loudness:
                    loudTargetText.Text = loudTargetVol.ToString();
                    ThreadPool.QueueUserWorkItem(o => MeasureVolume());
                    break;
            }
        }

        #region Loudness specific code

        private void MeasureVolume()
        {
            double[] vals = new double[15];

            while (reading)
            {
                for (int i = 0; i < vals.Length; i++)
                {
                    Thread.Sleep(75);
                    if (audioManager == null) return;
                    vals[i] = audioManager.GetAmplitude();
                }

                double total = 0;
                foreach (double vol in vals)
                {
                    total += vol;
                }

                loudCurrentVol = (int) (total/vals.Length);

                RunOnUiThread(() =>
                {
                    loudVolText.Text = loudCurrentVol.ToString();

                    if (loudCurrentVol >= loudTargetVol)
                    {
                        loudVolText.SetTextColor(Color.Green);
                    }
                    else
                    {
                        loudVolText.SetTextColor(Color.Red);
                    }
                });
            }
        }

        private void loud_targetButton_Click(object sender, EventArgs e)
        {
            Android.Support.V7.App.AlertDialog alert = new Android.Support.V7.App.AlertDialog.Builder(this)
                .SetTitle("Set new target volume?")
                .SetMessage(
                    "Press start and then talk as loud as you can until the timer finishes to set a new target volume!")
                .SetNegativeButton("Cancel", (arg1, arg2) => { })
                .SetPositiveButton("Start!",
                    (arg1, arg2) => { ThreadPool.QueueUserWorkItem(o => StartVolumeCountdown()); })
                .Create();
            alert.Show();
        }

        private void StartVolumeCountdown()
        {
            reading = false;
            StopAction(true);

            const string message = " seconds remaining...\nTalk as loudly as possible!";
            const int startNum = 10;
            int remaining = startNum;

            ProgressDialog countDialog = null;

            RunOnUiThread(() =>
            {
                countDialog = new ProgressDialog(this);
                countDialog.SetMessage(remaining + message);
                countDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                countDialog.SetCancelable(false);
                countDialog.Indeterminate = false;
                countDialog.Progress = 0;
                countDialog.Max = startNum;
                countDialog.Show();
            });

            audioManager.StartRecording(AppData.TempRecording.Path, 300);
            double[] vols = new double[startNum*5];

            while (remaining > 0)
            {
                if (countDialog == null)
                {
                    // Wait until the GUI thread has created the dialog window
                    Thread.Sleep(10);
                    continue;
                }

                RunOnUiThread(() =>
                {
                    countDialog.SetMessage(remaining + message);
                    countDialog.IncrementProgressBy(1);
                });

                int countStart = (startNum - remaining)*vols.Length/startNum;

                for (int i = countStart; i < countStart + vols.Length/startNum; i++)
                {
                    Thread.Sleep(200);
                    vols[i] = audioManager.GetAmplitude();
                }

                remaining--;
            }

            audioManager.StopRecording();

            double total = 0;

            foreach (double vol in vols)
            {
                total += vol;
            }

            loudTargetVol = (int) (total/vols.Length);

            RunOnUiThread(() => { countDialog.Hide(); });
        }

        #endregion

        #region Pacing specific code

        private void upBtn_Click(object sender, EventArgs e)
        {
            ChangeBpm(10);
        }

        private void downBtn_Click(object sender, EventArgs e)
        {
            ChangeBpm(-10);
        }

        private void ChangeBpm(int amount)
        {
            metronCurrentBpm += amount;

            metronCurrentBpm = Math.Min(MetronMaxBpm, metronCurrentBpm);
            metronCurrentBpm = Math.Max(MetronMinBpm, metronCurrentBpm);

            metronBpmText.Text = metronCurrentBpm + " BPM";
        }

        private void PlayMetronome()
        {
            const int amp = 10000;
            double twopi = 8*Math.Atan(1.0);
            const double fr = 440.0;
            double ph = 0.0;

            int lastBpm = metronCurrentBpm;

            Animation anim = new AlphaAnimation(0.5f, 1.0f);
            anim.Duration = (60000/metronCurrentBpm)/2;
            anim.StartOffset = 0;
            anim.RepeatMode = RepeatMode.Reverse;
            anim.RepeatCount = Animation.Infinite;
            RunOnUiThread(() => { metronBpmText.StartAnimation(anim); });

            metronAudioTrack = new AudioTrack(Android.Media.Stream.Music, 44100, ChannelOut.Mono,
                Encoding.Pcm16bit, metronBuffSize, AudioTrackMode.Stream);

            metronAudioTrack.Play();

            while (reading)
            {
                Thread.Sleep(60000/metronCurrentBpm);

                if (lastBpm != metronCurrentBpm)
                {
                    // The BPM has changed - change the animation speed!
                    lastBpm = metronCurrentBpm;
                    anim.Duration = (60000/metronCurrentBpm)/2;

                    RunOnUiThread(() =>
                    {
                        metronBpmText.ClearAnimation();
                        metronBpmText.StartAnimation(anim);
                    });
                }

                for (int i = 0; i < metronAudioBuffer.Length; i++)
                {
                    metronAudioBuffer[i] = (short) (amp*Math.Sin(ph));
                    ph += twopi*fr/44100;
                }

                metronAudioTrack.Write(metronAudioBuffer, 0, metronAudioBuffer.Length);
            }

            metronAudioTrack.Stop();
            metronAudioTrack.Release();

            RunOnUiThread(() => { metronBpmText.ClearAnimation(); });
        }

        #endregion
    }
}