using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using SQLite.Net.Attributes;
using SQLite.Net.Interop;
using SQLite.Net;

namespace SpeechingCommon
{
    public static class AppData
    {
        // Account related data
        public static SessionData session;
        // System data
        public static string cacheDir;
        public static string exportsCache;
        public static string avatarsCache;
        public static string placesImageCache;
        public static string placesRecordingsCache;
        public static string practiceRecording;
        public static Random rand;

        public static Func<bool> checkForConnection;
        public static Action onConnectionSuccess;
        public static bool connectionInitialized = false;

        static bool initializing = false;

        /// <summary>
        /// Checks that the app is connected to the network, performing first time inits if necessary
        /// </summary>
        /// <returns>Connection successful?</returns>
        public static bool CheckNetwork()
        {
            if(checkForConnection())
            {
                if(!connectionInitialized)
                {
                    connectionInitialized = true;
                    onConnectionSuccess();
                }
                return true;
            }

            connectionInitialized = false;
            return false;
        }

        /// <summary>
        /// For use at all entry points into the application, making sure that all data will be available
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> InitializeIfNeeded()
        {
            bool success = session != null;

            if(!success)
            {
                success = TryLoadExistingData();

                CleanupPlaces();
            }

            return success;
        }

        /// <summary>
        /// Initialise and create necessary cache folders
        /// </summary>
        /// <param name="rootFolder"></param>
        public static void AssignCacheLocations(string rootFolder)
        {
            cacheDir = rootFolder;
            placesImageCache = Path.Combine(cacheDir, "places/");
            avatarsCache = Path.Combine(cacheDir, "avatars/");
            placesRecordingsCache = Path.Combine(placesImageCache, "tempRecs/");
            exportsCache = Path.Combine(cacheDir, "exports/");
            practiceRecording = Path.Combine(cacheDir, "practice.mp4");

            if (!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }

            if (!Directory.Exists(avatarsCache))
            {
                Directory.CreateDirectory(avatarsCache);
            }

            if (!Directory.Exists(placesImageCache))
            {
                Directory.CreateDirectory(placesImageCache);
            }

            if (!Directory.Exists(placesRecordingsCache))
            {
                Directory.CreateDirectory(placesRecordingsCache);
            }

            if (!Directory.Exists(exportsCache))
            {
                Directory.CreateDirectory(exportsCache);
            }
        }

        /// <summary>
        /// Check the size of the places cache and clean it up if necessary
        /// Get the biggest files and delete them by ascending order of last date accessed until under the limit
        /// </summary>
        private static async void CleanupPlaces()
        {
            long size = Utils.DirSize(placesImageCache);
            long max = 1000000;// 1Mb

            if(size >= max)
            {
                DirectoryInfo di = new DirectoryInfo(placesImageCache);
                FileInfo[] allFiles = di.GetFiles();

                // Sort by file size
                Array.Sort<FileInfo>(allFiles, delegate(FileInfo a, FileInfo b)
                {
                    return b.Length.CompareTo(a.Length);
                });

                FileInfo[] biggest = new FileInfo[allFiles.Length / 2];

                for (int i = 0; i < biggest.Length; i++ )
                {
                    biggest[i] = allFiles[i];
                }

                // Sort by last accessed
                Array.Sort<FileInfo>(biggest, delegate(FileInfo a, FileInfo b)
                {
                    return a.LastAccessTime.CompareTo(b.LastAccessTime);
                });

                // Array should now be the biggest files, in order of date last accessed (earliest first)
                // Delete one by one until under the limit
                int count = 0;
                while (size >= max && count < biggest.Length)
                {
                    try
                    {
                        // Remove reference 
                        string thisKey = session.placesPhotos.FirstOrDefault(x => x.Value == biggest[count].FullName).Key;

                        if (thisKey != null)
                            session.placesPhotos.Remove(thisKey);

                        size -= biggest[count].Length;

                        File.Delete(biggest[count].FullName);
                        count++;
                    }
                    catch(Exception e)
                    {
                        throw e;
                        break;
                    }
                }
                AppData.SaveCurrentData();
            }

            
        }

        /// <summary>
        /// Attempts to load existing data stored in a local file.
        /// </summary>
        /// <returns>true if successful, false if a request to the server is needed</returns>
        public static bool TryLoadExistingData()
        {
            if (rand == null) rand = new Random();

            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "speechingData.db3");

            SQLiteConnection db;

            try
            {
                db = new SQLiteConnection(new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid(), dbPath);
                db.CreateTable<SpeechingTask>();
                db.CreateTable<User>();
                db.CreateTable<SpeechingActivityItem>();
                db.CreateTable<Scenario>();
                db.CreateTable<Guide>();
                db.CreateTable<ActivityCategory>();
                
            }
            catch(Exception ex)
            {
                throw ex;
            }
           

            try
            {
                if (File.Exists(cacheDir + "/offline.json"))
                {
                    var binder = new TypeNameSerializationBinder("SpeechingCommon.{0}, SpeechingCommon");
                    session = JsonConvert.DeserializeObject<SessionData>(File.ReadAllText(cacheDir + "/offline.json"), new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        Binder = binder
                    });
                    ServerData.storageRemoteDir = "uploads/" + session.currentUser.id + "/";

                    if (db.Table<ActivityCategory>().Count() != 0)
                    {
                        db.DropTable<ActivityCategory>();
                        db.CreateTable<ActivityCategory>();
                    }

                    ActivityCategory cat = new ActivityCategory
                    {
                        Title = "Test cat",
                        Activities = new[] { 
                            new Scenario{
                                Title = "Test scenario"
                            }
                         }
                    };

                    db.InsertWithChildren(cat, recursive: true);
                    //db.InsertAllWithChildren(session.categories);

                    TableQuery<ActivityCategory> table = db.Table<ActivityCategory>();

                    foreach (ActivityCategory found in table)
                    {
                        ActivityCategory fullfound = db.GetWithChildren<ActivityCategory>(found.Id, true);

                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading data: " + e);
            }

            session = new SessionData();
            return false;
        }

        /// <summary>
        /// Makes the current user the owner of this session
        /// </summary>
        public static void AssignCurrentUser(User thisUser)
        {
            if (session == null) session = new SessionData();
            session.currentUser = thisUser;
            ServerData.storageRemoteDir = "uploads/" + thisUser.id + "/";

            SaveCurrentData();
        }

        /// <summary>
        /// Saves the current data to the cache directory for loading later
        /// </summary>
        public static async void SaveCurrentData()
        {
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
                if (session.resultsToUpload[i].ParticipantActivityId == id) return true;
            }

            return false;
        }
    }
}