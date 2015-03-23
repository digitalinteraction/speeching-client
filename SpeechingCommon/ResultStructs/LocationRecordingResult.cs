using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingCommon
{
    public class LocationRecordingResult : IResultItem
    {
        private int id;
        private int userId;
        private string resource;
        private int activityId;
        private Utils.UploadStage state;
        private DateTime completedAt;

        public long Lat;
        public long Lng;
        public string GooglePlaceName;
        public string GooglePlaceID;

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public string ResourceUrl
        {
            get
            {
                return resource;
            }
            set
            {
                resource = value;
            }
        }

        public int ParticipantActivityId
        {
            get
            {
                return activityId;
            }
            set
            {
                activityId = value;
            }
        }


        public int UserId
        {
            get
            {
                return userId;
            }
            set
            {
                userId = value;
            }
        }

        public Utils.UploadStage UploadState
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }


        public DateTime CompletionDate
        {
            get
            {
                return completedAt;
            }
            set
            {
                completedAt = value;
            }
        }
    }
}