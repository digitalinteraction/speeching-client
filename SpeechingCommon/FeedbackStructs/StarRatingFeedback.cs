

namespace SpeechingCommon
{
    /// <summary>
    /// Feedback which contains a * rating out of 5
    /// </summary>
    public class StarRatingFeedback : IFeedbackItem
    {
        private int id;
        private string activityId;
        private string title;
        private string caption;

        public float Rating;

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