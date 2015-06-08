using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Preferences;
using Android.Provider;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Support.V7.Graphics;
using Android.Views;
using Android.Widget;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using SpeechingShared;

namespace DroidSpeeching
{
    [Activity(Label = "Scenario", ParentActivity = typeof (MainActivity),
        LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
    public class ScenarioActivity : ActionBarActivity, Palette.IPaletteAsyncListener
    {
        private readonly int minimumMillis = 500; //Mediarecorder has a minimum record time
        private AndroidUtils.RecordAudioManager audioManager;
        private TextView authorName;
        private bool autoSpeak = true;
        private View breakerView;
        private bool canSpeak;
        private ImageView choiceImage1;
        private ImageView choiceImage2;
        // Event choice layout
        private LinearLayout choiceLayout;
        private TextView choicePrompt;
        private int currIndex = -1;
        private ImageView eventImage;
        // Scenario event screen
        private LinearLayout eventLayout;
        private TextView eventPrompt;
        private TextView eventTranscript;
        private VideoView eventVideo;
        private string helpText;
        private TextView inputHint;
        private string localResourcesDirectory;
        private string localTempDirectory;
        private string localZipPath;
        private Button mainButton;
        // Event main layout
        private RelativeLayout mainLayout;
        private MediaPlayer mediaPlayer;
        private ISharedPreferences prefs;
        private ProgressDialog progress;
        private bool recording;
        private Dictionary<string, string> resources;
        private ScenarioResult results;
        private string resultsZipPath;
        private Scenario scenario;
        private TextView scenarioTitle;
        private Button startButton;
        private DateTime timeRecStarted;
        // Scenario title screen
        private RelativeLayout titleLayout;
        private TTSManager tts;

        /// <summary>
        /// Change the actionbar/window colour to reflect the colours in the main image
        /// </summary>
        /// <param name="palette"></param>
        public void OnGenerated(Palette palette)
        {
            Color vibrantDark = new Color(palette.GetDarkVibrantColor(Resource.Color.appMain));

            SupportActionBar.SetBackgroundDrawable(new ColorDrawable(vibrantDark));
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            breakerView.SetBackgroundColor(vibrantDark);
            Window.SetStatusBarColor(vibrantDark);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ScenarioActivity);
            choiceLayout = FindViewById<LinearLayout>(Resource.Id.scenarioChoiceLayout);
            choicePrompt = FindViewById<TextView>(Resource.Id.scenarioChoicePrompt);
            choiceImage1 = FindViewById<ImageView>(Resource.Id.scenarioChoice1);
            choiceImage1.Click += ChoiceImageClicked;
            choiceImage2 = FindViewById<ImageView>(Resource.Id.scenarioChoice2);
            choiceImage2.Click += ChoiceImageClicked;

            breakerView = FindViewById(Resource.Id.scenarioBreaker);

            eventLayout = FindViewById<LinearLayout>(Resource.Id.scenarioEventLayout);
            eventTranscript = FindViewById<TextView>(Resource.Id.scenarioText);
            eventTranscript.Click += SpeakableText_Click;

            mainLayout = FindViewById<RelativeLayout>(Resource.Id.scenarioRecordLayout);
            eventImage = FindViewById<ImageView>(Resource.Id.scenarioImage);
            eventVideo = FindViewById<VideoView>(Resource.Id.scenarioVideo);
            eventPrompt = FindViewById<TextView>(Resource.Id.scenarioPrompt);
            eventPrompt.Click += SpeakableText_Click;

            mainButton = FindViewById<Button>(Resource.Id.scenarioProgressBtn);
            mainButton.Click += MainButtonClicked;

            titleLayout = FindViewById<RelativeLayout>(Resource.Id.scenarioTitleLayout);
            scenarioTitle = FindViewById<TextView>(Resource.Id.scenarioTitle);
            authorName = FindViewById<TextView>(Resource.Id.scenarioAuthor);
            startButton = FindViewById<Button>(Resource.Id.scenarioStartBtn);
            startButton.Click += delegate
            {
                titleLayout.Visibility = ViewStates.Gone;
                eventLayout.Visibility = ViewStates.Visible;
                ShowNextEvent();
            };

            inputHint = FindViewById<TextView>(Resource.Id.scenarioPromptHead);

            titleLayout.Visibility = ViewStates.Visible;
            eventLayout.Visibility = ViewStates.Gone;

            helpText = "This is a short practiceActivity which may require you to perform multiple simple tasks." +
                       "\nOnce you have completed it, you'll be given the chance to upload your results for others to analyse and give you feedback." +
                       "\nPress the Start button to begin!";

            InitialiseData(savedInstanceState);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.scenarioActivityActions, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        private void SpeakableText_Click(object sender, EventArgs e)
        {
            if (!canSpeak) return;
            TextView textView = sender as TextView;
            if (textView != null) tts.SayLine(textView.Text, null, true);
        }

        private async void InitialiseData(Bundle savedInstanceState)
        {
            // Load the scenario with the id that was given inside the current intent
            scenario = (Scenario) await AppData.Session.FetchActivityWithId(Intent.GetIntExtra("ActivityId", 0));

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            string scenarioFormatted = scenario.Title.Replace(" ", string.Empty).Replace("/", string.Empty);

            localResourcesDirectory = AppData.Cache.Path + scenarioFormatted;
            localTempDirectory = localResourcesDirectory + "/temp";

            // If the scenario folder doesn't exist we need to download the additional files
            if (!GetPrefs().GetBoolean("DOWNLOADED", false) || !Directory.Exists(localResourcesDirectory))
            {
                if (!Directory.Exists(localResourcesDirectory))
                {
                    Directory.CreateDirectory(localResourcesDirectory);
                    Directory.CreateDirectory(localTempDirectory);
                }

                localZipPath = System.IO.Path.Combine(AppData.Exports.Path, scenarioFormatted + ".zip");
                try
                {
                    PrepareData();
                }
                catch (Exception except)
                {
                    Console.Write(except);
                    Directory.Delete(localResourcesDirectory, true);
                }
            }
            else
            {
                // We need to populate the resources dictionary with the existing files
                string[] files = Directory.GetFiles(localResourcesDirectory);
                resources = new Dictionary<string, string>();

                for (int i = 0; i < files.Length; i++)
                {
                    resources.Add(System.IO.Path.GetFileName(files[i]), files[i]);
                }

                //Remove existing data from the upload queue
                AppData.Session.DeleteAllPendingForScenario(scenario.Id);
            }

            scenarioTitle.Text = scenario.Title;

            if (scenario.Creator != null) authorName.Text = scenario.Creator.Name;

            resultsZipPath = System.IO.Path.Combine(AppData.Exports.Path, scenario.Id + "_final.zip");
            results = new ScenarioResult(scenario.Id, resultsZipPath, AppData.Session.CurrentUser.Id);

            if (savedInstanceState != null)
            {
                currIndex = savedInstanceState.GetInt("progress", -1);
            }

            if (currIndex < 0)
            {
                titleLayout.Visibility = ViewStates.Visible;
                eventLayout.Visibility = ViewStates.Gone;
                Title = scenario.Title;
            }
            else
            {
                titleLayout.Visibility = ViewStates.Gone;
                eventLayout.Visibility = ViewStates.Visible;

                // Resume from where we left off
                currIndex--;
                ShowNextEvent();
            }
        }

        private ISharedPreferences GetPrefs()
        {
            if (prefs == null)
            {
                prefs = GetSharedPreferences("ACT_" + scenario.Id, FileCreationMode.MultiProcess);
            }

            return prefs;
        }

        // For the home button in top left
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                NavUtils.NavigateUpFromSameTask(this);
                return true;
            }
            if (item.ItemId == Resource.Id.action_settings)
            {
                StartActivity(typeof (SettingsActivity));
                return true;
            }
            if (item.ItemId == Resource.Id.action_help)
            {
                Android.Support.V7.App.AlertDialog alert = new Android.Support.V7.App.AlertDialog.Builder(this)
                    .SetTitle("Help")
                    .SetMessage(helpText)
                    .SetPositiveButton("Confirm", (arg1, arg2) => { })
                    .SetCancelable(true)
                    .Create();
                alert.Show();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        /// <summary>
        /// Downloads the data needed to display the scenario
        /// </summary>
        private async void PrepareData()
        {
            RunOnUiThread(() => progress = ProgressDialog.Show(this, "Please Wait", "Downloading data!", true));

            // Ask the server for the scenario's tasks if they aren't present
            if (scenario.ParticipantTasks == null) await scenario.FetchTasks();

            resources = new Dictionary<string, string>();

            if (!File.Exists(localZipPath))
            {
                // Download the scenario's resource zip
                WebClient request = new WebClient();
                await request.DownloadFileTaskAsync(
                    new Uri(scenario.Resource),
                    localZipPath
                    );
                request.Dispose();
            }

            RunOnUiThread(() => progress.SetMessage("Unpacking data at " + localZipPath));

            ZipFile zip = null;
            try
            {
                //Unzip the downloaded file and add references to its contents in the resources dictionary
                zip = new ZipFile(File.OpenRead(localZipPath));

                foreach (ZipEntry entry in zip)
                {
                    string filename = System.IO.Path.Combine(localResourcesDirectory, entry.Name);
                    byte[] buffer = new byte[4096];
                    System.IO.Stream zipStream = zip.GetInputStream(entry);
                    using (FileStream streamWriter = File.Create(filename))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                    resources.Add(entry.Name, filename);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error! " + e.Message);
            }
            finally
            {
                if (zip != null)
                {
                    zip.IsStreamOwner = true;
                    zip.Close();
                }
            }

            ImageView icon = FindViewById<ImageView>(Resource.Id.scenarioIcon);
            AndroidUtils.PrepareIcon(icon, scenario);

            File.Delete(localZipPath);

            ISharedPreferencesEditor editor = GetPrefs().Edit();
            editor.PutBoolean("DOWNLOADED", true);
            editor.Apply();

            RunOnUiThread(() => progress.Hide());
        }

        protected override void OnResume()
        {
            base.OnResume();
            audioManager = new AndroidUtils.RecordAudioManager(this);

            mainButton.SetBackgroundResource(Resource.Drawable.recordButtonBlue);

            tts = new TTSManager(this, null);

            ISharedPreferences userPrefs = PreferenceManager.GetDefaultSharedPreferences(this);
            autoSpeak = userPrefs.GetBoolean("autoTTS", true);

            // Reload the resources for this stage of the scenario, incase they were lost (e.g. audio, video)
            if (currIndex >= 0)
            {
                currIndex--;
                ShowNextEvent();
            }
            else
            {
                ImageView icon = FindViewById<ImageView>(Resource.Id.scenarioIcon);
                AndroidUtils.PrepareIcon(icon, scenario);
            }
        }

        // Make sure that the recorder + player are unallocated when the app goes into the background
        protected override void OnPause()
        {
            base.OnPause();
            if (audioManager != null)
            {
                audioManager.CleanUp();
                audioManager = null;
            }
            if (mediaPlayer != null)
            {
                mediaPlayer.Release();
                mediaPlayer = null;
            }
            if (eventVideo.IsPlaying)
            {
                eventVideo.StopPlayback();
            }
            if (tts != null)
            {
                tts.Clean();
                tts = null;
            }
        }

        // Save the current index on rotation so progress isn't lost (used in createview)
        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt("progress", currIndex);
            base.OnSaveInstanceState(outState);
        }

        /// <summary>
        /// Progress to the next part of the scenario
        /// </summary>
        private void ShowNextEvent()
        {
            currIndex++;

            string ttsHelp = "Tap the text to listen to it spoken aloud!";

            mainButton.Text = "Record Response";
            mainButton.SetBackgroundResource(Resource.Drawable.recordButtonBlue);

            canSpeak = true;
            if (tts.IsSpeaking())
            {
                tts.StopSpeaking();
            }

            // Check if the scenario is complete
            if (currIndex >= scenario.ParticipantTasks.Length)
            {
                ExportRecordings();
                return;
            }

            Title = scenario.Title + " | " + (currIndex + 1) + " of " + scenario.ParticipantTasks.Length;
            inputHint.Visibility = ViewStates.Visible;

            // Use the alternative layout for giving the user a choice between 2 items
            if (scenario.ParticipantTasks[currIndex].ParticipantTaskResponse.Type == TaskResponse.ResponseType.Choice)
            {
                mainLayout.Visibility = ViewStates.Gone;
                choiceLayout.Visibility = ViewStates.Visible;

                choicePrompt.Text = scenario.ParticipantTasks[currIndex].ParticipantTaskResponse.Prompt;

                // Load the choice images // TODO allow for more choices
                if (scenario.ParticipantTasks[currIndex].ParticipantTaskResponse.Related != null)
                {
                    string choice1Key = scenario.ParticipantTasks[currIndex].ParticipantTaskResponse.Related[0];
                    if (resources.ContainsKey(choice1Key))
                    {
                        choiceImage1.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(resources[choice1Key])));
                    }
                }

                if (scenario.ParticipantTasks[currIndex].ParticipantTaskResponse.Related != null)
                {
                    string choice2Key = scenario.ParticipantTasks[currIndex].ParticipantTaskResponse.Related[1];
                    if (resources.ContainsKey(choice2Key))
                    {
                        choiceImage2.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(resources[choice2Key])));
                    }
                }

                helpText = ttsHelp +
                           "\nRead or listen to the prompt and decide which image is most likely the solution. Tap the image to make your choice!";
            }
            else
            {
                // Use the standard layout
                mainLayout.Visibility = ViewStates.Visible;
                choiceLayout.Visibility = ViewStates.Gone;

                if (scenario.ParticipantTasks[currIndex].ParticipantTaskResponse.Type == TaskResponse.ResponseType.None)
                {
                    canSpeak = false;
                    inputHint.Visibility = ViewStates.Gone;
                    eventPrompt.Text = "";
                    eventPrompt.SetTypeface(null, TypefaceStyle.Normal);
                    mainButton.Text = "Continue";

                    helpText = ttsHelp + "\nPress the Continue button to advance the practiceActivity.";
                }

                // Load text
                else if (scenario.ParticipantTasks[currIndex].ParticipantTaskResponse.Type == TaskResponse.ResponseType.Freeform)
                {
                    // Make freeform prompts italic
                    string given = scenario.ParticipantTasks[currIndex].ParticipantTaskResponse.Prompt;
                    eventPrompt.SetTypeface(null, TypefaceStyle.BoldItalic);
                    eventPrompt.Text = (given != null) ? given : "";
                    inputHint.Text = "Your response:";
                    helpText = ttsHelp +
                               "\nPress the record button follow the instruction below \"Your Response\". Speak as clearly and loud as you can!";
                }
                else
                {
                    eventPrompt.Text = scenario.ParticipantTasks[currIndex].ParticipantTaskResponse.Prompt;
                    eventPrompt.SetTypeface(null, TypefaceStyle.Normal);
                    inputHint.Text = "Please say this:";
                    helpText = ttsHelp +
                               "\nPress the record button read the text below \"Please say this\". Speak as clearly and loud as you can, trying to be as accurate to the text as possible!";
                }
            }

