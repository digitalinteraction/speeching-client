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
using Android.Support.V7.App;

namespace DroidSpeeching
{
    [Activity(Label = "Loudness Help")]
    public class HelpLoudnessActivity : ActionBarActivity
    {
        Button introVidBtn;
        Button tutVidBtn;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.HelpLoudnessActivity);

            introVidBtn = FindViewById<Button>(Resource.Id.loudnessIntroVidBtn);
            introVidBtn.Click += introVidBtn_Click;
            tutVidBtn = FindViewById<Button>(Resource.Id.loudnessTutVidBtn);
            tutVidBtn.Click += tutVidBtn_Click;
        }

        void tutVidBtn_Click(object sender, EventArgs e)
        {
            string videoUrl = "https://openlabdata.blob.core.windows.net/videotuts/loudnessTut.mp4";
            Intent i = new Intent(Intent.ActionView);
            i.SetData(Android.Net.Uri.Parse(videoUrl));
            StartActivity(i);
        }

        void introVidBtn_Click(object sender, EventArgs e)
        {
            string videoUrl = "https://openlabdata.blob.core.windows.net/videotuts/loudnessIntro.mp4";
            Intent i = new Intent(Intent.ActionView);
            i.SetData(Android.Net.Uri.Parse(videoUrl));
            StartActivity(i);
        }
    }
}