
namespace SpeechingShared
{
    public interface IAssessmentTask
    {
        int Id { get; set; }
        string Title { get; set; }
        ServerData.TaskType TaskType { get; set; }
        AssessmentRecordingPromptCol PromptCol { get; set; }
    }
}