using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingCommon
{
    public class FeedItemBase : IFeedItem
    {
        protected int id;
        protected string title;
        protected string description;
        protected DateTime date;
        protected bool dismissable;
        protected int importance;
        protected FeedItemInteraction interaction;

        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        public DateTime Date
        {
            get
            {
                return date;
            }
            set
            {
                date = value;
            }
        }

        public bool Dismissable
        {
            get
            {
                return dismissable;
            }
            set
            {
                dismissable = value;
            }
        }

        public int Importance
        {
            get
            {
                return importance;
            }
            set
            {
                importance = value;
            }
        }

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }


        public FeedItemInteraction Interaction
        {
            get
            {
                return interaction;
            }
            set
            {
                interaction = value;
            }
        }
    }
}