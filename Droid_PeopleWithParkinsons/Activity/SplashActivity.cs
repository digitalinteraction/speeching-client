using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Gcm;
using Android.OS;
using Android.Widget;
using SpeechingCommon;
using System;
using System.Collections.Generic;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Theme = "@style/Theme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {    
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            CreateData();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        /// <summary>
        /// Existing data wasn't found/failed to load so get details from the server
        /// </summary>
        private async void CreateData()
        {
            try
            {
                await AndroidUtils.InitSession(this);

                StartActivity(typeof(MainActivity));
            }
            catch(Exception e)
            {
                Console.WriteLine("Err: " + e);
            }
        }
    }
}