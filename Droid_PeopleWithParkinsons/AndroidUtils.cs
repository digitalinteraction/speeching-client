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
using System.Threading;
using Android.Media;
using Android.Support.V4.App;
using Android.Support.V4.View;
using SpeechingCommon;
using System.IO;
using System.Threading.Tasks;
using Android.Gms.Gcm;
using Android.Content.PM;

namespace Droid_PeopleWithParkinsons
{
    public static class AndroidUtils
    {
        public static String EXTRA_MESSAGE = "message";
        public static String PROPERTY_REG_ID = "registration_id";
        public static String PROPERTY_APP_VERSION = "app_version";
        public static int PLAY_SERVICES_RESOLUTION_REQUEST = 8675309;
        public static GoogleCloudMessaging gcm;
        public static string GooglePlayRegId;

        /// <summary>
        /// Set up Android specific variables and get the session loaded/created
        /// </summary>
        /// <returns></returns>
        public static async Task InitSession()
        {
            AppData.cacheDir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath + "/speeching";
            await AppData.InitializeIfNeeded();
        }

        /// <summary>
        /// Get the registration ID used by Google Cloud Messaging
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetGoogleRegId(Context context)
        {
            ISharedPreferences prefs = context.GetSharedPreferences("SpeechingData", 0);
            string regId = prefs.GetString(PROPERTY_REG_ID, "");
            if(string.IsNullOrEmpty(regId))
            {
                return "";
            }

            int registeredVersion = prefs.GetInt(PROPERTY_APP_VERSION, int.MinValue);
            int currentVersion = GetAppVersion(context);

            if(registeredVersion != currentVersion)
            {
                Console.WriteLine("Current version does not match the registered app version");
                return "";
            }

            return regId;
        }

        public static int GetAppVersion(Context context)
        {
            try
            {
                PackageInfo packageInfo = context.PackageManager.GetPackageInfo(context.PackageName, 0);
                return packageInfo.VersionCode;
            }
            catch(Exception except)
            {
                Console.WriteLine("Oh no! " + except);
                throw except;
            }
        }

        /// <summary>
        /// Registers for a new Google Play ID and saves it to the app preferences
        /// </summary>
        /// <param name="context"></param>
        public static void RegisterGCM(Context context)
        {
            try
            {
                if (gcm == null)
                {
                    gcm = GoogleCloudMessaging.GetInstance(context);
                }

                ThreadPool.QueueUserWorkItem(o =>
                    {
                        GooglePlayRegId = gcm.Register(ServerData.GoogleProjectNum);

                        // TODO send registration ID to server

                        // Save reg Id for later use
                        ISharedPreferences prefs = context.GetSharedPreferences("SpeechingData", 0);
                        int appVersion = GetAppVersion(context);
                        ISharedPreferencesEditor editor = prefs.Edit();
                        editor.PutString(PROPERTY_REG_ID, GooglePlayRegId);
                        editor.PutInt(PROPERTY_APP_VERSION, appVersion);
                        editor.Commit();
                    });
            }
            catch(Exception except)
            {
                throw except;
            }
        }

        /// <summary>
        /// Add a tab to the given activity's ActionBar
        /// </summary>
        /// <param name="tabName">The label to display on the tab</param>
        /// <param name="currentBundle">Intent data</param>
        /// <param name="activity">The current context</param>
        /// <param name="fragContainerId">Resource id of the element that will contain the fragment</param>
        /// <param name="view">The fragment to use</param>
        public static void AddTab(string tabName, Bundle currentBundle, Activity activity, int fragContainerId, Android.App.Fragment view )
        {
            // Do actionbar tab setup - Each tab is a fragment
            ActionBar.Tab tab = activity.ActionBar.NewTab();
            tab.SetText(tabName);
            tab.SetTag(tabName);
            tab.TabSelected += (sender, args) =>
            {
                if (currentBundle == null)
                {
                    currentBundle = activity.Intent.Extras == null ? new Bundle() : activity.Intent.Extras;
                }

                var fragment = activity.FragmentManager.FindFragmentById(fragContainerId);
                
                if(fragment != null)
                {
                    args.FragmentTransaction.Remove(fragment);
                }
                args.FragmentTransaction.Add(fragContainerId, view);
            };
            activity.ActionBar.AddTab(tab);
        }

