using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SpeechingCommon
{
    public class SessionData
    {
        public User currentUser;
        public List<ActivityCategory> categories;
        public List<ResultItem> resultsToUpload;
        public List<User> userCache;
        public List<ISpeechingActivityItem> activityCache;
        public bool serverFolderExists = false;

        public static int scenariosProcessing = 0;

        // TEMP - will be pulled from the server eventually but store here for now TODO
        public List<ResultItem> resultsOnServer;

        public SessionData()
        {
            currentUser = new User();
            categories = new List<ActivityCategory>();
            resultsToUpload = new List<ResultItem>();
            userCache = new List<User>();
            resultsOnServer = new List<ResultItem>();
        }

        /// <summary>
        /// Attempt to find an activity with the given id. Poll the server if it isn't found
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public async Task<ISpeechingActivityItem> FetchActivityWithId(string activityId)
        {
            // See if it is in one of the categories already in memory
            for (int i = 0; i < categories.Count; i++)
            {
                for (int j = 0; j < categories[i].activities.Length; j++)
                {
                    if (categories[i].activities[j].Id == activityId) return categories[i].activities[j];
                }
            }

            if (activityCache == null) activityCache = new List<ISpeechingActivityItem>();

            // Check if it's already been downloaded and exists in the cache
            foreach(ISpeechingActivityItem activity in activityCache)
            {
                if (activity.Id == activityId) return activity;
            }

            // We don't have it locally - check the server and add to the cache for next time!
            ISpeechingActivityItem newActivity = await ServerData.GetRequest<ISpeechingActivityItem>("activity", activityId, new ActivityConverter());
            activityCache.Add(newActivity);

            AppData.SaveCurrentData();

            return newActivity;

        }

        /// <summary>
        /// Removes the result object from the toUpload list and deletes the files on disk (local deletion only)
        /// </summary>
        /// <param name="result">The item to delete</param>
        public void DeleteResult(ResultItem result, bool save = true)
        {
            resultsToUpload.Remove(result);

            File.Delete(result.dataLoc);

            if (save) AppData.SaveCurrentData();
        }

        /// <summary>
        /// Find all results for the given scenario and remove them from the upload queue
        /// </summary>
        /// <param name="scenarioId"></param>
        public void DeleteAllPendingForScenario(string scenarioId)
        {
            List<ResultItem> toDelete = new List<ResultItem>();

            foreach (ResultItem item in resultsToUpload)
            {
                if (item.activityId == scenarioId) toDelete.Add(item);
            }

            foreach (ResultItem del in toDelete)
            {
                DeleteResult(del, false);
            }

            AppData.SaveCurrentData();
        }

        /// <summary>
        /// Prepare a new scenario based off of the given JSON
        /// </summary>
        /// <param name="json"></param>
        public async Task ProcessScenario(int catIndex, int scenIndex, bool shouldSave = true)
        {
            try
            {
                scenariosProcessing++;

                ISpeechingActivityItem activity = categories[catIndex].activities[scenIndex];
                if (activity.Id == null) activity.Id = "act_" + AppData.rand.Next().ToString();

                string localIconPath = AppData.cacheDir + "/" + Path.GetFileName(activity.Icon);

                // Download the icon if it isn't already stored locally
                if (!File.Exists(localIconPath))
                {
                    WebClient request = new WebClient();
                    await request.DownloadFileTaskAsync(
                        new Uri(activity.Icon),
                        localIconPath
                        );
                    request.Dispose();
                    request = null;
                }

                activity.Icon = localIconPath;
                categories[catIndex].activities[scenIndex] = activity;

                if (shouldSave) AppData.SaveCurrentData();
                scenariosProcessing--;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error adding new activity item: " + e);
            }

        }
    }

}