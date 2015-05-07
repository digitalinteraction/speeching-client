using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SpeechingShared
{
    /// <summary>
    /// An object which holds activities of a given type or genre, as decided by the server
    /// </summary>
    public class ActivityCategory
    {
        public string id;
        public string title;
        public string icon;
        public string localIcon;
        public bool recommended;
        public ISpeechingActivityItem[] activities;

        /// <summary>
        /// Download the icon for this category
        /// </summary>
        /// <returns></returns>
        public async Task<bool> PrepareIcon()
        {
            localIcon = await Utils.FetchLocalCopy(icon);

            return localIcon != null;
        }
    }
}