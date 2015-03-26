using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingCommon
{
    public interface IFeedbackItem
    {
        int Id { get; set; }
        string ActivityId { get; set; }
        string Title { get; set; }
        string Caption { get; set; }
    }
}