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
        public enum ActivityType { Scenario, Guide };

        // System data
        public static string cacheDir;

        public static string server = @"https://di.ncl.ac.uk/owncloud/remote.php/webdav/";
        public static string remoteUploads;
        public static string username = @"speeching";
        public static string password = @"BlahBlah123";
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
                    remoteUploads = "uploads/" + session.currentUser.id + "/";
                    return true;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Error loading data: " + e);
            }

            session = new SessionData();
            remoteUploads = "uploads/" + session.currentUser.id + "/";

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
        /// Get the categories from the server
        /// </summary>
        public static async Task FetchCategories()
        {
            try
            {
                string json = "[\r\n{\r\n    \"id\" : \"anId\",\r\n    \"title\" : \"Dysfluency\",\r\n    \"icon\"  : \"https://cdn0.iconfinder.com/data/icons/cosmo-medicine/40/test-tube_1-128.png\",\r\n    \"recommended\" : false,\r\n    \"activities\" : [\r\n    {\r\n        \"Id\" : \"testScenario\",\r\n        \"Creator\" : {\r\n            \"id\"    : \"thatId\",\r\n            \"name\"  : \"Justin Time\"\r\n        },\r\n        \"Title\" : \"Getting the Bus\",\r\n        \"Icon\"  : \"http://www.survivingamsterdam.com/public/files/e96fc9baf228c0cb8d210a1768995bb1.png\",\r\n        \"Resources\" : \"https://www.dropbox.com/s/0h2f8pyrh6xte3s/bus.zip?raw=1\",\r\n        \"ActivityType\" : \"Scenario\",\r\n        \"Tasks\" : [\r\n        {\r\n            \"id\" : \"sc1ev1\",\r\n            \"content\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"driver.jpg\",\r\n                \"audio\"  : \"hello.mp3\",\r\n                \"text\"   : \"Hello! Where would you like to go today?\"\r\n            },\r\n            \"response\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Hello, please may I have a return ticket to the train station?\"\r\n            }\r\n        },\r\n        {\r\n            \"id\" : \"sc1ev2\",\r\n            \"content\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"driver.jpg\",\r\n                \"audio\"  : \"thanks.mp3\",\r\n                \"text\"   : \"No problem at all, looks like you have a valid card. Take a seat!\"\r\n            },\r\n            \"response\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Thank you. Have a good day.\"\r\n            }\r\n        },\r\n        {\r\n            \"id\" : \"sc1ev3\",\r\n            \"content\" : {\r\n                \"type\"   : \"Text\",\r\n                \"visual\" : \"oldwoman.jpg\",\r\n                \"audio\"  : null,\r\n                \"text\"   : \"You sit next to an old woman, who asks what your plans are for the day.\"\r\n            },\r\n            \"response\" : {\r\n                \"type\"  : \"Freeform\",\r\n                \"prompt\" : \"Greet her and explain that you're catching a train to the seaside.\"\r\n            }\r\n        }\r\n        ]\r\n    },\r\n    {\r\n        \"Id\" : \"testScenario2\",\r\n        \"Creator\" : {\r\n            \"id\"    : \"thatId\",\r\n            \"name\"  : \"Justin Time\"\r\n        },\r\n        \"Title\" : \"Ordering a Pizza\",\r\n        \"Icon\"  : \"http://img3.wikia.nocookie.net/__cb20131231163822/cardfight/images/6/6f/Pizza_slice_combo_clipart.png\",\r\n        \"Resources\" : \"https://www.dropbox.com/s/8gt7pqh6zq6p18h/pizza.zip?raw=1\",\r\n        \"ActivityType\" : \"Scenario\",\r\n        \"tasks\" : [\r\n        {\r\n            \"id\" : \"sc2ev1\",\r\n            \"content\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"phone.jpg\",\r\n                \"audio\"  : \"hello.mp3\",\r\n                \"text\"   : \"You are ordering pizza over the phone for both yourself and a friend who has a gluten alergy.\\n'Tony's Pizza Parlour, how can I help you?'\"\r\n            },\r\n            \"response\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Hello, can I order a pizza please?\"\r\n            }\r\n        },\r\n        {\r\n            \"id\" : \"sc2ev2\",\r\n            \"content\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"pizza.jpg\",\r\n                \"audio\"  : \"order1.mp3\",\r\n                \"text\"   : \"Of course! What kind would you like?\"\r\n            },\r\n            \"response\" : {\r\n                \"type\"  : \"Freeform\",\r\n                \"prompt\" : \"Describe your favourite pizza\"\r\n            }\r\n        },\r\n        {\r\n            \"id\" : \"sc2ev3\",\r\n            \"content\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"pizza.jpg\",\r\n                \"audio\"  : \"order2.mp3\",\r\n                \"text\"   : \"That sounds delicious! Would you like anything else?\"\r\n            },\r\n            \"response\" : {\r\n                \"type\"  : \"Freeform\",\r\n                \"prompt\" : \"Describe another kind of pizza, but make sure it's gluten free!\"\r\n            }\r\n        },\r\n        {\r\n            \"id\" : \"sc2ev4\",\r\n            \"content\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"making.jpg\",\r\n                \"audio\"  : \"end.mp3\",\r\n                \"text\"   : \"No problem at all, we can do that. See you soon!\"\r\n            },\r\n            \"response\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Thank you, see you later.\"\r\n            }\r\n        },\r\n        ]\r\n    }\r\n    ]\r\n},\r\n{\r\n\"id\" : \"anId2\",\r\n    \"title\" : \"Dementia\",\r\n    \"icon\"  : \"https://cdn0.iconfinder.com/data/icons/cosmo-medicine/40/test-tube_1-128.png\",\r\n    \"recommended\" : false,\r\n    \"activities\" : [\r\n    {\r\n    \"Id\" : \"dmentia1\",\r\n    \"Creator\" : {\r\n        \"id\"    : \"thatId\",\r\n        \"name\"  : \"Justin Time\"\r\n    },\r\n    \"Title\" : \"Preparing Dinner\",\r\n    \"Icon\"  : \"http://img3.wikia.nocookie.net/__cb20110528210150/restaurantcity/images/4/46/Spaghetti_Bolognese.png\",\r\n    \"Resources\" : \"https://www.dropbox.com/s/3isleqzen5gt0hf/dinner.zip?raw=1\",\r\n    \"ActivityType\" : \"Scenario\",\r\n    \"tasks\" : [\r\n    {\r\n        \"id\" : \"sc3ev1\",\r\n        \"content\" : {\r\n            \"type\"   : \"AUDIO\",\r\n            \"visual\" : \"spagBol.jpg\",\r\n            \"audio\"  : \"spag1.mp3\",\r\n            \"text\"   : \"You've invited your best friend over for dinner and have decided to make spaghetti bolognese.\"\r\n        },\r\n        \"response\" : {\r\n            \"type\"  : \"none\",\r\n            \"prompt\" : null\r\n        }\r\n    },\r\n    {\r\n        \"id\" : \"sc3ev2\",\r\n        \"content\" : {\r\n            \"type\"   : \"AUDIO\",\r\n            \"visual\" : \"supermarket.jpg\",\r\n            \"audio\"  : \"spag2.mp3\",\r\n            \"text\"   : \"You go to the supermarket to buy some of the ingredients.\"\r\n        },\r\n        \"response\" : {\r\n            \"type\"  : \"Choice\",\r\n            \"prompt\" : \"Choose the spaghetti from the shelf.\",\r\n            \"choice1\" : \"spaghetti.jpg\",\r\n            \"choice2\"   : \"bakedBeans.png\"\r\n        }\r\n    },\r\n    {\r\n        \"id\" : \"sc3ev3\",\r\n        \"content\" : {\r\n            \"type\"   : \"AUDIO\",\r\n            \"visual\" : \"supermarket.jpg\",\r\n            \"audio\"  : \"spag3.mp3\",\r\n            \"text\"   : \"You also need something to make the sauce from...\"\r\n        },\r\n        \"response\" : {\r\n            \"type\"  : \"Choice\",\r\n            \"prompt\" : \"Which of these could you make a pasta sauce from?\",\r\n            \"choice1\" : \"dogFood.jpg\",\r\n            \"choice2\"   : \"tomatoes.jpg\"\r\n        }\r\n    },\r\n    {\r\n        \"id\" : \"sc3ev4\",\r\n        \"content\" : {\r\n            \"type\"   : \"AUDIO\",\r\n            \"visual\" : \"cashier.jpg\",\r\n            \"audio\"  : \"spag4.mp3\",\r\n            \"text\"   : \"As you pay for your items, the cashier asks about your bolognese recipe.\"\r\n        },\r\n        \"response\" : {\r\n            \"type\"  : \"Freeform\",\r\n            \"prompt\" : \"Describe the process of cooking spaghetti bolognese to the cashier.\"\r\n        }\r\n    },\r\n    {\r\n        \"id\" : \"sc3ev5\",\r\n        \"content\" : {\r\n            \"type\"   : \"AUDIO\",\r\n            \"visual\" : \"cashier.jpg\",\r\n            \"audio\"  : \"spag5.mp3\",\r\n            \"text\"   : \"Oh, that sounds delicious! Are you having anyone over?\"\r\n        },\r\n        \"response\" : {\r\n            \"type\"  : \"Freeform\",\r\n            \"prompt\" : \"Describe your best friend to the cashier.\"\r\n        }\r\n    },\r\n    ]\r\n}]\r\n}\r\n]";
                session.categories = JsonConvert.DeserializeObject<List<ActivityCategory>>(json, new ActivityConverter());

                // Loop over all categories, downloading icons as needed for them and their scenarios
                for (int i = 0; i < session.categories.Count; i++)
                {
                    session.categories[i].DownloadIcon();

                    for (int j = 0; j < session.categories[i].activities.Length; j++)
                    {
                        session.ProcessScenario(i, j, true);
                    }
                }

                // More efficient to await them all collectively than one at a time
                int timeout = 10000;
                int waited = 0;
                int interval = 100;

                while (waited < timeout)
                {
                    if (SessionData.scenariosProcessing == 0 && ActivityCategory.runningDLs == 0) return;
                    waited += interval;
                    await Task.Delay(interval);
                }
            }
            catch(Exception except)
            {
                Console.WriteLine(except);
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
        public static async Task PushResult(ResultItem toUpload, Action<bool> callback = null)
        {
            bool success = false;

            try
            {
                toUpload.uploadState = ResultItem.UploadState.Uploading;

                string millis = DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds.ToString();
                string filename = millis + "_" + toUpload.activityId + ".zip";

                FileStream fs = File.OpenRead(toUpload.dataLoc);
                byte[] content = new byte[fs.Length];
                fs.Read(content, 0, content.Length);
                fs.Close();

                HttpWebRequest putReq = (HttpWebRequest)WebRequest.Create(server + remoteUploads + filename);
                putReq.Credentials = new NetworkCredential(username, password);
                putReq.PreAuthenticate = true;
                putReq.Method = @"PUT";
                putReq.Headers.Add(@"Overwrite", @"T");
                putReq.ContentLength = content.Length;
                putReq.SendChunked = true;

                using(Stream reqStream = putReq.GetRequestStream())
                {
                    await reqStream.WriteAsync(content, 0, content.Length);
                    reqStream.Close();

                    try
                    {
                        HttpWebResponse putResp = (HttpWebResponse)putReq.GetResponse();
                        success = true;
                    }
                    catch (System.Net.WebException ex)
                    {
                        // Might need to make the folder first...
                        HttpWebRequest httpMkColRequest = (HttpWebRequest)WebRequest.Create(server + remoteUploads);
                        httpMkColRequest.Credentials = new NetworkCredential(username, password);
                        httpMkColRequest.PreAuthenticate = true;
                        httpMkColRequest.Method = @"MKCOL";
                        HttpWebResponse httpMkColResponse = (HttpWebResponse)httpMkColRequest.GetResponse();

                        // Try again!
                        reqStream.Write(content, 0, content.Length);
                        reqStream.Close();
                        HttpWebResponse putResp = (HttpWebResponse)putReq.GetResponse();
                        success = true;
                    }
                }
            }
            catch(Exception except)
            {
                Console.WriteLine("Oh dear! " + except);
            }

            if(success)
            {
                session.resultsToUpload.Remove(toUpload);
                toUpload.uploadState = ResultItem.UploadState.Uploaded;
            }

            if (callback != null) callback(success);
        }

        /// <summary>
        /// Uploads all items in the queue
        /// </summary>
        /// <param name="callback">The function to get called on an item finishing. Is called as true when all items have been processed.</param>
        /// <returns></returns>
        public static async Task PushAllResults(Action<bool> callback )
        {
            HttpWebRequest httpMkColRequest = (HttpWebRequest)WebRequest.Create(server + remoteUploads);
            httpMkColRequest.Credentials = new NetworkCredential(username, password);
            httpMkColRequest.PreAuthenticate = true;
            httpMkColRequest.Method = @"MKCOL";
            try
            {
                HttpWebResponse httpMkColResponse = (System.Net.HttpWebResponse)await httpMkColRequest.GetResponseAsync();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            DateTime old = DateTime.MinValue.AddYears(1969);
            ResultItem[] toUpload = session.resultsToUpload.ToArray();
            int completed = 0;

            Action<bool> OnUpload = (bool success) =>
                {
                    completed++;
                    callback(completed >= toUpload.Length);
                };

            for(int i = 0; i < toUpload.Length; i++)
            {
                await PushResult(toUpload[i], OnUpload);
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
                if (session.resultsToUpload[i].activityId == id) return true;
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
            //ret.activity = ISpeechingActivityItem.GetWithId(session.categories, ret.resultItem.activityId);

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
                fb.rating = rand.Next(1, 5);
            }

            return arr;
        }
    }

    public class SessionData
    {
        public User currentUser;
        public List<ActivityCategory> categories;
        public List<ResultItem> resultsToUpload;
        public List<User> userCache;
        public bool serverFolderExists = false;

        public static int scenariosProcessing = 0;

        // TEMP - will be pulled from the server eventually but store here for now TODO
        public List<ResultItem> resultsOnServer; 

        public SessionData()
        {
            currentUser = new User();
            categories = new List<ActivityCategory>();
            resultsToUpload = new List<ResultItem>();
            userCache = new List<User>();
            resultsOnServer = new List<ResultItem>();
        }

        /// <summary>
        /// Attempt to find an activity with the given id
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public ISpeechingActivityItem GetActivityWithId(string activityId)
        {
            for(int i = 0; i < categories.Count; i++)
            {
                for(int j = 0; i < categories[i].activities.Length; j++)
                {
                    if (categories[i].activities[j].Id == activityId) return categories[i].activities[j];
                }
            }

            // TODO check server
            return null;
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
        /// Find all results for the given scenario and remove them from the upload queue
        /// </summary>
        /// <param name="scenarioId"></param>
        public void DeleteAllPendingForScenario(string scenarioId)
        {
            List<ResultItem> toDelete = new List<ResultItem>();

            foreach(ResultItem item in resultsToUpload)
            {
                if (item.activityId == scenarioId) toDelete.Add(item);
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
        public async Task ProcessScenario(int catIndex, int scenIndex, bool shouldSave = true)
        {
            try
            {
                scenariosProcessing++;

                ISpeechingActivityItem activity = categories[catIndex].activities[scenIndex];
                if(activity.Id == null) activity.Id = "act_" + AppData.rand.Next().ToString();

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
                categories[catIndex].activities[scenIndex] = activity;

                if (shouldSave) AppData.SaveCurrentData();
                scenariosProcessing--;
            }
            catch(Exception e)
            {
                Console.WriteLine("Error adding new activity item: " + e);
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
            id = "placeholder";
            friends = new List<string>();
        }
    }

    public class ResultItem
    {
        public enum UploadState { Incomplete, Ready, Uploading, Uploaded };
        public string id;
        public string userId;
        public string activityId;
        public string dataLoc;
        public Dictionary<string, string> results;
        public UploadState uploadState;
        public bool isPublic;
        public List<string> allowedUsers;
        public DateTime completedAt;

        public ResultItem(string activityId, string dataLoc, string userId)
        {
            this.completedAt = DateTime.Now;
            this.id = activityId + "_" + completedAt.ToString();
            this.activityId = activityId;
            this.userId = userId;
            this.dataLoc = dataLoc;
            this.uploadState = UploadState.Ready;
            this.isPublic = false;
            this.allowedUsers = new List<string>();
            this.results = new Dictionary<string, string>();
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
        public ISpeechingActivityItem activity;
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
        public int rating;
        public string comments;
    }

    public class TaskContent
    {
        public enum ContentType { Audio, Video, Text };
        public ContentType type;
        public string visual;
        public string audio;
        public string text;
    }

    public class TaskResponse
    {
        public enum ResponseType { None, Prompted, Freeform, Choice };
        public ResponseType type;
        public string prompt;
        public string choice1;
        public string choice2;
    }

    public class SpeechingTask
    {
        public string id;
        public TaskContent content;
        public TaskResponse response;
    }

    public class ActivityCategory
    {
        public string id;
        public string title;
        public string icon;
        public bool recommended;
        public ISpeechingActivityItem[] activities;

        public static int runningDLs = 0;

        public async Task DownloadIcon()
        {
            runningDLs++;
            string localIconPath = AppData.cacheDir + "/" + Path.GetFileName(icon);

            try
            {
                // Download the icon if it isn't already stored locally
                if (!File.Exists(localIconPath))
                {
                    WebClient request = new WebClient();
                    await request.DownloadFileTaskAsync(
                        new Uri(icon),
                        localIconPath
                        );
                    request.Dispose();
                    request = null;
                }
            }
            catch(Exception e)
            {
                // We might be downloading into the same file simultaneously
                // Not actually an issue, as long as the icon path still gets reassigned to the local one below
            }

            icon = localIconPath;
            runningDLs--;
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
