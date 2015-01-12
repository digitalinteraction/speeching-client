using System;
using System.Collections.Generic;
using System.Text;

using Android.OS;
using Android.App;
using Android.Widget;
using Android.Content;
using Android.Media;

using System.Threading;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Sound Recorder")]
    class RecordSoundActivity : Activity
    {
        private Button soundRecorderButton;
        private Button playSoundButon;

        private AudioRecorder audioRecorder;
        private string outputPath;

        private TextView fileCountDisplay;
        private TextView selectedIndexDisplay;
        private TextView infoLogDisplay;

        private Button incIndexBtn;
        private Button decIndexBtn;

        private Button deleteAllBtn;
        private Button deleteIndvBtn;

        private MediaPlayer audioPlayer;
        private bool isPlayingAudio;
        private bool didPlayAudio;

        private TextView backgroundNoiseDisplay;
        private AudioRecorder backgroundAudioRecorder;
        private bool bgRunning = false;
        private bool bgShouldToggle = false;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.RecordSound);

            // Get and assign buttons

            soundRecorderButton = FindViewById<Button>(Resource.Id.SoundRecorderBtn);
            soundRecorderButton.Click += SoundRecorderButtonClicked;

            playSoundButon = FindViewById<Button>(Resource.Id.PlaySoundBtn);
            playSoundButon.Click += PlaySoundButtonClicked;

            // Set debug information

            fileCountDisplay = FindViewById<TextView>(Resource.Id.StoredSoundsValue);
            SetNewFileCount();

            selectedIndexDisplay = FindViewById<TextView>(Resource.Id.AudioTrackValue);
            infoLogDisplay = FindViewById<TextView>(Resource.Id.InfoText);

            incIndexBtn = FindViewById<Button>(Resource.Id.IncreaseBtn);
            incIndexBtn.Click += IncButtonPressed;

            decIndexBtn = FindViewById<Button>(Resource.Id.DecreseIndex);
            decIndexBtn.Click += DecButtonPressed;

            deleteAllBtn = FindViewById<Button>(Resource.Id.DeleteAllFilesBtn);
            deleteAllBtn.Click += DeleteAllButtonPressed;

            deleteIndvBtn = FindViewById<Button>(Resource.Id.DeleteFileBtn);
            deleteIndvBtn.Click += DeleteIndvButtonPressed;

            audioPlayer = new MediaPlayer();

            // Initiate main recorder
            outputPath = AudioFileManager.GetNewAudioFilePath();

            audioRecorder = new AudioRecorder();
            audioRecorder.PrepareAudioRecorder(outputPath);

            // Initiate background recorder
            backgroundAudioRecorder = new AudioRecorder();
            backgroundNoiseDisplay = FindViewById<TextView>(Resource.Id.BackgroundAudioDisplay);

            backgroundAudioRecorder.PrepareAudioRecorder(AudioFileManager.RootBackgroundAudioPath);
            bgRunning = true;
            bgShouldToggle = true;
            ThreadPool.QueueUserWorkItem(o => DoBackgroundNoiseChecker());
            ThreadPool.QueueUserWorkItem(o => DoBackgroundNoiseCycler());
        }


        protected override void OnPause()
        {
            base.OnPause();

            // Clean up access to recorder/player
            // and delete audio if currently recording to avoid incomplete files
            bool deleteFile = audioRecorder.isRecording;
            audioRecorder.Dispose();
            audioRecorder = null;
        
            if (deleteFile)
            {
                AudioFileManager.DeleteFile(outputPath);
                soundRecorderButton.Text = "Start Recording";
            }

            if (didPlayAudio)
            {
                audioPlayer.Dispose();
                audioPlayer.Release();
                audioPlayer = null;

                didPlayAudio = false;
            }

            bgRunning = false;
            bgShouldToggle = false;
            backgroundAudioRecorder.Dispose();
            backgroundAudioRecorder = null;
        }


        protected override void OnResume()
        {
            base.OnResume();

            // Re instantiate players
            audioPlayer = new MediaPlayer();

            audioRecorder = new AudioRecorder();
            audioRecorder.PrepareAudioRecorder(outputPath);

            backgroundAudioRecorder = new AudioRecorder();
            backgroundAudioRecorder.PrepareAudioRecorder(AudioFileManager.RootBackgroundAudioPath);

            bgRunning = true;
            bgShouldToggle = true;
            ThreadPool.QueueUserWorkItem(o => DoBackgroundNoiseChecker());
            ThreadPool.QueueUserWorkItem(o => DoBackgroundNoiseCycler());
        }


        private void SoundRecorderButtonClicked(object sender, EventArgs e)
        {
            // Toggles between recording or stopping the recording
            // If stopping recording, file is saved.
            if (!audioRecorder.isRecording)
            {
                backgroundAudioRecorder.StopAudio();
                bgShouldToggle = false;

                if (audioRecorder.StartAudio())
                {
                    soundRecorderButton.Text = "Stop Recording";                    
                }
            }
            else
            {
                if (audioRecorder.StopAudio())
                {
                    soundRecorderButton.Text = "Start Recording";
                    SetNewFileCount();

                    // audio recorded successfully.
                    // Set audio recorder ready for a new file
                    outputPath = AudioFileManager.GetNewAudioFilePath();
                    audioRecorder.PrepareAudioRecorder(outputPath);

                    backgroundAudioRecorder.StartAudio();
                    bgShouldToggle = true;
                }
            }
        }


        private void PlaySoundButtonClicked(object sender, EventArgs e)
        {
            // Plays sound currently based on the debug 'Selected Index' field.
            // TODO: If used in final version, clean up code and implement properly
            if (!isPlayingAudio)
            {
                if (selectedIndexDisplay.Text != "-1" && selectedIndexDisplay.Text != "0")
                {
                    if (AudioFileManager.IsExist(int.Parse(selectedIndexDisplay.Text)))
                    {
                        audioPlayer.SetDataSource(string.Concat(AudioFileManager.RootAudioDirectory, selectedIndexDisplay.Text, ".mp3"));
                        audioPlayer.Completion += delegate
                        {
                            playSoundButon.Text = "Play Sound";
                            isPlayingAudio = false;
                        };
                        audioPlayer.Prepare();
                        audioPlayer.Start();

                        playSoundButon.Text = "Stop Sound";
                        isPlayingAudio = true;
                        didPlayAudio = true;
                    }
                    else
                    {
                        SetInfoText("Selected index does not exist on the system");
                    }
                }
                else
                {
                    SetInfoText("No file selected to play. Index is invalid");
                }
            }
            else
            {
                audioPlayer.Stop();
                audioPlayer.Reset();

                playSoundButon.Text = "Play Sound";
                isPlayingAudio = false;
            }
        }


        private void SetNewFileCount()
        {
            int numFiles = AudioFileManager.GetNumAudioFiles();

            fileCountDisplay.Text = string.Concat(numFiles.ToString(), " files recorded.");
        }


        private void SetInfoText(string newText)
        {
            infoLogDisplay.Text = newText;
        }


        private void IncButtonPressed(object sender, EventArgs e)
        {
            int newIndex = AudioFileManager.GetNextAudioIndex(int.Parse(selectedIndexDisplay.Text), 1);
            selectedIndexDisplay.Text = newIndex.ToString();

            if (newIndex == -1)
            {
                SetInfoText("No files found. Have you recorded any audio?");
            }
            else
            {
                SetInfoText("File found");
            }
        }


        private void DecButtonPressed(object sender, EventArgs e)
        {
            int newIndex = AudioFileManager.GetNextAudioIndex(int.Parse(selectedIndexDisplay.Text), -1);
            selectedIndexDisplay.Text = newIndex.ToString();

            if (newIndex == -1)
            {
                SetInfoText("No files found. Have you recorded any audio?");
            }
            else
            {
                SetInfoText("File found");
            }
        }


        private void DeleteAllButtonPressed(object sender, EventArgs e)
        {
            AudioFileManager.DeleteAll();
            SetNewFileCount();

            SetInfoText("Deleted all files");
        }


        private void DeleteIndvButtonPressed(object sender, EventArgs e)
        {
            AudioFileManager.DeleteFileByIndex(int.Parse(selectedIndexDisplay.Text));
            SetNewFileCount();

            SetInfoText(string.Format("Deleted file: File index {0}", selectedIndexDisplay.Text));
        }


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
                        int? temp = backgroundAudioRecorder.maxAmplitude;
                    }
                    else
                    {
                        backgroundAudioRecorder.StartAudio();
                    }
                }

                Thread.Sleep(10000);
            }
        }


        private void DoBackgroundNoiseChecker()
        {
            while (bgRunning)
            {
                Thread.Sleep(1500);

                if (backgroundAudioRecorder != null)
                {
                    int? amplitude = backgroundAudioRecorder.maxAmplitude;

                    if (amplitude != null)
                    {
                        RunOnUiThread(() => backgroundNoiseDisplay.Text = string.Concat("Background noise amplitude: ", amplitude.ToString()));
                    }
                    else
                    {
                        RunOnUiThread(() => backgroundNoiseDisplay.Text = "Background noise information not available");
                    }
                }
            }
        }
    }

}
