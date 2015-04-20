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
        private enum AssessmentStage { Preamble, Running, Finished }

        AssessmentStage currentStage = AssessmentStage.Preamble;

        Button recButton;
        ImageView helpButton;
        TextView assessmentType;
        LinearLayout preambleContainer;
        FrameLayout fragmentContainer;

        int taskIndex = 0;
        AssessmentTask[] tasks;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);

            base.OnCreate(bundle);

            SetContentView(Resource.Layout.AssessmentActivity);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            recButton = FindViewById<Button>(Resource.Id.assessment_startBtn);
            recButton.Text = "Begin";
            recButton.Click += recButton_Click;

            assessmentType = FindViewById<TextView>(Resource.Id.assessment_type);
            assessmentType.Text = "";

            preambleContainer = FindViewById<LinearLayout>(Resource.Id.preamble_container);
            preambleContainer.Visibility = ViewStates.Visible;
            fragmentContainer = FindViewById<FrameLayout>(Resource.Id.fragment_container);
            fragmentContainer.Visibility = ViewStates.Gone;

            helpButton = FindViewById<ImageView>(Resource.Id.assessment_info);
            helpButton.Click += helpButton_Click;

            //TODO read from JSON

            FindViewById<TextView>(Resource.Id.assessment_preamble).Text = "Doing this short assessment will help us determine which parts of your speech might need some practice!";

            tasks = new AssessmentTask[2];

            tasks[0] = new QuickFireFragment(new string[]{ "first", "second", "third", "last" });
            tasks[1] = new AssessmentImgDescFragment(AppData.cacheDir + "/wikiImage.jpg", 
                new string[]{"What does the image show?", "Describe the colours of the image", "Describe an object within the image"});
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            string title = (currentStage == AssessmentStage.Running) ? tasks[taskIndex].GetTitle() : "Information";
            string message = (currentStage == AssessmentStage.Running) ? tasks[taskIndex].GetInstructions() :
                "If you need help during the assessment, this will show information about how to complete the current task!";

            AlertDialog alert = new AlertDialog.Builder(this)
                .SetTitle(title)
                .SetMessage(message)
                .SetPositiveButton("Got it", (args1, args2) => { })
                .Create();
            alert.Show();
        }

        private void StartAssessment()
        {
            preambleContainer.Visibility = ViewStates.Gone;

            FragmentManager.BeginTransaction().Add(Resource.Id.fragment_container, tasks[0]).Commit();
            fragmentContainer.Visibility = ViewStates.Visible;
            assessmentType.Text = tasks[0].GetTitle();

            recButton.Text = "Record";

            currentStage = AssessmentStage.Running;
        }

        private void FinishAssessment()
        {
            // TODO
            Toast.MakeText(this, "Assessment complete!", ToastLength.Long).Show();
            currentStage = AssessmentStage.Finished;
        }

        void recButton_Click(object sender, EventArgs e)
        {
            if(currentStage == AssessmentStage.Preamble)
            {
                StartAssessment();
                return;
            }

            if (!tasks[taskIndex].IsFinished())
            {
                tasks[taskIndex].NextAction();
            }
            else
            {
                FragmentManager.BeginTransaction().Remove(tasks[taskIndex]).Commit();
                taskIndex++;

                if(taskIndex < tasks.Length)
                {
                    FragmentManager.BeginTransaction().Add(Resource.Id.fragment_container, tasks[taskIndex]).Commit();
                    assessmentType.Text = tasks[taskIndex].GetTitle();
                }
                else
                {
                    FinishAssessment();
                }
            }
        }
    }
}