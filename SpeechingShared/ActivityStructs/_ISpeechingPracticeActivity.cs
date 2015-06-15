using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace SpeechingShared
{
    public interface ISpeechingPracticeActivity
    {
        int Id { get; set; }
        User Creator { get; set; }
        string Title { get; set; }
        string Resource { get; set; }
        string Icon { get; set; }
        string LocalIcon { get; set; }

        Task<bool> PrepareIcon();
    }
}