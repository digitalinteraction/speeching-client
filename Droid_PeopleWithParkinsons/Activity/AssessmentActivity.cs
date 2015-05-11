using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PCLStorage;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using SpeechingShared;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace DroidSpeeching
{
    [Activity(Label = "Assessment Activity", ParentActivity = typeof(MainActivity))]
    public class AssessmentActivity : ActionBarActivity
    {
        private enum AssessmentStage { Preparing, Preamble, Running, Finished }

        AssessmentStage currentStage = AssessmentStage.Preamble;

        Button recButton;
        ImageView helpButton;
        TextView assessmentType;
        LinearLayout preambleContainer;
        LinearLayout loadingContainer;
        FrameLayout fragmentContainer;

        Assessment assessment;
        string zipPath;
        ScenarioResult results;
        bool recording = false;
        int taskIndex = 0;
        string localTempDirectory;
        IAssessmentTask[] tasks;
        ISharedPreferences prefs;

        DateTime timeRecStarted;
        int minimumMillis = 500; //Mediarecorder has a minimum record time

        AssessmentFragment currentFragment;

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
            recButton.Visibility = ViewStates.Gone;

            assessmentType = FindViewById<TextView>(Resource.Id.assessment_type);
            assessmentType.Text = "";

            localTempDirectory = AppData.cache.Path + "/assessment";

            if(!Directory.Exists(localTempDirectory)) Directory.CreateDirectory(localTempDirectory);

            preambleContainer = FindViewById<LinearLayout>(Resource.Id.preamble_container);
            preambleContainer.Visibility = ViewStates.Gone;
            loadingContainer = FindViewById<LinearLayout>(Resource.Id.assessment_loading);
            loadingContainer.Visibility = ViewStates.Gone;
            fragmentContainer = FindViewById<FrameLayout>(Resource.Id.fragment_container);
            fragmentContainer.Visibility = ViewStates.Gone;

            helpButton = FindViewById<ImageView>(Resource.Id.assessment_info);
            helpButton.Click += helpButton_Click;
            helpButton.Visibility = ViewStates.Gone;

            LoadData(bundle);
        }

        private async void LoadData(Bundle bundle)
        {
            ProgressDialog dialog = null;

            currentStage = AssessmentStage.Preparing;

            RunOnUiThread(() =>
            {
                dialog = new ProgressDialog(this);
                dialog.SetTitle("Downloading...");
                dialog.SetMessage("Please wait while we prepare your assessment.");
                dialog.SetCancelable(false);
                dialog.Show();
            });

            assessment = await ServerData.FetchAssessment();
            
            if(assessment == null)
            {
                RunOnUiThread(() =>
                {
                    dialog.Hide();
                    SelfDestruct("It looks like you're offline...", "Oops! We couldn't download your assessment - check your internet connection and try again later!");
                });
                return;
            }

            FindViewById<TextView>(Resource.Id.assessment_preamble).Text = assessment.description;

            tasks = assessment.tasks;
            zipPath = Path.Combine(AppData.exports.Path, assessment.id + "_assessmentRes.zip");
            results = new ScenarioResult(assessment.id, zipPath, AppData.session.currentUser.id);
            results.isAssessment = true;

            int previous = -1;
            
            if(bundle != null)
            {
                previous = bundle.GetInt("ASSESSMENT_PROGRESS", -1);
            }

            if(previous >= 0 && previous < tasks.Length)
            {
                taskIndex = previous;
                StartAssessment(bundle.GetInt("TASK_PROGRESS", -1));
            }
            else
            {
                currentStage = AssessmentStage.Preamble;
                preambleContainer.Visibility = ViewStates.Visible;
                recButton.Visibility = ViewStates.Visible;
                helpButton.Visibility = ViewStates.Visible;
            }

            RunOnUiThread(() =>
            {
                dialog.Hide();
            });
        }

        /// <summary>
        /// Unrecoverable error - show a dialog and return to the previous activity. Can be called by child fragments
        /// </summary>
        public void SelfDestruct(string title = "Fatal error", string message = "Oops! Something terrible has happened - sorry about that. Closing the assessment.")
        {
            AlertDialog errDialog = new AlertDialog.Builder(this)
                .SetTitle(title)
                .SetMessage(message)
                .SetCancelable(false)
                .SetPositiveButton("Ok", (arg1, arg2) => { Finish(); })
                .Create();
            errDialog.Show();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            if(currentStage == AssessmentStage.Running)
            {
                outState.PutInt("ASSESSMENT_PROGRESS", taskIndex);
                outState.PutInt("TASK_PROGRESS", currentFragment.GetCurrentStage());
            }
            
            base.OnSaveInstanceState(outState);
        }

        protected override void OnPause()
        {
            base.OnPause();
            if(currentStage == AssessmentStage.Running)
            {
                FragmentManager.BeginTransaction().Remove(currentFragment);
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

        private void ShowInfo()
        {
            string title = (currentStage == AssessmentStage.Running) ? currentFragment.GetTitle() : "Information";
            string message = (currentStage == AssessmentStage.Running) ? currentFragment.GetInstructions() :
                "If you need help during the assessment, this will show information about how to complete the current task!";

            AlertDialog alert = new AlertDialog.Builder(this)
                .SetTitle(title)
                .SetMessage(message)
                .SetPositiveButton("Got it", (args1, args2) => { })
                .Create();

            RunOnUiThread(() =>
            {
                alert.Show();
            }); 
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            ShowInfo();
        }

        private void StartAssessment(int startPosition = -1)
        {
            preambleContainer.Visibility = 
                (Resources.Configuration.Orientation == Android.Content.Res.Orientation.Landscape)? ViewStates.Invisible : ViewStates.Gone;
            recButton.Visibility = ViewStates.Visible;
            helpButton.Visibility = ViewStates.Visible;

            currentStage = AssessmentStage.Running;

            if (startPosition >= 0)
            {
                currentFragment = FragmentManager.FindFragmentByTag<AssessmentFragment>("ASSESSMENT_TASK");
                currentFragment.GoToStage(startPosition);
            }
            else
            {
                SetNewFragment();
                FragmentManager.BeginTransaction().Add(Resource.Id.fragment_container, currentFragment, "ASSESSMENT_TASK").Commit();
            }
            
            fragmentContainer.Visibility = ViewStates.Visible;

            // If the device was rotated, the fragment might already exist and the data will be ready
            if(currentFragment.finishedCreating)
            {
                assessmentType.Text = currentFragment.GetTitle();
            }
            // Otherwise, the fragment's data is currently waiting to be unbundled and so actions should be queued
            else
            {
                currentFragment.runOnceCreated.Push(delegate
                {
                    RunOnUiThread(() =>
                    {
                        assessmentType.Text = currentFragment.GetTitle();
                    });
                });
            }

            recButton.Text = "Record";
        }

        private void FinishAssessment()
        {
            currentStage = AssessmentStage.Finished;

            ProgressDialog progress = null;

            RunOnUiThread(() => 
            {
                progress = new ProgressDialog(this);
                progress.SetTitle("Assessment Complete!");
                progress.SetMessage("Getting your recordings ready to upload...");
                progress.Show();
            });

            try
            {
                FastZip fastZip = new FastZip();
                bool recurse = true;
                fastZip.CreateZip(zipPath, localTempDirectory, recurse, null);

                results.CompletionDate = DateTime.Now;
                AppData.session.resultsToUpload.Add(results);
                AppData.SaveCurrentData();

                Directory.Delete(localTempDirectory, true);
                StartActivity(typeof(UploadsActivity));
                this.Finish();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void StartRecording()
        {
            timeRecStarted = DateTime.Now;
            recording = true;
            string fileAdd = Path.Combine(localTempDirectory,
                    currentFragment.GetRecordingId() + ".mp4");
            audioManager.StartRecording(fileAdd);
            recButton.SetBackgroundResource(Resource.Drawable.recordButtonRed);
            recButton.Text = "Stop Recording";
        }

        private void StopRecording()
        {
            audioManager.StopRecording();
            results.ParticipantTaskIdResults.Add(currentFragment.GetRecordingId(), currentFragment.GetRecordingId() + ".mp4");
            recording = false;
            recButton.SetBackgroundResource(Resource.Drawable.recordButtonBlue);
            recButton.Text = "Record";
        }

        private void SetNewFragment()
        {
            Type thisType = tasks[taskIndex].GetType();

            if(thisType == typeof(QuickFireTask))
            {
                currentFragment = QuickFireFragment.NewInstance(tasks[taskIndex]);
            }
            else if(thisType == typeof(ImageDescTask))
            {
                currentFragment = ImageDescFragment.NewInstance(tasks[taskIndex]);
            }
            else
            {
                currentFragment = null;
                return;
            }

            if (!GetPrefs().GetBoolean(thisType.ToString(), false))
            {
                currentFragment.runOnceCreated.Push(delegate { ShowInfo(); });
                ISharedPreferencesEditor editor = GetPrefs().Edit();
                editor.PutBoolean(thisType.ToString(), true);
                editor.Apply();
            }
        }

        private ISharedPreferences GetPrefs()
        {
            if (prefs == null)
            {
                prefs = GetSharedPreferences("ASSESSMENTS", FileCreationMode.MultiProcess);
            }

            return prefs;
        }

        private void recButton_Click(object sender, EventArgs e)
        {
            if (currentStage == AssessmentStage.Preparing) return;

            if(currentStage == AssessmentStage.Preamble)
            {
                StartAssessment();
                return;
            }

            if(recording)
            {
                TimeSpan recTime = DateTime.Now - timeRecStarted;

                if (recTime.TotalMilliseconds < minimumMillis) return;

                // Stop recording and move onto next task/finish
                StopRecording();

                if (!currentFragment.IsFinished())
                {
                    currentFragment.NextAction();
                }
                else
                {
                    taskIndex++;

                    if (taskIndex < tasks.Length)
                    {
                        loadingContainer.Visibility = ViewStates.Visible;
                        fragmentContainer.Visibility = ViewStates.Gone;

                        currentFragment = null;
                        SetNewFragment();

                        if(currentFragment != null)
                        {
                            FragmentManager.BeginTransaction().Replace(Resource.Id.fragment_container, currentFragment, "ASSESSMENT_TASK").Commit();
                        }

                        currentFragment.runOnceCreated.Push(delegate
                        {
                            RunOnUiThread(() =>
                            {
                                assessmentType.Text = currentFragment.GetTitle();
                            });
                        });

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