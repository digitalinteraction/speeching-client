using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SpeechingShared;
using System.Threading.Tasks;
using PCLStorage;
using System.IO;

namespace DroidSpeeching
{
    public class AndroidPCLHelper : IPlatformSpecifics
    {
        private long GetFolderSize(string path)
        {
            string[] allFiles = Directory.GetFiles(path, "*.*");

            long totalBytes = 0;
            foreach (string name in allFiles)
            {
                GetFileSize(name);
            }

            return totalBytes;
        }

        private long GetFileSize(string path)
        {
            FileInfo info = new FileInfo(path);
            return info.Length;
        }

        public async void CleanDirectory(IFolder folder, float maxMb)
        {
            string path = folder.Path;
            long max = (long)(maxMb * 1000000);
            long size = await Utils.DirSize(folder, GetFolderSize, GetFileSize);

            if (size < max) return;

            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] allFiles = di.GetFiles();

            // Sort by file size
            Array.Sort<FileInfo>(allFiles, delegate(FileInfo a, FileInfo b)
            {
                return b.Length.CompareTo(a.Length);
            });

            FileInfo[] biggest = new FileInfo[allFiles.Length / 2];

            for (int i = 0; i < biggest.Length; i++)
            {
                biggest[i] = allFiles[i];
            }

            // Sort by last accessed
            Array.Sort<FileInfo>(biggest, delegate(FileInfo a, FileInfo b)
            {
                return a.LastAccessTime.CompareTo(b.LastAccessTime);
            });

            // Array should now be the biggest files, in order of date last accessed (earliest first)
            // Delete one by one until under the limit
            int count = 0;
            while (size >= max && count < biggest.Length)
            {
                try
                {
                    // Remove reference 
                    string thisKey = AppData.session.placesPhotos.FirstOrDefault(x => x.Value == biggest[count].FullName).Key;

                    if (thisKey != null)
                    {
                        AppData.session.placesPhotos.Remove(thisKey);
                    }

                    size -= biggest[count].Length;

                    File.Delete(biggest[count].FullName);
                    count++;
                }
                catch (Exception e)
                {
                    throw e;
                    break;
                }
            }
            AppData.SaveCurrentData();

        }
    } 
}