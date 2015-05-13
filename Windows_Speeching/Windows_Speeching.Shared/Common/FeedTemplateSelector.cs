using SpeechingShared;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;

namespace Windows_Speeching.Common
{
    public class FeedTemplateSelector : TemplateSelector
    {
        public DataTemplate Standard { get; set; }
        public DataTemplate WithImage { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            IFeedItem feedItem = (item as IFeedItem);
            if (feedItem == null) throw new NotImplementedException();

            if(feedItem.GetType() == typeof(FeedItemImage))
            {
                return WithImage;
            }

            return Standard;
        }
    }
}
