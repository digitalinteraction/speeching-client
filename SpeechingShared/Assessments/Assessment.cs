using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechingShared
{
    public class Assessment : ISpeechingPracticeActivity
    {
        public string Description { get; set; }
        public DateTime DateSet { get; set; }
        public IAssessmentTask[] AssessmentTasks { get; set; }

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

        public async Task PrepareTasks()
        {
            foreach (IAssessmentTask task in AssessmentTasks)
            {
                if (task.GetType() != typeof(ImageDescTask)) continue;
                var imageDescTask = task as ImageDescTask;
                if (imageDescTask != null)
                    imageDescTask.Image = await Utils.FetchLocalCopy(imageDescTask.Image);
            }
        }
    }
}