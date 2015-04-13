using Android.Content;
using Android.Graphics;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using OxyPlot;
using OxyPlot.Xamarin.Android;
using RadialProgress;
using SpeechingCommon;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DroidSpeeching
{
    public class ResultsCardAdapter : RecyclerView.Adapter
    {
        public List<IFeedbackItem> data;
        private Dictionary<Type, int> viewTypes;
        private Context context;

        public ResultsCardAdapter(List<IFeedbackItem> feedback, Context context)
        {
            this.data = feedback;
            this.context = context;

            viewTypes = new Dictionary<Type, int>();
            viewTypes.Add(typeof(PercentageFeedback), 0);
            viewTypes.Add(typeof(StarRatingFeedback), 1);
            viewTypes.Add(typeof(CommentFeedback), 2);
            viewTypes.Add(typeof(FeedbackSubmissionButton), 3);
            viewTypes.Add(typeof(GraphFeedback), 4);
        }

        public override int GetItemViewType(int position)
        {
            return viewTypes[data[position].GetType()];
        }

        public override int ItemCount
        {
            get { return data.Count; }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup viewGroup, int viewType)
        {
            LayoutInflater inflater = LayoutInflater.From(viewGroup.Context);

            switch (viewType)
            {
                case 0:
                    View percentView = inflater.Inflate(Resource.Layout.ResultsCardPercentage, viewGroup, false);
                    ResultViewPercentHolder percentHolder = new ResultViewPercentHolder(percentView);
                    return percentHolder;
                case 2:
                    View commentView = inflater.Inflate(Resource.Layout.ResultsCardPerson, viewGroup, false);
                    ResultViewPersonHolder commentHolder = new ResultViewPersonHolder(commentView);
                    return commentHolder;
                case 4:
                    View graphView = inflater.Inflate(Resource.Layout.ResultsCardGraph, viewGroup, false);
                    ResultViewGraphHolder graphHolder = new ResultViewGraphHolder(graphView);
                    return graphHolder;
                default:
                    View v = inflater.Inflate(Resource.Layout.ResultsCardText, viewGroup, false);
                    ResultViewBaseHolder vh = new ResultViewBaseHolder(v);
                    return vh;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            (viewHolder as ResultViewBaseHolder).title.SetText(data[position].Title, TextView.BufferType.Normal);
            (viewHolder as ResultViewBaseHolder).caption.SetText(data[position].Caption, TextView.BufferType.Normal);

            if (viewHolder.GetType() == typeof(ResultViewPercentHolder))
            {
                (viewHolder as ResultViewPercentHolder).AnimatePercentage(((PercentageFeedback)data[position]).Percentage, 1200);
            }
            else if (viewHolder.GetType() == typeof(ResultViewGraphHolder))
            {
                try
                {
                    PlotModel model = ((GraphFeedback)data[position]).CreatePlotModel();
                    (viewHolder as ResultViewGraphHolder).PlotGraph(model);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else if (viewHolder.GetType() == typeof(ResultViewPersonHolder))
            {
                (viewHolder as ResultViewPersonHolder).LoadUserAvatar(((CommentFeedback)data[position]).Commenter, context);
            }
        }
    }

    public class ResultViewBaseHolder : RecyclerView.ViewHolder
    {
        public TextView title;
        public TextView caption;

        public ResultViewBaseHolder(View v)
            : base(v)
        {
            title = v.FindViewById<TextView>(Resource.Id.resultCard_title);
            caption = v.FindViewById<TextView>(Resource.Id.resultCard_caption);
        }
    }

    public class ResultViewPercentHolder : ResultViewBaseHolder
    {
        public RadialProgressView percent;

        public ResultViewPercentHolder(View v)
            : base(v)
        {
            percent = v.FindViewById<RadialProgressView>(Resource.Id.resultCard_percent);
        }

        public async Task AnimatePercentage(float toVal, float millis)
        {
            int waitTime = (int)(millis / toVal);
            float current = 0;
            while (current < toVal)
            {
                current++;
                percent.Value = current;
                await Task.Delay(waitTime);
            }
        }
    }

    public class ResultViewPersonHolder : ResultViewBaseHolder
    {
        private ImageView avatarView;
        private TextView username;

        public ResultViewPersonHolder(View v)
            : base(v)
        {
            avatarView = v.FindViewById<ImageView>(Resource.Id.resultCard_avatar);
            username = v.FindViewById<TextView>(Resource.Id.resultCard_username);
        }

        public async Task LoadUserAvatar(User user, Context context)
        {
            username.Text = user.name;

            string imageLoc = await Utils.FetchLocalCopy(user.avatar, typeof(User));

            if (string.IsNullOrEmpty(imageLoc)) return;

            Bitmap thisBitmap = MediaStore.Images.Media.GetBitmap(
                        context.ContentResolver,
                        Android.Net.Uri.FromFile(new Java.IO.File(imageLoc)));

            RoundedDrawable avatar = new RoundedDrawable(thisBitmap);
            avatarView.SetImageDrawable(avatar);
        }
    }

    public class ResultViewGraphHolder : ResultViewBaseHolder
    {
        public PlotView plotView;

        public ResultViewGraphHolder(View v)
            : base(v)
        {
            plotView = v.FindViewById<PlotView>(Resource.Id.resultCard_graph);
        }

        public void PlotGraph(PlotModel plotModel)
        {
            plotView.Model = plotModel;
            plotView.Model.Axes[0].Key = "Axis 0 Key here";
            plotView.Model.Axes[1].Key = "Axis 1 Key here";
        }
    }
}