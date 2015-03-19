using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingCommon
{
    public interface IResultItem
    {
        Utils.UploadStage UploadState { get; set; }
        int Id { get; set; }
        int UserId { get; set; }
        string ResourceUrl { get; set; }
        int CrowdActivityId { get; set; }
        DateTime CompletionDate { get; set; }
    }
}