using System.Collections.Generic;

namespace SpeechingShared
{
    public class AssessmentRecordingPromptCol
    {
        public enum PromptTaskType
        {
            MinimalPairs, ImageDesc
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public PromptTaskType PromptType { get; set; }
        public AssessmentRecordingPrompt[] Prompts { get; set; }
    }

    public class AssessmentRecordingPrompt
    {
        public int Id;
        public string Value;
    }
}
