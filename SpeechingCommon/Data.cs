using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechingCommon
{
    public static class AppData
    {
        // Account related data
        public static SessionData session;

        // System data
        public static string cacheDir;

        /// <summary>
        /// Attempts to load existing data stored in a local file.
        /// </summary>
        /// <param name="_cacheDir">The local data directory</param>
        /// <returns>true if successful, false if a request to the server is needed</returns>
        public static bool TryLoadExistingData()
        {
            try
            {
                if (File.Exists(cacheDir + "/offline.json"))
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
        public static ResultItem[] GetSubmittedResults()
        {
            return session.resultsOnServer.ToArray(); //TEMP
        }

        /// <summary>
        /// Uploads a single result package to the server
        /// </summary>
        /// <param name="toUpload">The item to upload</param>
        public static void UploadResult(ResultItem toUpload)
        {
            session.resultsToUpload.Remove(toUpload);
            toUpload.uploaded = true;
            session.resultsOnServer.Add(toUpload); //TEMP
            SaveCurrentData();
        }

        /// <summary>
        /// Complete all pending uploads
        /// </summary>
        public static void UploadAllResults()
        {
            for(int i = 0; i < session.resultsToUpload.Count; i++)
            {
                UploadResult(session.resultsToUpload[i]);
            }
        }
        
        /// <summary>
        /// Checks to see if the scenario has exported data either locally or on the server
        /// </summary>
        /// <param name="id">The id of the scenario</param>
        /// <returns>Is Completed? bool</returns>
        public static bool CheckIfScenarioCompleted(string id)
        {
            for(int i = 0; i < session.resultsToUpload.Count; i++)
            {
                if (session.resultsToUpload[i].scenarioId == id) return true;
            }

            for (int i = 0; i < session.resultsOnServer.Count; i++)
            {
                if (session.resultsOnServer[i].scenarioId == id) return true;
            }

            return false;
        }

        /// <summary>
        /// Sends a friend request to the server
        /// </summary>
        /// <param name="username">The unique username of the friend</param>
        /// <returns>User found true / not recognised false</returns>
        public static bool SendFriendRequest(string username)
        {
            // TODO
            User added = new User();
            added.name = username;
            added.status = User.FriendStatus.Sent;
            session.friends.Add(added);

            SaveCurrentData();
            return true;
        }
    }

    public class SessionData
    {
        public User currentUser;
        public List<Scenario> scenarios;
        public List<ResultItem> resultsToUpload;
        public List<User> friends;

        // TEMP - will be pulled from the server eventually but store here for now TODO
        public List<ResultItem> resultsOnServer; 

        public SessionData()
        {
            currentUser = new User();
            scenarios = new List<Scenario>();
            resultsToUpload = new List<ResultItem>();
            friends = new List<User>();

            resultsOnServer = new List<ResultItem>();
        }

        /// <summary>
        /// Removes the result object from the toUpload list and deletes the files on disk (local deletion only)
        /// </summary>
        /// <param name="result">The item to delete</param>
        public void DeleteResult(ResultItem result)
        {
            resultsToUpload.Remove(result);

            File.Delete(result.dataLoc);
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
            this.allowedUsers = new List<string>();
        }
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
