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
        public List<IResultItem> resultsToUpload;
        public List<User> userCache;
        public List<SpeechingActivityItem> activityCache;
        public bool serverFolderExists = false;
        public Dictionary<string, string> placesPhotos;

        public static int scenariosProcessing = 0;

        public SessionData()
        {
            currentUser = new User();
            categories = new List<ActivityCategory>();
            resultsToUpload = new List<IResultItem>();
            userCache = new List<User>();

            placesPhotos = new Dictionary<string, string>();
        }

        /// <summary>
        /// Attempt to find an activity with the given id. Poll the server if it isn't found
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public async Task<SpeechingActivityItem> FetchActivityWithId(int activityId)
        {
            // See if it is in one of the categories already in memory
            for (int i = 0; i < categories.Count; i++)
            {
                for (int j = 0; j < categories[i].Activities.Length; j++)
                {
                    if (categories[i].Activities[j].Id == activityId) return categories[i].Activities[j];
                }
            }

            if (activityCache == null) activityCache = new List<SpeechingActivityItem>();

            // Check if it's already been downloaded and exists in the cache
            foreach (SpeechingActivityItem activity in activityCache)
            {
                if (activity.Id == activityId) return activity;
            }

            // We don't have it locally - check the server and add to the cache for next time!
            SpeechingActivityItem newActivity = await ServerData.GetRequest<SpeechingActivityItem>("activity", activityId.ToString(), new ActivityConverter());
            activityCache.Add(newActivity);

            AppData.SaveCurrentData();

            return newActivity;

        }

        /// <summary>
        /// Removes the result object from the toUpload list and deletes the files on disk (local deletion only)
        /// </summary>
        /// <param name="result">The item to delete</param>
        public void DeleteResult(IResultItem result, bool save = true)
        {
            resultsToUpload.Remove(result);

            if(File.Exists(result.ResourceUrl))
                File.Delete(result.ResourceUrl);

            if (save) AppData.SaveCurrentData();
        }

        /// <summary>
        /// Find all results for the given scenario and remove them from the upload queue
        /// </summary>
        /// <param name="scenarioId"></param>
        public void DeleteAllPendingForScenario(int scenarioId)
        {
            List<IResultItem> toDelete = new List<IResultItem>();

            foreach (IResultItem item in resultsToUpload)
            {
                if (item.ParticipantActivityId == scenarioId) toDelete.Add(item);
            }

            foreach (IResultItem del in toDelete)
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

                SpeechingActivityItem activity = categories[catIndex].Activities[scenIndex];
                if (activity.Id == null) activity.Id = AppData.rand.Next(0, 1000);

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
                categories[catIndex].Activities[scenIndex] = activity;

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