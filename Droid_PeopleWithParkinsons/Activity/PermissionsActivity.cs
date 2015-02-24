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
using SpeechingCommon;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "PermissionsActivity")]
    public class PermissionsActivity : Activity
    {
        ResultItem resultItem;
        FeedbackItem[] feedback;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            resultItem = AppData.FetchFeedback(Intent.GetStringExtra("ResultId"));
        }
    }
}