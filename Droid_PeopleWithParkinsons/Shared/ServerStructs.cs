using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;

namespace Droid_PeopleWithParkinsons.Shared
{
    public class User
    {
        public enum UserType { Patient, Therapist, Rater };

        public string id;
        public string name;
        public string avatar;
        public UserType userType;
    }

    public class UserTask
    {
        public string id;
        public string title;
        public string icon;
        public DateTime lastActive;
    }

    public class EventContent
    {
        public string type;
        public string visual;
        public string audio;
        public string text;
    }

    public class EventResponse
    {
        public string type;
        public string prompt;
        public string resultPath;
    }

    public class ScenarioEvent
    {
        public EventContent content;
        public EventResponse response;
    }

    public class Scenario
    {
        public string id;
        public User creator;
        public string title;
        public string resources;
        public ScenarioEvent[] events;
    }

    public class Utils 
    {
        public static async Task LoadStringFromFile(string fileAddress, Action<string> callback)
        {
            callback(System.IO.File.ReadAllText(fileAddress));
        }
    }
   
}