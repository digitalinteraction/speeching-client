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
using System.Threading;
using Droid_PeopleWithParkinsons.Shared;

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