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
    [Activity(Label = "Pacing Help")]
    public class HelpPacingActivity : ActionBarActivity
    {
        Button introVidBtn;
        Button tutVidBtn;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.HelpPacingActivity);

            introVidBtn = FindViewById<Button>(Resource.Id.pacingIntroVidBtn);
            introVidBtn.Click += introVidBtn_Click;
            tutVidBtn = FindViewById<Button>(Resource.Id.pacingTutVidBtn);
            tutVidBtn.Click += tutVidBtn_Click;
        }

        void tutVidBtn_Click(object sender, EventArgs e)
        {
            string videoUrl = "https://openlabdata.blob.core.windows.net/videotuts/pacingTut.mp4";
            Intent i = new Intent(Intent.ActionView);
            i.SetData(Android.Net.Uri.Parse(videoUrl));
            StartActivity(i);
        }

        void introVidBtn_Click(object sender, EventArgs e)
        {
            string videoUrl = "https://openlabdata.blob.core.windows.net/videotuts/pacingIntro.mp4";
            Intent i = new Intent(Intent.ActionView);
            i.SetData(Android.Net.Uri.Parse(videoUrl));
            StartActivity(i);
        }
    }
}