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
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DroidSpeeching
{
    [Activity(Label = "Scenario", ParentActivity=typeof(MainActivity), LaunchMode=Android.Content.PM.LaunchMode.SingleTop)]
    public class ScenarioActivity : ActionBarActivity, Palette.IPaletteAsyncListener
    {
        private Scenario scenario;

        // Scenario title screen
        private RelativeLayout titleLayout;
        private TextView scenarioTitle;
        private TextView authorName;
        private Button startButton;

        // Scenario event screen
        private LinearLayout eventLayout;
        private ImageView eventImage;
        private VideoView eventVideo;
        private TextView eventTranscript;
        private MediaPlayer mediaPlayer;

        private TextView inputHint;

        // Event main layout
        private RelativeLayout mainLayout;
        private TextView eventPrompt;
        private Button mainButton;
        private View breakerView;

        // Event choice layout
        private LinearLayout choiceLayout;
        private TextView choicePrompt;
        private ImageView choiceImage1;
        private ImageView choiceImage2;

        private string resultsZipPath;
        private ScenarioResult results;

        private Dictionary<string, string> resources;
        private ProgressDialog progress;
        private string localResourcesDirectory;
        private string localTempDirectory;
        private string localZipPath;
        private ISharedPreferences prefs;
        private string helpText;

        private int currIndex = -1;
        private AndroidUtils.RecordAudioManager audioManager;
        private bool recording = false;

        private TTSManager tts;
        private bool canSpeak = false;
        private bool autoSpeak = true;

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
            startButton.Click += delegate(object sender, EventArgs e)
            {
                titleLayout.Visibility = ViewStates.Gone;
                eventLayout.Visibility = ViewStates.Visible;
                ShowNextEvent();
            };

            inputHint = FindViewById<TextView>(Resource.Id.scenarioPromptHead);

            titleLayout.Visibility = ViewStates.Visible;
            eventLayout.Visibility = ViewStates.Gone;

            helpText = "This is a short activity which may require you to perform multiple simple tasks." +
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
            tts.SayLine((sender as TextView).Text, null, true);
        }

        private async void InitialiseData(Bundle savedInstanceState)
        {
            // Load the scenario with the id that was given inside the current intent
            scenario = (Scenario)await AppData.session.FetchActivityWithId(Intent.GetIntExtra("ActivityId", 0));

            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            string scenarioFormatted = scenario.Title.Replace(" ", String.Empty).Replace("/", String.Empty);

            localResourcesDirectory = AppData.cache.Path + scenarioFormatted;
            localTempDirectory = localResourcesDirectory + "/temp";

            // If the scenario folder doesn't exist we need to download the additional files
            if (!GetPrefs().GetBoolean("DOWNLOADED", false) || !Directory.Exists(localResourcesDirectory))
            {
                if (!Directory.Exists(localResourcesDirectory))
                {
                    Directory.CreateDirectory(localResourcesDirectory);
                    Directory.CreateDirectory(localTempDirectory);
                }

                localZipPath = System.IO.Path.Combine(AppData.exports.Path, scenarioFormatted + ".zip");
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
                AppData.session.DeleteAllPendingForScenario(scenario.Id);
            }

            scenarioTitle.Text = scenario.Title;

            if (scenario.Creator != null) authorName.Text = scenario.Creator.name;

            resultsZipPath = System.IO.Path.Combine(AppData.exports.Path, scenario.Id + "_final.zip");
            results = new ScenarioResult(scenario.Id, resultsZipPath, AppData.session.currentUser.id);

            if (savedInstanceState != null)
            {
                currIndex = savedInstanceState.GetInt("progress", -1);
            }

            if (currIndex < 0)
            {
                titleLayout.Visibility = ViewStates.Visible;
                eventLayout.Visibility = ViewStates.Gone;
                this.Title = scenario.Title;
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
            if(item.ItemId == Android.Resource.Id.Home)
            {
                NavUtils.NavigateUpFromSameTask(this);
                return true;
            }
            if (item.ItemId == Resource.Id.action_settings)
            {
                StartActivity(typeof(SettingsActivity));
                return true;
            }
            if (item.ItemId == Resource.Id.action_help)
            {
                AlertDialog alert = new AlertDialog.Builder(this)
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
            if(scenario.Tasks == null) await scenario.FetchTasks();

            resources = new Dictionary<string, string>();

            if(!File.Exists(localZipPath))
            {
                // Download the scenario's resource zip
                WebClient request = new WebClient();
                await request.DownloadFileTaskAsync(
                    new Uri(scenario.Resource),
                    localZipPath
                    );
                request.Dispose();
                request = null;
            }
        
            RunOnUiThread(() => progress.SetMessage("Unpacking data at " + localZipPath));

            ZipFile zip = null; 
            try
            {
                //Unzip the downloaded file and add references to its contents in the resources dictionary
                zip = new ZipFile(File.OpenRead(localZipPath));

                foreach(ZipEntry entry in zip)
                {
                    string filename = System.IO.Path.Combine(localResourcesDirectory, entry.Name);
                    byte[] buffer = new byte[4096];
                    System.IO.Stream zipStream = zip.GetInputStream(entry);
                    using(FileStream streamWriter = File.Create(filename))
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
                if(zip != null)
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

        protected override void OnResume ()
        {
            base.OnResume();
            audioManager = new AndroidUtils.RecordAudioManager(this, null);

            mainButton.SetBackgroundResource(Resource.Drawable.recordButtonBlue);

            tts = new TTSManager(this, null);

            ISharedPreferences userPrefs = PreferenceManager.GetDefaultSharedPreferences(this);
            autoSpeak = userPrefs.GetBoolean("autoTTS", true);

            // Reload the resources for this stage of the scenario, incase they were lost (e.g. audio, video)
            if(currIndex >= 0)
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
        protected override void OnPause ()
        {
            base.OnPause();
            if(audioManager != null)
            {
                audioManager.CleanUp();
                audioManager = null;
            }
            if(mediaPlayer != null)
            {
                mediaPlayer.Release();
                mediaPlayer = null;
            }
            if(eventVideo.IsPlaying)
            {
                eventVideo.StopPlayback();
            }
            if(tts != null)
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
            if(tts.IsSpeaking())
            {
                tts.StopSpeaking();
            }

            // Check if the scenario is complete
            if (currIndex >= scenario.Tasks.Length)
            {
                FinishScenario();
                return;
            }

            this.Title = scenario.Title + " | " + (currIndex + 1) + " of " + scenario.Tasks.Length;
            inputHint.Visibility = ViewStates.Visible;

            // Use the alternative layout for giving the user a choice between 2 items
            if (scenario.Tasks[currIndex].TaskResponse.Type == TaskResponse.ResponseType.Choice)
            {
                mainLayout.Visibility = ViewStates.Gone;
                choiceLayout.Visibility = ViewStates.Visible;

                choicePrompt.Text = scenario.Tasks[currIndex].TaskResponse.Prompt;

                // Load the choice images // TODO allow for more choices
                if (scenario.Tasks[currIndex].TaskResponse.Related != null)
                {
                    string choice1Key = scenario.Tasks[currIndex].TaskResponse.Related[0];
                    if (resources.ContainsKey(choice1Key))
                    {
                        choiceImage1.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(resources[choice1Key])));
                    }
                }

                if(scenario.Tasks[currIndex].TaskResponse.Related != null)
                {
                    string choice2Key = scenario.Tasks[currIndex].TaskResponse.Related[1];
                    if (resources.ContainsKey(choice2Key))
                    {
                        choiceImage2.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(resources[choice2Key])));
                    }
                }

                helpText = ttsHelp + "\nRead or listen to the prompt and decide which image is most likely the solution. Tap the image to make your choice!";
            }
            else
            {
                // Use the standard layout
                mainLayout.Visibility = ViewStates.Visible;
                choiceLayout.Visibility = ViewStates.Gone;

                if (scenario.Tasks[currIndex].TaskResponse.Type == TaskResponse.ResponseType.None)
                {
                    canSpeak = false;
                    inputHint.Visibility = ViewStates.Gone;
                    eventPrompt.Text = "";
                    eventPrompt.SetTypeface(null, TypefaceStyle.Normal);
                    mainButton.Text = "Continue";

                    helpText = ttsHelp + "\nPress the Continue button to advance the activity.";
                }

                // Load text
                else if (scenario.Tasks[currIndex].TaskResponse.Type == TaskResponse.ResponseType.Freeform)
                {
                    // Make freeform prompts italic
                    string given = scenario.Tasks[currIndex].TaskResponse.Prompt;
                    eventPrompt.SetTypeface(null, TypefaceStyle.BoldItalic);
                    eventPrompt.Text = (given != null) ? given : "";
                    inputHint.Text = "Your response:";
                    helpText = ttsHelp + "\nPress the record button follow the instruction below \"Your Response\". Speak as clearly and loud as you can!";
                }
                else
                {
                    eventPrompt.Text = scenario.Tasks[currIndex].TaskResponse.Prompt;
                    eventPrompt.SetTypeface(null, TypefaceStyle.Normal);
                    inputHint.Text = "Please say this:";
                    helpText = ttsHelp + "\nPress the record button read the text below \"Please say this\". Speak as clearly and loud as you can, trying to be as accurate to the text as possible!";
                }
            }

            eventTranscript.Text = scenario.Tasks[currIndex].TaskContent.Text;

            if(scenario.Tasks[currIndex].TaskContent.Type == TaskContent.ContentType.Video)
            {
                // load video instead of audio + image
                SetDefaultWindowColours();
                eventVideo.Visibility = ViewStates.Visible;
                eventImage.Visibility = ViewStates.Gone;
                string vidKey = scenario.Tasks[currIndex].TaskContent.Visual;
                var vidUri = Android.Net.Uri.Parse( resources[vidKey]);
                eventVideo.SetVideoURI(vidUri);
                eventVideo.Start();
            }
            else
            {
                eventVideo.Visibility = ViewStates.Gone;
                eventImage.Visibility = ViewStates.Visible;

                // Load the image if it exists
                string visualKey = scenario.Tasks[currIndex].TaskContent.Visual;
                if(visualKey != null && resources.ContainsKey(visualKey))
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

                if (scenario.Tasks[currIndex].TaskContent.Type == TaskContent.ContentType.Audio)
                {
                    // Load audio
                    string audioKey = scenario.Tasks[currIndex].TaskContent.Audio;
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
                    else if(autoSpeak)
                    {
                        tts.SayLine(eventTranscript.Text, null, true);
                    }
                }
                else if(autoSpeak)
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
            if(sender == choiceImage1)
            {
                results.ParticipantTaskIdResults.Add(scenario.Tasks[currIndex].Id, scenario.Tasks[currIndex].TaskResponse.Related[0]);
            }
            else if(sender == choiceImage2)
            {
                results.ParticipantTaskIdResults.Add(scenario.Tasks[currIndex].Id, scenario.Tasks[currIndex].TaskResponse.Related[1]);
            }
            ShowNextEvent();
        }

        /// <summary>
        /// Start a new recording, or if already recording finish it and progress to the next event
        /// </summary>
        private void MainButtonClicked(object sender, EventArgs e)
        {
            if (currIndex >= scenario.Tasks.Length)
            {
                FinishScenario();
                return;
            }

            if (scenario.Tasks[currIndex].TaskResponse.Type == TaskResponse.ResponseType.None)
            {
                // No need to record
                ShowNextEvent();
            }
            else if(recording)
            {
                recording = false;
                audioManager.StopRecording();
                results.ParticipantTaskIdResults.Add(scenario.Tasks[currIndex].Id, scenario.Tasks[currIndex].Id + ".mp4");
                ShowNextEvent();
            }
            else
            {
                mainButton.SetBackgroundResource(Resource.Drawable.recordButtonRed);
                if(mediaPlayer != null)
                {
                    mediaPlayer.Stop();
                }

                if (tts.IsSpeaking()) tts.StopSpeaking();
                canSpeak = false;

                recording = true;
                string fileAdd = System.IO.Path.Combine(localTempDirectory, scenario.Tasks[currIndex].Id + ".mp4");
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
        /// Exports the recordings into a zip and marks them as being ready to upload (TODO)
        /// </summary>
        private void ExportRecordings()
        {
            // Compress exported recordings into zip (Delete existing zip first)
            // TODO set password? https://github.com/icsharpcode/SharpZipLib/wiki/Zip-Samples#anchorCreate  
            File.Delete(resultsZipPath);

            try
            {
                FastZip fastZip = new FastZip();
                bool recurse = true;
                fastZip.CreateZip(resultsZipPath, localTempDirectory, recurse, null);

                results.CompletionDate = DateTime.Now;

                AppData.session.resultsToUpload.Add(results);
                AppData.SaveCurrentData();

                // Clean up zipped files
                string[] toDel = Directory.GetFiles(localTempDirectory);

                for (int i = 0; i < toDel.Length; i++ )
                {
                    if (toDel[i] == resultsZipPath) continue;
                    File.Delete(toDel[i]);
                }
                    
                StartActivity(typeof(UploadsActivity));
                this.Finish();
            }
            catch(Exception except)
            {
                Console.Write(except.Message);
            }
        }

        /// <summary>
        /// Presents the user with the choice of exporting their recordings or restarting the scenario
        /// </summary>
        private void FinishScenario()
        {
            AlertDialog alert = new AlertDialog.Builder(this)
            .SetTitle("Scenario Complete!")
            .SetMessage("Well done! You've completed this scenario and we're ready to export your recordings. Would you like to export now or retry the scenario?")
            .SetCancelable(false)
            .SetNegativeButton("Restart", (EventHandler<DialogClickEventArgs>)null)
            .SetPositiveButton("Export Results", (s, args) => { ExportRecordings(); })
            .Create();

            alert.Show();

            // A second alert dialogue, confirming the decision to restart
            Button negative = alert.GetButton((int)DialogButtonType.Negative);
            negative.Click += delegate(object sender, EventArgs e)
            {
                AlertDialog.Builder confirm = new AlertDialog.Builder(this);
                confirm.SetTitle("Are you sure?");
                confirm.SetMessage("Restarting will wipe your current progress. Restart the scenario?");
                confirm.SetPositiveButton("Restart", (senderAlert, confArgs) =>
                {
                    RestartScenario();
                    alert.Dismiss();
                });
                confirm.SetNegativeButton("Cancel", (senderAlert, confArgs) => { });
                confirm.Show();
            };      
        }

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

        /// <summary>
        /// Restore the default app colours to this activity
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