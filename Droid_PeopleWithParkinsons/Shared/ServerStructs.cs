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
}