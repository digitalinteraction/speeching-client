using PCLStorage;
using SpeechingShared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Windows_Speeching
{
    class Win8PCLHelper : IPlatformSpecifics
    {
        private async Task<long> GetFolderSize(string path)
        {
            StorageFolder directory = await StorageFolder.GetFolderFromPathAsync(path);
            IReadOnlyList<StorageFile> allFiles = await directory.GetFilesAsync();

            long totalBytes = 0;
            foreach (StorageFile file in allFiles)
            {
                totalBytes += await GetFileSize(file);
            }

            return totalBytes;
        }

        private async Task<long> GetFileSize(StorageFile file)
        {
            BasicProperties props = await file.GetBasicPropertiesAsync();
            return (long)props.Size;
        }

        public async void CleanDirectory(IFolder folder, float maxMb)
        {
            // Do nothing for now

        }

        public void PrintToConsole(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
