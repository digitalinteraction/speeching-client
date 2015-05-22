using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Gcm;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Preferences;
using Android.Support.V4.App;
using Android.Widget;
using Newtonsoft.Json;
using RestSharp.Contrib;
using SpeechingShared;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DroidSpeeching
{
    public static class AndroidUtils
    {
        public static String EXTRA_MESSAGE = "message";
        public static String PROPERTY_REG_ID = "registration_id";
        public static String PROPERTY_APP_VERSION = "app_version";
        public static int PLAY_SERVICES_RESOLUTION_REQUEST = 8675309;
        public static GoogleCloudMessaging gcm;
        public static string GooglePlayRegId;
        public static NotificationManager notificationManager;
        public static MainActivity mainActivity;

        public static bool IsConnected()
        {
            bool connected = AppData.CheckNetwork();
            
            if(mainActivity != null)
            {
                mainActivity.ShowOfflineWarning(!connected);
            }
            return connected;
        }

        public static bool IsActivityAvailableOffline(int id, Context context)
        {
            ISharedPreferences prefs = context.GetSharedPreferences("ACT_" + id, FileCreationMode.MultiProcess);

            return prefs.GetBoolean("DOWNLOADED", false);
        }

        /// <summary>
        /// Set up Android specific variables and get the session loaded/created
        /// </summary>
        /// <returns>If a previous session was actively loaded</returns>
        public static async Task<bool> InitSession(Activity context = null)
        {
            await AppData.AssignCacheLocations(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath + "/speeching");

            AppData.Io = new AndroidPCLHelper();

            AppData.CheckForConnection = () =>
            {
                ConnectivityManager connectivityManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
                NetworkInfo activeNetworkInfo = connectivityManager.ActiveNetworkInfo;

                return activeNetworkInfo != null && activeNetworkInfo.IsConnected;
            };

            AppData.OnConnectionSuccess = () =>
            {
                AndroidUtils.gcm = GoogleCloudMessaging.GetInstance(context);
                AndroidUtils.GooglePlayRegId = AndroidUtils.GetGoogleRegId(context);

                if (string.IsNullOrEmpty(AndroidUtils.GooglePlayRegId))
                {
                    AndroidUtils.RegisterGCM(context);
                }
            };

            PreferenceManager.SetDefaultValues(context, Resource.Xml.user_settings, false);

            // Only allow this to be null if in background!
            if(context != null)
            {
                bool gpsSuccess = CheckForGooglePlayServices(context);

                if (!gpsSuccess)
                {
                   context.RunOnUiThread(()=> Toast.MakeText(context, "Error: Couldn't connect to Google Play Services", ToastLength.Long).Show());
                   return false;
                }
                
            }

            return await AppData.InitializeIfNeeded(); ;
        }

        /// <summary>
        /// Show a simple "you are offline" message
        /// </summary>
        /// <param name="context"></param>
        public static void OfflineAlert(Context context, string message = null)
        {
            if (message == null) message = "This feature requires access to the Internet. Please check your connection and try again.";

            Android.Support.V7.App.AlertDialog alert = new Android.Support.V7.App.AlertDialog.Builder(context)
                .SetTitle("You are offline!")
                .SetMessage(message)
                .SetPositiveButton("Ok", (s, a) => { })
                .Create();
            alert.Show();
        }

        private static NotificationCompat.Builder GetNotifBuilder(Context context, string title, string message, int priority)
        {
            NotificationCompat.Builder builder = new NotificationCompat.Builder(context)
                .SetPriority(0)
                .SetLights(300, 1000, 1000)
                .SetVisibility(1)
                .SetLocalOnly(false)
                .SetAutoCancel(true)
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(message));
            return builder;
        }

        /// <summary>
        /// Create a notification 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="context"></param>
        public static void SendNotification(string title, string message, Type activityTarget, Context context, int priority = 0)
        {
            if (notificationManager == null)
            {
                notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            }

            Android.App.TaskStackBuilder stackBuilder = Android.App.TaskStackBuilder.Create(context);
            stackBuilder.AddParentStack(Java.Lang.Class.FromType(activityTarget));
            stackBuilder.AddNextIntent(new Intent(context, activityTarget));

            PendingIntent contentIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder builder = GetNotifBuilder(context, title, message, priority);
            builder.SetSmallIcon(Resource.Drawable.notifIcon);
            builder.SetContentIntent(contentIntent);

            notificationManager.Notify(8675309, builder.Build());
        }

        public static void SendNotification(string title, string message, Type activityTarget, Intent intent, Context context, int priority = 0)
        {
            if (notificationManager == null)
            {
                notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            }

            Android.App.TaskStackBuilder stackBuilder = Android.App.TaskStackBuilder.Create(context);
            stackBuilder.AddParentStack(Java.Lang.Class.FromType(activityTarget));
            stackBuilder.AddNextIntent(intent);

            PendingIntent contentIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder builder = GetNotifBuilder(context, title, message, priority);
            builder.SetSmallIcon(Resource.Drawable.notifIcon);
            builder.SetContentIntent(contentIntent);

            notificationManager.Notify(8675309, builder.Build());
        }

        public static void SendNotification(string title, string message, Context context)
        {
            if (notificationManager == null)
            {
                notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            }

            NotificationCompat.Builder builder = GetNotifBuilder(context, title, message, 0);
            builder.SetSmallIcon(Resource.Drawable.notifIcon);
            notificationManager.Notify(8675309, builder.Build());
        }

        /// <summary>
        /// Attempt a connection to Google Play Services to make sure this application is able to recieve push messages
        /// </summary>
        /// <returns></returns>
        private static bool CheckForGooglePlayServices(Activity context)
        {
            int resultCode = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(context);
            if (resultCode != ConnectionResult.Success)
            {
                if (GooglePlayServicesUtil.IsUserRecoverableError(resultCode))
                {
                    GooglePlayServicesUtil.GetErrorDialog(resultCode, context, AndroidUtils.PLAY_SERVICES_RESOLUTION_REQUEST).Show();
                }
                else
                {
                    Android.Support.V7.App.AlertDialog alert = new Android.Support.V7.App.AlertDialog.Builder(context)
                        .SetTitle("Fatal Error")
                        .SetMessage("Speeching was unable to connect to Google Play Services - your device may not be supported.")
                        .SetCancelable(false)
                        .SetPositiveButton("Close Application", (s, a) => { })
                        .Show();
                }
                return false;
            }
            return true;
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
        /// Prepare an activity's icon within a given imageview
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="activity"></param>
        public static async void PrepareIcon(ImageView icon, ISpeechingActivityItem activity)
        {
            bool success = true;

            if (activity.LocalIcon == null && !(await activity.PrepareIcon()))
            {
                // Icon download attempt failed...
                success = false;
            }

            if (success && icon != null)
            {
                icon.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(activity.LocalIcon)));
            }
        }

        public static async void PrepareIcon(ImageView icon, ActivityCategory category)
        {
            bool success = true;

            if (category.localIcon == null && !(await category.PrepareIcon()))
            {
                // Icon download attempt failed...
                success = false;
            }

            if (success && icon != null)
            {
                icon.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(category.localIcon)));
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
                        //GooglePlayRegId = gcm.Register(ConfidentialData.GoogleProjectNum);

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

        public static bool CheckIfServiceIsRunning(Context context, Type serviceClass)
        {
            ActivityManager manager = (ActivityManager)context.GetSystemService(Context.ActivityService);
            
            foreach(Android.App.ActivityManager.RunningServiceInfo service in manager.GetRunningServices(int.MaxValue))
            {
                if(serviceClass.Name.Equals(service.Service.ClassName))
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task<WikipediaResult> GetTodaysWiki(Context context)
        {
            TimeSpan midnightTime = DateTime.Today - new DateTime(1970, 1, 1);;

            WikipediaResult toReturn;

            int daysSinceEpoch = (int)midnightTime.TotalDays;

            ISharedPreferences prefs = context.GetSharedPreferences("WikiData", FileCreationMode.MultiProcess);
            int storedDay = prefs.GetInt("DayNum", -1);
            string wikiJson = prefs.GetString("JsonData", null);

            if(storedDay != daysSinceEpoch || wikiJson == null)
            {
                // We have either the wrong data or no data stored
                toReturn = await ServerData.FetchWikiData(DecodeHTML);

                // Store so that we don't need to download again today
                ISharedPreferencesEditor editor = prefs.Edit();
                editor.PutInt("DayNum", daysSinceEpoch);
                editor.PutString("JsonData", JsonConvert.SerializeObject(toReturn));
                editor.Commit();

                return toReturn;
            }

            // Today's featured article has already been cached!
            return JsonConvert.DeserializeObject<WikipediaResult>(wikiJson);
        }

        public static string DecodeHTML(string toDecode)
        {
            return HttpUtility.HtmlDecode(toDecode);
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
        public static Android.Support.V7.App.AlertDialog.Builder CreateAlert(Activity context, string title, string message, 
                                        string posLabel = null, EventHandler<DialogClickEventArgs> posAction = null,
                                        string negLabel = null, EventHandler<DialogClickEventArgs> negAction = null,
                                        string neuLabel = null, EventHandler<DialogClickEventArgs> neuAction = null, bool cancelable = true)
        {
            Android.Support.V7.App.AlertDialog.Builder alert = new Android.Support.V7.App.AlertDialog.Builder(context)
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
        public class RecordAudioManager : Java.Lang.Object, MediaRecorder.IOnInfoListener
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
            private Activity context;
            private Action onMaxDuration;

            /// <summary>
            /// Class to help record and export audio
            /// </summary>
            /// <param name="context">The current activity</param>
            /// <param name="onMaxDurtion">Action performed when the max duration of the recording has been reached</param>
            /// <param name="backgroundNoise">TextView to display background noise level information</param>
            public RecordAudioManager(Activity context, Action onMaxDuration = null, TextView backgroundNoise = null)
            {
                this.context = context;
                this.backgroundNoiseDisplay = backgroundNoise;
                this.onMaxDuration = onMaxDuration;
            }
            
            public void StartBackgroundCheck()
            {
                backgroundAudioRecorder = new AudioRecorder();
                backgroundAudioRecorder.PrepareAudioRecorder(AppData.Cache.Path + "bgnoise.3gpp", false);
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
            public void StartRecording(string outputPath, int maxSeconds = -1)
            {
                this.outputPath = outputPath;

                //Delete the file if it already exists
                if(File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }

                audioRecorder = new MediaRecorder();
                audioRecorder.SetAudioSource(AudioSource.Mic);
                audioRecorder.SetOutputFormat(OutputFormat.Mpeg4);
                audioRecorder.SetAudioEncoder(AudioEncoder.Aac);
                audioRecorder.SetAudioEncodingBitRate(32000);
                audioRecorder.SetAudioSamplingRate(44100);
                audioRecorder.SetOutputFile(outputPath);

                if (maxSeconds > 0)
                {
                    audioRecorder.SetMaxDuration(maxSeconds * 1000);
                    audioRecorder.SetOnInfoListener(this);
                }

                audioRecorder.Prepare();

                if(backgroundAudioRecorder != null)
                {
                    backgroundAudioRecorder.StopAudio();
                }
                bgShouldToggle = false;

                audioRecorder.Start();
            }

            public double GetAmplitude()
            {
                if (audioRecorder != null)
                {
                    double pressure = audioRecorder.MaxAmplitude;
                    if (pressure == 0) return 0;

                    return (20 * Math.Log10(pressure / 2700.0) + 40); // Fluffed :/
                }  
                else
                {
                    return 0;
                }
                    
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
                    //if (recording) AudioFileManager.DeleteFile(outputPath);
                }

                if(backgroundAudioRecorder != null)
                {
                    backgroundAudioRecorder.Dispose();
                    backgroundAudioRecorder = null;
                }

                // Luke's TODO
                // Deletes background noise audio. Probably a better way to do this
                // Probably shouldn't be saving it on disk in the first place.
                //AudioFileManager.DeleteFile(AudioFileManager.RootBackgroundAudioPath);
            }

            public void OnInfo(MediaRecorder mr, MediaRecorderInfo what, int extra)
            {
                if((what == MediaRecorderInfo.MaxDurationReached || what == MediaRecorderInfo.MaxFilesizeReached) && onMaxDuration != null)
                {
                    StopRecording();
                    onMaxDuration();
                }
            }

        }
    }
}