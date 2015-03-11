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
        public string id;
        public string userId;
        public string activityId;
        public string dataLoc;
        public Dictionary<string, string> results;
        public UploadState uploadState;
        public bool isPublic;
        public List<string> allowedUsers;
        public DateTime completedAt;

        public ResultItem(string activityId, string dataLoc, string userId)
        {
            this.completedAt = DateTime.Now;
            this.id = activityId + "_" + completedAt.ToString();
            this.activityId = activityId;
            this.userId = userId;
            this.dataLoc = dataLoc;
            this.uploadState = UploadState.Ready;
            this.isPublic = false;
            this.allowedUsers = new List<string>();
            this.results = new Dictionary<string, string>();
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