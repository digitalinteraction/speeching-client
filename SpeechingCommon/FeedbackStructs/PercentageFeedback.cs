namespace SpeechingCommon
{
    /// <summary>
    /// Feedback which contains a percentage statistic
    /// </summary>
    public class PercentageFeedback : IFeedbackItem
    {
        private int id;
        private string activityId;
        private string title;
        private string caption;

        public float Percentage;

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