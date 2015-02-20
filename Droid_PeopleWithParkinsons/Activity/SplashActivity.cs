using Android.App;
using Android.OS;
using SpeechingCommon;
using System.Collections.Generic; 

namespace Droid_PeopleWithParkinsons
{
    [Activity(Theme = "@style/Theme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            AppData.session = new SessionData();
            AppData.session.scenarios = new List<Scenario>();
            AppData.session.resultsToUpload = new List<ResultItem>();
            AppData.session.currentUser = new User();
            AppData.session.currentUser.id = "aUserId";

            StartActivity(typeof(MainActivity));
        }
    }
}