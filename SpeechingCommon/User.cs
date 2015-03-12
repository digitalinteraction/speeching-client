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

        public int id;
        public string name;
        public string avatar;
        public UserType userType;
        public FriendStatus status;
        public List<int> friends;

        public User()
        {
            id = 7041992;
            friends = new List<int>();
        }
    }
}