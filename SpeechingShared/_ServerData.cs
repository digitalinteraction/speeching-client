using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using PCLStorage;

namespace SpeechingShared
{
    /// <summary>
    /// A collection of server structures, variables and methods
    /// Methods starting with "Push" request changes be made to server data. Methods starting with "Fetch" pull data from the server.
    /// </summary>
    public static class ServerData
    {
        public enum ActivityType
        {
            Scenario,
            Guide
        };

        public static string StorageServer = @"https://openlab.ncl.ac.uk/owncloud/remote.php/webdav/";
        public static string StorageRemoteDir;
        public static string ServiceUrl = "https://speeching.azurewebsites.net/api/";

        /// <summary>
        /// Send a POST request to the server and return an Object of type T in response
        /// </summary>
        /// <typeparam name="T">The object type to deserialize from JSON</typeparam>
        /// <param name="route">The route on the server to query</param>
        /// <param name="jsonData">Serialized data to send to the server in the request</param>
        /// <returns>The response's JSON in type T</returns>
        public static async Task<T> PostRequest<T>(string route, string jsonData = null)
        {
            if (!AppData.CheckNetwork()) return default(T);

            using (HttpClient client = new HttpClient())
            {
                Uri baseAddress = new Uri(ServiceUrl);
                client.BaseAddress = baseAddress;

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, ServiceUrl + route);

                if (AppData.Session != null && AppData.Session.CurrentUser != null)
                {
                    requestMessage.Headers.Add("Email", AppData.Session.CurrentUser.Email);
                    requestMessage.Headers.Add("Key", AppData.Session.CurrentUser.Key.ToString());
                }

                HttpContent content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
                requestMessage.Content = content;

                HttpResponseMessage response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    string toReturn = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(toReturn);
                }
                AppData.Io.PrintToConsole(await response.Content.ReadAsStringAsync());
                return default(T);
            }
        }

        /// <summary>
        /// Sends a get request with the given argument after the url
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="route"></param>
        /// <param name="data"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static async Task<T> GetRequest<T>(string route, string data = "", JsonConverter converter = null)
        {
            if (!AppData.CheckNetwork()) return default(T);

            string url = route;

            if (!string.IsNullOrEmpty(data))
            {
                url += "/" + data;
            }

            //string result = "[\r\n{\r\n    \"Id\" : 100011,\r\n    \"Title\" : \"Dysfluency\",\r\n    \"Icon\"  : \"https://cdn0.iconfinder.com/data/icons/cosmo-medicine/40/test-tube_1-128.png\",\r\n    \"Recommended\" : false,\r\n    \"Activities\" : [\r\n    {\r\n        \"Id\" : 1,\r\n        \"Creator\" : {\r\n            \"id\"    : 195606,\r\n            \"name\"  : \"Justin Time\"\r\n        },\r\n        \"Title\" : \"Getting the Bus\",\r\n        \"Icon\"  : \"http://www.survivingamsterdam.com/public/files/e96fc9baf228c0cb8d210a1768995bb1.png\",\r\n        \"Resource\" : \"https://www.dropbox.com/s/0h2f8pyrh6xte3s/bus.zip?raw=1\",\r\n        \"AssessmentTasks\" : [\r\n        {\r\n            \"Id\" : 1231231,\r\n            \"TaskContentModel\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"driver.jpg\",\r\n                \"audio\"  : \"hello.mp3\",\r\n                \"text\"   : \"Hello! Where would you like to go today?\"\r\n            },\r\n            \"TaskResponseModel\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Hello, please may I have a return ticket to the train station?\"\r\n            }\r\n        },\r\n        {\r\n            \"Id\" : 12414235,\r\n            \"TaskContentModel\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"driver.jpg\",\r\n                \"audio\"  : \"thanks.mp3\",\r\n                \"text\"   : \"No problem at all, looks like you have a valid card. Take a seat!\"\r\n            },\r\n            \"TaskResponseModel\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Thank you. Have a good day.\"\r\n            }\r\n        },\r\n        {\r\n            \"Id\" : 12536,\r\n            \"TaskContentModel\" : {\r\n                \"type\"   : \"Text\",\r\n                \"visual\" : \"oldwoman.jpg\",\r\n                \"audio\"  : null,\r\n                \"text\"   : \"You sit next to an old woman, who asks what your plans are for the day.\"\r\n            },\r\n            \"TaskResponseModel\" : {\r\n                \"type\"  : \"Freeform\",\r\n                \"prompt\" : \"Greet her and explain that you're catching a train to the seaside.\"\r\n            }\r\n        }\r\n        ]\r\n    },\r\n    {\r\n        \"Id\" : 2,\r\n        \"Creator\" : {\r\n            \"id\"    : 195606,\r\n            \"name\"  : \"Justin Time\"\r\n        },\r\n        \"Title\" : \"Ordering a Pizza\",\r\n        \"Icon\"  : \"http://img3.wikia.nocookie.net/__cb20131231163822/cardfight/images/6/6f/Pizza_slice_combo_clipart.png\",\r\n        \"Resource\" : \"https://www.dropbox.com/s/8gt7pqh6zq6p18h/pizza.zip?raw=1\",\r\n        \"AssessmentTasks\" : [\r\n        {\r\n            \"Id\" : 195864,\r\n            \"TaskContentModel\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"phone.jpg\",\r\n                \"audio\"  : \"hello.mp3\",\r\n                \"text\"   : \"You are ordering pizza over the phone for both yourself and a friend who has a gluten alergy.\\n'Tony's Pizza Parlour, how can I help you?'\"\r\n            },\r\n            \"TaskResponseModel\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Hello, can I order a pizza please?\"\r\n            }\r\n        },\r\n        {\r\n            \"Id\" : 35696,\r\n            \"TaskContentModel\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"pizza.jpg\",\r\n                \"audio\"  : \"order1.mp3\",\r\n                \"text\"   : \"Of course! What kind would you like?\"\r\n            },\r\n            \"TaskResponseModel\" : {\r\n                \"type\"  : \"Freeform\",\r\n                \"prompt\" : \"Describe your favourite pizza\"\r\n            }\r\n        },\r\n        {\r\n            \"Id\" : 979574,\r\n            \"TaskContentModel\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"pizza.jpg\",\r\n                \"audio\"  : \"order2.mp3\",\r\n                \"text\"   : \"That sounds delicious! Would you like anything else?\"\r\n            },\r\n            \"TaskResponseModel\" : {\r\n                \"type\"  : \"Freeform\",\r\n                \"prompt\" : \"Describe another kind of pizza, but make sure it's gluten free!\"\r\n            }\r\n        },\r\n        {\r\n            \"Id\" : 83241,\r\n            \"TaskContentModel\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"making.jpg\",\r\n                \"audio\"  : \"end.mp3\",\r\n                \"text\"   : \"No problem at all, we can do that. See you soon!\"\r\n            },\r\n            \"TaskResponseModel\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Thank you, see you later.\"\r\n            }\r\n        },\r\n        ]\r\n    }\r\n    ]\r\n},\r\n{\r\n    \"Id\" : 3124,\r\n    \"Title\" : \"Dementia\",\r\n    \"Icon\"  : \"https://cdn0.iconfinder.com/data/icons/cosmo-medicine/40/test-tube_1-128.png\",\r\n    \"Recommended\" : false,\r\n    \"Activities\" : [\r\n        {\r\n            \"Id\" : 3,\r\n            \"Creator\" : {\r\n                \"id\"    : 195606,\r\n                \"name\"  : \"Justin Time\"\r\n            },\r\n            \"Title\" : \"Preparing Dinner\",\r\n            \"Icon\"  : \"http://img3.wikia.nocookie.net/__cb20110528210150/restaurantcity/images/4/46/Spaghetti_Bolognese.png\",\r\n            \"Resource\" : \"https://www.dropbox.com/s/3isleqzen5gt0hf/dinner.zip?raw=1\",\r\n            \"AssessmentTasks\" : [\r\n            {\r\n                \"Id\" : 587908,\r\n                \"TaskContentModel\" : {\r\n                    \"type\"   : \"AUDIO\",\r\n                    \"visual\" : \"spagBol.jpg\",\r\n                    \"audio\"  : \"spag1.mp3\",\r\n                    \"text\"   : \"You've invited your best friend over for dinner and have decided to make spaghetti bolognese.\"\r\n                },\r\n                \"TaskResponseModel\" : {\r\n                    \"type\"  : \"none\",\r\n                    \"prompt\" : null\r\n                }\r\n            },\r\n            {\r\n                \"Id\" : 679806,\r\n                \"TaskContentModel\" : {\r\n                    \"type\"   : \"AUDIO\",\r\n                    \"visual\" : \"supermarket.jpg\",\r\n                    \"audio\"  : \"spag2.mp3\",\r\n                    \"text\"   : \"You go to the supermarket to buy some of the ingredients.\"\r\n                },\r\n                \"TaskResponseModel\" : {\r\n                    \"type\"  : \"Choice\",\r\n                    \"prompt\" : \"Choose the spaghetti from the shelf.\",\r\n                    \"choice1\" : \"spaghetti.jpg\",\r\n                    \"choice2\"   : \"bakedBeans.png\"\r\n                }\r\n            },\r\n            {\r\n                \"Id\" : 416597,\r\n                \"TaskContentModel\" : {\r\n                    \"type\"   : \"AUDIO\",\r\n                    \"visual\" : \"supermarket.jpg\",\r\n                    \"audio\"  : \"spag3.mp3\",\r\n                    \"text\"   : \"You also need something to make the sauce from...\"\r\n                },\r\n                \"TaskResponseModel\" : {\r\n                    \"type\"  : \"Choice\",\r\n                    \"prompt\" : \"Which of these could you make a pasta sauce from?\",\r\n                    \"choice1\" : \"dogFood.jpg\",\r\n                    \"choice2\"   : \"tomatoes.jpg\"\r\n                }\r\n            },\r\n            {\r\n                \"Id\" : 749604,\r\n                \"TaskContentModel\" : {\r\n                    \"type\"   : \"AUDIO\",\r\n                    \"visual\" : \"cashier.jpg\",\r\n                    \"audio\"  : \"spag4.mp3\",\r\n                    \"text\"   : \"As you pay for your items, the cashier asks about your bolognese recipe.\"\r\n                },\r\n                \"TaskResponseModel\" : {\r\n                    \"type\"  : \"Freeform\",\r\n                    \"prompt\" : \"Describe the process of cooking spaghetti bolognese to the cashier.\"\r\n                }\r\n            },\r\n            {\r\n                \"Id\" : 7498607,\r\n                \"TaskContentModel\" : {\r\n                    \"type\"   : \"AUDIO\",\r\n                    \"visual\" : \"cashier.jpg\",\r\n                    \"audio\"  : \"spag5.mp3\",\r\n                    \"text\"   : \"Oh, that sounds delicious! Are you having anyone over?\"\r\n                },\r\n                \"TaskResponseModel\" : {\r\n                    \"type\"  : \"Freeform\",\r\n                    \"prompt\" : \"Describe your best friend to the cashier.\"\r\n                }\r\n            }\r\n            ]\r\n        }\r\n    ]\r\n    },\r\n    {\r\n    \"Id\" : 629506,\r\n    \"Title\" : \"Helpful Guides\",\r\n    \"Icon\"  : \"https://cdn1.iconfinder.com/data/icons/MetroStation-PNG/128/MB__help.png\",\r\n    \"Recommended\" : false,\r\n    \"Activities\" : [\r\n        {\r\n            \"Id\" : 4,\r\n            \"Creator\" : {\r\n                \"id\"    : 195606,\r\n                \"name\"  : \"Justin Time\"\r\n            },\r\n            \"Title\" : \"Interaction Tips\",\r\n            \"Icon\"  : \"http://www.pursuittraining.co.uk/images/care-icon.gif\",\r\n            \"Resource\" : \"https://www.dropbox.com/s/pw1ubz20nwatxtl/guide.zip?raw=1\",\r\n            \"Pages\" : [\r\n                {\r\n                    \"visualMediaLoc\" : \"pic1.jpg\",\r\n                    \"text\"           : \"Try to think through how it might feel to struggle to communicate if you were living with dementia and think about what might help and what has helped in the past.\"\r\n                },\r\n                {\r\n                    \"visualMediaLoc\" : \"pic2.jpg\",\r\n                    \"text\"           : \"Smile where you can and offer reassuring physical contact where it is appropriate. Make sure people can see your face and that you have engaged their attention.\"\r\n                },\r\n                {\r\n                    \"visualMediaLoc\" : \"pic3.jpg\",\r\n                    \"text\"           : \"Relax as much as you can and help the person you are talking with to relax. Be prepared to be treated as someone you are not (for example being mistaken for another relative).\"\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n    }\r\n]";
            string result = await SendGetRequest(url);

            if (result == null) return default(T);

            try
            {
                return converter != null ? JsonConvert.DeserializeObject<T>(result, converter) : JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception exception)
            {

                return default(T);
            }
            
        }

        /// <summary>
        /// Send a GET request to the server and return an Object of type T in response
        /// </summary>
        /// <typeparam name="T">The expected object type to return from JSON</typeparam>
        /// <param name="route">The route on the server to query</param>
        /// <param name="data">Keys + values to include in the sent URL</param>
        /// <param name="converter"></param>
        /// <returns>The response's JSON as a type T</returns>
        public static async Task<T> GetRequest<T>(string route, Dictionary<string, string> data,
            JsonConverter converter = null)
        {
            if (!AppData.CheckNetwork()) return default(T);
            string url = route;

            if (data != null && data.Count > 0)
            {
                url += "?";
                int i = 0;
                foreach (KeyValuePair<string, string> entry in data)
                {
                    url += entry.Key + "=" + entry.Value;
                    if (++i != data.Count) url += "&";
                }
            }

            string result = await SendGetRequest(url);

            if (result == null) return default(T);

            return converter != null ? JsonConvert.DeserializeObject<T>(result, converter) : JsonConvert.DeserializeObject<T>(result);
        }

        /// <summary>
        /// Sends a get request to the given URL
        /// </summary>
        /// <returns></returns>
        private static async Task<string> SendGetRequest(string request)
        {
            if (!AppData.CheckNetwork()) return null;

            using (HttpClient client = new HttpClient()) //new NativeMessageHandler()))
            {
                Uri baseAddress = new Uri(ServiceUrl);
                client.BaseAddress = baseAddress;

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, baseAddress + request);

                if (AppData.Session != null && AppData.Session.CurrentUser != null)
                {
                    requestMessage.Headers.Add("Email", AppData.Session.CurrentUser.Email);
                    requestMessage.Headers.Add("Key", AppData.Session.CurrentUser.Key.ToString());
                }
                
                HttpResponseMessage response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    string toReturn = await response.Content.ReadAsStringAsync();
                    return toReturn;
                }
                string msg = await response.Content.ReadAsStringAsync();
                return null;
            }
        }

        /// <summary>
        /// Get the most recent categories from the server. If a category has been removed, make sure its local contents are deleted
        /// </summary>
        public static async Task<bool> FetchCategories()
        {
            try
            {
                if (!AppData.CheckNetwork()) return false;

                List<ISpeechingPracticeActivity> currentActs = new List<ISpeechingPracticeActivity>();
                if (AppData.Session.Categories != null)
                {
                    foreach (ActivityCategory cat in AppData.Session.Categories)
                    {
                        foreach (ISpeechingPracticeActivity act in cat.Activities)
                        {
                            currentActs.Add(act);
                        }
                    }
                }

                AppData.Session.Categories =
                    await GetRequest<List<ActivityCategory>>("category");
                List<Task<bool>> allTasks = new List<Task<bool>>();

                AppData.SaveCurrentData();

                // Loop over all categories, downloading icons as needed for them and their scenarios
                for (int i = 0; i < AppData.Session.Categories.Count; i++)
                {
                    allTasks.Add(AppData.Session.Categories[i].PrepareIcon());

                    for (int j = 0; j < AppData.Session.Categories[i].Activities.Length; j++)
                    {
                        allTasks.Add(AppData.Session.ProcessScenario(i, j));
                    }
                }

                // Set them all off at once, deal with them finishing one at a time
                while (allTasks.Count > 0)
                {
                    Task<bool> firstFinished = await Task.WhenAny(allTasks);
                    allTasks.Remove(firstFinished);

                    if (!await firstFinished)
                    {
                        return false;
                    }
                }

                // See if any activities have been removed and delete their local content if necessary
                foreach (ISpeechingPracticeActivity act in currentActs)
                {
                    foreach (ActivityCategory cat in AppData.Session.Categories)
                    {
                        if (Array.IndexOf(cat.Activities, act) != -1) continue;

                        string titleFormatted = act.Title.Replace(" ", string.Empty).Replace("/", string.Empty);

                        if (await AppData.Root.CheckExistsAsync(titleFormatted) == ExistenceCheckResult.FileExists)
                        {
                            IFolder folder = await AppData.Root.GetFolderAsync(titleFormatted);
                            await folder.DeleteAsync();
                        }
                        break;
                    }
                }
                AppData.SaveCurrentData();

                return true;
            }
            catch (Exception except)
            {
                AppData.Io.PrintToConsole(except.Message);
                return false;
            }
        }

        /// <summary>
        /// Returns an array of places near the given coords
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="radius">The radius of the search area, in metres</param>
        /// <param name="callback">Optional method to call upon completion</param>
        /// <param name="includeLocalities"></param>
        /// <returns></returns>
        public static async void FetchPlaces(string lat, string lng, int radius, Action<GooglePlace[]> callback = null,
            bool includeLocalities = false)
        {
            if (!AppData.CheckNetwork())
            {
                if (callback != null) callback(null);
                return;
            }

            string basePlaces = "https://maps.googleapis.com/maps/api/place/nearbysearch/";

            Uri baseAddress = new Uri(basePlaces);

            string placesParams = "json?location=" + lat + "," + lng;
            placesParams += "&radius=" + radius;
            placesParams += "&key=" + ConfidentialData.GoogleServerApiKey;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = baseAddress;

                HttpResponseMessage response = await client.GetAsync(placesParams);
                if (response.IsSuccessStatusCode)
                {
                    string toReturn = await response.Content.ReadAsStringAsync();
                    PlacesQueryResult places = JsonConvert.DeserializeObject<PlacesQueryResult>(toReturn);

                    // Remove cities from the list unless otherwise requested
                    if (!includeLocalities)
                    {
                        List<GooglePlace> trimmed = new List<GooglePlace>();

                        foreach (GooglePlace place in places.results)
                        {
                            if (Array.IndexOf(place.types, "locality") == -1) trimmed.Add(place);
                        }

                        places.results = trimmed.ToArray();
                    }

                    if (callback != null)
                    {
                        callback(places.results);
                    }
                }
                else
                {
                    string msg = await response.Content.ReadAsStringAsync();
                    throw new Exception(msg);
                }
            }
        }

        /// <summary>
        /// Returns the address to the local copy of the given photoRef
        /// </summary>
        /// <param name="place">The Geofence object</param>
        /// <param name="maxWidth">Max width in pixels</param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        public static async Task<string> FetchPlacePhoto(PlaceGeofence place, int maxWidth, int maxHeight)
        {
            if (!AppData.CheckNetwork()) return null;
            return await FetchPlacePhoto(place.placeId, place.imageRef, maxHeight, maxWidth);
        }

        /// <summary>
        /// Returns the address to the local copy of the given photoRef
        /// </summary>
        /// <param name="place">The Google Place object</param>
        /// <param name="maxWidth">Max width in pixels</param>
        /// <param name="maxHeight">Max height in pixels</param>
        /// <returns></returns>
        public static async Task<string> FetchPlacePhoto(GooglePlace place, int maxWidth, int maxHeight)
        {
            if (!AppData.CheckNetwork()) return null;
            return await FetchPlacePhoto(place.place_id, place.photos[0].photo_reference, maxHeight, maxWidth);
        }

        private static async Task<string> FetchPlacePhoto(string placeId, string photoRef, int maxHeight, int maxWidth)
        {
            if (!AppData.CheckNetwork()) return null;

            string localRef = placeId + "_" + maxWidth + "_" + maxHeight;

            if (AppData.Session.PlacesPhotos.ContainsKey(localRef))
            {
                // We already have this locally!
                return AppData.Session.PlacesPhotos[localRef];
            }

            const string basePlaces = "https://maps.googleapis.com/maps/api/place/";

            Uri baseAddress = new Uri(basePlaces);

            string placesParams = "photo?photoreference=" + photoRef;

            placesParams += "&maxwidth=" + maxWidth;
            placesParams += "&maxheight=" + maxHeight;
            placesParams += "&key=" + ConfidentialData.GoogleServerApiKey;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = baseAddress;

                HttpResponseMessage response = await client.GetAsync(placesParams);
                if (response.IsSuccessStatusCode)
                {
                    byte[] data = await response.Content.ReadAsByteArrayAsync();
                    string localFileName = localRef + ".png";

                    IFile file =
                        await AppData.Cache.CreateFileAsync(localFileName, CreationCollisionOption.ReplaceExisting);
                    using (Stream stream = await file.OpenAsync(FileAccess.ReadAndWrite))
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    AppData.Session.PlacesPhotos.Add(localRef, file.Path);

                    return file.Path;
                }
                string msg = await response.Content.ReadAsStringAsync();
                throw new Exception(msg);
            }
        }

        private static async Task<string> UploadFile(string toUrl, IFile file, HttpClientHandler credentials)
        {
            using (HttpClient client = new HttpClient(credentials))
            {
                using (StreamContent content = new StreamContent(await file.OpenAsync(FileAccess.Read)))
                {

                    using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, toUrl))
                    {
                        req.Content = content;

                        using (HttpResponseMessage res = await client.SendAsync(req))
                        {
                            res.EnsureSuccessStatusCode();

                            return await res.Content.ReadAsStringAsync();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Uploads a single result package to the storage server
        /// </summary>
        /// <param name="toUpload">The item to upload</param>
        /// <param name="onUpdate"></param>
        /// <param name="callback"></param>
        /// <param name="cancelToken"></param>
        public static async Task PushResult(IResultItem toUpload, Action onUpdate, Action<bool> callback,
            CancellationToken cancelToken)
        {
            if (!AppData.CheckNetwork())
            {
                if (callback != null) callback(false);
                return;
            }

            bool success = true;
            string millis = DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds.ToString();
            string filename = millis + "_" + toUpload.ParticipantActivityId + ".zip";

            try
            {
                if (toUpload.UploadState < Utils.UploadStage.OnStorage)
                {
                    toUpload.UploadState = Utils.UploadStage.Uploading;
                    if (onUpdate != null)
                    {
                        onUpdate();
                    }

                    using (HttpClientHandler handler = new HttpClientHandler
                    {
                        Credentials = new NetworkCredential(
                            ConfidentialData.storageUsername,
                            ConfidentialData.storagePassword)
                    })
                    {
                        if (!AppData.Session.ServerFolderExists)
                        {
                            // Try to create the remote dir
                            try
                            {
                                // Create the HttpWebRequest object.
                                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(StorageServer + StorageRemoteDir);

                                // Add the network credentials to the request.
                                request.Credentials = handler.Credentials;

                                // Specify the MKCOL method.
                                request.Method = "MKCOL";

                                // Send the MKCOL method request, get the
                                // method response from the server.
                                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

                                AppData.Session.ServerFolderExists = true;
                            }
                            catch (Exception e)
                            {
                                AppData.Io.PrintToConsole(e.Message);
                                AppData.Session.ServerFolderExists = true;
                            }
                        }

                        await UploadFile(StorageServer + StorageRemoteDir + filename,
                            await AppData.Exports.GetFileAsync(Path.GetFileName(toUpload.ResourceUrl), cancelToken), handler);
                    }
                }
            }
            catch (Exception except)
            {
                AppData.Io.PrintToConsole(except.Message);
                success = false;
                toUpload.UploadState = Utils.UploadStage.Ready;
                if (onUpdate != null) onUpdate();
            }

            if (success)
            {
                // We've uploaded the file
                string original = toUpload.ResourceUrl;

                toUpload.ResourceUrl = StorageServer + StorageRemoteDir + filename;
                toUpload.UploadState = Utils.UploadStage.OnStorage;
                if (onUpdate != null) onUpdate();

                AppData.SaveCurrentData();

                success = await PushResultToDatabase(toUpload);
                if (success)
                {
                    // The web service knows about the file!
                    AppData.Session.ResultsToUpload.Remove(toUpload);
                    toUpload.UploadState = Utils.UploadStage.Finished;
                    if (onUpdate != null) onUpdate();
                    AppData.SaveCurrentData();

                    // Delete the local zip file now that it isn't needed
                    IFile toDel = await AppData.Exports.GetFileAsync(Path.GetFileName(original), cancelToken);
                    await toDel.DeleteAsync(cancelToken);
                }
            }

            if (callback != null) callback(success);
        }

        /// <summary>
        /// Uploads all items in the queue
        /// </summary>
        /// <param name="onUpdate"></param>
        /// <param name="onFinish">The function to get called on an item finishing. Is called as true when all items have been processed.</param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public static async void PushAllResults(Action onUpdate, Action<bool> onFinish, CancellationToken cancelToken)
        {
            if (!AppData.CheckNetwork())
            {
                if (onFinish != null) onFinish(false);
                return;
            }

            IResultItem[] toUpload = AppData.Session.ResultsToUpload.ToArray();
            int completed = 0;

            Action<bool> onUpload = success =>
            {
                completed++;
                onFinish(completed >= toUpload.Length);
            };

            foreach (IResultItem resultItem in toUpload)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    onFinish(false);
                    return;
                }
                await PushResult(resultItem, onUpdate, onUpload, cancelToken);
            }
        }

        /// <summary>
        /// Notify the web service that an output zip has been uploaded
        /// </summary>
        /// <param name="toUpload"></param>
        /// <returns></returns>
        private static async Task<bool> PushResultToDatabase(IResultItem toUpload)
        {
            if (!AppData.CheckNetwork())
            {
                return false;
            }

            try
            {
                string serialized = JsonConvert.SerializeObject(toUpload);
                await PostRequest<ServerError>("ActivityResult", serialized);
            }
            catch (Exception ex)
            {
                AppData.Io.PrintToConsole(ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sends the server a user account. The server will create/update the account in its database
        /// and send back a key for authorization with this account in future requests.
        /// </summary>
        /// <param name="user">Basic user details including email and name</param>
        /// <returns>A User object complete with a secret key</returns>
        public static async Task<User> PostUserAccount(User user)
        {
            if (!AppData.CheckNetwork())
            {
                return null;
            }

            string serializedAccount = JsonConvert.SerializeObject(user);
            return await PostRequest<User>("Account", serializedAccount);
        }

        /// <summary>
        /// Requests the deletion of the result from the remote database
        /// </summary>
        public static void PushResultDeletion(IResultItem toDelete)
        {
            //TODO
            if (!AppData.CheckNetwork()) return;

            AppData.SaveCurrentData();
        }

        /// <summary>
        /// Fetch a single user using the given id - TEMP will search by email
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static User FetchUser(string username)
        {
            if (!AppData.CheckNetwork()) return null;

            foreach (User user in AppData.Session.UserCache)
            {
                if (user.Name == username)
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
        public static async Task<User[]> FetchUsers(List<string> userIds)
        {
            if (!AppData.CheckNetwork()) return null;

            await Task.Delay(1000);

            List<User> users = new List<User>();

            //TEMP - will be polling the server (although checking the cache would be useful...)
            foreach (User user in AppData.Session.UserCache)
            {
                if (!userIds.Contains(user.Id)) continue;

                users.Add(user);
                if (users.Count >= userIds.Count) break; //Found all of them! :)
            }

            return users.ToArray();
        }

        /// <summary>
        /// Polls the server for all available feedback for the given activity
        /// </summary>
        /// <param name="resultId"></param>
        /// <returns></returns>
        public static async Task<List<IFeedItem>> FetchFeedbackFor(int resultId)
        {
            if (!AppData.CheckNetwork()) return null;

            //Dictionary<string, int> data = new Dictionary<string, int> {{"submissionId", resultId}};

            //return await GetRequest<IFeedbackItem[]>("GetFeedback", data);
            await Task.Delay(1000);

            // TEMP
            List<IFeedItem> arr = new List<IFeedItem>();

            for (int i = 0; i < 12; i++)
            {
                int thisRand = AppData.Rand.Next(0, 150);
                if (thisRand < 60)
                {
                    FeedItemPercentage fb = new FeedItemPercentage
                    {
                        Id = AppData.Rand.Next(1000000),
                        Title = "Stammering",
                        Percentage = AppData.Rand.Next(0, 100)
                    };
                    fb.Description = (int) fb.Percentage + "% of users thought you stammered over the word \"sausage\"";
                    arr.Add(fb);
                }
                else if (thisRand < 80)
                {
                    FeedItemStarRating fb = new FeedItemStarRating
                    {
                        Id = AppData.Rand.Next(1000000),
                        Title = "Your Rating",
                        Description = "This is your rating for something you did. Hopefully it's meaningful!",
                        Rating = (float) AppData.Rand.Next(0, 10)/2
                    };
                    arr.Add(fb);
                }
                else if (thisRand < 110)
                {
                    FeedItemUser fb = new FeedItemUser
                    {
                        Id = AppData.Rand.Next(1000000),
                        Title = "A comment on a recording",
                        Description =
                            "I really liked this recording - you should try to do it more like this in the future. Excellent work!",
                        UserAccount = new User
                        {
                            Name = "Tom Hanks",
                            Avatar =
                                "http://media.nu.nl/m/m1mxjewa2jvj_sqr256.jpg/tom-hanks-produceert-filmversie-van-carole-king-musical.jpg"
                        }
                    };
                    arr.Add(fb);
                }
                else
                {
                    FeedItemGraph fb = new FeedItemGraph
                    {
                        Id = AppData.Rand.Next(1000000),
                        Title = "Your progress",
                        Description = "This is a graph showing some data!",
                        BottomAxisLength = 12,
                        LeftAxisLength = 100,
                        DataPoints = new TimeGraphPoint[12]
                    };

                    for (int j = 0; j < fb.DataPoints.Length; j++)
                    {
                        fb.DataPoints[j] = new TimeGraphPoint
                        {
                            XVal = DateTime.Now.AddDays(-j),
                            YVal = AppData.Rand.Next(100)
                        };
                    }

                    arr.Add(fb);
                }
            }

            return arr;
        }

        /// <summary>
        /// Pull today's featured article from Wikipedia
        /// </summary>
        /// <returns></returns>
        public static async Task<WikipediaResult> FetchWikiData(Func<string, string> htmlDecode)
        {
            try
            {
                string dateString = DateTime.Now.ToString("MMMM_d,_yyyy");

                HttpWebRequest request =
                    (HttpWebRequest)
                        WebRequest.Create(
                            "http://en.wikipedia.org/w/api.php?action=parse&&prop=text|images&page=Wikipedia:Today%27s_featured_article/" +
                            dateString + "&format=json");

                string pageText;
                using (HttpWebResponse resp = (HttpWebResponse) (await request.GetResponseAsync()))
                {
                    using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                    {
                        pageText = await reader.ReadToEndAsync();
                    }
                }

                WikipediaResult res = JsonConvert.DeserializeObject<WikipediaResult>(pageText);

                if (res.parse.images != null && res.parse.images.Length > 0)
                {
                    // There's an image available from this page! Unfortunately we have to request the actual URL of the file separately (limit to 600px to avoid HUGE files being pulled)
                    HttpWebRequest imgReq =
                        (HttpWebRequest)
                            WebRequest.Create("http://en.wikipedia.org/w/api.php?action=query&continue=&titles=Image:" +
                                              res.parse.images[0] +
                                              "&prop=imageinfo&iiprop=url&iiurlwidth=600&format=json");
                    string imgText;
                    using (HttpWebResponse resp = (HttpWebResponse) (await imgReq.GetResponseAsync()))
                    {
                        using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                        {
                            imgText = await reader.ReadToEndAsync();
                        }
                    }

                    WikipediaResult imgRes = JsonConvert.DeserializeObject<WikipediaResult>(imgText);

                    // Store the image location in the main wiki result obj
                    foreach (KeyValuePair<string, QueryWikiInfo> info in imgRes.query.pages)
                    {
                        string imageUrl = (string.IsNullOrEmpty(info.Value.imageInfo[0].thumbUrl))
                            ? info.Value.imageInfo[0].url
                            : info.Value.imageInfo[0].thumbUrl;

                        res.imageURL = await Utils.FetchLocalCopy(imageUrl, typeof (WikipediaResult));
                        break;
                    }
                }

                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(res.parse.text.HTML);

                pageText = "";

                // Scrape the HTML for all content text
                foreach (HtmlNode node in htmlDoc.DocumentNode.ChildNodes)
                {
                    if (node.Name == "p")
                    {
                        pageText += htmlDecode(node.InnerText) + "\n";
                    }
                }

                //Remove the (Full article...) text and all following it
                int index = pageText.IndexOf("(Full article...)", StringComparison.Ordinal);
                if (index > 0) pageText = pageText.Substring(0, index);

                res.parse = null;
                res.content = pageText;

                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<Assessment> FetchAssessment()
        {
            if (!AppData.CheckNetwork()) return null;

            await Task.Delay(1000); // Network simulation TODO

            const string jsonString =
                "{\"AssessmentTasks\":[{\"PromptCol\":[{\"Id\":1,\"Value\":\"Easy\"},{\"Id\":2,\"Value\":\"Trickier\"},{\"Id\":3,\"Value\":\"Simple\"},{\"Id\":4,\"Value\":\"More Difficult\"},{\"Id\":5,\"Value\":\"Exquisite\"},{\"Id\":6,\"Value\":\"Borderline\"}],\"TaskType\":1,\"Id\":1,\"Title\":\"QuickFire Speaking!\",\"Instructions\":\"Press the record button and say the shown word as clearly as you can, then press stop.\"},{\"PromptCol\":[{\"Id\":7,\"Value\":\"What does the image show?\"},{\"Id\":8,\"Value\":\"Describe the colours in the image.\"},{\"Id\":9,\"Value\":\"Describe the dominant feature of the image.\"},{\"Id\":10,\"Value\":\"What does the image make you think of?\"}],\"TaskType\":0,\"Id\":2,\"Title\":\"Image Description\",\"Instructions\":\"Press the 'Record' button and follow the instruction in the image's caption\",\"Image\":\"http://th00.deviantart.net/fs71/PRE/i/2013/015/d/c/a_hobbit_hole_by_uberpicklemonkey-d5rmn8n.jpg\"}],\"CrowdCategory\":{\"Activities\":[],\"Id\":4,\"ExternalId\":\"Assessments\",\"Title\":\"Progress Assessments\",\"Icon\":\"https://cdn1.iconfinder.com/data/icons/MetroStation-PNG/128/MB__help.png\",\"Recommended\":false,\"DefaultSubscription\":true},\"CrowdPages\":[],\"ParticipantResults\":[],\"ParticipantTasks\":[],\"Id\":5,\"ExternalId\":\"assess1\",\"Title\":\"Your first assessment!\",\"Icon\":\"http://www.pursuittraining.co.uk/images/care-icon.gif\",\"CrowdCategoryId\":4,\"Description\":\"Doing this short assessment will help us determine which parts of your speech might need some practice!\",\"DateSet\":\"2015-05-26T10:32:02.807\"}";
            try
            {
                return JsonConvert.DeserializeObject<Assessment>(jsonString);
            }
            catch (Exception except)
            {
                AppData.Io.PrintToConsole(except.Message);
                return null;
            }
        }

        public static async Task<List<IFeedItem>> FetchMainFeed()
        {
            if (!AppData.CheckNetwork()) return null;

            List<IFeedItem> items = await GetRequest<List<IFeedItem>>("Feed", "", new FeedItemConverter());

            if (items != null) AppData.Session.CurrentFeed = items;

            return items;

            //string jsonString = "[\r\n\t{\r\n\t\t\"Title\"\t\t\t: \"A New Assessment Is Available!\",\r\n\t\t\"Description\" \t: \"There's a new assessment available for you to complete! The feedback from completing this short activity will help you to keep track of your progress.\",\r\n\t\t\"Date\"\t\t\t: \"2015-04-21T18:25:43.511Z\",\r\n\t\t\"Dismissable\" \t: false,\r\n\t\t\"Importance\"\t: 10,\r\n\t\t\"Interaction\"\t: {\r\n\t\t\t\"type\"\t: \"ASSESSMENT\",\r\n\t\t\t\"value\"\t: \"\",\r\n\t\t\t\"label\"\t: \"Start Assessment\"\r\n\t\t}\r\n\t},\r\n\t{\r\n\t\t\"Title\"\t\t\t: \"Feedback From Your Last Assessment\",\r\n\t\t\"Description\" \t: \"\\\"Your control over your rate of speech has really improved since last month's assessment! It was much easier to understand what you were saying. Keep it up! :D\\\"\",\r\n\t\t\"Date\"\t\t\t: \"2015-04-21T18:25:43.511Z\",\r\n\t\t\"Dismissable\" \t: true,\r\n\t\t\"Importance\"\t: 8,\r\n\t\t\"UserAccount\"\t: {\r\n\t\t\t\"id\"\t: 768705735,\r\n\t\t\t\"name\"\t: \"Tom Hanks\",\r\n\t\t\t\"avatar\": \"http://assets-s3.mensjournal.com/img/article/tom-hanks-the-mj-interview/298_298_tom-hanks-the-mj-interview.jpg\"\r\n\t\t}\r\n\t},\r\n\t{\r\n\t\t\"Title\"\t\t\t: \"Recommended Activity\",\r\n\t\t\"Description\" \t: \"A short roleplay where you catch the bus.\",\r\n\t\t\"Date\"\t\t\t: \"2015-04-21T18:25:43.511Z\",\r\n\t\t\"Dismissable\" \t: true,\r\n\t\t\"Importance\"\t: 9,\r\n\t\t\"Rationale\"\t\t: [\"Good for practicing voice pitch\", \"Roisin shared this\"],\r\n\t\t\"Activity\"\t\t: {\r\n\t\t\t\"Id\"\t: 1,\r\n\t\t\t\"Title\"\t: \"Getting the Bus\",\r\n\t\t\t\"Icon\"\t: \"http://www.survivingamsterdam.com/public/files/e96fc9baf228c0cb8d210a1768995bb1.png\"\r\n\t\t},\r\n\t\t\"Interaction\"\t: {\r\n\t\t\t\"type\"\t: \"ACTIVITY\",\r\n\t\t\t\"value\"\t: \"1\",\r\n\t\t\t\"label\"\t: \"Start Activity\"\r\n\t\t}\r\n\t},\r\n    {\r\n        \"Title\": \"Featured Article: 'Stuttering is in the genes not the head, say scientists'\",\r\n        \"Description\": \"Stuttering is not to do with nervousness or a traumatic childhood as portrayed in the award winning film The King\u2019s Speech but has its root cause in a genetic disorder, new research suggests.\",\r\n        \"Date\": \"2015-04-21T18:25:43.511Z\",\r\n        \"Dismissable\": true,\r\n        \"Importance\": 4,\r\n        \"Image\": \"http://i.telegraph.co.uk/multimedia/archive/01830/speech_1830638c.jpg\",\r\n        \"Interaction\": {\r\n            \"type\": \"URL\",\r\n            \"value\": \"http://www.telegraph.co.uk/news/science/science-news/8336493/Stuttering-is-in-the-genes-not-the-head-say-scientists.html\",\r\n            \"label\": \"Read More\"\r\n        }\r\n    },\r\n    {\r\n\t\t\"Title\"\t\t\t: \"A Friend Is Looking For Feedback!\",\r\n\t\t\"Description\" \t: \"Help Tom out by leaving him feedback on this recent recording!\",\r\n\t\t\"Date\"\t\t\t: \"2015-04-21T18:25:43.511Z\",\r\n\t\t\"Dismissable\" \t: true,\r\n\t\t\"Importance\"\t: 5,\r\n        \"UserAccount\": {\r\n            \"id\": 768705735,\r\n            \"name\": \"Tom Hanks\",\r\n            \"avatar\": \"http://assets-s3.mensjournal.com/img/article/tom-hanks-the-mj-interview/298_298_tom-hanks-the-mj-interview.jpg\"\r\n        },\r\n        \"Interaction\": {\r\n            \"type\": \"URL\",\r\n            \"value\": \"http://www.telegraph.co.uk/news/science/science-news/8336493/Stuttering-is-in-the-genes-not-the-head-say-scientists.html\",\r\n            \"label\": \"Listen\"\r\n        }\r\n\t}\r\n]";

            //string jsonString =
            //  "[{\"Rating\":3.33333325,\"Id\":0,\"Title\":\"Accent Influence\",\"Description\":\"This rating shows how much your accent affected listeners' understanding of your speech.\",\"Date\":\"2015-05-20T09:41:57.9391882+01:00\",\"Dismissable\":false,\"Importance\":10},{\"Rating\":3.0,\"Id\":0,\"Title\":\"Difficulty of Understanding\",\"Description\":\"This rating shows how difficult listeners find understanding what you say.\",\"Date\":\"2015-05-20T09:41:57.9631727+01:00\",\"Dismissable\":false,\"Importance\":10},{\"DataPoints\":[{\"YVal\":3.0,\"XVal\":\"2015-05-01T16:32:45\"},{\"YVal\":5.0,\"XVal\":\"2015-05-16T16:32:44\"},{\"YVal\":1.0,\"XVal\":\"2015-06-01T16:32:45\"}],\"Id\":0,\"Title\":\"Understanding Progress\",\"Description\":\"How understandable people have found you over time.\",\"Date\":\"2015-05-20T09:41:57.9631727+01:00\",\"Dismissable\":false,\"Importance\":8}]";

            //return JsonConvert.DeserializeObject<List<IFeedItem>>(jsonString, new FeedItemConverter());
        }
    }

    public class ServerError
    {
        public string Id;
        public string Message;
        public string Title;
    }
}