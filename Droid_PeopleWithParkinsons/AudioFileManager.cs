using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Droid_PeopleWithParkinsons
{
    // TODO: Set up a temporary path to save files. Only export to main file directory upon completion.
    // That way we can clean out the temp folder if the system crashes for any reason
    // Without affecting the data we actually want to keep.

    class AudioFileManager
    {
        public static string fileExtension = ".pcm";
        private const string audioTempPath = "/audioTemp/";
        private const string audioPath = "/audio/";
        private const string backgroundAudioPath = "/bgaudio/";

#if __ANDROID__
        public static string RootTempAudioDirectory
        {
            get
            {
                string dir = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioTempPath);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioTempPath);
            }
        }

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
        private static string tempFilePath { get { return string.Concat(RootTempAudioDirectory, fileName); } }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns a generated folder path and file name (without extension) for a new audio recording in the temp directory</returns>
        public static string GetNewAudioFilePath()
        {
            // Get file name via unique integer ID.
            fileCount = 1;

            bool found = false;
            // Loop until free file found
            while (!found)
            {
                if (File.Exists(filePath))
                {
                    ++fileCount;
                }
                else if (File.Exists(tempFilePath))
                {
                    ++fileCount;
                }
                else
                {
                    found = true;
                }
            }

            return tempFilePath;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>Total number of files in Audio directory</returns>
        private static int GetNumAudioFiles()
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
        public static void DeleteFileByIndex(int index, bool isTemp)
        {
            string usingPath = isTemp ? RootTempAudioDirectory : RootAudioDirectory;
            string dPath = string.Concat(usingPath, index.ToString(), fileExtension);
            File.Delete(dPath);
        }


        /// <summary>
        /// Checks whether or not an audio file with given index exists on the system
        /// </summary>
        /// <param name="path">Full file path including extension to item</param>
        public static bool IsExist(string path)
        {
            return File.Exists(path);
        }


        /// <summary>
        /// Moves temp file to root audio directory.
        /// </summary>
        /// <param name="tempPath">Path to temporary file</param>
        /// <returns>The new root audio directory filepath if operation was successful. Empty string otherwise.</returns>
        public static string FinaliseAudio(string tempPath)
        {
            if (IsExist(tempPath))
            {
                string fileName = Path.GetFileName(tempPath);
                string newFilePath = RootAudioDirectory + fileName;
                File.Move(tempPath, newFilePath);
                return newFilePath;
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// Returns full filepath for each file in target directory
        /// </summary>
        /// <param name="isTemp">Using temp audio directory, or finalised audio directory</param>
        /// <returns>List of strings</returns>
        public static List<string> GetAllFiles(bool isTemp)
        {
            string usingString = isTemp ? RootTempAudioDirectory : RootAudioDirectory;
            string[] files = Directory.GetFiles(usingString);
            List<string> list = new List<string>(files);

            return list;
        }
    }
}