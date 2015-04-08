using Android.App;
using Android.Graphics;
using Android.Provider;
using Android.Views;
using Android.Widget;
using RadialProgress;
using SpeechingCommon;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DroidSpeeching
{
    public class FeedbackTypesAdapter : BaseAdapter<IFeedbackItem>
    {
        private Activity context;
        private List<int> seen;
        public List<IFeedbackItem> feedbackItems;
        private Dictionary<Type, int> viewTypes;

        /// <summary>
        /// Lists feedback in multiple layout and object types
        /// </summary>
        public FeedbackTypesAdapter(Activity context, int resource, List<IFeedbackItem> data)
        {
            this.context = context;
            this.feedbackItems = data;

            viewTypes = new Dictionary<Type, int>();
            viewTypes.Add(typeof(PercentageFeedback), 0);
            viewTypes.Add(typeof(StarRatingFeedback), 1);
            viewTypes.Add(typeof(CommentFeedback), 2);
            viewTypes.Add(typeof(FeedbackSubmissionButton), 3);
            seen = new List<int>();
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override IFeedbackItem this[int position]
        {
            get { return feedbackItems[position]; }
        }

        public override int Count
        {
            get { return feedbackItems.Count; }
        }

        public override int ViewTypeCount
        {
            get { return viewTypes.Count; }
        }

        public override void NotifyDataSetChanged()
        {
            base.NotifyDataSetChanged();
            seen.Clear();
        }

        // Helps decide which layout to use, based on object type
        public override int GetItemViewType(int position)
        {
            return viewTypes[feedbackItems[position].GetType()];
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            IFeedbackItem thisItem = feedbackItems[position];

            if (thisItem.GetType() == typeof(PercentageFeedback))
            {
                if (convertView == null)
                {
                    convertView = context.LayoutInflater.Inflate(Resource.Layout.FeedbackPercentItem, null);
                }

                // Keep track of which items have been seen so that we don't animate them multiple times!
                if (!seen.Contains(feedbackItems[position].Id))
                {
                    AnimatePercentage(((PercentageFeedback)feedbackItems[position]).Percentage, 1500, convertView.FindViewById<RadialProgressView>(Resource.Id.feedback_progressView));
                    seen.Add(feedbackItems[position].Id);
                }
                else
                {
                    convertView.FindViewById<RadialProgressView>(Resource.Id.feedback_progressView).Value = ((PercentageFeedback)feedbackItems[position]).Percentage;
                }
            }
            else if (thisItem.GetType() == typeof(StarRatingFeedback))
            {
                if (convertView == null)
                {
                    convertView = context.LayoutInflater.Inflate(Resource.Layout.FeedbackRatingItem, null);
                }
                convertView.FindViewById<RatingBar>(Resource.Id.feedback_ratingBar).Rating = ((StarRatingFeedback)thisItem).Rating;
            }
            else if (thisItem.GetType() == typeof(CommentFeedback))
            {
                if (convertView == null)
                {
                    convertView = context.LayoutInflater.Inflate(Resource.Layout.FeedbackCommentItem, null);
                }
                LoadUserAvatar(((CommentFeedback)thisItem).Commenter, convertView.FindViewById<ImageView>(Resource.Id.feedback_commentAvatar));
                convertView.FindViewById<TextView>(Resource.Id.feedback_comment_Username).Text = ((CommentFeedback)thisItem).Commenter.name;
            }
            else if(thisItem.GetType() == typeof(FeedbackSubmissionButton))
            {
                if (convertView == null)
                {
                    convertView = context.LayoutInflater.Inflate(Resource.Layout.FeedbackViewRecordingsItem, null);
                }
                return convertView;
            }

            // If this list is going to be really big, it might be worth setting up a view holder? Don't think it will be
            convertView.FindViewById<TextView>(Resource.Id.feedback_itemTitle).Text = thisItem.Title;
            convertView.FindViewById<TextView>(Resource.Id.feedback_itemCaption).Text = thisItem.Caption;

            return convertView;
        }

        /// <summary>
        /// Make the progress view gradually count up to the given value
        /// </summary>
        /// <param name="toVal">The eventual target value</param>
        /// <param name="millis">The total time for the animation</param>
        /// <param name="progressView">The view to affect</param>
        /// <returns>Awaitable</returns>
        private async Task AnimatePercentage(float toVal, float millis, RadialProgressView progressView)
        {
            int waitTime = (int)(millis / toVal);
            float current = 0;
            while (current < toVal)
            {
                current++;
                progressView.Value = current;
                await Task.Delay(waitTime);
            }
        }

        /// <summary>
        /// Load the given user's avatar into the ImageView as a circular drawable
        /// </summary>
        private async Task LoadUserAvatar(User user, ImageView view)
        {
            string imageLoc = await Utils.FetchLocalCopy(user.avatar, typeof(User));

            if (string.IsNullOrEmpty(imageLoc)) return;

            Bitmap thisBitmap = MediaStore.Images.Media.GetBitmap(
                        context.ContentResolver,
                        Android.Net.Uri.FromFile(new Java.IO.File(imageLoc)));

            RoundedDrawable avatar = new RoundedDrawable(thisBitmap);
            view.SetImageDrawable(avatar);
        }
    }
}