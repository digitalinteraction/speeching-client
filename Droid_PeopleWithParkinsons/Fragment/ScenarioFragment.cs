using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Droid_PeopleWithParkinsons.Shared;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Droid_PeopleWithParkinsons
{
    public class ScenarioFragment : Fragment
    {
        private Scenario scenario;

        // Scenario title screen
        private RelativeLayout titleLayout;
        private TextView scenarioTitle;
        private TextView authorName;
        private Button startButton;

        // Scenario event screen
        private RelativeLayout eventLayout;
        private ImageView eventImage; //TODO make this a sub-fragment to allow switching between image and video
        private AutoResizeTextView eventTranscript;
        private AutoResizeTextView eventPrompt;
        private Button continueButton;

        Dictionary<string, string> resources;
        ProgressDialog progress;
        string documentsPath;
        string localResourcesDirectory;
        string localZipPath;

        private int currIndex = -1;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // TODO
            string jsonString = "{\r\n\t\"id\" : \"testScenario\",\r\n\t\"creator\" : {\r\n\t\t\"id\"\t: \"thatId\",\r\n\t\t\"name\"\t: \"Justin Time\"\r\n\t},\r\n\t\"title\" : \"Getting the Bus\",\r\n\t\"resources\" : \"https://www.dropbox.com/s/0h2f8pyrh6xte3s/bus.zip?raw=1\",\r\n\t\"events\" : [\r\n\t{\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"AUDIO\",\r\n\t\t\t\"visual\" : \"driver.jpg\",\r\n\t\t\t\"audio\"\t : \"greeting.mp3\",\r\n\t\t\t\"text\"\t : \"Hello! Where would you like to go today?\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"promptedSpeech\",\r\n\t\t\t\"prompt\" : \"Hello, please may I have a return ticket to the train station?\"\r\n\t\t}\r\n\t},\r\n\t{\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"VIDEO\",\r\n\t\t\t\"visual\" : \"driver.jpg\",\r\n\t\t\t\"audio\"\t : null,\r\n\t\t\t\"text\"\t : \"No problem at all, looks like you have a valid card. Take a seat!\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"promptedSpeech\",\r\n\t\t\t\"prompt\" : \"Thank you. Have a good day.\"\r\n\t\t}\r\n\t},\r\n\t{\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"TEXT\",\r\n\t\t\t\"visual\" : \"oldwoman.jpg\",\r\n\t\t\t\"audio\"\t : null,\r\n\t\t\t\"text\"\t : \"You sit next to an old woman, who asks what your plans are for the day. Greet her and explain how you're catching a train to the seaside.\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"freeformSpeech\",\r\n\t\t\t\"prompt\" : null\r\n\t\t}\r\n\t}\r\n\t]\r\n}";
            scenario = JsonConvert.DeserializeObject<Scenario>(jsonString);
            string scenarioFormatted = scenario.title.Replace(" ", String.Empty);


            documentsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath + "/speeching";
            localResourcesDirectory = documentsPath + "/" + scenarioFormatted;
            try
            {
                Directory.CreateDirectory(documentsPath);
                if (Directory.Exists(localResourcesDirectory))
                {
                    Directory.Delete(localResourcesDirectory, true);
                }
                Directory.CreateDirectory(localResourcesDirectory);
            }
            catch(Exception except)
            {
                Console.WriteLine(except.Message);
            }

            localZipPath = Path.Combine(documentsPath, scenarioFormatted + ".zip");

            try
            {
                DownloadData();
            }
            catch(Exception except)
            {
                Console.Write(except);
            }
        }

        private async void DownloadData()
        {
            Activity.RunOnUiThread(() => progress = ProgressDialog.Show(Activity, "Please Wait", "Downloading data to " + localZipPath, true));
            resources = new Dictionary<string, string>();

            WebClient request = new WebClient();
            await request.DownloadFileTaskAsync(
                new Uri(scenario.resources),
                localZipPath
                );
            request.Dispose();
            request = null;

            Activity.RunOnUiThread(() => progress.SetMessage("Unpacking data at " + localZipPath));

            ZipFile zip = null; 
            try
            {
                //Unzip the downloaded file
                zip = new ZipFile(File.OpenRead(localZipPath));

                foreach(ZipEntry entry in zip)
                {
                    string filename = Path.Combine(localResourcesDirectory, entry.Name);
                    byte[] buffer = new byte[4096];
                    Stream zipStream = zip.GetInputStream(entry);
                    using(FileStream streamWriter = File.Create(filename))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                    resources.Add(entry.Name, filename);
                }
                
            }
            finally
            {
                if(zip != null)
                {
                    zip.IsStreamOwner = true;
                    zip.Close();
                }
            }
           
            Activity.RunOnUiThread(() => progress.Hide());
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.ScenarioFragment, container, false);

            titleLayout = view.FindViewById<RelativeLayout>(Resource.Id.scenarioTitleLayout);
            scenarioTitle = view.FindViewById<TextView>(Resource.Id.scenarioTitle);
            authorName = view.FindViewById<TextView>(Resource.Id.scenarioAuthor);
            startButton = view.FindViewById<Button>(Resource.Id.scenarioStartBtn);
            startButton.Click += delegate(object sender, EventArgs e)
            {
                titleLayout.Visibility = ViewStates.Gone;
                eventLayout.Visibility = ViewStates.Visible;
                ShowNextEvent();
            };          

            scenarioTitle.Text = scenario.title;
            authorName.Text = scenario.creator.name;

            eventLayout = view.FindViewById<RelativeLayout>(Resource.Id.scenarioEventLayout);
            eventTranscript = view.FindViewById<AutoResizeTextView>(Resource.Id.scenarioText);
            eventPrompt = view.FindViewById<AutoResizeTextView>(Resource.Id.scenarioPrompt);
            eventImage = view.FindViewById<ImageView>(Resource.Id.scenarioImage);
            continueButton = view.FindViewById<Button>(Resource.Id.scenarioProgressBtn);
            continueButton.Click += delegate(object sender, EventArgs e)
            {
                ShowNextEvent();
            };

            titleLayout.Visibility = ViewStates.Visible;
            eventLayout.Visibility = ViewStates.Gone;

            return view;
        }

        private void ShowNextEvent()
        {
            currIndex++;

            if (currIndex >= scenario.events.Length)
            {
                return;
            }

            if(currIndex + 1 >= scenario.events.Length)
            {
                continueButton.Text = "Finish";
            }

            eventTranscript.Text = scenario.events[currIndex].content.text;
            
            if(scenario.events[currIndex].response.type == "freeformSpeech")
            {
                eventPrompt.Text = "";
            }
            else
            {
                eventPrompt.Text = scenario.events[currIndex].response.prompt;
            }

            // Load the visual media
            string key = scenario.events[currIndex].content.visual;
            if(key != null && resources.ContainsKey(key))
            {
                eventImage.SetImageURI(Android.Net.Uri.FromFile( new Java.IO.File( resources[key]) ));
            }
        }
    }
}