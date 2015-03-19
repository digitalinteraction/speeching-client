using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SpeechingCommon
{
    /// <summary>
    /// Useful functions that are likely to be needed across all platforms
    /// </summary>
    public class Utils
    {
        public enum UploadStage { Incomplete, Ready, Uploading, OnStorage, Finished };

        public static async Task LoadStringFromFile(string fileAddress, Action<string> callback)
        {
            callback(System.IO.File.ReadAllText(fileAddress));
        }

        public static long DirSize(string directoryAddress)
        {
            DirectoryInfo d = new DirectoryInfo(directoryAddress);
            return DirSize(d);
        }

        public static long DirSize(DirectoryInfo d)
        {
            long Size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                Size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                Size += DirSize(di);
            }
            return (Size);
        }

        /// <summary>
        /// Returns a local version of the linked file, downloading it if necessary
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <returns></returns>
        public static async Task<string> FetchLocalCopy(string remoteUrl)
        {
            string localIconPath = AppData.cacheDir + "/" + Path.GetFileName(remoteUrl);

            try
            {
                // Download the file if it isn't already stored locally
                if (!File.Exists(localIconPath))
                {
                    WebClient request = new WebClient();
                    await request.DownloadFileTaskAsync(
                        new Uri(remoteUrl),
                        localIconPath
                        );
                    request.Dispose();
                    request = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return localIconPath;
        }
    }
}