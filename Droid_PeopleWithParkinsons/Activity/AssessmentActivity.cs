using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using ICSharpCode.SharpZipLib.Zip;
using SpeechingShared;

namespace DroidSpeeching
{
    [Activity(Label = "Speeching Assessment", ParentActivity = typeof (MainActivity))]
    public class AssessmentActivity : ActionBarActivity
    {
        private const int MinimumMillis = 500; //Mediarecorder has a minimum record time
        private Assessment assessment;
        private TextView assessmentType;
        private AndroidUtils.RecordAudioManager audioManager;
        private AssessmentFragment currentFragment;
        private AssessmentStage currentStage = AssessmentStage.Preamble;
        private FrameLayout fragmentContainer;
        private ImageView helpButton;
        private LinearLayout loadingContainer;
        private string localTempDirectory;
        private LinearLayout preambleContainer;
        private ISharedPreferences prefs;
        private Button recButton;
        private bool recording;
        private ScenarioResult results;
        private int taskIndex;
        private IAssessmentTask[] tasks;
        private DateTime timeRecStarted;
        private string zipPath;
        private Dictionary<ServerData.TaskType, ActivityHelp> helpers; 

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

            localTempDirectory = AppData.Cache.Path + "/assessment";

            if (!Directory.Exists(localTempDirectory)) Directory.CreateDirectory(localTempDirectory);

            preambleContainer = FindViewById<LinearLayout>(Resource.Id.preamble_container);
            preambleContainer.Visibility = ViewStates.Gone;
            loadingContainer = FindViewById<LinearLayout>(Resource.Id.assessment_loading);
            loadingContainer.Visibility = ViewStates.Gone;
            fragmentContainer = FindViewById<FrameLayout>(Resource.Id.fragment_container);
            fragmentContainer.Visibility = ViewStates.Gone;

            helpButton = FindViewById<ImageView>(Resource.Id.assessment_info);
            helpButton.Click += helpButton_Click;
            helpButton.Visibility = ViewStates.Gone;

            AndroidUtils.ShowMicDistancePrompt(this);

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

            int id = Intent.GetIntExtra("ActivityId", -1);

            if (id >= 0)
            {
                assessment = (Assessment)await AppData.Session.FetchActivityWithId(id);
            }
            else
            {
                assessment = await ServerData.FetchAssessment();
            }

            if (assessment == null)
            {
                RunOnUiThread(() =>
                {
                    dialog.Hide();
                    SelfDestruct("It looks like you're offline...",
                        "Oops! We couldn't download your assessment - check your internet connection and try again later!");
                });
                return;
            }

            helpers = await assessment.PrepareTasks();

            FindViewById<TextView>(Resource.Id.assessment_preamble).Text = assessment.Description;

            tasks = assessment.AssessmentTasks;
            zipPath = Path.Combine(AppData.Exports.Path, assessment.Id + "_assessmentRes.zip");
            results = new ScenarioResult(assessment.Id, zipPath) { IsAssessment = true };

            if (bundle != null)
            {
                var previous = bundle.GetInt("ASSESSMENT_PROGRESS", -1);

                if (previous >= 0 && previous < tasks.Length)
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
            }
            else
            {
                currentStage = AssessmentStage.Preamble;
                preambleContainer.Visibility = ViewStates.Visible;
                recButton.Visibility = ViewStates.Visible;
                helpButton.Visibility = ViewStates.Gone;
            }

            RunOnUiThread(() => { dialog.Hide(); });
        }

        /// <summary>
        /// Unrecoverable error - show a dialog and return to the previous practiceActivity. Can be called by child fragments
        /// </summary>
        public void SelfDestruct(string title = "Fatal error",
            string message = "Oops! Something terrible has happened - sorry about that. Closing the assessment.")
        {
            Android.Support.V7.App.AlertDialog errDialog = new Android.Support.V7.App.AlertDialog.Builder(this)
                .SetTitle(title)
                .SetMessage(message)
                .SetCancelable(false)
                .SetPositiveButton("Ok", (arg1, arg2) => { Finish(); })
                .Create();
            errDialog.Show();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            if (currentStage == AssessmentStage.Running)
            {
                outState.PutInt("ASSESSMENT_PROGRESS", taskIndex);
                outState.PutInt("TASK_PROGRESS", currentFragment.GetCurrentStage());
            }

            base.OnSaveInstanceState(outState);
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (currentStage == AssessmentStage.Running)
            {
                FragmentManager.BeginTransaction().Remove(currentFragment);
            }
            tasks = null;

            if (audioManager == null) return;

            if (recording)
            {
                StopRecording();
            }
            audioManager.CleanUp();
            audioManager = null;
        }

        protected override void OnResume()
        {
            base.OnResume();
            audioManager = new AndroidUtils.RecordAudioManager(this);
            recording = false;
            recButton.SetBackgroundResource(Resource.Drawable.recordButtonBlue);
        }

