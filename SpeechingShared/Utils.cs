using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using PCLStorage;
using System.Collections.Generic;
using System.Net.Http;

namespace SpeechingShared
{
    /// <summary>
    /// Useful functions that are likely to be needed across all platforms
    /// </summary>
    public class Utils
    {
        public enum UploadStage { Incomplete, Ready, Uploading, OnStorage, Finished };

        public static async Task<long> DirSize(IFolder d, Func<string, long> GetFolderSize, Func<string, long> GetFileSize)
        {
            long Size = 0;
            // Add file sizes.
            IList<IFile> fis = await d.GetFilesAsync();
            foreach (IFile fi in fis)
            {
                Size += GetFileSize(fi.Path);
            }
            // Add subdirectory sizes.
            IList<IFolder> fols = await d.GetFoldersAsync();
            foreach (IFile fo in fols)
            {
                Size += GetFolderSize(fo.Path);
            }
            return (Size);
        }

        /// <summary>
        /// Returns a local version of the linked file, downloading it if necessary
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <returns></returns>
        public static async Task<string> FetchLocalCopy(string remoteUrl, Type ownerType = null)
        {
            string localIconPath;
            bool exists = false;

            string filename = (ownerType == typeof(WikipediaResult))? "wikiImage.jpg" : Path.GetFileName(remoteUrl);

            localIconPath = AppData.cache.Path + filename;
            
            exists = await AppData.cache.CheckExistsAsync(filename) == ExistenceCheckResult.FileExists;

            if (ownerType == typeof(WikipediaResult) && exists)
            {
                IFile existing = await AppData.cache.GetFileAsync(filename);
                await existing.DeleteAsync();
                exists = false;
            }

            try
            {
                // Download the file if it isn't already stored locally
                if (!exists)
                {
                    IFile file = await AppData.cache.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

                    using(HttpClient client = new HttpClient())
                    {
                        using(HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, remoteUrl))
                        {
                            using(Stream content =  await (await client.SendAsync(req)).Content.ReadAsStreamAsync())
                            {
                                using (Stream filestream = await file.OpenAsync(FileAccess.ReadAndWrite))
                                {
                                    await content.CopyToAsync(filestream);
                                }
                            }
                                
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return localIconPath;
        }
    }
}