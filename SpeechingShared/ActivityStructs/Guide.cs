using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingCommon
{
    public class Guide : ISpeechingActivityItem
    {
        public struct Page
        {
            public string MediaLocation;
            public string Text;
        };

        private int id;
        private User creator;
        private string title;
        private string resources;
        private string icon;

        public Page[] Guides;

        public int Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public User Creator
        {
            get
            {
                return this.creator;
            }
            set
            {
                this.creator = value;
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }

        public string Resource
        {
            get
            {
                return this.resources;
            }
            set
            {
                this.resources = value;
            }
        }

        public string Icon
        {
            get
            {
                return this.icon;
            }
            set
            {
                this.icon = value;
            }
        }

    }
}