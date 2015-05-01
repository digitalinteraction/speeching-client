using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingCommon
{
    public class Assessment
    {
        public int id;
        public string title;
        public string description;
        public DateTime dateSet;
        public IAssessmentTask[] tasks;
    }
}