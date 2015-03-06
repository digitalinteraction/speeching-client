using Android.App;
using Android.OS;
using Android.Widget;
using SpeechingCommon;
using System;

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

        private async void CreateData()
        {
            try
            {
                 // FUDGE DATA - TODO
                AppData.session.currentUser.id = "aUserId";

                await AppData.FetchCategories();

                AppData.SaveCurrentData();
                Toast.MakeText(this, "Created data", ToastLength.Short).Show();

                StartActivity(typeof(MainActivity));
            }
            catch(Exception e)
            {
                Console.WriteLine("Err: " + e);
            }
           
        }
    }
}