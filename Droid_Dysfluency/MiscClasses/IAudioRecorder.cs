
namespace Droid_Dysfluency
{
    interface IAudioRecorder
    {
        void PrepareAudioRecorder(string _filePath, bool enableNoiseSuppression);
        bool StartAudio();
        bool StopAudio();
    }
}
