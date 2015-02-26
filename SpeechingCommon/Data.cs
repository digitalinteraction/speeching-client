using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpeechingCommon
{
    /// <summary>
    /// A collection of server structures and methods, along with functions that will be used commonly across all platforms.
    /// Methods starting with "Push" request changes be made to server data. Methods starting with "Fetch" pull data from the server.
    /// </summary>
    public static class AppData
    {
        // Account related data
        public static SessionData session;

        // System data
        public static string cacheDir;

        public static Random rand;

        /// <summary>
        /// Attempts to load existing data stored in a local file.
        /// </summary>
        /// <returns>true if successful, false if a request to the server is needed</returns>
        public static bool TryLoadExistingData()
        {
            if (rand == null) rand = new Random();

            try
            {
                if(!Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                }
                else if (File.Exists(cacheDir + "/offline.json"))
                {
                    session = JsonConvert.DeserializeObject<SessionData>(File.ReadAllText(cacheDir + "/offline.json"));
                    return true;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Error loading data: " + e);
            }

            session = new SessionData();
            return false;
        }

        /// <summary>
        /// Saves the current data to the cache directory for loading later
        /// </summary>
        public static async void SaveCurrentData()
        {
            if (session == null || cacheDir == null) return; // Nothing to save

            string dataString = JsonConvert.SerializeObject(session);

            try
            {
                if (!Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                }

                File.WriteAllText(cacheDir + "/offline.json", dataString);
            }
            catch(Exception e)
            {
                Console.WriteLine("ERROR SAVING DATA: " + e);
            }
        }

        /// <summary>
        /// Get a list of the user's submitted data from the server
        /// </summary>
        public static ResultItem[] FetchSubmittedResults()
        {
            return session.resultsOnServer.ToArray(); //TEMP
        }

        /// <summary>
        /// Get the submission with the given ID
        /// </summary>
        /// <param name="resultId"></param>
        /// <returns></returns>
        public static ResultItem FetchSubmittedResult(string resultId)
        {
            // Check if the item exists in cache before polling the server
            foreach(ResultItem result in session.resultsOnServer)
            {
                if(result.id == resultId) return result;
            }

            //TODO poll server
            return null;
        }

        /// <summary>
        /// Uploads a single result package to the server
        /// </summary>
        /// <param name="toUpload">The item to upload</param>
        public static void PushResult(ResultItem toUpload)
        {
            session.resultsToUpload.Remove(toUpload);
            toUpload.uploaded = true;
            session.resultsOnServer.Add(toUpload); //TEMP
            SaveCurrentData();
        }

        /// <summary>
        /// Complete all pending uploads
        /// </summary>
        public static void PushAllResults()
        {
            for(int i = 0; i < session.resultsToUpload.Count; i++)
            {
                PushResult(session.resultsToUpload[i]);
            }
        }

        /// <summary>
        /// Requests the deletion of the result from the remote database
        /// </summary>
        public static void PushResultDeletion(ResultItem toDelete)
        {
            //TEMP
            session.resultsOnServer.Remove(toDelete);
            SaveCurrentData();
        }
        
        /// <summary>
        /// Checks to see if the scenario has exported data waiting for upload
        /// </summary>
        /// <param name="id">The id of the scenario</param>
        /// <returns>Is Completed? bool</returns>
        public static bool CheckIfScenarioCompleted(string id)
        {
            for(int i = 0; i < session.resultsToUpload.Count; i++)
            {
                if (session.resultsToUpload[i].scenarioId == id) return true;
            }

            return false;
        }

        /// <summary>
        /// Fetch a single user using the given id - TEMP will search by email
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static User FetchUser(string username)
        {
            foreach (User user in session.userCache)
            {
                if (user.name == username)
                {
                    return user;
                }
            }
            return null;
        }

        /// <summary>
        /// Polls the server for the users of the given IDs
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public static User[] FetchUsers(List<string> userIds)
        {
            List<User> users = new List<User>();

            //TEMP - will be polling the server (although checking the cache would be useful...)
            foreach(User user in session.userCache)
            {
                if(userIds.Contains(user.id))
                {
                    users.Add(user);
                    if (users.Count >= userIds.Count) break; //Found all of them! :)
                }
            }

            return users.ToArray();
        }

        /// <summary>
        /// Filtered version of the friends list showing only users who have accepted a friend request
        /// </summary>
        /// <returns></returns>
        public static List<User> FetchAcceptedFriends()
        {
            List<User> toRet = new List<User>();
            User[] allFriends = AppData.FetchUsers(AppData.session.currentUser.friends);

            foreach (User friend in allFriends)
            {
                if (friend.id != AppData.session.currentUser.id && friend.status == User.FriendStatus.Accepted)
                {
                    toRet.Add(friend);
                }
            }

            return toRet;
        }

        /// <summary>
        /// Sends a friend request to the server
        /// </summary>
        /// <param name="username">The unique username of the friend</param>
        /// <returns>User found true / not recognised false</returns>
        public static bool PushFriendRequest(string username)
        {
            // TODO push friend request to the server, which will return user details if successful
            User added = new User();
            added.name = username;
            added.status = User.FriendStatus.Sent;
            added.id = DateTime.Now.ToLongTimeString();

            // TODO Check if the server returns an error saying they're already friends, or the user doesn't exist etc

            // If the user has already been cached, update the object. Else, just add it
            bool cached = false;
            for (int i = 0; i < session.userCache.Count; i++)
            {
                if (session.userCache[i].id == added.id)
                {
                    session.userCache[i] = added;
                    cached = true;
                    break;
                }
            }
            if(!cached) session.userCache.Add(added);

            session.currentUser.friends.Add(added.id);

            SaveCurrentData();
            return true;
        }

        /// <summary>
        /// Prepares all of the submission's data, including audio recordings
        /// </summary>
        /// <param name="resultId"></param>
        /// <returns></returns>
        public static async Task<ResultPackage> FetchResultItemComplete(string resultId)
        {
            ResultPackage ret = null;

            // TEMP - should fetch from server
            foreach(ResultItem item in session.resultsOnServer)
            {
                if(item.id == resultId)
                {
                    ret = new ResultPackage(item);
                    ret.resources = null;
                    break;
                }
            }
            if (ret == null) return null; // Not found on 'server'

            string extractPath = cacheDir + "/DL_" + resultId;

            ret.resources = new Dictionary<string, string>();
            ret.scenario = Scenario.GetWithId(session.scenarios, ret.resultItem.scenarioId);

            // No need to download + unpack zip if this folder already exists
            if(Directory.Exists(extractPath))
            {
                string[] files = Directory.GetFiles(extractPath);
                foreach(string file in files)
                {
                    ret.resources.Add(Path.GetFileName(file), file);
                }

                return ret;
            }

            // Data isn't already present - download the zip and extract it!
            // TODO zip needs to download
            ZipFile zip = null;
            try
            {
                //Unzip the downloaded file and add references to its contents in the resources dictionary
                zip = new ZipFile(File.OpenRead(ret.resultItem.dataLoc));

                foreach (ZipEntry entry in zip)
                {
                    string filename = Path.Combine(extractPath, entry.Name);
                    byte[] buffer = new byte[4096];
                    System.IO.Stream zipStream = zip.GetInputStream(entry);
                    using (FileStream streamWriter = File.Create(filename))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                    ret.resources.Add(entry.Name, filename);
                }
            }
            finally
            {
                if (zip != null)
                {
                    zip.IsStreamOwner = true;
                    zip.Close();
                    File.Delete(ret.resultItem.dataLoc);
                }
            }

            return ret;
        }

        /// <summary>
        /// Polls the server for all available feedback for the given result
        /// </summary>
        /// <param name="resultId"></param>
        /// <returns></returns>
        public static FeedbackItem[] FetchFeedback(string resultId)
        {
            // TEMP
            FeedbackItem[] arr = new FeedbackItem[5];

            for(int i = 0; i < arr.Length; i++)
            {
                FeedbackItem fb = new FeedbackItem();
                fb.comments = "Placeholder feedback to do with your speech - this is feedback item number " + i;
                fb.id = "fb" + i;
                fb.resultId = resultId;
                fb.easeOfListeningRating = rand.Next(1, 5);
            }

            return arr;
        }
    }

    public class SessionData
    {
        public User currentUser;
        public List<Scenario> scenarios;
        public List<ResultItem> resultsToUpload;
        public List<User> userCache;

        // TEMP - will be pulled from the server eventually but store here for now TODO
        public List<ResultItem> resultsOnServer; 

        public SessionData()
        {
            currentUser = new User();
            scenarios = new List<Scenario>();
            resultsToUpload = new List<ResultItem>();
            userCache = new List<User>();
            resultsOnServer = new List<ResultItem>();
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
        /// Find all results for the given scenario aand remove them from the upload queue
        /// </summary>
        /// <param name="scenarioId"></param>
        public void DeleteAllPendingForScenario(string scenarioId)
        {
            List<ResultItem> toDelete = new List<ResultItem>();

            foreach(ResultItem item in resultsToUpload)
            {
                if (item.scenarioId == scenarioId) toDelete.Add(item);
            }

            foreach(ResultItem del in toDelete)
            {
                DeleteResult(del, false);
            }

            AppData.SaveCurrentData();
        }

        /// <summary>
        /// Prepare a new scenario based off of the given JSON
        /// </summary>
        /// <param name="json"></param>
        public async Task ProcessNewScenario(string json, bool shouldSave = true)
        {
            try
            {
                Scenario scenario = JsonConvert.DeserializeObject<Scenario>(json);
                scenario.id = "scenario_" + AppData.rand.Next().ToString();

                string localIconPath = AppData.cacheDir + "/" + Path.GetFileName(scenario.icon);

                // Download the icon if it isn't already stored locally
                if (!File.Exists(localIconPath))
                {
                    WebClient request = new WebClient();
                    await request.DownloadFileTaskAsync(
                        new Uri(scenario.icon),
                        localIconPath
                        );
                    request.Dispose();
                    request = null;
                }

                scenario.icon = localIconPath;
                scenarios.Add(scenario);

                if (shouldSave) AppData.SaveCurrentData();
            }
            catch(Exception e)
            {
                Console.WriteLine("Error adding new scenario: " + e);
            }
            
        }
    }

    public class User
    {
        public enum UserType { Patient, Therapist, Rater };
        public enum FriendStatus { Accepted, Denied, Sent, Received };

        public string id;
        public string name;
        public string avatar;
        public UserType userType;
        public FriendStatus status;
        public List<string> friends;

        public User()
        {
            friends = new List<string>();
        }
    }

    public class UserTask
    {
        public string id;
        public string title;
        public string icon;
        public DateTime lastActive;
    }

    public class ResultItem
    {
        public string id;
        public string userId;
        public string scenarioId;
        public string dataLoc;
        public bool uploaded;
        public bool isPublic;
        public List<string> allowedUsers;
        public DateTime completedAt;

        public ResultItem(string scenarioId, string dataLoc, string userId)
        {
            this.completedAt = DateTime.Now;
            this.id = scenarioId + "_" + completedAt.ToString();
            this.scenarioId = scenarioId;
            this.userId = userId;
            this.dataLoc = dataLoc;
            this.uploaded = false;
            this.isPublic = false;
            this.allowedUsers = new List<string>();
        }

        /// <summary>
        /// Commit the current permissions list to the server
        /// </summary>
        public void PushPermissionUpdates()
        {
            //TODO
        }
    }

    public class ResultPackage
    {
        // Includes addresses for unzipped recordings and scenario for easy access
        public Dictionary<string, string> resources;
        public Scenario scenario;
        public ResultItem resultItem;

        public ResultPackage(ResultItem result)
        {
            resultItem = result;
            resources = new Dictionary<string, string>();
        }
    }

    // Are there different feedback models?
    public class FeedbackItem
    {
        public string id;
        public string resultId;
        public string userId; //Null for HipHopAnonymous?
        public int easeOfListeningRating;
        public string comments;
    }

    public class EventContent
    {
        public string type;
        public string visual;
        public string audio;
        public string text;
    }

    public class EventResponse
    {
        public string type;
        public string prompt;
        public string resultPath;
    }

    public class ScenarioEvent
    {
        public EventContent content;
        public EventResponse response;
    }

    public class Scenario
    {
        public string id;
        public User creator;
        public string title;
        public string resources;
        public string icon;
        public ScenarioEvent[] events;

        public static Scenario GetWithId(List<Scenario> coll, string id)
        {
            for (int i = 0; i < coll.Count; i++)
            {
                if (coll[i].id == id) return coll[i];
            }

            return null;
        }
    }

    public class ServerError
    {
        public string id;
        public string title;
        public string message;
    }

    public class Utils
    {
        public static async Task LoadStringFromFile(string fileAddress, Action<string> callback)
        {
            callback(System.IO.File.ReadAllText(fileAddress));
        }
    }
}
