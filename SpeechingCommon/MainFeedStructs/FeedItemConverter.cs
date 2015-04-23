using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingCommon
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
            else if(FieldExists("Activity", jObject))
            {
                return new FeedItemActivity();
            }
            else
            {
                return new FeedItemBase();
            }
        }

        private bool FieldExists(string fieldName, JObject jObject)
        {
            return jObject[fieldName] != null;
        }
    }
}