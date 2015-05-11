using System;
using System.IO;
using System.Threading.Tasks;
using PCLStorage;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace SpeechingShared
{
    /// <summary>
    /// Useful functions that are likely to be needed across all platforms
    /// </summary>
    public class Utils
    {
        public enum UploadStage { Incomplete, Ready, Uploading, OnStorage, Finished };

        private static Dictionary<string, SemaphoreSlim> semaphores = new Dictionary<string, SemaphoreSlim>();

        public static SemaphoreSlim GetSemaphore(string filename)
        {
            if (semaphores.ContainsKey(filename))
                return semaphores[filename];

            var semaphore = new SemaphoreSlim(1);
            semaphores[filename] = semaphore;
            return semaphore;
        }

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
            foreach (IFolder fo in fols)
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

            await Utils.GetSemaphore(filename).WaitAsync();

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
                    if (!AppData.CheckNetwork()) return null;

                    file = await AppData.cache.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

                    using (HttpClient client = new HttpClient()) // TODO get ModernHttpClient working
                    {
                        Uri address = new Uri(remoteUrl);
                        client.Timeout = TimeSpan.FromSeconds(30);

                        await client.GetAsync(address).ContinueWith(async (requestTask) =>
                            {
                                HttpResponseMessage response = requestTask.Result;
                                response.EnsureSuccessStatusCode();

                                Stream fileStream = await file.OpenAsync(FileAccess.ReadAndWrite);
                                await response.Content.CopyToAsync(fileStream);
                                fileStream.Dispose();
                            });

                        return file.Path;
                        
                    }
                }
            }
            catch (Exception e)
            {
                AppData.IO.PrintToConsole(e.Message);
                if(file != null)
                {
                    file.DeleteAsync().Start();
                }
                return null;
            }
            finally
            {
                Utils.GetSemaphore(filename).Release();
            }
            return localIconPath;
        }
    }
}