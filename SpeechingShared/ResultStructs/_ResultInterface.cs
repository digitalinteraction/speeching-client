using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingShared
{
    public interface IResultItem
    {
        Utils.UploadStage UploadState { get; set; }
        int Id { get; set; }
        string UserId { get; set; }
        string ResourceUrl { get; set; }
        int ParticipantActivityId { get; set; }
        DateTime CompletionDate { get; set; }
        bool IsAssessment { get; set; }
    }
}