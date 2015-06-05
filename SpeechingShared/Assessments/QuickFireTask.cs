namespace SpeechingShared
{
    public class QuickFireTask : IAssessmentTask
    {
        public AssessmentRecordingPromptCol PromptCol { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Instructions { get; set; }
    }
}