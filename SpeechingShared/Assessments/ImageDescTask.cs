namespace SpeechingShared
{
    public class ImageDescTask : IAssessmentTask
    {
        public string Image;
        public AssessmentRecordingPrompt[] Prompts;
        public int Id { get; set; }
        public string Title { get; set; }
        public string Instructions { get; set; }
    }
}