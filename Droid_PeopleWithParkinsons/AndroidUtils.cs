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

namespace Droid_PeopleWithParkinsons
{
    public class AndroidUtils
    {
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

        public class SuperAwesomeCardFragment : Android.Support.V4.App.Fragment
	{
		private int _position;
		private bool _enableProgressButton;

		//this uses an event to show one way to get a fragment to notify its host that its title needs to change in some way
		public event EventHandler<int> ChangeTitleRequested;

		//the progress bar on/off change is done alternatively through a direct back-reference to the ITabProvider when the
		//MainActivity's MyPageAdapter2 is used, to show another way of doing it.

		public SuperAwesomeCardFragment()
		{
			Android.Util.Log.Info("SuperAwesomeCardFragment", "Default constructor called.");
		}

		protected SuperAwesomeCardFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {
			Android.Util.Log.Info("SuperAwesomeCardFragment", "Protected constructor called.");
		}

		internal void Configure(int position, bool enableProgressButton = false)
		{
			Android.Util.Log.Info("SuperAwesomeCardFragment", string.Format("Configure called with position {0}, enableProgressButton: {1}", position, enableProgressButton));
			_position = position;
			_enableProgressButton = enableProgressButton;
			//reset this event
			ChangeTitleRequested = null;
		}

        public override void OnCreate(Android.OS.Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Android.Util.Log.Info("SuperAwesomeCardFragment", string.Format("OnCreate called"));
			if (savedInstanceState != null)
			{
				_position = savedInstanceState.GetInt("position", 0);
				_enableProgressButton = savedInstanceState.GetBoolean("enableProgressButton", false);
			}
			if (this.View != null)
			{
				Android.Util.Log.Info("SuperAwesomeCardFragment", string.Format(string.Format("View already present for position {0}", _position)));
			}
		}

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
		{
			Android.Util.Log.Info("SuperAwesomeCardFragment", string.Format("CreateView being called for position {0}", _position));
			var view = inflater.Inflate(Resource.Layout.MainFriendsListFragment, container, false);
			var v = view.FindViewById<TextView>(Android.Resource.Id.Text1);

			/*if (_enableProgressButton)
			{
				var button = view.FindViewById<Button>(Resource.Id.btnToggleProgress);
				button.Click += btnToggleProgress_Clicked;
				button.Enabled = true;
				button.Dispose();
			}
			else
			{
				var button = view.FindViewById<Button>(Resource.Id.btnToggleProgress);
				button.Click -= btnToggleProgress_Clicked;
				button.Enabled = false;
				button.Dispose();
			}

			var button2 = view.FindViewById<Button>(Resource.Id.btnChangeTitle);
			//shows one way of signifying that something has to change in the tabs
			//using a public event on the fragment - getting this right is tricky, though,
			//as you have to ensure you remain subscribed during orientation changes and
			//fragment recycling.

			button2.Click -= btnChangeTitle_Clicked;
			button2.Click += btnChangeTitle_Clicked;
			button2.Dispose();
            */
			v.Text = string.Format("CARD {0}", _position + 1);
			v.Dispose();

			return view;
		}

        public override void OnSaveInstanceState(Android.OS.Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutInt("position", _position);
            outState.PutBoolean("enableProgressButton", _enableProgressButton);
        }

        public void btnChangeTitle_Clicked(object sender, EventArgs e)
        {
            var evt = ChangeTitleRequested;
            if (evt != null)
                evt(this, _position);
        }

        public void btnToggleProgress_Clicked(object sender, EventArgs e)
        {
            //the static event fired by SharedState 
            //SharedState.SetInProgress(_position, !SharedState.GetInProgress(_position));
        }
    }
    }
}