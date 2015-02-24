using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using SpeechingCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "ScenarioActivity", ParentActivity=typeof(MainActivity))]
    public class ScenarioActivity : Activity
    {
        private Scenario scenario;

        // Scenario title screen
        private RelativeLayout titleLayout;
        private TextView scenarioTitle;
        private TextView authorName;
        private Button startButton;

        // Scenario event screen
        private RelativeLayout eventLayout;
        private ImageView eventImage; //TODO make this a sub-fragment to allow switching between image and video
        private TextView eventTranscript;
        private TextView eventPrompt;
        private Button recButton;
        private MediaPlayer mediaPlayer;

        Dictionary<string, string> resources;
        ProgressDialog progress;
        string documentsPath;
        string localResourcesDirectory;
        string localExportDirectory;
        string localZipPath;

        private int currIndex = -1;
        private AndroidUtils.RecordAudioManager audioManager;
        private bool recording = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Load the scenario with the id that was given inside the current intent
            scenario = Scenario.GetWithId(AppData.session.scenarios, Intent.GetStringExtra("ScenarioId"));

            ActionBar.SetDisplayHomeAsUpEnabled(true);

            string scenarioFormatted = scenario.title.Replace(" ", String.Empty);

            documentsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath + "/speeching";
            localResourcesDirectory = documentsPath + "/" + scenarioFormatted;
            localExportDirectory = localResourcesDirectory + "/exports";

            // Create these directories if they don't already exist
            if (!Directory.Exists(documentsPath))
            {
                Directory.CreateDirectory(documentsPath);
            }
            
            // If the scenario folder doesn't exist we need to download the additional files
            if (!Directory.Exists(localResourcesDirectory))
            {
                Directory.CreateDirectory(localResourcesDirectory);
                Directory.CreateDirectory(localExportDirectory);

                localZipPath = Path.Combine(localResourcesDirectory, scenarioFormatted + ".zip");
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

                for(int i = 0; i < files.Length; i++)
                {
                    resources.Add(Path.GetFileName(files[i]), files[i]);
                }
            }

            SetContentView(Resource.Layout.ScenarioActivity);
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

            scenarioTitle.Text = scenario.title;
            authorName.Text = scenario.creator.name;

            eventLayout = FindViewById<RelativeLayout>(Resource.Id.scenarioEventLayout);
            eventTranscript = FindViewById<TextView>(Resource.Id.scenarioText);

            eventPrompt = FindViewById<TextView>(Resource.Id.scenarioPrompt);
            eventImage = FindViewById<ImageView>(Resource.Id.scenarioImage);

            recButton = FindViewById<Button>(Resource.Id.scenarioProgressBtn);
            recButton.Click += SoundRecorderButtonClicked;

            if(savedInstanceState != null)
            {
                currIndex = savedInstanceState.GetInt("progress", -1);
            }

            if(currIndex < 0)
            {
                titleLayout.Visibility = ViewStates.Visible;
                eventLayout.Visibility = ViewStates.Gone;
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

        // For the home button in top left
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if(item.ItemId == Android.Resource.Id.Home)
            {
                NavUtils.NavigateUpFromSameTask(this);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        /// <summary>
        /// Downloads the data from the scenario's address
        /// </summary>
        private async void PrepareData()
        {
            RunOnUiThread(() => progress = ProgressDialog.Show(this, "Please Wait", "Downloading data to " + localZipPath, true));
            resources = new Dictionary<string, string>();

            WebClient request = new WebClient();
            await request.DownloadFileTaskAsync(
                new Uri(scenario.resources),
                localZipPath
                );
            request.Dispose();
            request = null;

            RunOnUiThread(() => progress.SetMessage("Unpacking data at " + localZipPath));

            ZipFile zip = null; 
            try
            {
                //Unzip the downloaded file and add references to its contents in the resources dictionary
                zip = new ZipFile(File.OpenRead(localZipPath));

                foreach(ZipEntry entry in zip)
                {
                    string filename = Path.Combine(localResourcesDirectory, entry.Name);
                    byte[] buffer = new byte[4096];
                    System.IO.Stream zipStream = zip.GetInputStream(entry);
                    using(FileStream streamWriter = File.Create(filename))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                    resources.Add(entry.Name, filename);
                }
                
            }
            finally
            {
                if(zip != null)
                {
                    zip.IsStreamOwner = true;
                    zip.Close();
                }
            }
           
            RunOnUiThread(() => progress.Hide());
        }

        protected override void OnResume ()
        {
            base.OnResume();
            audioManager = new AndroidUtils.RecordAudioManager(this, null);

            // Reload the resources for this stage of the scenario, incase they were lost (e.g. audio, video)
            if(currIndex >= 0)
            {
                currIndex--;
                ShowNextEvent();
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

            recButton.Text = "Record Response";

            // Check if the scenario is complete
            if (currIndex >= scenario.events.Length)
            {
                FinishScenario();
                return;
            }

            // Load text
            eventTranscript.Text = scenario.events[currIndex].content.text;
            if(scenario.events[currIndex].response.type == "freeformSpeech")
            {
                eventPrompt.Text = "";
            }
            else
            {
                eventPrompt.Text = scenario.events[currIndex].response.prompt;
            }

            // Load audio
            string audioKey = scenario.events[currIndex].content.audio;
            if(audioKey != null && resources.ContainsKey(audioKey))
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

            // Load the visual media (TODO add video capabilities)
            string visualKey = scenario.events[currIndex].content.visual;
            if(visualKey != null && resources.ContainsKey(visualKey))
            {
                eventImage.SetImageURI(Android.Net.Uri.FromFile( new Java.IO.File( resources[visualKey]) ));
            }
        }

        /// <summary>
        /// Start a new recording, or if already recording finish it and progress to the next event
        /// </summary>
        private void SoundRecorderButtonClicked(object sender, EventArgs e)
        {
            if(recording)
            {
                recording = false;
                audioManager.StopRecording();
                ShowNextEvent();
            }
            else
            {
                if(mediaPlayer != null)
                {
                    mediaPlayer.Stop();
                }

                recording = true;
                string fileAdd = Path.Combine(localExportDirectory, "res" + currIndex + ".3gpp");
                scenario.events[currIndex].response.resultPath = fileAdd;
                audioManager.StartRecording(fileAdd);
                recButton.Text = "Stop";
            }
        }

        /// <summary>
        /// Restarts the scenario, deleting already exported data
        /// </summary>
        private void RestartScenario()
        {
            currIndex = -1;

            string[] files = Directory.GetFiles(localExportDirectory);

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
            string zipPath = Path.Combine(localExportDirectory, "final.zip");
            File.Delete(zipPath);

            try
            {
                FastZip fastZip = new FastZip();
                bool recurse = true;
                string filter = @"-final\.zip$"; // Don't include yourself, you daft thing
                fastZip.CreateZip(zipPath, localExportDirectory, recurse, filter);

                ResultItem res = new ResultItem(scenario.id, zipPath, AppData.session.currentUser.id);
                AppData.session.resultsToUpload.Add(res);
                AppData.SaveCurrentData();

                // Clean up zipped files
                string[] toDel = Directory.GetFiles(localExportDirectory);

                for (int i = 0; i < toDel.Length; i++ )
                {
                    if (toDel[i] == zipPath) continue;
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
    }
}