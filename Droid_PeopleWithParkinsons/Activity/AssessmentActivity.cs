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
using SpeechingCommon;

namespace DroidSpeeching
{
    [Activity(Label = "Assessment Activity", ParentActivity = typeof(MainActivity))]
    public class AssessmentActivity : ActionBarActivity
    {
        Button recButton;

        int taskIndex = 0;
        AssessmentTask[] tasks;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);

            base.OnCreate(bundle);

            SetContentView(Resource.Layout.AssessmentActivity);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            recButton = FindViewById<Button>(Resource.Id.assessment_startBtn);
            recButton.Click += recButton_Click;

            FindViewById<ImageView>(Resource.Id.assessment_info).Click += AssessmentActivity_Click;

            tasks = new AssessmentTask[2];

            tasks[0] = new QuickFireFragment(new string[]{ "first", "second", "third", "last" });
            tasks[1] = new AssessmentImgDescFragment(AppData.cacheDir + "/wikiImage.jpg");

            FragmentManager.BeginTransaction().Add(Resource.Id.fragment_container, tasks[taskIndex]).Commit();
        }

        void AssessmentActivity_Click(object sender, EventArgs e)
        {
            AlertDialog alert = new AlertDialog.Builder(this)
                .SetTitle("Instructions for " + tasks[taskIndex].GetTitle())
                .SetMessage(tasks[taskIndex].GetInstructions())
                .SetPositiveButton("Got it", (args1, args2) => { })
                .Create();
            alert.Show();
        }

        void recButton_Click(object sender, EventArgs e)
        {
            if (!tasks[taskIndex].IsFinished())
            {
                tasks[taskIndex].NextAction();
            }
            else
            {
                FragmentManager.BeginTransaction().Remove(tasks[taskIndex]).Commit();
                taskIndex++;

                if(taskIndex < tasks.Length)
                FragmentManager.BeginTransaction().Add(Resource.Id.fragment_container, tasks[taskIndex]).Commit();
            }
        }
    }
}