using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using RestSharp.Portable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

        public static string storageServer = @"https://di.ncl.ac.uk/owncloud/remote.php/webdav/";
        public static string storageRemoteDir;
        public static string storageUsername = @"speeching";
        public static string storagePassword = @"BlahBlah123";
        public static Random rand;

        public static string serviceUrl = " http://api.opescode.com/api/";


        /// <summary>
        /// Send a POST request to the server and return an Object of type T in response
        /// </summary>
        /// <typeparam name="T">The object type to deserialize from JSON</typeparam>
        /// <param name="route">The route on the server to query</param>
        /// <param name="jsonData">Serialized data to send to the server in the request</param>
        /// <returns>The response's JSON in type T</returns>
        public static async Task<T> PostRequest<T>(string route, string jsonData = null)
        {
           var request = HttpWebRequest.Create(serviceUrl + route);
            request.ContentType = "application/json";
            request.Method = "POST";

            try
            {
                if (!string.IsNullOrWhiteSpace(jsonData))
                {
                    using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                    {
                        writer.Write(jsonData);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            using(HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            {
                if(response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine("Error fetching data!");
                    return default(T);
                }
                using(StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string content = await reader.ReadToEndAsync();
                    response.Close();
                    if(string.IsNullOrWhiteSpace(content))
                    {
                        Console.WriteLine("Error fetching data! Returned empty data");
                        return default(T);
                    }
                    else
                    {
                        Console.Out.WriteLine("Response Body: \r\n {0}", content);

                        try
                        {
                            return JsonConvert.DeserializeObject<T>(content);
                        }
                        catch(Exception except)
                        {
                            Console.WriteLine(except);
                            return default(T);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends a get request with the given argument after the url
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="route"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<T> GetRequest<T>(string route, string data = "")
        {
            string url = serviceUrl + route;

            if (!string.IsNullOrEmpty(data))
            {
                url += "/" + data;
            }

            string result = await SendGetRequest(url);

            if (result == null) return default(T);

            return JsonConvert.DeserializeObject<T>(result);
        }

        /// <summary>
        /// Send a GET request to the server and return an Object of type T in response
        /// </summary>
        /// <typeparam name="T">The expected object type to return from JSON</typeparam>
        /// <param name="route">The route on the server to query</param>
        /// <param name="data">Keys + values to include in the sent URL</param>
        /// <returns>The response's JSON as a type T</returns>
        public static async Task<T> GetRequest<T>(string route, Dictionary<string, string> data)
        {
            string url = serviceUrl + route;

            if(data != null && data.Count > 0)
            {
                url += "?";
                int i = 0;
                foreach(KeyValuePair<string, string> entry in data)
                {
                    url += entry.Key + "=" + entry.Value;
                    if(++i != data.Count) url += "&";
                }
            }

            string result = await SendGetRequest(url);

            if (result == null) return default(T);

            return JsonConvert.DeserializeObject<T>(result);
        }

        /// <summary>
        /// Sends a get request to the given URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task<string> SendGetRequest(string url)
        {
            var request = HttpWebRequest.Create(url);
            request.ContentType = "application/json";
            request.Method = "GET";

            using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine("Error fetching data!");
                    return null;
                }
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string content = await reader.ReadToEndAsync();
                    response.Close();
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        Console.WriteLine("Error fetching data! Returned empty data");
                        return null;
                    }
                    else
                    {
                        Console.Out.WriteLine("Response Body: \r\n {0}", content);

                        return content;
                    }
                }
            }
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
                if(!Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                }
                else if (File.Exists(cacheDir + "/offline.json"))
                {
                    var binder = new TypeNameSerializationBinder("SpeechingCommon.{0}, SpeechingCommon");
                    session = JsonConvert.DeserializeObject<SessionData>(File.ReadAllText(cacheDir + "/offline.json"), new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        Binder = binder
                    });
                    storageRemoteDir = "uploads/" + session.currentUser.id + "/";
                    return true;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Error loading data: " + e);
            }

            session = new SessionData();
            storageRemoteDir = "uploads/" + session.currentUser.id + "/";

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
                session.categories = await AppData.GetRequest<List<ActivityCategory>>("category", "");

                // Loop over all categories, downloading icons as needed for them and their scenarios
                for (int i = 0; i < session.categories.Count; i++)
                {
                    session.categories[i].DownloadIcon();

                    /*for (int j = 0; j < session.categories[i].activities.Length; j++)
                    {
                        session.ProcessScenario(i, j, true);
                    }*/
                }

                // More efficient to await them all collectively than one at a time
                int timeout = 10000;
                int waited = 0;
                int interval = 100;

                while (waited < timeout)
                {
                    if (SessionData.scenariosProcessing == 0 && ActivityCategory.runningDLs == 0)
                    {
                        AppData.SaveCurrentData();
                        return;
                    }
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
        /// Uploads a single result package to the storage server
        /// </summary>
        /// <param name="toUpload">The item to upload</param>
        public static async Task PushResult(ResultItem toUpload, Action<bool> callback = null)
        {
            bool success = false;
            string millis = DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds.ToString();
            string filename = millis + "_" + toUpload.activityId + ".zip";

            try
            {
                toUpload.uploadState = ResultItem.UploadState.Uploading;

                FileStream fs = File.OpenRead(toUpload.dataLoc);
                byte[] content = new byte[fs.Length];
                fs.Read(content, 0, content.Length);
                fs.Close();

                HttpWebRequest putReq = (HttpWebRequest)WebRequest.Create(storageServer + storageRemoteDir + filename);
                putReq.Credentials = new NetworkCredential(storageUsername, storagePassword);
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
                        HttpWebRequest httpMkColRequest = (HttpWebRequest)WebRequest.Create(storageServer + storageRemoteDir);
                        httpMkColRequest.Credentials = new NetworkCredential(storageUsername, storagePassword);
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
                toUpload.dataLoc = storageServer + storageRemoteDir + filename;
                success = await PushResultToDatabase(toUpload);
                if(success)
                {
                    session.resultsToUpload.Remove(toUpload);
                    toUpload.uploadState = ResultItem.UploadState.Uploaded;
                }
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
            HttpWebRequest httpMkColRequest = (HttpWebRequest)WebRequest.Create(storageServer + storageRemoteDir);
            httpMkColRequest.Credentials = new NetworkCredential(storageUsername, storagePassword);
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

        private static async Task<bool> PushResultToDatabase(ResultItem toUpload)
        {
            bool success = false;

            AppData.PostRequest<ServerError>("SubmitResult", JsonConvert.SerializeObject(toUpload));

            return success;
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
        /// TODO Prepares all of the submission's data, including audio recordings
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
        /// Get a list of submissions from the server
        /// </summary>
        /// <returns></returns>
        public static async Task<ResultItem[]> FetchSubmittedList()
        {
            return await GetRequest<ResultItem[]>("GetSubmissions", "");
        }

        /// <summary>
        /// Polls the server for all available feedback for the given activity
        /// </summary>
        /// <param name="resultId"></param>
        /// <returns></returns>
        public static async Task<IFeedbackItem[]> FetchFeedbackFor(string resultId)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("submissionId", resultId);

            //return await GetRequest<IFeedbackItem[]>("GetFeedback", data);

            // TEMP
            IFeedbackItem[] arr = new IFeedbackItem[12];

            for(int i = 0; i < arr.Length; i++)
            {
                if(AppData.rand.Next(0, 100) >= 50)
                {
                    PercentageFeedback fb = new PercentageFeedback();
                    fb.Title = "Stammering";
                    fb.Percentage = AppData.rand.Next(0, 100);
                    fb.Caption = (int)fb.Percentage + "% of users thought you stammered over the word \"sausage\"";
                    fb.ActivityId = "sossie";
                    arr[i] = fb;
                }
                else
                {
                    StarRatingFeedback st = new StarRatingFeedback();
                    st.Title = "Your Rating";
                    st.Caption = "This is your rating for something you did. Hopefully it's meaningful!";
                    st.ActivityId = "sossie";
                    st.Rating = (float)AppData.rand.Next(0, 10) /2;
                    arr[i] = st;
                }
            }

            return arr;
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

        /// <summary>
        /// Returns a local version of the linked file, downloading it if necessary
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <returns></returns>
        public static async Task<string> FetchLocalCopy(string remoteUrl)
        {
            string localIconPath = AppData.cacheDir + "/" + Path.GetFileName(remoteUrl);

            try
            {
                // Download the file if it isn't already stored locally
                if (!File.Exists(localIconPath))
                {
                    WebClient request = new WebClient();
                    await request.DownloadFileTaskAsync(
                        new Uri(remoteUrl),
                        localIconPath
                        );
                    request.Dispose();
                    request = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return localIconPath;
        }
    }
}
