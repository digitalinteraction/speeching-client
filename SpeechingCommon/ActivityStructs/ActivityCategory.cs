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
            string localIconPath = AppData.cacheDir + "/" + Path.GetFileName(icon);

            try
            {
                // Download the icon if it isn't already stored locally
                if (!File.Exists(localIconPath))
                {
                    WebClient request = new WebClient();
                    await request.DownloadFileTaskAsync(
                        new Uri(icon),
                        localIconPath
                        );
                    request.Dispose();
                    request = null;
                }
            }
            catch (Exception e)
            {
                // We might be downloading into the same file simultaneously
                // Not actually an issue, as long as the icon path still gets reassigned to the local one below
                Console.WriteLine(e);
            }

            icon = localIconPath;
            runningDLs--;
        }
    }
}