            eventTranscript.Text = scenario.ParticipantTasks[currIndex].ParticipantTaskContent.Text;

            if (scenario.ParticipantTasks[currIndex].ParticipantTaskContent.Type == TaskContent.ContentType.Video)
            {
                // load video instead of audio + image
                SetDefaultWindowColours();
                eventVideo.Visibility = ViewStates.Visible;
                eventImage.Visibility = ViewStates.Gone;
                string vidKey = scenario.ParticipantTasks[currIndex].ParticipantTaskContent.Visual;
                var vidUri = Android.Net.Uri.Parse(resources[vidKey]);
                eventVideo.SetVideoURI(vidUri);
                eventVideo.Start();
            }
            else
            {
                eventVideo.Visibility = ViewStates.Gone;
                eventImage.Visibility = ViewStates.Visible;

                // Load the image if it exists
                string visualKey = scenario.ParticipantTasks[currIndex].ParticipantTaskContent.Visual;
                if (visualKey != null && resources.ContainsKey(visualKey))
                {
                    Android.Net.Uri imageUri = Android.Net.Uri.FromFile(new Java.IO.File(resources[visualKey]));
                    Bitmap bitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, imageUri);

                    eventImage.SetImageBitmap(bitmap);

                    Palette.GenerateAsync(bitmap, this);
                }
                else
                {
                    SetDefaultWindowColours();
                }

