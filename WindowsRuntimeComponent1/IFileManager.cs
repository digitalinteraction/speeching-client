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
        bool DirectoryExists(string path);
        void CreateDirectory(string path);
        void DeleteDirectory(string path, bool recursive);
        string[] GetFiles(string directoryPath);
        Task CleanDirectory(string path, int maxMb);

        bool FileExists(string path);
        string ReadStringFromFile(string path);
        Stream OpenFileStream(string path);
        void WriteToFile(string path, string content);
        void WriteToFile(string path, byte[] bytes);
        Stream CreateFile(string path);
        void DeleteFile(string path);
    }
}
