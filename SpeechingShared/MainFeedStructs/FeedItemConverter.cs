using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingShared
{
    public class FeedItemConverter : JsonCreationConverter<IFeedItem>
    {
        protected override IFeedItem Create(Type objectType, JObject jObject)
        {
            if (FieldExists("Image", jObject))
            {
                return new FeedItemImage();
            }
            if(FieldExists("UserAccount", jObject))
            {
                return new FeedItemUser();
            }
            if(FieldExists("Activity", jObject))
            {
                return new FeedItemActivity();
            }
            if (FieldExists("Rating", jObject))
            {
                return new FeedItemStarRating();
            }
            if (FieldExists("DataPoints", jObject))
            {
                return new FeedItemGraph();
            }
            if (FieldExists("Percentage", jObject))
            {
                return new FeedItemPercentage();
            }
            return new FeedItemBase();
        }

        private bool FieldExists(string fieldName, JObject jObject)
        {
            return jObject[fieldName] != null;
        }
    }
}