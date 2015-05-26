namespace SpeechingShared
{
    public class QuickFireTask : IAssessmentTask
    {
        public AssessmentRecordingPrompt[] Prompts;
        public int Id { get; set; }
        public string Title { get; set; }
        public string Instructions { get; set; }
    }
}