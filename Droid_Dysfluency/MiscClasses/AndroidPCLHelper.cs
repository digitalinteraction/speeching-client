using System;
using System.IO;
using System.Linq;
using PCLStorage;
using SpeechingShared;

namespace Droid_Dysfluency
{
    public class AndroidPCLHelper : IPlatformSpecifics
    {
        public async void CleanDirectory(IFolder folder, float maxMb)
        {
            string path = folder.Path;
            long max = (long) (maxMb*1000000);
            long size = await Utils.DirSize(folder, GetFolderSize, GetFileSize);

            if (size < max) return;

            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] allFiles = di.GetFiles();

            // Sort by file size
            Array.Sort(allFiles, delegate(FileInfo a, FileInfo b) { return b.Length.CompareTo(a.Length); });

            FileInfo[] biggest = new FileInfo[allFiles.Length/2];

            for (int i = 0; i < biggest.Length; i++)
            {
                biggest[i] = allFiles[i];
            }

            // Sort by last accessed
            Array.Sort(biggest,
                delegate(FileInfo a, FileInfo b) { return a.LastAccessTime.CompareTo(b.LastAccessTime); });

            // Array should now be the biggest files, in order of date last accessed (earliest first)
            // Delete one by one until under the limit
            int count = 0;
            while (size >= max && count < biggest.Length)
            {
                try
                {
                    // Remove reference 
                    string thisKey =
                        AppData.Session.PlacesPhotos.FirstOrDefault(x => x.Value == biggest[count].FullName).Key;

                    if (thisKey != null)
                    {
                        AppData.Session.PlacesPhotos.Remove(thisKey);
                    }

                    size -= biggest[count].Length;

                    File.Delete(biggest[count].FullName);
                    count++;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            AppData.SaveCurrentData();
        }

        public void PrintToConsole(string message)
        {
            Console.WriteLine(message);
        }

        private long GetFolderSize(string path)
        {
            string[] allFiles = Directory.GetFiles(path, "*.*");

            long totalBytes = 0;
            foreach (string name in allFiles)
            {
                totalBytes += GetFileSize(name);
            }

            return totalBytes;
        }

        private long GetFileSize(string path)
        {
            FileInfo info = new FileInfo(path);
            return info.Length;
        }
    }
}