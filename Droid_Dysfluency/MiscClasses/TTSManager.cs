using Android.Content;
using Android.Speech.Tts;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Droid_Dysfluency
{
    /// <summary>
    /// Creates and manages a TextToSpeech Engine
    /// </summary>
    public class TTSManager : UtteranceProgressListener, TextToSpeech.IOnInitListener
    {
        private TextToSpeech tts;
        private Action<string> callback;

        public TTSManager(Context context, Action<string> OnUtteranceComplete)
        {
            tts = new TextToSpeech(context, this);
            callback = OnUtteranceComplete;
        }

        public void OnInit(OperationResult status)
        {
            if(status == OperationResult.Success)
            {
                tts.SetLanguage(Locale.Default);
                tts.SetOnUtteranceProgressListener(this);
            }
            else
            {
                tts = null;
            }
        }

        /// <summary>
        /// Make the TextToSpeech engine say the given string
        /// </summary>
        /// <param name="line">The text to speak</param>
        /// <param name="interrupt">Say this line even if already speaking</param>
        public void SayLine(string line, string id = null, bool interrupt = false)
        {
            if (tts == null || (tts.IsSpeaking && !interrupt)) return;

            if (tts.IsSpeaking) tts.Stop();

            tts.Speak(line, QueueMode.Flush, null, id);
        }

        /// <summary>
        /// Make the TextToSpeech engine say the given string when next available
        /// </summary>
        /// <param name="line">The text to speak</param>
        public void QueueLine(string line, string id = null)
        {
            if (tts == null) return;

            tts.Speak(line, QueueMode.Add, null, id);
        }

        public void StopSpeaking()
        {
            if (tts != null && tts.IsSpeaking) tts.Stop();
        }

        /// <summary>
        /// Releases the manager's resources safely
        /// </summary>
        public void Clean()
        {
            if(tts != null)
            {
                tts.Stop();
                tts.Shutdown();
            }
        }

        public bool IsSpeaking()
        {
            return tts.IsSpeaking;
        }

        public override void OnDone(string utteranceId)
        {
            if(callback != null) callback(utteranceId);
        }

        public override void OnError(string utteranceId)
        {
            
        }

        public override void OnStart(string utteranceId)
        {
            
        }
    }
}