        /// <summary>
        /// Cast from Java Object to class type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Cast<T>(this Java.Lang.Object obj) where T : class
        {
            var propertyInfo = obj.GetType().GetProperty("Instance");
            return propertyInfo == null ? null : propertyInfo.GetValue(obj, null) as T;
        }

        /// <summary>
        /// Helper function for creating alert dialogues
        /// </summary>
        public static AlertDialog.Builder CreateAlert(Activity context, string title, string message, 
                                        string posLabel = null, EventHandler<DialogClickEventArgs> posAction = null,
                                        string negLabel = null, EventHandler<DialogClickEventArgs> negAction = null,
                                        string neuLabel = null, EventHandler<DialogClickEventArgs> neuAction = null, bool cancelable = true)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(context)
                .SetTitle(title)
                .SetMessage(message);

            if (posAction != null)
                alert.SetPositiveButton(posLabel, posAction);

            if(negAction != null)
                alert.SetNegativeButton(negLabel, negAction);

            if(neuAction != null)
                alert.SetNeutralButton(neuLabel, neuAction);

            alert.SetCancelable(cancelable);
            alert.Show();

            return alert;
        }

        /// <summary>
        /// Helper class for recording audio
        /// </summary>
        public class RecordAudioManager
        {
            public static int LOW_BACKGROUND_NOISE = 25;
            public static int MEDIUM_BACKGROUND_NOISE = 50;
            public static int HIGH_BACKGROUND_NOISE = 75;

            public static string LOW_BACKGROUND_STRING = "It's quiet here. This is a good time to record.";
            public static string MEDIUM_BACKGROUND_STRING = "It's a little loud here.";
            public static string HIGH_BACKGROUND_STRING = "It's loud here. Try moving somewhere quieter before recording.";

            private MediaRecorder audioRecorder;
            private string outputPath;

            private TextView backgroundNoiseDisplay;
            private AudioRecorder backgroundAudioRecorder;
            private bool bgRunning = false;
            private bool bgShouldToggle = false;
            private bool recording = false;
            private Activity context;

            /// <summary>
            /// Class to help record and export audio
            /// </summary>
            /// <param name="context">The current activity</param>
            /// <param name="outputPath">Path to save this recording</param>
            /// <param name="backgroundNoise">TextView to display background noise level information</param>
            public RecordAudioManager(Activity context, TextView backgroundNoise = null)
            {
                this.context = context;
                this.backgroundNoiseDisplay = backgroundNoise;
            }
            
            public void StartBackgroundCheck()
            {
                backgroundAudioRecorder = new AudioRecorder();
                backgroundAudioRecorder.PrepareAudioRecorder(AudioFileManager.RootBackgroundAudioPath, false);
                bgRunning = true;
                bgShouldToggle = true;

                ThreadPool.QueueUserWorkItem(o => DoBackgroundNoiseChecker());
                ThreadPool.QueueUserWorkItem(o => DoBackgroundNoiseCycler());
            }

