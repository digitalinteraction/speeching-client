using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
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

        [PrimaryKey, AutoIncrement]
        public int localId { get; set; }

        public string id {get; set;}

        //[OneToMany(CascadeOperations = CascadeOperation.All)]
        //public List<SpeechingActivityItem> CreatedActivities { get; set; }

        public string nickname { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
        public string email { get; set; }
        public UserType userType { get; set; }
        public FriendStatus status { get; set; }
        //public List<string> friends;

        public User()
        {
            //friends = new List<string>();
        }
    }
}