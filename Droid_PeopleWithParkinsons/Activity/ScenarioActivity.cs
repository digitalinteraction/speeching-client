using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using SpeechingCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Scenario", ParentActivity=typeof(MainActivity))]
    public class ScenarioActivity : Activity
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
        private GridView eventChoicesGrid;
        private TextView eventTranscript;
        private TextView eventPrompt;
        private Button mainButton;
        private MediaPlayer mediaPlayer;

        string resultsZipPath;
        ResultItem results;

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

                localZipPath = System.IO.Path.Combine(localResourcesDirectory, scenarioFormatted + ".zip");
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
                    resources.Add(System.IO.Path.GetFileName(files[i]), files[i]);
                }

                //Remove existing data from the upload queue
                AppData.session.DeleteAllPendingForScenario(scenario.id);
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

            eventLayout = FindViewById<LinearLayout>(Resource.Id.scenarioEventLayout);
            eventTranscript = FindViewById<TextView>(Resource.Id.scenarioText);

            eventPrompt = FindViewById<TextView>(Resource.Id.scenarioPrompt);
            eventImage = FindViewById<ImageView>(Resource.Id.scenarioImage);
            eventVideo = FindViewById<VideoView>(Resource.Id.scenarioVideo);

            mainButton = FindViewById<Button>(Resource.Id.scenarioProgressBtn);
            mainButton.Click += MainButtonClicked;

            resultsZipPath = System.IO.Path.Combine(localExportDirectory, "final.zip");
            results = new ResultItem(scenario.id, resultsZipPath, AppData.session.currentUser.id);

            if(savedInstanceState != null)
            {
                currIndex = savedInstanceState.GetInt("progress", -1);
            }

            if(currIndex < 0)
            {
                titleLayout.Visibility = ViewStates.Visible;
                eventLayout.Visibility = ViewStates.Gone;
                this.Title = scenario.title;
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
            
            if(icon != null)
            {
                icon.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(scenario.icon)));
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
            else
            {
                ImageView icon = FindViewById<ImageView>(Resource.Id.scenarioIcon);

                if (icon != null)
                {
                    icon.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(scenario.icon)));
                }
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

            mainButton.Text = "Record Response";

            // Check if the scenario is complete
            if (currIndex >= scenario.events.Length)
            {
                FinishScenario();
                return;
            }

            this.Title = scenario.title + " | " + (currIndex + 1) + " of " + scenario.events.Length;

            if (scenario.events[currIndex].response.type == "none")
            {
                eventPrompt.Text = "";
                eventPrompt.SetTypeface(null, TypefaceStyle.Normal);
                mainButton.Text = "Continue";
                return;
            }
            else if (scenario.events[currIndex].response.type == "choice")
            {
                mainButton.Text = "Make a choice";
            }

            // Load text
            eventTranscript.Text = scenario.events[currIndex].content.text;
            if(scenario.events[currIndex].response.type == "freeformSpeech")
            {
                // Make freeform prompts italic
                string given = scenario.events[currIndex].response.prompt;
                eventPrompt.SetTypeface(null, TypefaceStyle.BoldItalic);
                eventPrompt.Text = (given != null) ? given : ""; ;
            }
            else
            {
                eventPrompt.Text = scenario.events[currIndex].response.prompt;
                eventPrompt.SetTypeface(null, TypefaceStyle.Normal);
            }

            if(scenario.events[currIndex].content.type == "VIDEO")
            {
                // load video instead of audio + image
                eventVideo.Visibility = ViewStates.Visible;
                eventImage.Visibility = ViewStates.Gone;
                string vidKey = scenario.events[currIndex].content.visual;
                var vidUri = Android.Net.Uri.Parse( resources[vidKey]);
                eventVideo.SetVideoURI(vidUri);
                eventVideo.Start();
            }
            else
            {
                eventVideo.Visibility = ViewStates.Gone;
                eventImage.Visibility = ViewStates.Visible;

                // Load the image if it exists
                string visualKey = scenario.events[currIndex].content.visual;
                if(visualKey != null && resources.ContainsKey(visualKey))
                {
                    eventImage.SetImageURI(Android.Net.Uri.FromFile( new Java.IO.File( resources[visualKey]) ));
                }

                if (scenario.events[currIndex].content.type == "AUDIO")
                {
                    // Load audio
                    string audioKey = scenario.events[currIndex].content.audio;
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
                }
            }
        }

        /// <summary>
        /// Start a new recording, or if already recording finish it and progress to the next event
        /// </summary>
        private void MainButtonClicked(object sender, EventArgs e)
        {
            if(scenario.events[currIndex].response.type == "choice")
            {
                ChoiceItem[] choices = JsonConvert.DeserializeObject<ChoiceItem[]>(scenario.events[currIndex].response.data);

                View popupView = LayoutInflater.Inflate(Resource.Layout.ScenarioChoicePopup, null);
                popupView.FindViewById<TextView>(Resource.Id.choice_title).Text = scenario.events[currIndex].response.prompt;
                GridView grid = popupView.FindViewById<GridView>(Resource.Id.choice_grid);
                grid.Adapter = new ChoiceListAdapter(this, Resource.Id.choice_grid, choices);
                PopupWindow popup = new PopupWindow(popupView, WindowManagerLayoutParams.WrapContent, WindowManagerLayoutParams.WrapContent);

                //centre of screen
                popup.ShowAtLocation(this.FindViewById(Android.Resource.Id.Content), GravityFlags.Center, 0, 0);

            }
            else if(recording)
            {
                recording = false;
                audioManager.StopRecording();
                results.results.Add(scenario.events[currIndex].id, scenario.events[currIndex].id + ".3gpp");
                ShowNextEvent();
            }
            else
            {
                if(mediaPlayer != null)
                {
                    mediaPlayer.Stop();
                }

                recording = true;
                string fileAdd = System.IO.Path.Combine(localExportDirectory, scenario.events[currIndex].id + ".3gpp");
                scenario.events[currIndex].response.resultPath = fileAdd;
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
            File.Delete(resultsZipPath);

            try
            {
                FastZip fastZip = new FastZip();
                bool recurse = true;
                string filter = @"-final\.zip$"; // Don't include yourself, you daft thing
                fastZip.CreateZip(resultsZipPath, localExportDirectory, recurse, filter);

                AppData.session.resultsToUpload.Add(results);
                AppData.SaveCurrentData();

                // Clean up zipped files
                string[] toDel = Directory.GetFiles(localExportDirectory);

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

        public class ChoiceListAdapter : BaseAdapter<ChoiceItem>
        {
            Activity context;
            ChoiceItem[] choices;

            /// <summary>
            /// An adapter to be able to display choices in a grid or list
            /// </summary>
            public ChoiceListAdapter(Activity context, int resource, ChoiceItem[] data)
            {
                this.context = context;
                this.choices = data;
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override ChoiceItem this[int position]
            {
                get { return choices[position]; }
            }

            public override int Count
            {
                get { return choices.Length; }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View view = convertView;

                if (view == null)
                {
                    view = context.LayoutInflater.Inflate(Resource.Layout.ScenarioChoiceItem, null);
                }

                view.FindViewById<TextView>(Resource.Id.choice_name).Text = choices[position].name;
                //view.FindViewById<ImageView>(Resource.Id.choice_image).SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(choices[position].image)));
                return view;
            }
        }
    }
}