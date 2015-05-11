using System;
using System.Collections.Generic;

namespace SpeechingShared
{
    /// <summary>
    /// Holds information regarding a user's results from completing a Scenario
    /// </summary>
    public class ScenarioResult : IResultItem
    {
        public Dictionary<int, string> ParticipantTaskIdResults;

        private int id;
        private string userId;
        private string resource;
        private int activityId;
        private Utils.UploadStage state;
        private DateTime completedAt;

        public bool isAssessment = false;

        public ScenarioResult(int activityId, string dataLoc, string userId)
        {
            this.id = AppData.rand.Next(0, 100000); // TEMP
            this.activityId = activityId;
            this.resource = dataLoc;
            this.state = Utils.UploadStage.Ready;
            this.ParticipantTaskIdResults = new Dictionary<int, string>();
            this.userId = userId;
        }

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

        public string UserId
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