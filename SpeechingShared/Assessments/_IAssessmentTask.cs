using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingShared
{
    public interface IAssessmentTask
    {
        int Id { get; set; }
        string Title { get; set; }
        string Instructions { get; set; }
    }
}