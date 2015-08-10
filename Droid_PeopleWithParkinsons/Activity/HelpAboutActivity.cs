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
    [Activity(Label = "About Speeching")]
    public class HelpAboutActivity : ActionBarActivity
    {
        Button introVidBtn;
        Button closeBtn;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.AboutActivity);

            introVidBtn = FindViewById<Button>(Resource.Id.introVidBtn);
            introVidBtn.Click += introVidBtn_Click;

            closeBtn = FindViewById<Button>(Resource.Id.closeBtn);
            closeBtn.Click += closeBtn_Click;
        }

        void introVidBtn_Click(object sender, EventArgs e)
        {
            string videoUrl = "https://openlabdata.blob.core.windows.net/videotuts/welcome.mp4";
            Intent i = new Intent(Intent.ActionView);
            i.SetData(Android.Net.Uri.Parse(videoUrl));
            StartActivity(i);
        }

        void closeBtn_Click(object sender, EventArgs e)
        {
            Finish();
        }

    }
}