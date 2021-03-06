using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingShared
{
    public interface IFeedItem : IEquatable<IFeedItem>
    {
        int Id { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        DateTime Date { get; set; }
        bool Dismissable { get; set; }
        int Importance { get; set; }
        FeedItemInteraction Interaction { get; set; }
    }
}