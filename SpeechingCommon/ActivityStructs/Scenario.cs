using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechingCommon
{
    public class Scenario : SpeechingActivityItem
    {
        //[ManyToMany(typeof(ScenarioTaskRelationship), CascadeOperations = CascadeOperation.All)]
        public SpeechingTask[] Tasks; //{ get; set; }

        /// <summary>
        /// Returns the scenario's tasks or fetches them from the server if they aren't present
        /// </summary>
        /// <param name="force">Force a server refresh even if data already exists</param>
        /// <returns></returns>
        public async Task<SpeechingTask[]> FetchTasks(bool force = false)
        {
            if (!force && (Tasks != null && Tasks.Length > 0)) return Tasks;

            Tasks = await ServerData.GetRequest<SpeechingTask[]>("task", Id.ToString());

            AppData.SaveCurrentData();

            return Tasks;
        }
    }

    public class ScenarioTaskRelationship
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [ForeignKey(typeof(Scenario))]
        public int ScenarioId { get; set; }
        [ForeignKey(typeof(SpeechingTask))]
        public int TaskId { get; set; }
    }

    public class TaskContent
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public enum ContentType { Audio, Video, Text };
        public ContentType Type { get; set; }
        public string Visual { get; set; }
        public string Audio { get; set; }
        public string Text { get; set; }
    }

    public class TaskResponse
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public enum ResponseType { None, Prompted, Freeform, Choice };
        public ResponseType Type { get; set; }
        public string Prompt { get; set; }
        public string[] Related { get; set; }
    }

    public class SpeechingTask
    {
        [PrimaryKey]
        public int Id { get; set; }
        [OneToOne]
        public TaskContent TaskContent { get; set; }
        [OneToOne]
        public TaskResponse TaskResponse { get; set; }

        //[ManyToMany(typeof(ScenarioTaskRelationship))]
        //public SpeechingActivityItem[] ActivitiesUsedIn { get; set; }
    }

}