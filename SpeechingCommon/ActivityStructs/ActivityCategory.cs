using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SpeechingCommon
{
    public class ActivityCategory
    {
        public string id;
        public string title;
        public string icon;
        public bool recommended;
        public ISpeechingActivityItem[] activities;

        public static int runningDLs = 0;

        public async Task DownloadIcon()
        {
            runningDLs++;
            
            string local = await Utils.FetchLocalCopy(icon);

            icon = (local != null)? local : icon;
            runningDLs--;
        }
    }
}