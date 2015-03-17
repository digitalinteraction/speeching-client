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

            bool gpsSuccess = CheckForGooglePlayServices();

            if (!gpsSuccess) return;

            AndroidUtils.gcm = GoogleCloudMessaging.GetInstance(this);
            AndroidUtils.GooglePlayRegId = AndroidUtils.GetGoogleRegId(this);

            if(string.IsNullOrEmpty(AndroidUtils.GooglePlayRegId))
            {
                AndroidUtils.RegisterGCM(this);
            }

            AppData.cacheDir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath + "/speeching";
            bool loaded = AppData.TryLoadExistingData();

            if(!loaded)
            {
                CreateData();
            }
            else
            {
                Toast.MakeText(this, "Loaded existing data", ToastLength.Short).Show();
                StartActivity(typeof(MainActivity));
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            CheckForGooglePlayServices();
        }

        /// <summary>
        /// Existing data wasn't found/failed to load so get details from the server
        /// </summary>
        private async void CreateData()
        {
            try
            {
                 // FUDGE USER DATA - TODO
                AppData.session.currentUser.id = 7041992;

                await ServerData.FetchCategories();

                Toast.MakeText(this, "Downloaded data!", ToastLength.Short).Show();

                StartActivity(typeof(MainActivity));
            }
            catch(Exception e)
            {
                Console.WriteLine("Err: " + e);
            }
        }

        /// <summary>
        /// Attempt a connection to Google Play Services to make sure this application is able to recieve push messages
        /// </summary>
        /// <returns></returns>
        private bool CheckForGooglePlayServices()
        {
            int resultCode = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(this);
            if(resultCode != ConnectionResult.Success)
            {
                if(GooglePlayServicesUtil.IsUserRecoverableError(resultCode))
                {
                    GooglePlayServicesUtil.GetErrorDialog(resultCode, this, AndroidUtils.PLAY_SERVICES_RESOLUTION_REQUEST).Show();
                }
                else
                {
                    AlertDialog alert = new AlertDialog.Builder(this)
                        .SetTitle("Fatal Error")
                        .SetMessage("Speeching was unable to connect to Google Play Services - your device may not be supported.")
                        .SetCancelable(false)
                        .SetPositiveButton("Close Application", (s, a) => { this.Finish(); })
                        .Show();
                }
                return false;
            }
            return true;
        }
    }
}