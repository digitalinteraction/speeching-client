using System;
using System.Collections.Generic;
using System.Text;

using Android.Media;

namespace Droid_PeopleWithParkinsons
{
    class AudioRecorder : IAudioRecorder, IDisposable
    {
        private bool _isRecording = false;
        public bool isRecording { get { return _isRecording; } protected set { _isRecording = value; } }

        public int? maxAmplitude { get { return isRecording ? (int?) audioRecorder.MaxAmplitude : null; } }

        private MediaRecorder audioRecorder;
        private string filePath = "";
        private bool prepared = false;    

        /// <summary>
        /// Must be called before beginning initial audio options. Can be recalled to set new file path safely.
        /// </summary>
        /// <param name="_filePath">Output folder path to save file with filename and extension. Example: Assets/RecordedAudio/newAudioFile.mp3</param>
        public void PrepareAudioRecorder(string _filePath)
        {
            if (!prepared)
            {
                audioRecorder = new MediaRecorder();
                
                prepared = true;
            }

            filePath = _filePath;
        }

        /// <summary>
        /// Begins recording audio.
        /// </summary>
        /// <returns>true if recorder has started.</returns>
        public bool StartAudio()
        {
            if (prepared && !isRecording)
            {
                isRecording = true;

                audioRecorder.SetAudioSource(AudioSource.Mic);
                audioRecorder.SetOutputFormat(OutputFormat.ThreeGpp);
                audioRecorder.SetAudioEncoder(AudioEncoder.Aac);
                audioRecorder.SetOutputFile(filePath);
                audioRecorder.Prepare();
                audioRecorder.Start();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Stops recording audio and saves data to file.
        /// </summary>
        /// <returns>true if audio was recording and has been stopped.</returns>
        public bool StopAudio()
        {
            if (prepared && isRecording)
            {
                isRecording = false;
                audioRecorder.Stop();
                audioRecorder.Reset();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Cleanup. Call this before destoying object; safely releases recorder.
        /// </summary>
        public void Dispose()
        {
            if (audioRecorder != null)
            {
                if (isRecording)
                {
                    StopAudio();
                }

                audioRecorder.Release();
                audioRecorder.Dispose();
                audioRecorder = null;

                filePath = "";
                prepared = false;
            }
        }
    }
}
