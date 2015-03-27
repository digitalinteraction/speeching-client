using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingCommon
{
    public class CommentFeedback : IFeedbackItem
    {
        private int id;
        private string activityId;
        private string title;
        private string caption;

        public User Commenter;

        public string ActivityId
        {
            get
            {
                return activityId;
            }
            set
            {
                activityId = value;
            }
        }

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

        public string Caption
        {
            get
            {
                return caption;
            }
            set
            {
                caption = value;
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
    }
}