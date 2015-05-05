using System;
using System.IO;
using ModernHttpClient;
using System.Threading.Tasks;
using PCLStorage;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;

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

            localIconPath = AppData.cache.Path + "/" + filename;
            IFile file = null;

            try
            {
                ExistenceCheckResult checkRes = await AppData.cache.CheckExistsAsync(filename);
                exists =  (checkRes == ExistenceCheckResult.FileExists);

                if (ownerType == typeof(WikipediaResult) && exists)
                {
                    try
                    {
                        IFile existing = await AppData.cache.GetFileAsync(filename);
                        await existing.DeleteAsync();
                        exists = false;
                    }
                    catch(Exception e)
                    {
                        throw e;
                    }
                }

                // Download the file if it isn't already stored locally
                if (!exists)
                {
                    file = await AppData.cache.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

                    using (HttpClient client = new HttpClient())//new NativeMessageHandler()))
                    {
                        Uri baseAddress = new Uri(remoteUrl);
                        client.BaseAddress = new Uri(baseAddress.Scheme + "://" + baseAddress.Authority);

                        try
                        {
                            HttpResponseMessage response = await client.GetAsync(baseAddress);
                            if (response.IsSuccessStatusCode)
                            {

                                Stream fileStream = await file.OpenAsync(FileAccess.ReadAndWrite);
                                Stream contentStream = await response.Content.ReadAsStreamAsync();
                                contentStream.CopyTo(fileStream);

                                return file.Path;
                            }
                            else
                            {
                                string msg = await response.Content.ReadAsStringAsync();
                                throw new Exception(msg);
                            }
                        }
                        catch (Exception except)
                        {
                            throw except;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if(file != null)
                {
                    file.DeleteAsync();
                }
            }

            return localIconPath;
        }
    }
}