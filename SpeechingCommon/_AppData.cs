using Android.Content;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SpeechingCommon
{
    public static class AppData
    {
        // Account related data
        public static SessionData session;
        // System data
        public static string cacheDir;
        public static string placesCache = "/places";
        public static Random rand;

        static bool initializing = false;

        public static async Task InitializeIfNeeded()
        {
            if (session != null) return;

            while(initializing)
            {
                Task.Delay(100);
            }
            initializing = true;

            if(session == null)
            {
                if(!TryLoadExistingData())
                {

                    AppData.session.currentUser.id = 7041992;

                    await ServerData.FetchCategories();
                }
            }
            initializing = false;
        }

        /// <summary>
        /// Attempts to load existing data stored in a local file.
        /// </summary>
        /// <returns>true if successful, false if a request to the server is needed</returns>
        public static bool TryLoadExistingData()
        {
            if (rand == null) rand = new Random();

            try
            {
                if (!Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                    Directory.CreateDirectory(cacheDir + placesCache);
                }
                else if (File.Exists(cacheDir + "/offline.json"))
                {
                    var binder = new TypeNameSerializationBinder("SpeechingCommon.{0}, SpeechingCommon");
                    session = JsonConvert.DeserializeObject<SessionData>(File.ReadAllText(cacheDir + "/offline.json"), new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        Binder = binder
                    });
                    ServerData.storageRemoteDir = "uploads/" + session.currentUser.id + "/";
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading data: " + e);
            }

            session = new SessionData();
            ServerData.storageRemoteDir = "uploads/" + session.currentUser.id + "/";

            return false;
        }

        /// <summary>
        /// Saves the current data to the cache directory for loading later
        /// </summary>
        public static async void SaveCurrentData()
        {
            if (session == null || cacheDir == null) return; // Nothing to save
            var binder = new TypeNameSerializationBinder("SpeechingCommon.{0}, SpeechingCommon");
            string dataString = JsonConvert.SerializeObject(session, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Binder = binder
            });

            try
            {
                if (!Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                }

                File.WriteAllText(cacheDir + "/offline.json", dataString);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR SAVING DATA: " + e);
            }
        }

        /// <summary>
        /// Checks to see if the scenario has exported data waiting for upload
        /// </summary>
        /// <param name="id">The id of the scenario</param>
        /// <returns>Is Completed? bool</returns>
        public static bool CheckForActivityResultData(int id)
        {
            for (int i = 0; i < session.resultsToUpload.Count; i++)
            {
                if (session.resultsToUpload[i].CrowdActivityId == id) return true;
            }

            return false;
        }
    }
}