            /// <summary>
            /// Polls for the background noise level in a loop and displays it to the user.
            /// </summary>
            private void DoBackgroundNoiseChecker()
            {
                while (bgRunning)
                {
                    Thread.Sleep(1500);

                    if (backgroundAudioRecorder != null)
                    {
                        int? soundLevel = backgroundAudioRecorder.soundLevel;

                        if (soundLevel != null)
                        {
                            string displayString = HIGH_BACKGROUND_STRING;

                            if (soundLevel < MEDIUM_BACKGROUND_NOISE)
                            {
                                displayString = MEDIUM_BACKGROUND_STRING;
                            }
                            if (soundLevel < LOW_BACKGROUND_NOISE)
                            {
                                displayString = LOW_BACKGROUND_STRING;
                            }

                            if (backgroundNoiseDisplay != null)
                            {
                                context.RunOnUiThread(() => backgroundNoiseDisplay.Text = string.Concat(displayString, "\n", "Background noise level: ", soundLevel.ToString()));
                            }

                        }
                        else
                        {
                            if (backgroundNoiseDisplay != null)
                            {
                                context.RunOnUiThread(() => backgroundNoiseDisplay.Text = "Background noise information not available");
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Cycles between 10 second audio recordings of background noise
            /// </summary>
            private void DoBackgroundNoiseCycler()
            {
                while (bgRunning)
                {
                    if (bgShouldToggle)
                    {
                        if (backgroundAudioRecorder.isRecording)
                        {
                            backgroundAudioRecorder.StopAudio();
                            backgroundAudioRecorder.StartAudio();
                            int? temp = backgroundAudioRecorder.soundLevel;
                        }
                        else
                        {
                            backgroundAudioRecorder.StartAudio();
                        }
                    }

                    Thread.Sleep(10000);
                }
            }

            /// <summary>
            /// Starts the audio recording
            /// </summary>
            /// <param name="outputPath">The path to output the resulting recording to</param>
            public void StartRecording(string outputPath)
            {
                this.outputPath = outputPath;

                audioRecorder = new MediaRecorder();
                audioRecorder.SetAudioSource(AudioSource.Mic);
                audioRecorder.SetOutputFormat(OutputFormat.ThreeGpp);
                audioRecorder.SetAudioEncoder(AudioEncoder.AmrNb);
                audioRecorder.SetOutputFile(outputPath);
                audioRecorder.Prepare();

                if(backgroundAudioRecorder != null)
                {
                    backgroundAudioRecorder.StopAudio();
                }
                bgShouldToggle = false;

                recording = true;

                audioRecorder.Start();
            }

           /// <summary>
            /// Stops the recording, exporting the file.
           /// </summary>
           /// <returns>Path of resulting file; null if failed</returns>
            public string StopRecording()
            {
                if(audioRecorder == null) return null;

                audioRecorder.Stop();
                audioRecorder.Reset();

                recording = false;

                return outputPath;
                
            }

            /// <summary>
            /// Cleans up any data if the recording needs to be cancelled
            /// </summary>
            public void CleanUp()
            {
                bgRunning = false;
                bgShouldToggle = false;

                if(audioRecorder != null)
                {
                    audioRecorder.Release();
                    audioRecorder.Dispose();
                    audioRecorder = null;
                    if (recording) AudioFileManager.DeleteFile(outputPath);
                }

                if(backgroundAudioRecorder != null)
                {
                    backgroundAudioRecorder.Dispose();
                    backgroundAudioRecorder = null;
                }

                // Luke's TODO
                // Deletes background noise audio. Probably a better way to do this
                // Probably shouldn't be saving it on disk in the first place.
                AudioFileManager.DeleteFile(AudioFileManager.RootBackgroundAudioPath);
            }
        }

        /// <summary>
        /// A list adapter for the results and responses that the user has exported
        /// </summary>
        public class ExportedListAdapter : BaseAdapter<ResultItem>
        {
            Activity context;
            ResultItem[] results;

            /// <summary>
            /// Display details about a result in a list entry
            /// </summary>
            public ExportedListAdapter(Activity context, int resource, ResultItem[] data)
            {
                this.context = context;
                this.results = data;
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override ResultItem this[int position]
            {
                get { return results[position]; }
            }

            public override int Count
            {
                get { return results.Length; }
            }

            private async Task PopulateView(int activityId, View view)
            {
                ISpeechingActivityItem thisItem = await AppData.session.FetchActivityWithId(activityId);

                view.FindViewById<TextView>(Resource.Id.uploadsList_scenarioTitle).Text = thisItem.Title;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View view = convertView;

                if (view == null)
                {
                    view = context.LayoutInflater.Inflate(Resource.Layout.UploadsListItem, null);
                }

                PopulateView(results[position].CrowdActivityId, view);

                view.FindViewById<TextView>(Resource.Id.uploadsList_completedAt).Text = "Completed on: " + results[position].completedAt.ToString();

                if (results[position].uploadState == ResultItem.UploadState.Uploading)
                {
                    view.FindViewById<TextView>(Resource.Id.uploadsList_uploadStatus).Text = "Uploading...";
                }
                else if (results[position].uploadState == ResultItem.UploadState.Ready)
                {
                    view.FindViewById<TextView>(Resource.Id.uploadsList_uploadStatus).Text = "Ready to upload";
                }

                return view;
            }
        }
    }
}