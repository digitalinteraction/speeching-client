using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SpeechingCommon
{
    /// <summary>
    /// An object which holds activities of a given type or genre, as decided by the server
    /// </summary>
    public class ActivityCategory
    {
        public string id;
        public string title;
        public string icon;
        public bool recommended;
        public ISpeechingActivityItem[] activities;

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
            
            string local = await Utils.FetchLocalCopy(icon);

            icon = (local != null)? local : icon;
            runningDLs--;
        }
    }
}