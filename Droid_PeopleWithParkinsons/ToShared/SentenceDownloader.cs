using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RestSharp;

namespace Droid_PeopleWithParkinsons
{
    class SentenceDownloader
    {
        public delegate void sentencesDownloadedHandler();
        public event sentencesDownloadedHandler sentencesDownloadedEvent;

        public void BeginDownloadProcess()
        {
            bool successful = false;

            while (!successful)
            {
                successful = DownloadSentences();
            }

            if (sentencesDownloadedEvent != null)
            {
                sentencesDownloadedEvent();
            }
        }


        /// <summary>
        /// Uploads an item from the given file path. Blocks the thread.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private bool DownloadSentences()
        {
            do
            {
                Thread.Sleep(500);
            } while (Reachability.HasNetworkConnection() == false);

            try
            {
                // TODO: Probably have a proper request and proper response handling
                // Of course we need to wait for the server stuff to be set up before
                // we can do this.
                RestClient mClient = new RestClient("http://www.speeching.co.uk/question.php");

                RestRequest mRequest = new RestRequest(Method.GET);

                IRestResponse mResponse = mClient.Execute(mRequest);
                string contents = mResponse.Content;

                if (contents == "ok")
                {
                    SaveSentences(contents);
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

        private void SaveSentences(string questions)
        {
            // TODO: Fill out when we have a proper question structure.
            // A list in JSON would be grrrrrrrreeeaaaatt.

            while (SentenceManager.sentences.Count < SentenceManager.DOWNLOAD_SENTENCES)
            {
                SentenceManager.AddSentence(PlaceholderStrings.GetRandomSentence());
            }
        }
    }
}
