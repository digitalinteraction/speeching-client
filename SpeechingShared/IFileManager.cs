using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechingShared
{
    /// <summary>
    /// Portable Class Libraries can't access System.IO so has to be done via an interface 
    /// and then implemented on platform-specific projects
    /// </summary>
    public interface IFileManager
    {
        Task CleanDirectory(string path, int maxMb);
    }
}
