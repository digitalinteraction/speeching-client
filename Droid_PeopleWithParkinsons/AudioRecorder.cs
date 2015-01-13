using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

using Android.Media;
using Android.Media.Audiofx;

namespace Droid_PeopleWithParkinsons
{
    // TODO: App crashes when starting recorder occasionally? Why is this?
    // Probably to do with trying to do an action when it isn't in the appropriate 'state'.
    // Arguably this is the problem of the calling code, and not this class?
    // Although, to be fair, the class should probably wrap itself to protect from that...

    // TODO: Make sure that when pausing application any current audio is discarded
    // TODO: Make sure that the background noise recorder is deleted when finished.
    class AudioRecorder : IAudioRecorder, IDisposable
    {
        private bool _isRecording = false;
        public bool isRecording { get { return _isRecording; } protected set { _isRecording = value; } }

        public int? soundLevel { get { return isRecording ? (int?) CalculateSoundLevel() : null; } }

        private AudioRecord audioRecorder;
        private string filePath = "";
        private bool prepared = false;    
		
		private int bufferSizeBytes;
		private int bufferLength;
		private ChannelIn channelConfiguration = ChannelIn.Mono;
		private Android.Media.Encoding audioEncoding = Android.Media.Encoding.Pcm16bit; 
		private static int SAMPPERSEC = 44100; //samp per sec 8000, 11025, 22050 44100 48000
		private static short[] buffer;

        private bool isNoiseSupression = false;
        private NoiseSuppressor noiseSuppressor;

        /// <summary>
        /// Must be called before beginning initial audio options. Can be recalled to set new file path safely.
        /// </summary>
        /// <param name="_filePath">Output folder path to save file with filename and extension. Example: Assets/RecordedAudio/newAudioFile.mp3</param>
        public void PrepareAudioRecorder(string _filePath, bool enableNoiseSuppression)
        {
            if (!prepared)
            {
				bufferSizeBytes = AudioRecord.GetMinBufferSize(SAMPPERSEC, channelConfiguration, audioEncoding);
				buffer = new short[bufferSizeBytes];
				bufferLength = bufferSizeBytes / 2;
				
                audioRecorder = new AudioRecord(AudioSource.Mic, SAMPPERSEC, channelConfiguration, audioEncoding, bufferSizeBytes);

                isNoiseSupression = enableNoiseSuppression;
                
                prepared = true;
            }

            filePath = _filePath;
        }

		int? CalculateSoundLevel()
		{
            // read the data into the buffer
            int read = audioRecorder.Read(buffer, 0, buffer.Length);

            double sumLevel = 0;

            for (int i = 0; i < read; i++)
            {
                sumLevel += buffer[i];
            }

            int? lastLevel;
            return lastLevel = (int?) Math.Abs((sumLevel / read));
		}  
	
        /// <summary>
        /// Begins recording audio.
        /// </summary>
        /// <returns>true if recorder has started.</returns>
        public bool StartAudio()
        {
            if (prepared && !isRecording)
            {				
               if (NoiseSuppressor.IsAvailable)
               {
                   if (isNoiseSupression)
                   {                       
                       noiseSuppressor = NoiseSuppressor.Create(audioRecorder.AudioSessionId);

                        // Differences on some platforms according to API?
                       if (noiseSuppressor.Enabled == false)
                       {
                           noiseSuppressor.SetEnabled(true);
                       }
                   }
               }
               else
               {
                   Console.WriteLine("Noise suppression is not enabled on this device.");
               }

               ThreadPool.QueueUserWorkItem(o => WriteAudioDataToFile());

               isRecording = true;
               audioRecorder.StartRecording();

               return true;
            }
            else
            {
                return false;
            }
        }
		
		private void WriteAudioDataToFile()
		{
			// Write the output audio in byte
			short[] sData = new short[bufferSizeBytes / 2];

			Java.IO.FileOutputStream os = null;
			try
			{
				os = new Java.IO.FileOutputStream(filePath);
			}
			catch (FileNotFoundException e)
			{
                Console.WriteLine(e);
			}

			while (isRecording)
            {
				// Gets the voice output from microphone to byte format
				audioRecorder.Read(sData, 0, bufferSizeBytes/2);

				try
				{
					// Writes the data to file from buffer
					// Stores the voice buffer
					byte[] bData = short2byte(sData);
					os.Write(bData, 0, bufferSizeBytes);
				}
				catch (IOException e)
				{
                    Console.WriteLine(e);
				}
			}
			try
			{
				os.Close();
			}
			catch (IOException e)
			{
                Console.WriteLine(e);
			}
		}
		
		private byte[] short2byte(short[] sData)
		{
			int shortArrsize = sData.Length;
			byte[] bytes = new byte[shortArrsize * 2];
			
			for (int i = 0; i < shortArrsize; i++)
			{
				bytes[i * 2] = (byte) (sData[i] & 0x00FF);
				bytes[(i * 2) + 1] = (byte) (sData[i] >> 8);
				sData[i] = 0;
			}
			return bytes;

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

                if (noiseSuppressor != null)
                {
                    noiseSuppressor.Release();
                    noiseSuppressor.Dispose();                    
                    noiseSuppressor = null;
                }

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
