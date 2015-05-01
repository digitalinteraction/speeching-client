using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SpeechingCommon
{
    public class FeedItemActivity : FeedItemBase
    {
        [JsonConverter(typeof(ActivityConverter))]
        public SpeechingActivityItem Activity;
        public string[] Rationale;
    }
}