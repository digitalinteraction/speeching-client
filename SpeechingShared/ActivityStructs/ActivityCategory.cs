using System.Threading.Tasks;

namespace SpeechingShared
{
    /// <summary>
    /// An object which holds Activities of a given type or genre, as decided by the server
    /// </summary>
    public class ActivityCategory
    {
        public ISpeechingPracticeActivity[] Activities;
        public string Icon;
        public string Id;
        public string LocalIcon;
        public bool Recommended;
        public string Title;

        /// <summary>
        /// Download the icon for this category
        /// </summary>
        /// <returns></returns>
        public async Task<bool> PrepareIcon()
        {
            LocalIcon = await Utils.FetchLocalCopy(Icon);

            return LocalIcon != null;
        }
    }
}