using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Droid_PeopleWithParkinsons
{
    class AudioFileManager
    {
        public static string fileExtension = ".mp3";
        private const string audioPath = "/audio/";
        private const string backgroundAudioPath = "/bgaudio/";

        #if __ANDROID__
        public static string RootAudioDirectory
        {
            get
            {
                string dir = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioPath);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                
                return string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioPath);
            }
        }

        public static string RootBackgroundAudioPath
        {
            get
            {
                string dir = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Personal), backgroundAudioPath);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Personal), backgroundAudioPath, "bg", fileExtension);
            }
        }
        #endif

        private static int fileCount = 0;
        private static string fileName { get { return string.Concat(fileCount.ToString(), fileExtension); } }
        private static string filePath { get { return string.Concat(RootAudioDirectory, fileName); } }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns a generated folder path and file name (without extension) for a new audio recording</returns>
        public static string GetNewAudioFilePath()
        {
            // Get file name via unique integer ID.
            fileCount = 1;
            
            // Loop until free file found
            while (File.Exists(filePath))
            {
                ++fileCount;
            }

            return filePath;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>Total number of files in Audio directory</returns>
        public static int GetNumAudioFiles()
        {
            return Directory.GetFiles(RootAudioDirectory).Length;
        }

        /// <summary>
        /// Deletes given file.
        /// </summary>
        /// <param name="path">Full path to file</param>
        public static void DeleteFile(string path)
        {
            File.Delete(path);
        }

        /// <summary>
        /// Delete individual audio file by index.
        /// </summary>
        /// <param name="index">Filename without extension</param>
        public static void DeleteFileByIndex(int index)
        {
            string dPath = string.Concat(RootAudioDirectory, index.ToString(), fileExtension);
            File.Delete(dPath);
        }

        /// <summary>
        /// Deletes all files in the audio directory.
        /// </summary>
        public static void DeleteAll()
        {
            Directory.Delete(RootAudioDirectory, true);
        }

        /// <summary>
        /// Searched for next valid audio index
        /// </summary>
        /// <param name="current">Starting index value</param>
        /// <param name="dir">Direction to travel in (1, -1) </param>
        /// <returns>-1 if no index found, else valid index</returns>
        public static int GetNextAudioIndex(int current, int dir)
        {
            int result = current;
            int maxVal = GetNumAudioFiles() + 2;

            if (result < 1)
            {
                result = 0;
                dir = 1;
            }
            else if ( result > maxVal)
            {
                result = maxVal;
                dir = -1;
            }

            string startPath = string.Concat(RootAudioDirectory, result.ToString(), fileExtension);

            string fPath = string.Concat(RootAudioDirectory, result.ToString(), fileExtension);

            while (!File.Exists(fPath) || fPath == startPath)
            {
                result += dir;
                fPath = string.Concat(RootAudioDirectory, result.ToString(), fileExtension);

                if (result < 1 || result > maxVal)
                {
                    result = -1;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks whether or not an audio file with given index exists on the system.
        /// </summary>
        /// <param name="index">Index/file identifier of the audio</param>
        public static bool IsExist(int index)
        {
            string fPath = string.Concat(RootAudioDirectory, index.ToString(), fileExtension);

            return File.Exists(fPath);

        }

        /// <summary>
        /// Checks whether or not an audio file with given index exists on the system
        /// </summary>
        /// <param name="path">Full file path including extension to item</param>
        public static bool IsExist(string path)
        {
            return File.Exists(path);
        }
    }
}
