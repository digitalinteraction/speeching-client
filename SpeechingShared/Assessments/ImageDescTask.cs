namespace SpeechingShared
{
    public class ImageDescTask : IAssessmentTask
    {
        public string Image;
        public ServerData.TaskType TaskType { get; set; }
        public AssessmentRecordingPromptCol PromptCol { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
    }
}