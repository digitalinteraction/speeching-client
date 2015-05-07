using System.Net.Http;
using System.Threading.Tasks;

namespace SpeechingShared
{
    /// <summary>
    /// Portable Class Libraries can't access System.IO so has to be done via an interface 
    /// and then implemented on platform-specific projects
    /// </summary>
    public interface IPlatformSpecifics
    {
        Task CleanDirectory(string path, int maxMb);
    }
}
