using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System.Threading.Tasks;

namespace SpeechingCommon
{
    /// <summary>
    /// An object which holds activities of a given type or genre, as decided by the server
    /// </summary>
    public class ActivityCategory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public bool Recommended { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public SpeechingActivityItem[] Activities { get; set; }

        /// <summary>
        /// How many icon downloads are currently running
        /// </summary>
        public static int runningDLs;

        /// <summary>
        /// Download the icon for this category
        /// </summary>
        /// <returns></returns>
        public async Task DownloadIcon()
        {
            runningDLs++;
            
            string local = await Utils.FetchLocalCopy(Icon);

            Icon = (local != null)? local : Icon;
            runningDLs--;
        }
    }
}