using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace SpeechingCommon
{
    public abstract class SpeechingActivityItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        //[ManyToOne(CascadeOperations = CascadeOperation.All)]
        [Ignore]
        public User Creator { get; set; }
        public string Title { get; set; }
        public string Resource { get; set; }
        public string Icon { get; set; }

        [ForeignKey(typeof(ActivityCategory))]
        public int ActivityCategoryId { get; set; }

        [ManyToOne(CascadeOperations = CascadeOperation.All)]
        public ActivityCategory Category { get; set; }
    }
}