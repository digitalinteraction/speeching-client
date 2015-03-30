using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Gcm;
using Android.Gms.Location;
using Android.OS;
using Android.Widget;
using SpeechingCommon;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Droid_PeopleWithParkinsons
{
    [Activity( MainLauncher = true, NoHistory = true, Theme = "@style/Theme.Splash")]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Splash);

            ThreadPool.QueueUserWorkItem(o => CreateData());
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        private async Task CreateData()
        {
            try
            {
                bool successfulLoad = await AndroidUtils.InitSession(this);

                if(successfulLoad)
                    StartActivity(typeof(MainActivity));
                else
                {
                    RunOnUiThread(() => 
                        {
                            AlertDialog alert = new AlertDialog.Builder(this)
                                .SetTitle("Internet connection required!")
                                .SetMessage("We were unable to load offline data and failed to connect to the service. Please try again later.")
                                .SetCancelable(false)
                                .SetPositiveButton("Ok", (p1, p2) => { Finish(); })
                                .Create();
                            alert.Show();  
                        });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Err: " + e);
            }
        }

    }
    
}