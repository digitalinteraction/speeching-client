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
        public User CurrentUser;
        public List<ActivityCategory> Categories;
        public List<IResultItem> ResultsToUpload;
        public List<User> UserCache;
        public List<ISpeechingPracticeActivity> ActivityCache;
        public bool ServerFolderExists = false;
        public Dictionary<string, string> PlacesPhotos;
        public static int ScenariosProcessing;
        public List<IFeedItem> CurrentFeed;

        public SessionData()
        {
            CurrentUser = new User();
            Categories = new List<ActivityCategory>();
            ResultsToUpload = new List<IResultItem>();
            UserCache = new List<User>();

            PlacesPhotos = new Dictionary<string, string>();
        }

        /// <summary>
        /// Attempt to find an practiceActivity with the given id. Poll the server if it isn't found
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public async Task<ISpeechingPracticeActivity> FetchActivityWithId(int activityId)
        {
            // See if it is in one of the categories already in memory
            foreach (ActivityCategory cat in Categories)
            {
                foreach (ISpeechingPracticeActivity act in cat.Activities)
                {
                    if (act.Id == activityId) return act;
                }
            }

            if (ActivityCache == null) ActivityCache = new List<ISpeechingPracticeActivity>();
            ISpeechingPracticeActivity newPracticeActivity = null;

            try
            {
                // Check if it's already been downloaded and exists in the cache
                foreach (ISpeechingPracticeActivity activity in ActivityCache)
                {
                    if ( activity != null && activity.Id == activityId) return activity;
                }

                // We don't have it locally - check the server and add to the cache for next time!
                newPracticeActivity = await ServerData.GetRequest<ISpeechingPracticeActivity>("Activity", activityId.ToString(), new ActivityConverter());
                ActivityCache.Add(newPracticeActivity);
            }
            catch (Exception ex)
            {
                return null;
            }
            

            AppData.SaveCurrentData();

            return newPracticeActivity;

        }

        /// <summary>
        /// Removes the result object from the toUpload list and deletes the files on disk (local deletion only)
        /// </summary>
        /// <param name="result">The item to delete</param>
        public async void DeleteResult(IResultItem result, bool save = true)
        {
            ResultsToUpload.Remove(result);

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

            foreach (IResultItem item in ResultsToUpload)
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
                ScenariosProcessing++;

                ISpeechingPracticeActivity practiceActivity = Categories[catIndex].Activities[scenIndex];
                if (practiceActivity.Id == null) practiceActivity.Id = AppData.Rand.Next(0, 10000);

                string result = await Utils.FetchLocalCopy(practiceActivity.Icon);

                if (result != null)
                {
                    practiceActivity.LocalIcon = result;
                    Categories[catIndex].Activities[scenIndex] = practiceActivity;
                    ScenariosProcessing--;
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