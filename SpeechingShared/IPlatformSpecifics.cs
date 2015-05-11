using PCLStorage;
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
        void CleanDirectory(IFolder path, float maxMb);
        void PrintToConsole(string message);
    }
}
