using System.Collections.Generic;

namespace SpeechingCommon
{
    /// <summary>
    /// A user of the Speeching service
    /// </summary>
    public class User
    {
        public enum UserType { Patient, Therapist, Rater };
        public enum FriendStatus { Accepted, Denied, Sent, Received };

        public string id;
        public string nickname;
        public string name;
        public string avatar;
        public string email;
        public UserType userType;
        public FriendStatus status;
        public List<string> friends;

        public User()
        {
            friends = new List<string>();
        }
    }
}