        private void ShowInfo()
        {
            ActivityHelp help = currentFragment.GetHelp();

            VideoPlayerFragment helpVidFragment = new VideoPlayerFragment(help.HelpVideo, help.ActivityName, help.ActivityDescription);
            helpVidFragment.Show(SupportFragmentManager, "video_helper");

            helpVidFragment.StartVideo();
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            ShowInfo();
        }

        private void StartAssessment(int startPosition = -1)
        {
            preambleContainer.Visibility =
                (Resources.Configuration.Orientation == Android.Content.Res.Orientation.Landscape)
                    ? ViewStates.Invisible
                    : ViewStates.Gone;
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
                FragmentManager.BeginTransaction()
                    .Add(Resource.Id.fragment_container, currentFragment, "ASSESSMENT_TASK")
                    .Commit();
            }

            fragmentContainer.Visibility = ViewStates.Visible;

            // If the device was rotated, the fragment might already exist and the data will be ready
            if (currentFragment.finishedCreating)
            {
                assessmentType.Text = currentFragment.GetTitle();
            }
            // Otherwise, the fragment's data is currently waiting to be unbundled and so actions should be queued
            else
            {
                currentFragment.runOnceCreated.Push(
                    delegate { RunOnUiThread(() => { assessmentType.Text = currentFragment.GetTitle(); }); });
            }

            recButton.Text = "Record";
        }

        private async void FinishAssessment()
        {
            currentStage = AssessmentStage.Finished;
            ProgressDialog progress;
            AppCompatDialog ratingsDialog = null;
            bool rated = false;

            RunOnUiThread(() =>
            {
                progress = new ProgressDialog(this);
                progress.SetTitle("Assessment Complete!");
                progress.SetMessage("Getting your recordings ready to upload...");

                ratingsDialog = new Android.Support.V7.App.AlertDialog.Builder(this)
                    .SetTitle("Assessment Complete!")
                    .SetMessage("How do you feel you did?")
                    .SetView(Resource.Layout.RatingDialog)
                    .SetCancelable(false)
                    .SetPositiveButton("Done", (par1, par2) =>
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        // ReSharper disable once AccessToModifiedClosure
                        RatingBar rating = ratingsDialog.FindViewById<RatingBar>(Resource.Id.dialogRatingBar);
                        results.UserRating = rating.Rating;
                        progress.Show();
                        rated = true;
                    })
                    .Create();

                ratingsDialog.Show();
            });

            FastZip fastZip = new FastZip();
            fastZip.CreateZip(zipPath, localTempDirectory, true, null);
            Directory.Delete(localTempDirectory, true);

            while (!rated)
            {
                await Task.Delay(200);
            }

            results.CompletionDate = DateTime.Now;
            AppData.Session.ResultsToUpload.Add(results);
            AppData.SaveCurrentData();
            StartActivity(typeof (UploadsActivity));
            Finish();
        }

        private void StartRecording()
        {
            timeRecStarted = DateTime.Now;
            recording = true;
            string fileAdd = Path.Combine(localTempDirectory,
                currentFragment.GetRecordingPath() + ".mp4");
            audioManager.StartRecording(fileAdd);
            recButton.SetBackgroundResource(Resource.Drawable.recordButtonRed);
            recButton.Text = "Stop Recording";
        }

        private void StopRecording()
        {
            audioManager.StopRecording();
            results.Data.Add(new ParticipantResultData
            {
                FilePath = currentFragment.GetRecordingPath() + ".mp4",
                ParticipantAssessmentTaskId = tasks[taskIndex].Id,
                ParticipantAssessmentTaskPromptId =
                    currentFragment.GetTask().PromptCol.Prompts[currentFragment.GetCurrentStage()].Id
            });
            recording = false;
            recButton.SetBackgroundResource(Resource.Drawable.recordButtonBlue);
            recButton.Text = "Start Recording";
        }

        private void SetNewFragment()
        {
            Type thisType = tasks[taskIndex].GetType();

            if (thisType == typeof (QuickFireTask))
            {
                currentFragment = QuickFireFragment.NewInstance(tasks[taskIndex], helpers[tasks[taskIndex].TaskType]);
            }
            else if (thisType == typeof (ImageDescTask))
            {
                currentFragment = ImageDescFragment.NewInstance(tasks[taskIndex], helpers[tasks[taskIndex].TaskType]);
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

            if (currentStage == AssessmentStage.Preamble)
            {
                StartAssessment();
                return;
            }

            if (recording)
            {
                TimeSpan recTime = DateTime.Now - timeRecStarted;

                if (recTime.TotalMilliseconds < MinimumMillis) return;

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

                        if (currentFragment != null)
                        {
                            FragmentManager.BeginTransaction()
                                .Replace(Resource.Id.fragment_container, currentFragment, "ASSESSMENT_TASK")
                                .Commit();

                            currentFragment.runOnceCreated.Push(
                            delegate { RunOnUiThread(() => { assessmentType.Text = currentFragment.GetTitle(); }); });
                        }

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

        private enum AssessmentStage
        {
            Preparing,
            Preamble,
            Running,
            Finished
        }
    }
}