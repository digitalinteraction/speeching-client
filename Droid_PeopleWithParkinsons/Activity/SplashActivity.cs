using Android.App;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;
using SpeechingCommon;
using System.Collections.Generic; 

namespace Droid_PeopleWithParkinsons
{
    [Activity(Theme = "@style/Theme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            AppData.cacheDir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath + "/speeching";
            bool loaded = AppData.TryLoadExistingData();

            if(!loaded)
            {
                // FUDGE DATA - TODO
                AppData.session.currentUser.id = "aUserId";

                string jsonString = "{\r\n\t\"id\" : \"testScenario\",\r\n\t\"creator\" : {\r\n\t\t\"id\"\t: \"thatId\",\r\n\t\t\"name\"\t: \"Justin Time\"\r\n\t},\r\n\t\"title\" : \"Getting the Bus\",\r\n\t\"resources\" : \"https://www.dropbox.com/s/0h2f8pyrh6xte3s/bus.zip?raw=1\",\r\n\t\"events\" : [\r\n\t{\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"AUDIO\",\r\n\t\t\t\"visual\" : \"driver.jpg\",\r\n\t\t\t\"audio\"\t : \"hello.mp3\",\r\n\t\t\t\"text\"\t : \"Hello! Where would you like to go today?\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"promptedSpeech\",\r\n\t\t\t\"prompt\" : \"Hello, please may I have a return ticket to the train station?\"\r\n\t\t}\r\n\t},\r\n\t{\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"VIDEO\",\r\n\t\t\t\"visual\" : \"driver.jpg\",\r\n\t\t\t\"audio\"\t : \"thanks.mp3\",\r\n\t\t\t\"text\"\t : \"No problem at all, looks like you have a valid card. Take a seat!\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"promptedSpeech\",\r\n\t\t\t\"prompt\" : \"Thank you. Have a good day.\"\r\n\t\t}\r\n\t},\r\n\t{\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"TEXT\",\r\n\t\t\t\"visual\" : \"oldwoman.jpg\",\r\n\t\t\t\"audio\"\t : null,\r\n\t\t\t\"text\"\t : \"You sit next to an old woman, who asks what your plans are for the day. Greet her and explain how you're catching a train to the seaside.\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"freeformSpeech\",\r\n\t\t\t\"prompt\" : null\r\n\t\t}\r\n\t}\r\n\t]\r\n}";

                for (int i = 0; i < 13; i++)
                {
                    Scenario scenario = JsonConvert.DeserializeObject<Scenario>(jsonString);
                    scenario.title += "_" + i;
                    scenario.id = "scenario_" + i;
                    AppData.session.scenarios.Add(scenario);
                }

                AppData.SaveCurrentData();
                Toast.MakeText(this, "Created data", ToastLength.Short).Show();
            }
            else
            {
                Toast.MakeText(this, "Loaded existing data", ToastLength.Short).Show();
            }
           
            StartActivity(typeof(MainActivity));
        }
    }
}