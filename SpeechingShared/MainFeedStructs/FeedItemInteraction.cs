using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SpeechingShared
{
    public class FeedItemInteraction
    {
        public enum InteractionType { None, Url, Assessment, Activity }

        public InteractionType Type;
        public string Value;
        public string Label;
    }
}