using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingShared
{
    public class Assessment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateSet { get; set; }
        public IAssessmentTask[] AssessmentTasks { get; set; }
    }
}