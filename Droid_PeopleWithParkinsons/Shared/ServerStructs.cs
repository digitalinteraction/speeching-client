using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Droid_PeopleWithParkinsons.Shared
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
        public static bool TryLoadExistingData(string _cacheDir)
        {
            cacheDir = _cacheDir;

            try
            {
                if (File.Exists(cacheDir + "/offline.json"))
                {
                    session = JsonConvert.DeserializeObject<SessionData>(File.ReadAllText(cacheDir + "/offline.json"));
                    return true;
                }
            }
            catch
            {
                return false;
            }
            
            return false;
        }

        /// <summary>
        /// Saves the current data to the cache directory for loading later
        /// </summary>
        public static async void SaveCurrentData(string _cacheDir = null)
        {
            if (_cacheDir != null) cacheDir = _cacheDir;

            if (session == null || cacheDir == null) return; // Nothing to save

            string dataString = JsonConvert.SerializeObject(session);
            File.WriteAllText(cacheDir + "/offline.json", dataString);
        }
    }

    public class SessionData
    {
        public User currentUser;
        public List<Scenario> scenarios;
        public List<ResultItem> resultsToUpload;
        public List<User> friends;
    }

    public class User
    {
        public enum UserType { Patient, Therapist, Rater };

        public string id;
        public string name;
        public string avatar;
        public UserType userType;
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

        public ResultItem(string scenarioId, string dataLoc)
        {
            this.completedAt = DateTime.Now;
            this.id = scenarioId + "_" + completedAt.ToString();
            this.scenarioId = scenarioId;
            this.userId = AppData.session.currentUser.id;
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
            for(int i = 0; i < coll.Count; i++)
            {
                if (coll[i].id == id) return coll[i];
            }

            return null;
        }
    }

    public class Utils 
    {
        public static async Task LoadStringFromFile(string fileAddress, Action<string> callback)
        {
            callback(System.IO.File.ReadAllText(fileAddress));
        }
    }
   
}