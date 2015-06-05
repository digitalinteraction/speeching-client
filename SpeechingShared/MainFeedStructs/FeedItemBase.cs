using System;

namespace SpeechingShared
{
    public class FeedItemBase : IFeedItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public bool Dismissable { get; set; }
        public int Importance { get; set; }
        public int Id { get; set; }
        public FeedItemInteraction Interaction { get; set; }

        public bool Equals(IFeedItem other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.GetType() != GetType()) return false;
            if (Id != other.Id) return false;
            if (Title != other.Title) return false;
            if (Description != other.Description) return false;
            if (Interaction != other.Interaction) return false;
            if (Dismissable != other.Dismissable) return false;
            if (Importance != other.Importance) return false;

            return true;
        }
    }
}