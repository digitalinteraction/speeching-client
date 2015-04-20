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
using System.IO;

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
        LinearLayout loadingContainer;
        FrameLayout fragmentContainer;

        bool recording = false;
        int taskIndex = 0;
        string localTempDirectory;
        AssessmentTask[] tasks;
        AndroidUtils.RecordAudioManager audioManager;

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

            localTempDirectory = AppData.cacheDir + "/assessment";

            if(!Directory.Exists(localTempDirectory)) Directory.CreateDirectory(localTempDirectory);

            preambleContainer = FindViewById<LinearLayout>(Resource.Id.preamble_container);
            preambleContainer.Visibility = ViewStates.Visible;
            loadingContainer = FindViewById<LinearLayout>(Resource.Id.assessment_loading);
            loadingContainer.Visibility = ViewStates.Gone;
            fragmentContainer = FindViewById<FrameLayout>(Resource.Id.fragment_container);
            fragmentContainer.Visibility = ViewStates.Gone;

            helpButton = FindViewById<ImageView>(Resource.Id.assessment_info);
            helpButton.Click += helpButton_Click;

            //TODO read from JSON

            FindViewById<TextView>(Resource.Id.assessment_preamble).Text = "Doing this short assessment will help us determine which parts of your speech might need some practice!";

            tasks = new AssessmentTask[2];

            tasks[0] = new QuickFireFragment(new string[]{  });
            tasks[1] = new AssessmentImgDescFragment(AppData.cacheDir + "/wikiImage.jpg", 
                new string[]{});
        }

        protected override void OnPause()
        {
            base.OnPause();
            if(currentStage == AssessmentStage.Running)
            {
                FragmentManager.BeginTransaction().Remove(tasks[taskIndex]);
            }
            tasks = null;

            if(audioManager != null)
            {
                if(recording)
                {
                    StopRecording();
                }
                audioManager.CleanUp();
                audioManager = null;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            audioManager = new AndroidUtils.RecordAudioManager(this, null);
            recording = false;
            recButton.SetBackgroundResource(Resource.Drawable.recordButtonBlue);
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

        private void StartRecording()
        {
            recording = true;
            string fileAdd = System.IO.Path.Combine(localTempDirectory,
                    tasks[taskIndex].GetRecordingId() + ".mp4");
            audioManager.StartRecording(fileAdd);
            recButton.SetBackgroundResource(Resource.Drawable.recordButtonRed);
            recButton.Text = "Stop Recording";
        }

        private void StopRecording()
        {
            audioManager.StopRecording();
            recording = false;
            recButton.SetBackgroundResource(Resource.Drawable.recordButtonBlue);
            recButton.Text = "Record";
        }

        void recButton_Click(object sender, EventArgs e)
        {
            if(currentStage == AssessmentStage.Preamble)
            {
                StartAssessment();
                return;
            }

            if(recording)
            {
                // Stop recording and move onto next task/finish
                StopRecording();

                if (!tasks[taskIndex].IsFinished())
                {
                    tasks[taskIndex].NextAction();
                }
                else
                {
                    FragmentManager.BeginTransaction().Remove(tasks[taskIndex]).Commit();
                    taskIndex++;

                    if (taskIndex < tasks.Length)
                    {
                        loadingContainer.Visibility = ViewStates.Visible;
                        fragmentContainer.Visibility = ViewStates.Gone;

                        FragmentManager.BeginTransaction().Remove(tasks[taskIndex - 1]);
                        FragmentManager.BeginTransaction().Add(Resource.Id.fragment_container, tasks[taskIndex]).Commit();

                        assessmentType.Text = tasks[taskIndex].GetTitle();

                        loadingContainer.Visibility = ViewStates.Gone;
                        fragmentContainer.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        FinishAssessment();
                    }
                }
            }
            else
            {
                StartRecording();
            }
            
        }
    }
}