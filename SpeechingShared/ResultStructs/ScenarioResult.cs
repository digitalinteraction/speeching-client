using System;
using System.Collections.Generic;

namespace SpeechingShared
{
    /// <summary>
    /// Holds information regarding a user's results from completing a Scenario
    /// </summary>
    public class ScenarioResult : IResultItem
    {
        public int Id { get; set; }
        public string ResourceUrl { get; set; }
        public int ParticipantActivityId { get; set; }
        public Utils.UploadStage UploadState { get; set; }
        public string UserId { get; set; }
        public DateTime CompletionDate { get; set; }
        public List<ParticipantResultData> Data { get; set; }

        public bool IsAssessment { get; set; }

        public ScenarioResult(int activityId, string dataLoc, string userId)
        {
            Id = AppData.Rand.Next(0, 100000); // TEMP
            ParticipantActivityId = activityId;
            ResourceUrl = dataLoc;
            UploadState = Utils.UploadStage.Ready;
            Data = new List<ParticipantResultData>();
            UserId = userId;
        }
    }

    public class ParticipantResultData
    {
        public int? ParticipantTaskId { get; set; }
        public int? ParticipantAssessmentTaskId { get; set; }
        public int? ParticipantAssessmentTaskPromptId { get; set; }

        public string FilePath { get; set; }
        public string ExtraData { get; set; }
    }
}