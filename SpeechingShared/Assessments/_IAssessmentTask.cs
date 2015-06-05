
namespace SpeechingShared
{
    public interface IAssessmentTask
    {
        int Id { get; set; }
        string Title { get; set; }
        string Instructions { get; set; }
        AssessmentRecordingPromptCol PromptCol { get; set; }
    }
}