using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Droid_PeopleWithParkinsons
{
    class ContentUploader
    {
        public delegate void itemUploadedHandler(string filepath);
        public delegate void allItemsUploadedHandler();
        public event allItemsUploadedHandler allItemsUploadedEvent;
        public event itemUploadedHandler itemUploadedEvent;
        public event allItemsUploadedHandler operationFinishedEvent;

        private List<string> filesToUpload = new List<string>();

        public bool AddFileToUploadQueue(string filePath)
        {
            if (filesToUpload.Contains(filePath))
            {
                return false;
            }
            else
            {
                filesToUpload.Add(filePath);
                return true;
            }
        }

        public void BeginUploadProcess()
        {
            if (filesToUpload.Count > 0)
            {
                while (filesToUpload.Count > 0)
                {
                    string fPath = filesToUpload[0];

                    UploadItem(fPath);
                    filesToUpload.RemoveAt(0);

                    Thread.Sleep(5 * 1000);
                }

                if (allItemsUploadedEvent != null)
                {
                    allItemsUploadedEvent();
                }
                if (operationFinishedEvent != null)
                {
                    operationFinishedEvent();
                }
            }
            else
            {
                if (operationFinishedEvent != null)
                {
                    operationFinishedEvent();
                }
            }
        }

        private void UploadItem(string filepath)
        {
            do
            {
                Thread.Sleep(500);
            } while (Reachability.HasNetworkConnection() == false);

            if (AudioFileManager.IsExist(filepath))
            {
               
                // Fake upload
                // Rest sharp can block the thread until a return is received. Upon return we parse
                // the response. If we are successful, then we can remove the file, then; just return to the 
                // upload file root main loop. Firing events as neccessary, obviously.
                // Maybe we can return a bool to indicate success in uploading or not.
                if (itemUploadedEvent != null)
                {
                    itemUploadedEvent(filepath);
                }
            }
            else
            {
                // Whaaaa?
            }
        }
    }
}
