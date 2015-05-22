using System.Collections.Generic;

namespace SpeechingShared
{
    /// <summary>
    /// A user of the Speeching service
    /// </summary>
    public class User
    {
        public enum UserType { Patient, Therapist, Rater };
        public enum FriendStatus { Accepted, Denied, Sent, Received };

        public string Id;
        public string Nickname;
        public string Name;
        public string Avatar;
        public string Email;
        public int Key;
        public UserType userType;
        public FriendStatus Status;
        public List<string> Friends;

        public User()
        {
            Friends = new List<string>();
        }
    }
}