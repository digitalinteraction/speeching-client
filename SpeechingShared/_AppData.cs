using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PCLStorage;

namespace SpeechingShared
{
    public static class AppData
    {
        // Account related data
        public static SessionData Session;
        // System data
        public static Random Rand;
        public static IFolder Root;
        public static IFolder Cache;
        public static IFolder Exports;
        public static IFile TempRecording;
        public static IPlatformSpecifics Io;
        private static IFile _saveFile;
        public static Func<bool> CheckForConnection;
        public static Action OnConnectionSuccess;
        public static bool ConnectionInitialized;

        /// <summary>
        /// Checks that the app is connected to the network, performing first time inits if necessary
        /// </summary>
        /// <returns>Connection successful?</returns>
        public static bool CheckNetwork()
        {
            if (CheckForConnection())
            {
                if (!ConnectionInitialized)
                {
                    ConnectionInitialized = true;
                    OnConnectionSuccess();
                }
                return true;
            }

            ConnectionInitialized = false;
            return false;
        }

        /// <summary>
        /// For use at all entry points into the application, making sure that all data will be available
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> InitializeIfNeeded()
        {
            bool success = Session != null;

            if (!success)
            {
                success = await TryLoadExistingData();

                TrimCacheDirectory();
            }

            return success;
        }

        /// <summary>
        /// Initialise and create necessary cache folders
        /// </summary>
        /// <param name="rootFolder"></param>
        public static async Task AssignCacheLocations(string rootFolder = null)
        {
            if (rootFolder != null)
            {
                Root = await FileSystem.Current.GetFolderFromPathAsync(rootFolder);
            }
            else
            {
                Root = FileSystem.Current.LocalStorage;
            }

            ExistenceCheckResult cacheExists = await Root.CheckExistsAsync("cache");
            if (cacheExists == ExistenceCheckResult.NotFound)
            {
                Cache = await Root.CreateFolderAsync("cache", CreationCollisionOption.ReplaceExisting);
            }
            else
            {
                Cache = await Root.GetFolderAsync("cache");
            }

            ExistenceCheckResult exportExists = await Root.CheckExistsAsync("exports");
            if (exportExists == ExistenceCheckResult.NotFound)
            {
                Exports = await Root.CreateFolderAsync("exports", CreationCollisionOption.FailIfExists);
            }
            else
            {
                Exports = await Root.GetFolderAsync("exports");
            }

            ExistenceCheckResult tempFileExists = await Root.CheckExistsAsync("tempRec.mp4");
            if (tempFileExists == ExistenceCheckResult.NotFound)
            {
                TempRecording = await Root.CreateFileAsync("tempRec.mp4", CreationCollisionOption.FailIfExists);
            }
            else
            {
                TempRecording = await Root.GetFileAsync("tempRec.mp4");
            }
        }

        /// <summary>
        /// Check the size of the places cache and clean it up if necessary
        /// Get the biggest files and delete them by ascending order of last date accessed until under the limit
        /// </summary>
        private static void TrimCacheDirectory()
        {
            Io.CleanDirectory(Cache, 8);
        }

        /// <summary>
        /// Attempts to load existing data stored in a local file.
        /// </summary>
        /// <returns>true if successful, false if a request to the server is needed</returns>
        public static async Task<bool> TryLoadExistingData()
        {
            if (Rand == null) Rand = new Random();

            try
            {
                if ((await Root.CheckExistsAsync("offline.json")) == ExistenceCheckResult.FileExists)
                {
                    IFile json = await Root.GetFileAsync("offline.json");

                    var binder = new TypeNameSerializationBinder("SpeechingShared.{0}, SpeechingShared");
                    Session = JsonConvert.DeserializeObject<SessionData>(await json.ReadAllTextAsync(),
                        new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto,
                            Binder = binder
                        });
                    ServerData.StorageRemoteDir = "uploads/" + Session.currentUser.Id + "/";
                    return true;
                }
            }
            catch (Exception e)
            {
                Io.PrintToConsole(e.Message);
            }

            Session = new SessionData();
            return false;
        }

        /// <summary>
        /// Makes the current user the owner of this session
        /// </summary>
        public static void AssignCurrentUser(User thisUser)
        {
            if (Session == null) Session = new SessionData();
            Session.currentUser = thisUser;
            ServerData.StorageRemoteDir = "uploads/" + thisUser.Id + "/";

            SaveCurrentData();
        }

        /// <summary>
        /// Saves the current data to the cache directory for loading later
        /// </summary>
        public static async void SaveCurrentData()
        {
            var binder = new TypeNameSerializationBinder("SpeechingShared.{0}, SpeechingShared");
            string dataString = JsonConvert.SerializeObject(Session, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Binder = binder
            });

            if (_saveFile == null)
            {
                if (await Root.CheckExistsAsync("offline.json") != ExistenceCheckResult.FileExists)
                {
                    await Root.CreateFileAsync("offline.json", CreationCollisionOption.ReplaceExisting);
                }
                _saveFile = await Root.GetFileAsync("offline.json");
            }

            await Utils.GetSemaphore("offline.json").WaitAsync();
            try
            {
                // Make sure only 1 thread is allowed here at a time :)
                await _saveFile.WriteAllTextAsync(dataString);
            }
            finally
            {
                Utils.GetSemaphore("offline.json").Release();
            }
        }

        /// <summary>
        /// Checks to see if the scenario has exported data waiting for upload
        /// </summary>
        /// <param name="id">The id of the scenario</param>
        /// <returns>Is Completed? bool</returns>
        public static bool CheckForActivityResultData(int id)
        {
            foreach (IResultItem result in Session.resultsToUpload)
            {
                if (result.ParticipantActivityId == id) return true;
            }

            return false;
        }
    }
}