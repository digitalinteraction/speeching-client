using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SpeechingShared
{
    public class FeedItemActivity : FeedItemBase
    {
        [JsonConverter(typeof(ActivityConverter))]
        public ISpeechingPracticeActivity PracticeActivity;
        public string[] Rationale;
    }
}