                if (scenario.ParticipantTasks[currIndex].ParticipantTaskContent.Type == TaskContent.ContentType.Audio)
                {
                    // Load audio
                    string audioKey = scenario.ParticipantTasks[currIndex].ParticipantTaskContent.Audio;
                    if (audioKey != null && resources.ContainsKey(audioKey))
                    {
                        if (mediaPlayer == null)
                        {
                            mediaPlayer = new MediaPlayer();
                        }
                        else
                        {
                            mediaPlayer.Reset();
                        }
                        mediaPlayer.SetDataSource(resources[audioKey]);
                        mediaPlayer.Prepare();
                        mediaPlayer.Looping = false;
                        mediaPlayer.Start();
                    }
                    else if (autoSpeak)
                    {
                        tts.SayLine(eventTranscript.Text, null, true);
                    }
                }
                else if (autoSpeak)
                {
                    tts.SayLine(eventTranscript.Text, null, true);
                }
            }
        }

        /// <summary>
        /// Called when the user taps one of the two choices
        /// </summary>
        private void ChoiceImageClicked(object sender, EventArgs e)
        {
            int index = (sender == choiceImage1) ? 0 : 1; // TODO support more images

            results.Data.Add(new ParticipantResultData
            {
                ParticipantTaskId = scenario.ParticipantTasks[currIndex].Id,
                FilePath = scenario.ParticipantTasks[currIndex].ParticipantTaskResponse.Related[index]
            });

            ShowNextEvent();
        }

        /// <summary>
        /// Start a new recording, or if already recording finish it and progress to the next event
        /// </summary>
        private void MainButtonClicked(object sender, EventArgs e)
        {
            if (currIndex >= scenario.ParticipantTasks.Length)
            {
                ExportRecordings();
                return;
            }

            if (scenario.ParticipantTasks[currIndex].ParticipantTaskResponse.Type == TaskResponse.ResponseType.None)
            {
                // No need to record
                ShowNextEvent();
            }
            else if (recording)
            {
                TimeSpan recTime = DateTime.Now - timeRecStarted;
                if (recTime.TotalMilliseconds < minimumMillis) return;

                recording = false;
                audioManager.StopRecording();
                results.Data.Add(new ParticipantResultData
                {
                    ParticipantTaskId = scenario.ParticipantTasks[currIndex].Id,
                    FilePath = scenario.ParticipantTasks[currIndex].Id + ".mp4"
                });
                ShowNextEvent();
            }
            else
            {
                timeRecStarted = DateTime.Now;
                mainButton.SetBackgroundResource(Resource.Drawable.recordButtonRed);
                if (mediaPlayer != null)
                {
                    mediaPlayer.Stop();
                }

                if (tts.IsSpeaking()) tts.StopSpeaking();
                canSpeak = false;

                recording = true;
                string fileAdd = System.IO.Path.Combine(localTempDirectory, scenario.ParticipantTasks[currIndex].Id + ".mp4");
                audioManager.StartRecording(fileAdd);
                mainButton.Text = "Stop";
            }
        }

        /// <summary>
        /// Restarts the scenario, deleting already exported data
        /// </summary>
        private void RestartScenario()
        {
            currIndex = -1;

            string[] files = Directory.GetFiles(localTempDirectory);

            for (int i = 0; i < files.Length; i++)
            {
                File.Delete(files[i]);
            }

            titleLayout.Visibility = ViewStates.Visible;
            eventLayout.Visibility = ViewStates.Gone;
        }

        /// <summary>
        /// Exports the recordings into a zip and marks them as being ready to upload
        /// </summary>
        private async void ExportRecordings()
        {
            // Compress exported recordings into zip (Delete existing zip first)
            File.Delete(resultsZipPath);

            ProgressDialog progressDialog;
            AppCompatDialog ratingsDialog = null;
            bool rated = false;

            RunOnUiThread(() =>
            {
                progressDialog = new ProgressDialog(this);
                progressDialog.SetTitle("Scenario Complete!");
                progressDialog.SetMessage("Getting your recordings ready to upload...");

                ratingsDialog = new Android.Support.V7.App.AlertDialog.Builder(this)
                    .SetTitle("Scenario Complete!")
                    .SetMessage("How do you feel you did?")
                    .SetView(Resource.Layout.RatingDialog)
                    .SetCancelable(false)
                    .SetPositiveButton("Done", (par1, par2) =>
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        // ReSharper disable once AccessToModifiedClosure
                        RatingBar rating = ratingsDialog.FindViewById<RatingBar>(Resource.Id.dialogRatingBar);
                        results.UserRating = rating.Rating;
                        progressDialog.Show();
                        rated = true;
                    })
                    .Create();

                ratingsDialog.Show();
            });

            try
            {
                FastZip fastZip = new FastZip();
                fastZip.CreateZip(resultsZipPath, localTempDirectory, true, null);

                // Clean up zipped files
                string[] toDel = Directory.GetFiles(localTempDirectory);

                foreach (string path in toDel)
                {
                    if (path == resultsZipPath) continue;
                    File.Delete(path);
                }

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
            catch (Exception except)
            {
                Console.Write(except.Message);
            }
        }

        /// <summary>
        /// Restore the default app colours to this practiceActivity
        /// </summary>
        private void SetDefaultWindowColours()
        {
            Color appMainCol = new Color(Resource.Color.appMain);

            SupportActionBar.SetBackgroundDrawable(new ColorDrawable(appMainCol));
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            breakerView.SetBackgroundColor(appMainCol);
            Window.SetStatusBarColor(new Color(Resource.Color.appDark));
        }
    }
}