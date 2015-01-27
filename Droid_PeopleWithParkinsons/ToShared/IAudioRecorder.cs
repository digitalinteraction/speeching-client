using System;
using System.Collections.Generic;
using System.Text;

namespace Droid_PeopleWithParkinsons
{
    interface IAudioRecorder
    {
        void PrepareAudioRecorder(string _filePath, bool enableNoiseSuppression);
        bool StartAudio();
        bool StopAudio();
    }
}
