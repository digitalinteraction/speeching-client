using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RestSharp;

namespace Droid_PeopleWithParkinsons
{
    class ContentUploader
    {
        public delegate void itemUploadedHandler(string filepath);
        public delegate void allItemsUploadedHandler();
        public event allItemsUploadedHandler allItemsUploadedEvent;
        public event itemUploadedHandler itemUploadedEvent;
        public event allItemsUploadedHandler operationFinishedEvent;

        public void BeginUploadProcess()
        {
            if (ModelManager.uploads.Count > 0)
            {
                // TODO: Have a notification manager class which also includes toasts
                // For both platforms
                // Then, call that platform code here to say that things are actually uploading.

                while (ModelManager.uploads.Count > 0)
                {
                    SentenceModel _model = ModelManager.uploads[0];
                    string fPath = _model.path;

                    bool didUpload = UploadItem(fPath);

                    if (didUpload)
                    {
                        AudioFileManager.DeleteFile(fPath);
                        ModelManager.DeleteModel(_model);                        
                    }

                    Thread.Sleep(5 * 1000);
                }

                if (allItemsUploadedEvent != null)
                {
                    allItemsUploadedEvent();
                }
            }
       
            if (operationFinishedEvent != null)
            {
                operationFinishedEvent();
            }
        }


        /// <summary>
        /// Uploads an item from the given file path. Blocks the thread.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private bool UploadItem(string filepath)
        {
            do
            {
                Thread.Sleep(500);
            } while (Reachability.HasNetworkConnection() == false);

            if (AudioFileManager.IsExist(filepath))
            {
                try
                {
                    // TODO: Probably have a proper request and proper response handling
                    // Of course we need to wait for the server stuff to be set up before
                    // we can do this.
                    RestClient mClient = new RestClient("http://www.speeching.co.uk/upload.php");

                    RestRequest mRequest = new RestRequest(Method.POST);
                    mRequest.AddFile("file", filepath);

                    IRestResponse mResponse = mClient.Execute(mRequest);
                    string contents = mResponse.Content;

                    if (contents == "ok")
                    {
                        if (itemUploadedEvent != null)
                        {
                            itemUploadedEvent(filepath);
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return false;
                }                
            }
            else
            {
                return false;
                // Whaaa? In theory this should never happen, but what if it does?
            }
        }
    }
}
