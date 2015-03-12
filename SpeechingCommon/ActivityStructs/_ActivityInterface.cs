using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace SpeechingCommon
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