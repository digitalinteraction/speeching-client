using Android.App;
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

        /// <summary>
        /// Existing data wasn't found/failed to load so get details from the server
        /// </summary>
        private async void CreateData()
        {
            try
            {
                 // FUDGE USER DATA - TODO
                AppData.session.currentUser.id = "aUserId";

                await ServerData.FetchCategories();

                Toast.MakeText(this, "Downloaded data!", ToastLength.Short).Show();

                StartActivity(typeof(MainActivity));
            }
            catch(Exception e)
            {
                Console.WriteLine("Err: " + e);
            }
           
        }
    }
}