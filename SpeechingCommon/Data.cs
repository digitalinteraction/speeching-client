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

        public static string server = @"https://di.ncl.ac.uk/owncloud/remote.php/webdav/";
        public static string remoteUploads;
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
            string json = "[\r\n{\r\n    \"id\" : \"anId\",\r\n    \"title\" : \"Dysfluency\",\r\n    \"icon\"  : \"https://cdn0.iconfinder.com/data/icons/cosmo-medicine/40/test-tube_1-128.png\",\r\n    \"recommended\" : false,\r\n    \"scenarios\" : [\r\n    {\r\n        \"id\" : \"testScenario\",\r\n        \"creator\" : {\r\n            \"id\"    : \"thatId\",\r\n            \"name\"  : \"Justin Time\"\r\n        },\r\n        \"title\" : \"Getting the Bus\",\r\n        \"icon\"  : \"http://www.survivingamsterdam.com/public/files/e96fc9baf228c0cb8d210a1768995bb1.png\",\r\n        \"resources\" : \"https://www.dropbox.com/s/0h2f8pyrh6xte3s/bus.zip?raw=1\",\r\n        \"tasks\" : [\r\n        {\r\n            \"id\" : \"sc1ev1\",\r\n            \"content\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"driver.jpg\",\r\n                \"audio\"  : \"hello.mp3\",\r\n                \"text\"   : \"Hello! Where would you like to go today?\"\r\n            },\r\n            \"response\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Hello, please may I have a return ticket to the train station?\"\r\n            }\r\n        },\r\n        {\r\n            \"id\" : \"sc1ev2\",\r\n            \"content\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"driver.jpg\",\r\n                \"audio\"  : \"thanks.mp3\",\r\n                \"text\"   : \"No problem at all, looks like you have a valid card. Take a seat!\"\r\n            },\r\n            \"response\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Thank you. Have a good day.\"\r\n            }\r\n        },\r\n        {\r\n            \"id\" : \"sc1ev3\",\r\n            \"content\" : {\r\n                \"type\"   : \"Text\",\r\n                \"visual\" : \"oldwoman.jpg\",\r\n                \"audio\"  : null,\r\n                \"text\"   : \"You sit next to an old woman, who asks what your plans are for the day. Greet her and explain how you're catching a train to the seaside.\"\r\n            },\r\n            \"response\" : {\r\n                \"type\"  : \"Freeform\",\r\n                \"prompt\" : null\r\n            }\r\n        }\r\n        ]\r\n    },\r\n    {\r\n        \"id\" : \"testScenario2\",\r\n        \"creator\" : {\r\n            \"id\"    : \"thatId\",\r\n            \"name\"  : \"Justin Time\"\r\n        },\r\n        \"title\" : \"Ordering a Pizza\",\r\n        \"icon\"  : \"http://img3.wikia.nocookie.net/__cb20131231163822/cardfight/images/6/6f/Pizza_slice_combo_clipart.png\",\r\n        \"resources\" : \"https://www.dropbox.com/s/8gt7pqh6zq6p18h/pizza.zip?raw=1\",\r\n        \"tasks\" : [\r\n        {\r\n            \"id\" : \"sc2ev1\",\r\n            \"content\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"phone.jpg\",\r\n                \"audio\"  : \"hello.mp3\",\r\n                \"text\"   : \"You are ordering pizza over the phone for both yourself and a friend who has a gluten alergy.\\n'Tony's Pizza Parlour, how can I help you?'\"\r\n            },\r\n            \"response\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Hello, can I order a pizza please?\"\r\n            }\r\n        },\r\n        {\r\n            \"id\" : \"sc2ev2\",\r\n            \"content\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"pizza.jpg\",\r\n                \"audio\"  : \"order1.mp3\",\r\n                \"text\"   : \"Of course! What kind would you like?\"\r\n            },\r\n            \"response\" : {\r\n                \"type\"  : \"Freeform\",\r\n                \"prompt\" : \"Describe your favourite pizza\"\r\n            }\r\n        },\r\n        {\r\n            \"id\" : \"sc2ev3\",\r\n            \"content\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"pizza.jpg\",\r\n                \"audio\"  : \"order2.mp3\",\r\n                \"text\"   : \"That sounds delicious! Would you like anything else?\"\r\n            },\r\n            \"response\" : {\r\n                \"type\"  : \"Freeform\",\r\n                \"prompt\" : \"Describe another kind of pizza, but make sure it's gluten free!\"\r\n            }\r\n        },\r\n        {\r\n            \"id\" : \"sc2ev4\",\r\n            \"content\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"making.jpg\",\r\n                \"audio\"  : \"end.mp3\",\r\n                \"text\"   : \"No problem at all, we can do that. See you soon!\"\r\n            },\r\n            \"response\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Thank you, see you later.\"\r\n            }\r\n        },\r\n        ]\r\n    }\r\n    ]\r\n},\r\n{\r\n\"id\" : \"anId2\",\r\n    \"title\" : \"Dementia\",\r\n    \"icon\"  : \"https://cdn0.iconfinder.com/data/icons/cosmo-medicine/40/test-tube_1-128.png\",\r\n    \"recommended\" : false,\r\n    \"scenarios\" : [\r\n    {\r\n    \"id\" : \"dmentia1\",\r\n    \"creator\" : {\r\n        \"id\"    : \"thatId\",\r\n        \"name\"  : \"Justin Time\"\r\n    },\r\n    \"title\" : \"Preparing Dinner\",\r\n    \"icon\"  : \"http://img3.wikia.nocookie.net/__cb20110528210150/restaurantcity/images/4/46/Spaghetti_Bolognese.png\",\r\n    \"resources\" : \"https://www.dropbox.com/s/3isleqzen5gt0hf/dinner.zip?raw=1\",\r\n    \"tasks\" : [\r\n    {\r\n        \"id\" : \"sc3ev1\",\r\n        \"content\" : {\r\n            \"type\"   : \"AUDIO\",\r\n            \"visual\" : \"spagBol.jpg\",\r\n            \"audio\"  : \"spag1.mp3\",\r\n            \"text\"   : \"You've invited your best friend over for dinner and have decided to make spaghetti bolognese.\"\r\n        },\r\n        \"response\" : {\r\n            \"type\"  : \"none\",\r\n            \"prompt\" : null\r\n        }\r\n    },\r\n    {\r\n        \"id\" : \"sc3ev2\",\r\n        \"content\" : {\r\n            \"type\"   : \"AUDIO\",\r\n            \"visual\" : \"supermarket.jpg\",\r\n            \"audio\"  : \"spag2.mp3\",\r\n            \"text\"   : \"You go to the supermarket to buy some of the ingredients.\"\r\n        },\r\n        \"response\" : {\r\n            \"type\"  : \"Choice\",\r\n            \"prompt\" : \"Choose the spaghetti from the shelf.\",\r\n            \"choice1\" : \"spaghetti.jpg\",\r\n            \"choice2\"   : \"bakedBeans.png\"\r\n        }\r\n    },\r\n    {\r\n        \"id\" : \"sc3ev3\",\r\n        \"content\" : {\r\n            \"type\"   : \"AUDIO\",\r\n            \"visual\" : \"supermarket.jpg\",\r\n            \"audio\"  : \"spag3.mp3\",\r\n            \"text\"   : \"You also need something to make the sauce from...\"\r\n        },\r\n        \"response\" : {\r\n            \"type\"  : \"Choice\",\r\n            \"prompt\" : \"Which of these could you make a pasta sauce from?\",\r\n            \"choice1\" : \"dogFood.jpg\",\r\n            \"choice2\"   : \"tomatoes.jpg\"\r\n        }\r\n    },\r\n    {\r\n        \"id\" : \"sc3ev4\",\r\n        \"content\" : {\r\n            \"type\"   : \"AUDIO\",\r\n            \"visual\" : \"cashier.jpg\",\r\n            \"audio\"  : \"spag4.mp3\",\r\n            \"text\"   : \"As you pay for your items, the cashier asks about your bolognese recipe.\"\r\n        },\r\n        \"response\" : {\r\n            \"type\"  : \"Freeform\",\r\n            \"prompt\" : \"Describe the process of cooking spaghetti bolognese to the cashier.\"\r\n        }\r\n    },\r\n    {\r\n        \"id\" : \"sc3ev5\",\r\n        \"content\" : {\r\n            \"type\"   : \"AUDIO\",\r\n            \"visual\" : \"cashier.jpg\",\r\n            \"audio\"  : \"spag5.mp3\",\r\n            \"text\"   : \"Oh, that sounds delicious! Are you having anyone over?\"\r\n        },\r\n        \"response\" : {\r\n            \"type\"  : \"Freeform\",\r\n            \"prompt\" : \"Describe your best friend to the cashier.\"\r\n        }\r\n    },\r\n    ]\r\n}]\r\n}\r\n]";
            session.categories = JsonConvert.DeserializeObject<List<ScenarioCategory>>(json);

            // Loop over all categories, downloading icons as needed for them and their scenarios
            for(int i = 0; i < session.categories.Count; i++)
            {
                session.categories[i].DownloadIcon();
                
                for(int j = 0; j < session.categories[i].scenarios.Length; j++)
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
                if (SessionData.scenariosProcessing == 0 && ScenarioCategory.runningDLs == 0) return;
                waited += interval;
                await Task.Delay(interval);
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
                string millis = DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds.ToString();
                string filename = millis + "_" + toUpload.scenarioId + ".zip";

                FileStream fs = File.OpenRead(toUpload.dataLoc);
                byte[] content = new byte[fs.Length];
                fs.Read(content, 0, content.Length);
                fs.Close();

                string username = @"speeching";
                string password = @"BlahBlah123";

                HttpWebRequest putReq = (HttpWebRequest)WebRequest.Create(server + remoteUploads + filename);
                putReq.Credentials = new NetworkCredential(username, password);
                putReq.PreAuthenticate = true;
                putReq.Method = @"PUT";
                putReq.Headers.Add(@"Overwrite", @"T");
                putReq.ContentLength = content.Length;
                putReq.SendChunked = true;

                Stream reqStream = putReq.GetRequestStream();
                await reqStream.WriteAsync(content, 0, content.Length);
                reqStream.Close();

                try
                {
                    HttpWebResponse putResp = (HttpWebResponse)putReq.GetResponse();
                    success = true;
                }
                catch(System.Net.WebException ex)
                {
                    // Might need to make the folder first...
                    HttpWebRequest httpMkColRequest = (HttpWebRequest)WebRequest.Create(server + remoteUploads);
                    httpMkColRequest.Credentials = new NetworkCredential(username, password);
                    httpMkColRequest.PreAuthenticate = true;
                    httpMkColRequest.Method = @"MKCOL";
                    HttpWebResponse httpMkColResponse = (HttpWebResponse)httpMkColRequest.GetResponse();

                    // Try again!
                    reqStream = putReq.GetRequestStream();
                    reqStream.Write(content, 0, content.Length);
                    reqStream.Close();
                    HttpWebResponse putResp = (HttpWebResponse)putReq.GetResponse();
                    success = true;
                }

            }
            catch(Exception except)
            {
                Console.WriteLine("Oh dear! " + except);
            }

            if(success)
            {
                session.resultsToUpload.Remove(toUpload);
                SaveCurrentData();
            }

            if (callback != null) callback(success);
        }

        /// <summary>
        /// Complete all pending uploads
        /// </summary>
        public static void PushAllResults()
        {
            for(int i = 0; i < session.resultsToUpload.Count; i++)
            {
                //PushResult(session.resultsToUpload[i]); TODO
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
            ret.scenario = Scenario.GetWithId(session.categories, ret.resultItem.scenarioId);

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
        public List<ScenarioCategory> categories;
        public List<ResultItem> resultsToUpload;
        public List<User> userCache;
        public bool serverFolderExists = false;

        public static int scenariosProcessing = 0;

        // TEMP - will be pulled from the server eventually but store here for now TODO
        public List<ResultItem> resultsOnServer; 

        public SessionData()
        {
            currentUser = new User();
            categories = new List<ScenarioCategory>();
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
        /// Find all results for the given scenario and remove them from the upload queue
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
        public async Task ProcessScenario(int catIndex, int scenIndex, bool shouldSave = true)
        {
            try
            {
                scenariosProcessing++;

                Scenario scenario = categories[catIndex].scenarios[scenIndex];
                if(scenario.id == null) scenario.id = "scenario_" + AppData.rand.Next().ToString();

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
                categories[catIndex].scenarios[scenIndex] = scenario;

                if (shouldSave) AppData.SaveCurrentData();
                scenariosProcessing--;
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
            id = "placeholder";
            friends = new List<string>();
        }
    }

    public class ResultItem
    {
        public string id;
        public string userId;
        public string scenarioId;
        public string dataLoc;
        public Dictionary<string, string> results;
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

    public class Scenario
    {
        public string id;
        public User creator;
        public string title;
        public string resources;
        public string icon;
        public SpeechingTask[] tasks;

        public static Scenario GetWithId(List<ScenarioCategory> coll, string id)
        {
            foreach(ScenarioCategory cat in coll)
            {
                foreach(Scenario scen in cat.scenarios)
                {
                    if (scen.id == id) return scen;
                }
            }

            return null;
        }
    }

    public class ScenarioCategory
    {
        public string id;
        public string title;
        public string icon;
        public bool recommended;
        public Scenario[] scenarios;

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
