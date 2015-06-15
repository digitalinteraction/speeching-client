using System.Threading.Tasks;

namespace SpeechingShared
{
    public class Guide : ISpeechingPracticeActivity
    {
        public Page[] Guides;
        public int Id { get; set; }
        public User Creator { get; set; }
        public string Title { get; set; }
        public string Resource { get; set; }
        public string Icon { get; set; }
        public string LocalIcon { get; set; }

        public async Task<bool> PrepareIcon()
        {
            LocalIcon = await Utils.FetchLocalCopy(Icon);

            return LocalIcon != null;
        }

        public struct Page
        {
            public string MediaLocation;
            public string Text;
        };
    }
}