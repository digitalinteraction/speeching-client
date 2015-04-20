using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingCommon
{
    interface IAssessmentTask
    {
        string Id { get; set; }
        bool IsFinished { get; set; }
        string Title { get; set; }
        string Instructions { get; set; }
        void NextAction();
    }
}