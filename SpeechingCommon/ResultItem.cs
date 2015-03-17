using System;
using System.Collections.Generic;

namespace SpeechingCommon
{
    /// <summary>
    /// Holds information regarding a user's results from completing an Activity
    /// </summary>
    public class ResultItem
    {
        public enum UploadState { Incomplete, Ready, Uploading, Uploaded };
        public int id;
        public int userId;
        public int CrowdActivityId;
        public string ResourceUrl;
        public Dictionary<int, string> ParticipantTaskIdResults;
        public UploadState uploadState;
        public bool isPublic;
        public List<int> allowedUsers;
        public DateTime completedAt;

        public ResultItem(int activityId, string dataLoc, int userId)
        {
            this.completedAt = DateTime.Now;
            this.id = AppData.rand.Next(0, 100000); // TEMP
            this.CrowdActivityId = activityId;
            this.userId = userId;
            this.ResourceUrl = dataLoc;
            this.uploadState = UploadState.Ready;
            this.isPublic = false;
            this.allowedUsers = new List<int>();
            this.ParticipantTaskIdResults = new Dictionary<int, string>();
        }

        /// <summary>
        /// Commit the current permissions list to the server
        /// </summary>
        public void PushPermissionUpdates()
        {
            //TODO
        }
    }
}