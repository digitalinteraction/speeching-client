using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

namespace SpeechingShared
{
    public class Scenario : ISpeechingPracticeActivity
    {
        public SpeechingTask[] ParticipantTasks;

        public int Id { get; set; }
        public User Creator { get; set; }
        public string Title { get; set; }
        public string Resource { get; set; }
        public string Icon { get; set; }
        public string LocalIcon { get; set; }

        public async Task<bool> PrepareIcon()
        {
            LocalIcon = await Utils.FetchLocalCopy(Icon);

            return LocalIcon != null;
        }

        /// <summary>
        /// Returns the scenario's tasks or fetches them from the server if they aren't present
        /// </summary>
        /// <param name="force">Force a server refresh even if data already exists</param>
        /// <returns></returns>
        public async Task<SpeechingTask[]> FetchTasks(bool force = false)
        {
            if (!force && (ParticipantTasks != null && ParticipantTasks.Length > 0)) return ParticipantTasks;

            ParticipantTasks = await ServerData.GetRequest<SpeechingTask[]>("task", Id.ToString());

            AppData.SaveCurrentData();

            return ParticipantTasks;
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
        public TaskContent ParticipantTaskContent;
        public TaskResponse ParticipantTaskResponse;
    }

}