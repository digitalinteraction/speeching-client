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
    public class WikiPaceActivity : ActionBarActivity
    {
        TextView wikiText;
        ImageView wikiImage;
        Button startBtn;

        LinearLayout controlsLayout;
        TextView bpmText;
        Button downBtn;
        Button upBtn;

        WikipediaResult wiki;

        int currentBPM = 80;
        int maxBPM = 140;
        int minBPM = 60;

        bool reading = false;

        AudioTrack audioTrack;
        short[] audioBuffer;
        int buffSize;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.WikiPace);

            wikiText = FindViewById<TextView>(Resource.Id.wiki_text);
            wikiImage = FindViewById<ImageView>(Resource.Id.wiki_image);
            controlsLayout = FindViewById<LinearLayout>(Resource.Id.wiki_speedControls);
            bpmText = FindViewById<TextView>(Resource.Id.wiki_bpm);

            startBtn = FindViewById<Button>(Resource.Id.wiki_startBtn);
            startBtn.Click += startBtn_Click;

            downBtn = FindViewById<Button>(Resource.Id.wiki_downBtn);
            downBtn.Click += downBtn_Click;

            upBtn = FindViewById<Button>(Resource.Id.wiki_upBtn);
            upBtn.Click += upBtn_Click;

            LoadWikiInfo();
            ChangeBPM(0);

            buffSize = AudioTrack.GetMinBufferSize(44100, ChannelOut.Mono, Android.Media.Encoding.Pcm16bit);

            audioBuffer = new short[buffSize];
        }

        protected override void OnPause()
        {
            base.OnPause();
            if(reading)
            {
                reading = false;
                controlsLayout.Visibility = ViewStates.Gone;
                startBtn.Text = "Start!";
            }
        }

        private void PlayMetronome()
        {
            int amp = 10000;
            double twopi = 8 * Math.Atan(1.0);
            double fr = 440.0;
            double ph = 0.0;

            int lastBPM = currentBPM;

            Animation anim = new AlphaAnimation(0.5f, 1.0f);
            anim.Duration = (60000 / currentBPM) / 2;
            anim.StartOffset = 0;
            anim.RepeatMode = RepeatMode.Reverse;
            anim.RepeatCount = Animation.Infinite;
            RunOnUiThread( () => { bpmText.StartAnimation(anim); } );

            audioTrack = new AudioTrack(Stream.Music, 44100, ChannelOut.Mono, Android.Media.Encoding.Pcm16bit, buffSize, AudioTrackMode.Stream);

            audioTrack.Play();

            while(reading)
            {
                Thread.Sleep(60000 / currentBPM);

                if(lastBPM != currentBPM)
                {
                    // The BPM has changed - change the animation speed!
                    lastBPM = currentBPM;
                    anim.Duration = (60000 / currentBPM) / 2;

                    RunOnUiThread(() => { 
                        bpmText.ClearAnimation();
                        bpmText.StartAnimation(anim);
                    });
                }

                 for(int i=0; i < audioBuffer.Length; i++)
                 {
                     audioBuffer[i] = (short)(amp * Math.Sin(ph));
                     ph += twopi * fr / 44100;
                 }

                 audioTrack.Write(audioBuffer, 0, audioBuffer.Length);
            }

            audioTrack.Stop();
            audioTrack.Release();

            RunOnUiThread( () => { bpmText.ClearAnimation(); } );
        }

        void startBtn_Click(object sender, EventArgs e)
        {
            reading = !reading;

            if(reading)
            {
                controlsLayout.Visibility = ViewStates.Visible;
                startBtn.Text = "Stop!";

                ThreadPool.QueueUserWorkItem(o =>PlayMetronome());
            }
            else
            {
                controlsLayout.Visibility = ViewStates.Gone;
                startBtn.Text = "Start!";
            }
        }

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
            currentBPM += amount;

            currentBPM = Math.Min(maxBPM, currentBPM);
            currentBPM = Math.Max(minBPM, currentBPM);

            bpmText.Text = currentBPM.ToString() + " BPM";
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

            foreach(string sentence in sentences)
            {
                if(finalText.Length < charTarget)
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
            if(finalText.Length > 520 && 
                ((Resources.Configuration.ScreenLayout & Android.Content.Res.ScreenLayout.SizeMask) <= Android.Content.Res.ScreenLayout.SizeNormal))
            {
                wikiText.SetTextSize(Android.Util.ComplexUnitType.Sp, 15);
            }
            
            if(wiki.imageURL != null)
            {
                wikiImage.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(wiki.imageURL)));
                wikiImage.Visibility = ViewStates.Visible;
            }
            else
            {
                wikiImage.Visibility = ViewStates.Gone;
            }

            dialog.Hide();
        }
    }
}