using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingShared
{
    public class LocationRecordingResult : IResultItem
    {
        public long Lat;
        public long Lng;
        public string GooglePlaceName;
        public string GooglePlaceId;

        public int Id { get; set; }
        public string ResourceUrl { get; set; }
        public int ParticipantActivityId { get; set; }
        public string UserId { get; set; }
        public Utils.UploadStage UploadState { get; set; }
        public DateTime CompletionDate { get; set; }

        public bool IsAssessment { get; set; }
    }
}