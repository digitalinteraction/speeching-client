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
using System.Threading.Tasks;

namespace SpeechingCommon
{
    public class Scenario : ISpeechingActivityItem
    {
        private string id;
        private User creator;
        private string title;
        private string resources;
        private string icon;

        public SpeechingTask[] tasks;

        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public User Creator
        {
            get
            {
                return this.creator;
            }
            set
            {
                this.creator = value;
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }

        public string Resources
        {
            get
            {
                return this.resources;
            }
            set
            {
                this.resources = value;
            }
        }

        public string Icon
        {
            get
            {
                return this.icon;
            }
            set
            {
                this.icon = value;
            }
        }

        /// <summary>
        /// Returns the scenario's tasks or fetches them from the server if they aren't present
        /// </summary>
        /// <param name="force">Force a server refresh even if data already exists</param>
        /// <returns></returns>
        public async Task<SpeechingTask[]> FetchTasks(bool force = false)
        {
            if (!force && (tasks != null && tasks.Length > 0)) return tasks;

            tasks = await AppData.GetRequest<SpeechingTask[]>("activity", id);

            AppData.SaveCurrentData();

            return tasks;
        }
    }

    public class TaskContent
    {
        public enum ContentType { Audio, Video, Text };
        public ContentType type;
        public string visual;
        public string audio;
        public string text;
    }

    public class TaskResponse
    {
        public enum ResponseType { None, Prompted, Freeform, Choice };
        public ResponseType type;
        public string prompt;
        public string choice1;
        public string choice2;
    }

    public class SpeechingTask
    {
        public string id;
        public TaskContent content;
        public TaskResponse response;
    }

}