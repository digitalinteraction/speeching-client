using PCLStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SpeechingShared
{
    public class SessionData
    {
        public User currentUser;
        public List<ActivityCategory> categories;
        public List<IResultItem> resultsToUpload;
        public List<User> userCache;
        public List<ISpeechingActivityItem> activityCache;
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
        public async Task<ISpeechingActivityItem> FetchActivityWithId(int activityId)
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
            ISpeechingActivityItem newActivity = await ServerData.GetRequest<ISpeechingActivityItem>("activity", activityId.ToString(), new ActivityConverter());
            activityCache.Add(newActivity);

            AppData.SaveCurrentData();

            return newActivity;

        }

        /// <summary>
        /// Removes the result object from the toUpload list and deletes the files on disk (local deletion only)
        /// </summary>
        /// <param name="result">The item to delete</param>
        public async void DeleteResult(IResultItem result, bool save = true)
        {
            resultsToUpload.Remove(result);

            if(await AppData.Exports.CheckExistsAsync(Path.GetFileName(result.ResourceUrl)) == ExistenceCheckResult.FileExists)
            {
                IFile toDel = await AppData.Exports.GetFileAsync(Path.GetFileName(result.ResourceUrl));

                await toDel.DeleteAsync();
            }

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
        public async Task<bool> ProcessScenario(int catIndex, int scenIndex, bool shouldSave = true)
        {
            try
            {
                scenariosProcessing++;

                ISpeechingActivityItem activity = categories[catIndex].activities[scenIndex];
                if (activity.Id == null) activity.Id = AppData.Rand.Next(0, 10000);

                string result = await Utils.FetchLocalCopy(activity.Icon);

                if (result != null)
                {
                    activity.LocalIcon = result;
                    categories[catIndex].activities[scenIndex] = activity;
                    scenariosProcessing--;
                    if (shouldSave) AppData.SaveCurrentData();
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                AppData.Io.PrintToConsole(e.Message);
                return false;
            }

        }
    }

}