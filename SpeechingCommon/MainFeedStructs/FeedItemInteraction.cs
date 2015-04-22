using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SpeechingCommon
{
    public class FeedItemInteraction
    {
        public enum InteractionType { URL, ASSESSMENT, ACTIVITY }

        public InteractionType type;
        public string value;
        public string label;
    }
}