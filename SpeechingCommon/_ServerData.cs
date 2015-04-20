using HtmlAgilityPack;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SpeechingCommon
{
    /// <summary>
    /// A collection of server structures, variables and methods
    /// Methods starting with "Push" request changes be made to server data. Methods starting with "Fetch" pull data from the server.
    /// </summary>
    public static class ServerData
    {
        public enum ActivityType { Scenario, Guide };

        public static string storageServer = @"https://di.ncl.ac.uk/owncloud/remote.php/webdav/";
        public static string storageRemoteDir;

        public static string serviceUrl = "http://api.opescode.com/api/";

        /// <summary>
        /// Send a POST request to the server and return an Object of type T in response
        /// </summary>
        /// <typeparam name="T">The object type to deserialize from JSON</typeparam>
        /// <param name="route">The route on the server to query</param>
        /// <param name="jsonData">Serialized data to send to the server in the request</param>
        /// <returns>The response's JSON in type T</returns>
        public static async Task<T> PostRequest<T>(string route, string jsonData = null)
        {
            try
            {
                if (!AppData.CheckNetwork()) return default(T);

                using (HttpClient client = new HttpClient())
                {
                    Uri baseAddress = new Uri(serviceUrl);
                    client.BaseAddress = baseAddress;

                    HttpContent content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(route, content);
                    if (response.IsSuccessStatusCode)
                    {
                        string toReturn = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<T>(toReturn);
                    }
                    else
                    {
                        string msg = await response.Content.ReadAsStringAsync();
                        throw new Exception(msg);
                    }
                }
            }
            catch(Exception except)
            {
                Console.WriteLine(except);
                throw except;
            }
        }

        /// <summary>
        /// Sends a get request with the given argument after the url
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="route"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<T> GetRequest<T>(string route, string data = "", JsonConverter converter = null)
        {
            if (!AppData.CheckNetwork()) return default(T);

            string url = route;

            if (!string.IsNullOrEmpty(data))
            {
                url += "/" + data;
            }

            //string result = "[\r\n{\r\n    \"Id\" : 100011,\r\n    \"Title\" : \"Dysfluency\",\r\n    \"Icon\"  : \"https://cdn0.iconfinder.com/data/icons/cosmo-medicine/40/test-tube_1-128.png\",\r\n    \"Recommended\" : false,\r\n    \"Activities\" : [\r\n    {\r\n        \"Id\" : 1,\r\n        \"Creator\" : {\r\n            \"id\"    : 195606,\r\n            \"name\"  : \"Justin Time\"\r\n        },\r\n        \"Title\" : \"Getting the Bus\",\r\n        \"Icon\"  : \"http://www.survivingamsterdam.com/public/files/e96fc9baf228c0cb8d210a1768995bb1.png\",\r\n        \"Resource\" : \"https://www.dropbox.com/s/0h2f8pyrh6xte3s/bus.zip?raw=1\",\r\n        \"Tasks\" : [\r\n        {\r\n            \"Id\" : 1231231,\r\n            \"TaskContentModel\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"driver.jpg\",\r\n                \"audio\"  : \"hello.mp3\",\r\n                \"text\"   : \"Hello! Where would you like to go today?\"\r\n            },\r\n            \"TaskResponseModel\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Hello, please may I have a return ticket to the train station?\"\r\n            }\r\n        },\r\n        {\r\n            \"Id\" : 12414235,\r\n            \"TaskContentModel\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"driver.jpg\",\r\n                \"audio\"  : \"thanks.mp3\",\r\n                \"text\"   : \"No problem at all, looks like you have a valid card. Take a seat!\"\r\n            },\r\n            \"TaskResponseModel\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Thank you. Have a good day.\"\r\n            }\r\n        },\r\n        {\r\n            \"Id\" : 12536,\r\n            \"TaskContentModel\" : {\r\n                \"type\"   : \"Text\",\r\n                \"visual\" : \"oldwoman.jpg\",\r\n                \"audio\"  : null,\r\n                \"text\"   : \"You sit next to an old woman, who asks what your plans are for the day.\"\r\n            },\r\n            \"TaskResponseModel\" : {\r\n                \"type\"  : \"Freeform\",\r\n                \"prompt\" : \"Greet her and explain that you're catching a train to the seaside.\"\r\n            }\r\n        }\r\n        ]\r\n    },\r\n    {\r\n        \"Id\" : 2,\r\n        \"Creator\" : {\r\n            \"id\"    : 195606,\r\n            \"name\"  : \"Justin Time\"\r\n        },\r\n        \"Title\" : \"Ordering a Pizza\",\r\n        \"Icon\"  : \"http://img3.wikia.nocookie.net/__cb20131231163822/cardfight/images/6/6f/Pizza_slice_combo_clipart.png\",\r\n        \"Resource\" : \"https://www.dropbox.com/s/8gt7pqh6zq6p18h/pizza.zip?raw=1\",\r\n        \"Tasks\" : [\r\n        {\r\n            \"Id\" : 195864,\r\n            \"TaskContentModel\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"phone.jpg\",\r\n                \"audio\"  : \"hello.mp3\",\r\n                \"text\"   : \"You are ordering pizza over the phone for both yourself and a friend who has a gluten alergy.\\n'Tony's Pizza Parlour, how can I help you?'\"\r\n            },\r\n            \"TaskResponseModel\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Hello, can I order a pizza please?\"\r\n            }\r\n        },\r\n        {\r\n            \"Id\" : 35696,\r\n            \"TaskContentModel\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"pizza.jpg\",\r\n                \"audio\"  : \"order1.mp3\",\r\n                \"text\"   : \"Of course! What kind would you like?\"\r\n            },\r\n            \"TaskResponseModel\" : {\r\n                \"type\"  : \"Freeform\",\r\n                \"prompt\" : \"Describe your favourite pizza\"\r\n            }\r\n        },\r\n        {\r\n            \"Id\" : 979574,\r\n            \"TaskContentModel\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"pizza.jpg\",\r\n                \"audio\"  : \"order2.mp3\",\r\n                \"text\"   : \"That sounds delicious! Would you like anything else?\"\r\n            },\r\n            \"TaskResponseModel\" : {\r\n                \"type\"  : \"Freeform\",\r\n                \"prompt\" : \"Describe another kind of pizza, but make sure it's gluten free!\"\r\n            }\r\n        },\r\n        {\r\n            \"Id\" : 83241,\r\n            \"TaskContentModel\" : {\r\n                \"type\"   : \"Audio\",\r\n                \"visual\" : \"making.jpg\",\r\n                \"audio\"  : \"end.mp3\",\r\n                \"text\"   : \"No problem at all, we can do that. See you soon!\"\r\n            },\r\n            \"TaskResponseModel\" : {\r\n                \"type\"  : \"Prompted\",\r\n                \"prompt\" : \"Thank you, see you later.\"\r\n            }\r\n        },\r\n        ]\r\n    }\r\n    ]\r\n},\r\n{\r\n    \"Id\" : 3124,\r\n    \"Title\" : \"Dementia\",\r\n    \"Icon\"  : \"https://cdn0.iconfinder.com/data/icons/cosmo-medicine/40/test-tube_1-128.png\",\r\n    \"Recommended\" : false,\r\n    \"Activities\" : [\r\n        {\r\n            \"Id\" : 3,\r\n            \"Creator\" : {\r\n                \"id\"    : 195606,\r\n                \"name\"  : \"Justin Time\"\r\n            },\r\n            \"Title\" : \"Preparing Dinner\",\r\n            \"Icon\"  : \"http://img3.wikia.nocookie.net/__cb20110528210150/restaurantcity/images/4/46/Spaghetti_Bolognese.png\",\r\n            \"Resource\" : \"https://www.dropbox.com/s/3isleqzen5gt0hf/dinner.zip?raw=1\",\r\n            \"Tasks\" : [\r\n            {\r\n                \"Id\" : 587908,\r\n                \"TaskContentModel\" : {\r\n                    \"type\"   : \"AUDIO\",\r\n                    \"visual\" : \"spagBol.jpg\",\r\n                    \"audio\"  : \"spag1.mp3\",\r\n                    \"text\"   : \"You've invited your best friend over for dinner and have decided to make spaghetti bolognese.\"\r\n                },\r\n                \"TaskResponseModel\" : {\r\n                    \"type\"  : \"none\",\r\n                    \"prompt\" : null\r\n                }\r\n            },\r\n            {\r\n                \"Id\" : 679806,\r\n                \"TaskContentModel\" : {\r\n                    \"type\"   : \"AUDIO\",\r\n                    \"visual\" : \"supermarket.jpg\",\r\n                    \"audio\"  : \"spag2.mp3\",\r\n                    \"text\"   : \"You go to the supermarket to buy some of the ingredients.\"\r\n                },\r\n                \"TaskResponseModel\" : {\r\n                    \"type\"  : \"Choice\",\r\n                    \"prompt\" : \"Choose the spaghetti from the shelf.\",\r\n                    \"choice1\" : \"spaghetti.jpg\",\r\n                    \"choice2\"   : \"bakedBeans.png\"\r\n                }\r\n            },\r\n            {\r\n                \"Id\" : 416597,\r\n                \"TaskContentModel\" : {\r\n                    \"type\"   : \"AUDIO\",\r\n                    \"visual\" : \"supermarket.jpg\",\r\n                    \"audio\"  : \"spag3.mp3\",\r\n                    \"text\"   : \"You also need something to make the sauce from...\"\r\n                },\r\n                \"TaskResponseModel\" : {\r\n                    \"type\"  : \"Choice\",\r\n                    \"prompt\" : \"Which of these could you make a pasta sauce from?\",\r\n                    \"choice1\" : \"dogFood.jpg\",\r\n                    \"choice2\"   : \"tomatoes.jpg\"\r\n                }\r\n            },\r\n            {\r\n                \"Id\" : 749604,\r\n                \"TaskContentModel\" : {\r\n                    \"type\"   : \"AUDIO\",\r\n                    \"visual\" : \"cashier.jpg\",\r\n                    \"audio\"  : \"spag4.mp3\",\r\n                    \"text\"   : \"As you pay for your items, the cashier asks about your bolognese recipe.\"\r\n                },\r\n                \"TaskResponseModel\" : {\r\n                    \"type\"  : \"Freeform\",\r\n                    \"prompt\" : \"Describe the process of cooking spaghetti bolognese to the cashier.\"\r\n                }\r\n            },\r\n            {\r\n                \"Id\" : 7498607,\r\n                \"TaskContentModel\" : {\r\n                    \"type\"   : \"AUDIO\",\r\n                    \"visual\" : \"cashier.jpg\",\r\n                    \"audio\"  : \"spag5.mp3\",\r\n                    \"text\"   : \"Oh, that sounds delicious! Are you having anyone over?\"\r\n                },\r\n                \"TaskResponseModel\" : {\r\n                    \"type\"  : \"Freeform\",\r\n                    \"prompt\" : \"Describe your best friend to the cashier.\"\r\n                }\r\n            }\r\n            ]\r\n        }\r\n    ]\r\n    },\r\n    {\r\n    \"Id\" : 629506,\r\n    \"Title\" : \"Helpful Guides\",\r\n    \"Icon\"  : \"https://cdn1.iconfinder.com/data/icons/MetroStation-PNG/128/MB__help.png\",\r\n    \"Recommended\" : false,\r\n    \"Activities\" : [\r\n        {\r\n            \"Id\" : 4,\r\n            \"Creator\" : {\r\n                \"id\"    : 195606,\r\n                \"name\"  : \"Justin Time\"\r\n            },\r\n            \"Title\" : \"Interaction Tips\",\r\n            \"Icon\"  : \"http://www.pursuittraining.co.uk/images/care-icon.gif\",\r\n            \"Resource\" : \"https://www.dropbox.com/s/pw1ubz20nwatxtl/guide.zip?raw=1\",\r\n            \"Pages\" : [\r\n                {\r\n                    \"visualMediaLoc\" : \"pic1.jpg\",\r\n                    \"text\"           : \"Try to think through how it might feel to struggle to communicate if you were living with dementia and think about what might help and what has helped in the past.\"\r\n                },\r\n                {\r\n                    \"visualMediaLoc\" : \"pic2.jpg\",\r\n                    \"text\"           : \"Smile where you can and offer reassuring physical contact where it is appropriate. Make sure people can see your face and that you have engaged their attention.\"\r\n                },\r\n                {\r\n                    \"visualMediaLoc\" : \"pic3.jpg\",\r\n                    \"text\"           : \"Relax as much as you can and help the person you are talking with to relax. Be prepared to be treated as someone you are not (for example being mistaken for another relative).\"\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n    }\r\n]";
            string result = await SendGetRequest(url);

            if (result == null) return default(T);

            if (converter != null)
            {
                return JsonConvert.DeserializeObject<T>(result, converter);
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(result);
            }
        }

        /// <summary>
        /// Send a GET request to the server and return an Object of type T in response
        /// </summary>
        /// <typeparam name="T">The expected object type to return from JSON</typeparam>
        /// <param name="route">The route on the server to query</param>
        /// <param name="data">Keys + values to include in the sent URL</param>
        /// <returns>The response's JSON as a type T</returns>
        public static async Task<T> GetRequest<T>(string route,  Dictionary<string, string> data, JsonConverter converter = null)
        {
            if (!AppData.CheckNetwork()) return default(T);
            string url = route;

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

            if (converter != null)
            {
                return JsonConvert.DeserializeObject<T>(result, converter);
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(result);
            }
        }

        /// <summary>
        /// Sends a get request to the given URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task<string> SendGetRequest(string request)
        {
            if (!AppData.CheckNetwork()) return null;

            using(HttpClient client = new HttpClient())
            {
                Uri baseAddress = new Uri(serviceUrl);
                client.BaseAddress = baseAddress;
                
                try
                {
                    HttpResponseMessage response = await client.GetAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        string toReturn = await response.Content.ReadAsStringAsync();
                        return toReturn;
                    }
                    else
                    {
                        string msg = await response.Content.ReadAsStringAsync();
                        throw new Exception(msg);
                    }
                }
                catch(Exception except)
                {
                    throw except;
                }
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

                List<ISpeechingActivityItem> currentActs = new List<ISpeechingActivityItem>();
                if (AppData.session.categories != null)
                {
                    foreach(ActivityCategory cat in AppData.session.categories)
                    {
                        foreach(ISpeechingActivityItem act in cat.activities)
                        {
                            currentActs.Add(act);
                        }
                    }
                }

                AppData.session.categories = await ServerData.GetRequest<List<ActivityCategory>>("category", "", new ActivityConverter());

                // Loop over all categories, downloading icons as needed for them and their scenarios
                for (int i = 0; i < AppData.session.categories.Count; i++)
                {
                    AppData.session.categories[i].DownloadIcon();

                    for (int j = 0; j < AppData.session.categories[i].activities.Length; j++)
                    {
                        AppData.session.ProcessScenario(i, j, true);
                    }
                }

                // More efficient to await them all collectively than one at a time
                int timeout = 10000;
                int waited = 0;
                int interval = 100;

                while (waited < timeout)
                {
                    if (SessionData.scenariosProcessing == 0 && ActivityCategory.runningDLs == 0)
                    {
                        break;
                    }
                    waited += interval;
                    await Task.Delay(interval);
                }

                // See if any activities have been removed and delete their local content if necessary
                foreach(ISpeechingActivityItem act in currentActs)
                {
                    foreach (ActivityCategory cat in AppData.session.categories)
                    {
                        if(Array.IndexOf(cat.activities, act) == -1)
                        {
                            string titleFormatted = act.Title.Replace(" ", String.Empty).Replace("/", String.Empty);
                            string localResourcesDirectory = AppData.cacheDir + "/" + titleFormatted;

                            if(Directory.Exists(localResourcesDirectory))
                            {
                                Directory.Delete(localResourcesDirectory, true);
                            }
                            break;
                        }
                    }
                }
                AppData.SaveCurrentData();

                return true;
            }
            catch(Exception except)
            {
                Console.WriteLine(except);
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
        /// <returns></returns>
        public static async Task<GooglePlace[]> FetchPlaces(string lat, string lng, int radius, Action<GooglePlace[]> callback = null, bool includeLocalities = false)
        {
            if (!AppData.CheckNetwork()) return null;

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
                    try
                    {
                        string toReturn = await response.Content.ReadAsStringAsync();
                        PlacesQueryResult places = JsonConvert.DeserializeObject<PlacesQueryResult>(toReturn);

                        // Remove cities from the list unless otherwise requested
                        if(!includeLocalities)
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

                        return places.results;
                    }
                    catch(Exception except)
                    {
                        throw except;
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

            if (AppData.session.placesPhotos.ContainsKey(localRef))
            {
                // We already have this locally!
                return AppData.session.placesPhotos[localRef];
            }

            string basePlaces = "https://maps.googleapis.com/maps/api/place/";

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
                    try
                    {
                        Byte[] data = await response.Content.ReadAsByteArrayAsync();
                        string localFileName = localRef + ".png";
                        string localPath = Path.Combine(AppData.placesImageCache, localFileName);
                        File.WriteAllBytes(localPath, data);

                        AppData.session.placesPhotos.Add(localRef, localPath);

                        return localPath;
                    }
                    catch (Exception except)
                    {
                        throw except;
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
        /// Uploads a single result package to the storage server
        /// </summary>
        /// <param name="toUpload">The item to upload</param>
        public static async Task PushResult(IResultItem toUpload, Action onUpdate, Action<bool> callback, CancellationToken cancelToken)
        {
            if (!AppData.CheckNetwork())
            {
                if (callback != null) callback(false);
                return;
            }

            bool success = false;
            string millis = DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds.ToString();
            string filename = millis + "_" + toUpload.ParticipantActivityId + ".zip";

            try
            {
                if(toUpload.UploadState < Utils.UploadStage.OnStorage)
                {
                    toUpload.UploadState = Utils.UploadStage.Uploading;
                    if (onUpdate != null)
                    {
                        onUpdate();
                    }

                    FileStream fs = File.OpenRead(toUpload.ResourceUrl);
                    byte[] content = new byte[fs.Length];
                    fs.Read(content, 0, content.Length);
                    fs.Close();

                    HttpWebRequest putReq = (HttpWebRequest)WebRequest.Create(storageServer + storageRemoteDir + filename);
                    putReq.Credentials = new NetworkCredential(ConfidentialData.storageUsername, ConfidentialData.storagePassword);
                    putReq.PreAuthenticate = true;
                    putReq.Method = @"PUT";
                    putReq.Headers.Add(@"Overwrite", @"T");
                    putReq.ContentLength = content.Length;
                    putReq.SendChunked = true;

                    using (Stream reqStream = putReq.GetRequestStream())
                    {
                        await reqStream.WriteAsync(content, 0, content.Length, cancelToken);
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
                            httpMkColRequest.Credentials = new NetworkCredential(ConfidentialData.storageUsername, ConfidentialData.storagePassword);
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
            }
            catch(Exception except)
            {
                Console.WriteLine("Oh dear! " + except);
                toUpload.UploadState = Utils.UploadStage.Ready;
                if (onUpdate != null) onUpdate();
            }

            if(success)
            {
                // We've uploaded the file
                toUpload.ResourceUrl = storageServer + storageRemoteDir + filename;
                toUpload.UploadState = Utils.UploadStage.OnStorage;
                if (onUpdate != null) onUpdate();

                AppData.SaveCurrentData();

                success = true;
                success = await PushResultToDatabase(toUpload);
                if(success)
                {
                    // The web service knows about the file!
                    AppData.session.resultsToUpload.Remove(toUpload);
                    toUpload.UploadState = Utils.UploadStage.Finished;
                    if (onUpdate != null) onUpdate();
                    AppData.SaveCurrentData();
                }
            }

            if (callback != null) callback(success);
        }

        /// <summary>
        /// Uploads all items in the queue
        /// </summary>
        /// <param name="onFinish">The function to get called on an item finishing. Is called as true when all items have been processed.</param>
        /// <returns></returns>
        public static async Task PushAllResults(Action onUpdate, Action<bool> onFinish, CancellationToken cancelToken)
        {
            if (!AppData.CheckNetwork())
            {
                if (onFinish != null) onFinish(false);
                return;
            }

            // Make sure that the folder exists on the server
            HttpWebRequest httpMkColRequest = (HttpWebRequest)WebRequest.Create(storageServer + storageRemoteDir);
            httpMkColRequest.Credentials = new NetworkCredential(ConfidentialData.storageUsername, ConfidentialData.storagePassword);
            httpMkColRequest.PreAuthenticate = true;
            httpMkColRequest.Method = @"MKCOL";

            using(cancelToken.Register(() => httpMkColRequest.Abort(), useSynchronizationContext: false))
            {
                try
                {
                    HttpWebResponse httpMkColResponse = (System.Net.HttpWebResponse)await httpMkColRequest.GetResponseAsync();
                    cancelToken.ThrowIfCancellationRequested();
                }
                catch (Exception e)
                {
                    if(cancelToken.IsCancellationRequested)
                    {
                        onFinish(false);
                        return;
                    }

                    Console.WriteLine(e);
                }
            }
           
            DateTime old = DateTime.MinValue.AddYears(1969);
            IResultItem[] toUpload = AppData.session.resultsToUpload.ToArray();
            int completed = 0;

            Action<bool> onUpload = (bool success) =>
                {
                    completed++;
                    onFinish(completed >= toUpload.Length);
                };

            for(int i = 0; i < toUpload.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    onFinish(false);
                    return;
                }
                await PushResult(toUpload[i], onUpdate, onUpload, cancelToken);
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
                await ServerData.PostRequest<ServerError>("ActivityResult", JsonConvert.SerializeObject(toUpload));
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
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
        /// <param name="userId"></param>
        /// <returns></returns>
        public static User FetchUser(string username)
        {
            if (!AppData.CheckNetwork()) return null;

            foreach (User user in AppData.session.userCache)
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
        public static async Task<User[]> FetchUsers(List<string> userIds)
        {
            if (!AppData.CheckNetwork()) return null;

            await Task.Delay(1000);

            List<User> users = new List<User>();

            //TEMP - will be polling the server (although checking the cache would be useful...)
            foreach (User user in AppData.session.userCache)
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
        public static async Task<User[]> FetchAcceptedFriends()
        {
            if (!AppData.CheckNetwork()) return null;

            List<User> toRet = new List<User>();
            User[] allFriends = await ServerData.FetchUsers(AppData.session.currentUser.friends);

            foreach (User friend in allFriends)
            {
                if (friend.id != AppData.session.currentUser.id && friend.status == User.FriendStatus.Accepted)
                {
                    toRet.Add(friend);
                }
            }

            return toRet.ToArray();
        }

        /// <summary>
        /// Sends a friend request to the server
        /// </summary>
        /// <param name="username">The unique username of the friend</param>
        /// <returns>User found true / not recognised false</returns>
        public static bool PushFriendRequest(string username)
        {
            if (!AppData.CheckNetwork()) return false;

            // TODO push friend request to the server, which will return user details if successful
            User added = new User();
            added.name = username;
            added.status = User.FriendStatus.Sent;
            added.id = AppData.rand.Next(0, 10000).ToString();

            // TODO Check if the server returns an error saying they're already friends, or the user doesn't exist etc

            // If the user has already been cached, update the object. Else, just add it
            bool cached = false;
            for (int i = 0; i < AppData.session.userCache.Count; i++)
            {
                if (AppData.session.userCache[i].id == added.id)
                {
                    AppData.session.userCache[i] = added;
                    cached = true;
                    break;
                }
            }
            if (!cached) AppData.session.userCache.Add(added);

            AppData.session.currentUser.friends.Add(added.id);

            AppData.SaveCurrentData();
            return true;
        }

        /// <summary>
        /// TODO Prepares all of the submission's data, including audio recordings
        /// </summary>
        /// <param name="resultId"></param>
        /// <returns></returns>
        public static async Task<ResultPackage> FetchResultItemWithResources(int resultId)
        {
            ResultPackage ret = null;

            // TO TEST

            string extractPath = AppData.cacheDir + "/DL_" + resultId;

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
                zip = new ZipFile(File.OpenRead(ret.resultItem.ResourceUrl));

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
                    File.Delete(ret.resultItem.ResourceUrl);
                }
            }

            return ret;
        }

        /// <summary>
        /// Get a list of submissions from the server
        /// </summary>
        /// <returns></returns>
        public static async Task<IResultItem[]> FetchSubmittedList()
        {
            if (!AppData.CheckNetwork()) return null;

            return AppData.session.resultsToUpload.ToArray();
            //return await GetRequest<IResultItem[]>("GetSubmissions", "");
        }

        /// <summary>
        /// Polls the server for all available feedback for the given activity
        /// </summary>
        /// <param name="resultId"></param>
        /// <returns></returns>
        public static async Task<List<IFeedbackItem>> FetchFeedbackFor(int resultId)
        {
            if (!AppData.CheckNetwork()) return null;

            Dictionary<string, int> data = new Dictionary<string, int>();
            data.Add("submissionId", resultId);

            //return await GetRequest<IFeedbackItem[]>("GetFeedback", data);

            // TEMP
            List<IFeedbackItem> arr = new List<IFeedbackItem>();

            for(int i = 0; i < 12; i++)
            {
                int thisRand = AppData.rand.Next(0, 150);
                if(thisRand < 60)
                {
                    PercentageFeedback fb = new PercentageFeedback();
                    fb.Id = AppData.rand.Next(1000000);
                    fb.Title = "Stammering";
                    fb.Percentage = AppData.rand.Next(0, 100);
                    fb.Caption = (int)fb.Percentage + "% of users thought you stammered over the word \"sausage\"";
                    fb.ActivityId = "sossie";
                    arr.Add(fb);
                }
                else if(thisRand < 80)
                {
                    StarRatingFeedback fb = new StarRatingFeedback();
                    fb.Id = AppData.rand.Next(1000000);
                    fb.Title = "Your Rating";
                    fb.Caption = "This is your rating for something you did. Hopefully it's meaningful!";
                    fb.ActivityId = "sossie";
                    fb.Rating = (float)AppData.rand.Next(0, 10) / 2;
                    arr.Add(fb);
                }
                else if(thisRand < 110)
                {
                    CommentFeedback fb = new CommentFeedback();
                    fb.Id = AppData.rand.Next(1000000);
                    fb.Title = "A comment on a recording";
                    fb.Caption = "I really liked this recording - you should try to do it more like this in the future. Excellent work!";
                    fb.ActivityId = "sossie";
                    fb.Commenter = new User();
                    fb.Commenter.name = "Tom Hanks";
                    fb.Commenter.avatar = "http://media.nu.nl/m/m1mxjewa2jvj_sqr256.jpg/tom-hanks-produceert-filmversie-van-carole-king-musical.jpg";
                    arr.Add(fb);
                }
                else
                {
                    GraphFeedback fb = new GraphFeedback();
                    fb.Id = AppData.rand.Next(1000000);
                    fb.Title = "Your progress";
                    fb.Caption = "This is a graph showing some data!";
                    fb.ActivityId = "sossie";
                    fb.BottomAxisLength = 12;
                    fb.LeftAxisLength = 100;
                    fb.DataPoints = new TimeGraphPoint[12];

                    for (int j = 0; j < fb.DataPoints.Length; j++)
                    {
                        fb.DataPoints[j] = new TimeGraphPoint { 
                            XVal = DateTime.Now.AddDays(-j),
                            YVal = (Double)AppData.rand.Next(100)
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
        public static async Task<WikipediaResult> FetchWikiData()
        {
            try
            {
                string dateString = DateTime.Now.ToString("MMMM_dd,_yyyy");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://en.wikipedia.org/w/api.php?action=parse&&prop=text|images&page=Wikipedia:Today%27s_featured_article/" + dateString + "&format=json");

                string pageText = null;
                using (HttpWebResponse resp = (HttpWebResponse)(await request.GetResponseAsync()))
                {
                    using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                    {
                        pageText = await reader.ReadToEndAsync();
                    }
                }

                WikipediaResult res = JsonConvert.DeserializeObject<WikipediaResult>(pageText);

                if(res.parse.images != null && res.parse.images.Length > 0)
                {
                    // There's an image available from this page! Unfortunately we have to request the actual URL of the file separately
                    HttpWebRequest imgReq = (HttpWebRequest)WebRequest.Create("http://en.wikipedia.org/w/api.php?action=query&continue=&titles=Image:"+ res.parse.images[0] +"&prop=imageinfo&iiprop=url&format=json");
                    string imgText = null;
                    using (HttpWebResponse resp = (HttpWebResponse)(await imgReq.GetResponseAsync()))
                    {
                        using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                        {
                            imgText = await reader.ReadToEndAsync();
                        }
                    }

                    dynamic json = JsonConvert.DeserializeObject<dynamic>(imgText);

                    WikipediaResult imgRes = JsonConvert.DeserializeObject<WikipediaResult>(imgText);

                    // Store the image location in the main wiki result obj

                    foreach (KeyValuePair<string, QueryWikiInfo> info in imgRes.query.pages)
                    {
                        res.imageURL = await Utils.FetchLocalCopy(info.Value.imageInfo[0].url, typeof(WikipediaResult));
                        break;
                    }
                    
                }

                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(res.parse.text.HTML);

                pageText = "";

                // Scrape the HTML for all content text
                foreach(HtmlNode node in htmlDoc.DocumentNode.ChildNodes)
                {
                    if(node.Name == "p")
                    {
                        pageText += HttpUtility.HtmlDecode(node.InnerText) + "\n";
                    }
                }

                //Remove the (Full article...) text and all following it
                int index = pageText.IndexOf("(Full article...)");
                if (index > 0) pageText = pageText.Substring(0, index);

                res.parse = null;
                res.content = pageText;

                return res;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }
    }

    public class ResultPackage
    {
        // Includes addresses for unzipped recordings and scenario for easy access
        public Dictionary<string, string> resources;
        public ISpeechingActivityItem activity;
        public IResultItem resultItem;

        public ResultPackage(IResultItem result)
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
}