using Android.App;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;
using SpeechingCommon;
using System;
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
                CreateData();
            }
            else
            {
                Toast.MakeText(this, "Loaded existing data", ToastLength.Short).Show();
                StartActivity(typeof(MainActivity));
            }
        }

        private async void CreateData()
        {
            try
            {
                 // FUDGE DATA - TODO
                AppData.session.currentUser.id = "aUserId";

                string jsonString = "{\r\n\t\"id\" : \"testScenario\",\r\n\t\"creator\" : {\r\n\t\t\"id\"\t: \"thatId\",\r\n\t\t\"name\"\t: \"Justin Time\"\r\n\t},\r\n\t\"title\" : \"Getting the Bus\",\r\n\t\"icon\"\t: \"http://www.survivingamsterdam.com/public/files/e96fc9baf228c0cb8d210a1768995bb1.png\",\r\n\t\"resources\" : \"https://www.dropbox.com/s/0h2f8pyrh6xte3s/bus.zip?raw=1\",\r\n\t\"events\" : [\r\n\t{\r\n\t\t\"id\" : \"sc1ev1\",\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"AUDIO\",\r\n\t\t\t\"visual\" : \"driver.jpg\",\r\n\t\t\t\"audio\"\t : \"hello.mp3\",\r\n\t\t\t\"text\"\t : \"Hello! Where would you like to go today?\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"promptedSpeech\",\r\n\t\t\t\"prompt\" : \"Hello, please may I have a return ticket to the train station?\"\r\n\t\t}\r\n\t},\r\n\t{\r\n\t\t\"id\" : \"sc1ev2\",\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"AUDIO\",\r\n\t\t\t\"visual\" : \"driver.jpg\",\r\n\t\t\t\"audio\"\t : \"thanks.mp3\",\r\n\t\t\t\"text\"\t : \"No problem at all, looks like you have a valid card. Take a seat!\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"promptedSpeech\",\r\n\t\t\t\"prompt\" : \"Thank you. Have a good day.\"\r\n\t\t}\r\n\t},\r\n\t{\r\n\t\t\"id\" : \"sc1ev3\",\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"TEXT\",\r\n\t\t\t\"visual\" : \"oldwoman.jpg\",\r\n\t\t\t\"audio\"\t : null,\r\n\t\t\t\"text\"\t : \"You sit next to an old woman, who asks what your plans are for the day. Greet her and explain how you're catching a train to the seaside.\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"freeformSpeech\",\r\n\t\t\t\"prompt\" : null\r\n\t\t}\r\n\t}\r\n\t]\r\n}";
                await AppData.session.ProcessNewScenario(jsonString, false);

                jsonString = "{\r\n\t\"id\" : \"testScenario2\",\r\n\t\"creator\" : {\r\n\t\t\"id\"\t: \"thatId\",\r\n\t\t\"name\"\t: \"Justin Time\"\r\n\t},\r\n\t\"title\" : \"Ordering a Pizza\",\r\n\t\"icon\"\t: \"http://img3.wikia.nocookie.net/__cb20131231163822/cardfight/images/6/6f/Pizza_slice_combo_clipart.png\",\r\n\t\"resources\" : \"https://www.dropbox.com/s/8gt7pqh6zq6p18h/pizza.zip?raw=1\",\r\n\t\"events\" : [\r\n\t{\r\n\t\t\"id\" : \"sc2ev1\",\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"AUDIO\",\r\n\t\t\t\"visual\" : \"phone.jpg\",\r\n\t\t\t\"audio\"\t : \"hello.mp3\",\r\n\t\t\t\"text\"\t : \"You are ordering pizza over the phone for both yourself and a friend who has a gluten alergy.\\n'Tony's Pizza Parlour, how can I help you?'\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"promptedSpeech\",\r\n\t\t\t\"prompt\" : \"Hello, can I order a pizza please?\"\r\n\t\t}\r\n\t},\r\n\t{\r\n\t\t\"id\" : \"sc2ev2\",\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"AUDIO\",\r\n\t\t\t\"visual\" : \"pizza.jpg\",\r\n\t\t\t\"audio\"\t : \"order1.mp3\",\r\n\t\t\t\"text\"\t : \"Of course! What kind would you like?\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"freeformSpeech\",\r\n\t\t\t\"prompt\" : \"Describe your favourite pizza\"\r\n\t\t}\r\n\t},\r\n\t{\r\n\t\t\"id\" : \"sc2ev3\",\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"AUDIO\",\r\n\t\t\t\"visual\" : \"pizza.jpg\",\r\n\t\t\t\"audio\"\t : \"order2.mp3\",\r\n\t\t\t\"text\"\t : \"That sounds delicious! Would you like anything else?\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"freeformSpeech\",\r\n\t\t\t\"prompt\" : \"Describe another kind of pizza, but make sure it's gluten free!\"\r\n\t\t}\r\n\t},\r\n\t{\r\n\t\t\"id\" : \"sc2ev4\",\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"AUDIO\",\r\n\t\t\t\"visual\" : \"making.jpg\",\r\n\t\t\t\"audio\"\t : \"end.mp3\",\r\n\t\t\t\"text\"\t : \"No problem at all, we can do that. See you soon!\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"promptedSpeech\",\r\n\t\t\t\"prompt\" : \"Thank you, see you later.\"\r\n\t\t}\r\n\t},\r\n\t]\r\n}";
                await AppData.session.ProcessNewScenario(jsonString, false);

                //jsonString = "{\r\n\t\"id\" : \"testScenario2\",\r\n\t\"creator\" : {\r\n\t\t\"id\"\t: \"thatId\",\r\n\t\t\"name\"\t: \"Justin Time\"\r\n\t},\r\n\t\"title\" : \"Choice test\",\r\n\t\"icon\"\t: \"http://img3.wikia.nocookie.net/__cb20131231163822/cardfight/images/6/6f/Pizza_slice_combo_clipart.png\",\r\n\t\"resources\" : \"https://www.dropbox.com/s/8gt7pqh6zq6p18h/pizza.zip?raw=1\",\r\n\t\"events\" : [\r\n\t{\r\n\t\t\"id\" : \"sc2ev1\",\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"AUDIO\",\r\n\t\t\t\"visual\" : \"phone.jpg\",\r\n\t\t\t\"audio\"\t : \"hello.mp3\",\r\n\t\t\t\"text\"\t : \"You are ordering pizza over the phone for both yourself and a friend who has a gluten alergy.\\n'Tony's Pizza Parlour, how can I help you?'\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"choice\",\r\n\t\t\t\"prompt\" : \"Choose the spaghetti from the shelf.\",\r\n\t\t\t\"choice1\" : \"pizza.jpg\",\r\n\t\t\t\"choice2\"\t: \"making.jpg\"\r\n\t\t}\r\n\t},\r\n\t{\r\n\t\t\"id\" : \"sc2ev2\",\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"AUDIO\",\r\n\t\t\t\"visual\" : \"pizza.jpg\",\r\n\t\t\t\"audio\"\t : \"order1.mp3\",\r\n\t\t\t\"text\"\t : \"Of course! What kind would you like?\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"freeformSpeech\",\r\n\t\t\t\"prompt\" : \"Describe your favourite pizza\"\r\n\t\t}\r\n\t},\r\n\t{\r\n\t\t\"id\" : \"sc2ev3\",\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"AUDIO\",\r\n\t\t\t\"visual\" : \"pizza.jpg\",\r\n\t\t\t\"audio\"\t : \"order2.mp3\",\r\n\t\t\t\"text\"\t : \"That sounds delicious! Would you like anything else?\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"freeformSpeech\",\r\n\t\t\t\"prompt\" : \"Describe another kind of pizza, but make sure it's gluten free!\"\r\n\t\t}\r\n\t},\r\n\t{\r\n\t\t\"id\" : \"sc2ev4\",\r\n\t\t\"content\" : {\r\n\t\t\t\"type\"\t : \"AUDIO\",\r\n\t\t\t\"visual\" : \"making.jpg\",\r\n\t\t\t\"audio\"\t : \"end.mp3\",\r\n\t\t\t\"text\"\t : \"No problem at all, we can do that. See you soon!\"\r\n\t\t},\r\n\t\t\"response\" : {\r\n\t\t\t\"type\"\t: \"promptedSpeech\",\r\n\t\t\t\"prompt\" : \"Thank you, see you later.\"\r\n\t\t}\r\n\t},\r\n\t]\r\n}";
                //await AppData.session.ProcessNewScenario(jsonString, false);

                AppData.SaveCurrentData();
                Toast.MakeText(this, "Created data", ToastLength.Short).Show();

                StartActivity(typeof(MainActivity));
            }
            catch(Exception e)
            {
                Console.WriteLine("Err: " + e);
            }
           
        }
    }
}