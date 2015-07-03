using System.Collections.Generic;

namespace SpeechingShared
{
    /// <summary>
    /// A user of the Speeching service
    /// </summary>
    public class User
    {
        public enum AppType { None = 0, Speeching = 1, Fluent = 2};
        public enum UserType { Patient, Therapist, Rater };
        public enum FriendStatus { Accepted, Denied, Sent, Received };

        public string Nickname;
        public string Name;
        public string Avatar;
        public string Email;
        public string Key;
        public UserType userType;
        public FriendStatus Status;
        public List<string> Friends;
        public string IdToken;
        public AppType App;

        public User()
        {
            Friends = new List<string>();
        }
    }
}