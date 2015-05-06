using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingShared
{
    public interface ISpeechingActivityItem
    {
        int Id { get; set; }
        User Creator { get; set; }
        string Title { get; set; }
        string Resource { get; set; }
        string Icon { get; set; }
    }
}