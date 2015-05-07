using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PCLStorage;
using System.Threading;

namespace SpeechingShared
{
    public static class AppData
    {
        // Account related data
        public static SessionData session;
        // System data
        public static Random rand;
        public static IFolder root;
        public static IFolder cache;
        public static IFolder exports;
        public static IFile tempRecording;
        public static IPlatformSpecifics IO;
        private static IFile saveFile;

        public static Func<bool> checkForConnection;
        public static Action onConnectionSuccess;
        public static bool connectionInitialized = false;

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
                success = await TryLoadExistingData();

                CleanupPlaces(cache.Path, 10);
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
                root = await FileSystem.Current.GetFolderFromPathAsync(rootFolder);
            }
            else
            {
                root = FileSystem.Current.LocalStorage;
            }

            ExistenceCheckResult cacheExists = await root.CheckExistsAsync("cache");
            if(cacheExists == ExistenceCheckResult.NotFound)
            {
                cache = await root.CreateFolderAsync("cache", CreationCollisionOption.ReplaceExisting);
            }
            else
            {
                cache = await root.GetFolderAsync("cache");
            }

            ExistenceCheckResult exportExists = await root.CheckExistsAsync("exports");
            if (exportExists == ExistenceCheckResult.NotFound)
            {
                exports = await root.CreateFolderAsync("exports", CreationCollisionOption.FailIfExists);
            }
            else
            {
                exports = await root.GetFolderAsync("exports");
            }

            ExistenceCheckResult tempFileExists = await root.CheckExistsAsync("tempRec.mp4");
            if (tempFileExists == ExistenceCheckResult.NotFound)
            {
                tempRecording = await root.CreateFileAsync("tempRec.mp4", CreationCollisionOption.FailIfExists);
            }
            else
            {
                tempRecording = await root.GetFileAsync("tempRec.mp4");
            }
        }

        /// <summary>
        /// Check the size of the places cache and clean it up if necessary
        /// Get the biggest files and delete them by ascending order of last date accessed until under the limit
        /// </summary>
        private static async void CleanupPlaces(string path, int maxMb)
        {
            //await IO.CleanDirectory(path, maxMb);
            /*long size = Utils.DirSize(placesImageCache);
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
            }*/

            
        }

        /// <summary>
        /// Attempts to load existing data stored in a local file.
        /// </summary>
        /// <returns>true if successful, false if a request to the server is needed</returns>
        public static async Task<bool> TryLoadExistingData()
        {
            if (rand == null) rand = new Random();

            try
            {
                if ((await root.CheckExistsAsync("offline.json")) == ExistenceCheckResult.FileExists)
                {
                    IFile json = await root.GetFileAsync("offline.json");

                    var binder = new TypeNameSerializationBinder("SpeechingShared.{0}, SpeechingShared");
                    session = JsonConvert.DeserializeObject<SessionData>(await json.ReadAllTextAsync(), new JsonSerializerSettings
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
            var binder = new TypeNameSerializationBinder("SpeechingShared.{0}, SpeechingShared");
            string dataString = JsonConvert.SerializeObject(session, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Binder = binder
            });

            try
            {
                if(saveFile == null)
                {
                    if (await root.CheckExistsAsync("offline.json") != ExistenceCheckResult.FileExists)
                    {
                        await root.CreateFileAsync("offline.json", CreationCollisionOption.ReplaceExisting);
                    }
                    saveFile = await root.GetFileAsync("offline.json");
                }

                await Utils.GetSemaphore("offline.json").WaitAsync();
                try
                {
                    // Make sure only 1 thread is allowed here at a time :)
                    await saveFile.WriteAllTextAsync(dataString);
                }
                finally
                {
                    Utils.GetSemaphore("offline.json").Release();
                }
                
            }
            catch (Exception e)
            {
                throw e;
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