using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

namespace SpeechingShared
{
    public class Scenario : ISpeechingPracticeActivity
    {
        private int id;
        private User creator;
        private string title;
        private string resources;
        private string icon;
        private string localIcon;

        public SpeechingTask[] Tasks;

        public int Id
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

        public string Resource
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
            if (!force && (Tasks != null && Tasks.Length > 0)) return Tasks;

            Tasks = await ServerData.GetRequest<SpeechingTask[]>("task", id.ToString());

            AppData.SaveCurrentData();

            return Tasks;
        }


        public string LocalIcon
        {
            get
            {
                return localIcon;
            }
            set
            {
                localIcon = value;
            }
        }

        public async Task<bool> PrepareIcon()
        {
            LocalIcon = await Utils.FetchLocalCopy(Icon);

            return LocalIcon != null;
        }
    }

    public class TaskContent
    {
        public enum ContentType { Audio, Video, Text };
        public ContentType Type;
        public string Visual;
        public string Audio;
        public string Text;
    }

    public class TaskResponse
    {
        public enum ResponseType { None, Prompted, Freeform, Choice };
        public ResponseType Type;
        public string Prompt;
        public string[] Related;
    }

    public class SpeechingTask
    {
        public int Id;
        public TaskContent TaskContent;
        public TaskResponse TaskResponse;
    }

}