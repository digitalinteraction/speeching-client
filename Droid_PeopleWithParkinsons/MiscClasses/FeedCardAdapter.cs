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
    public class FeedCardAdapter : RecyclerView.Adapter
    {
        public List<IFeedItem> data;
        private Dictionary<Type, int> viewTypes;
        private Context context;

        public FeedCardAdapter(List<IFeedItem> feedData, Context context)
        {
            this.data = feedData;
            this.context = context;

            viewTypes = new Dictionary<Type, int>();
            viewTypes.Add(typeof(FeedItemBase), 0);
            viewTypes.Add(typeof(FeedItemImage), 1);
            viewTypes.Add(typeof(FeedItemPercentage), 2);
            viewTypes.Add(typeof(FeedItemGraph), 3);
            viewTypes.Add(typeof(FeedItemUser), 4);
            viewTypes.Add(typeof(FeedbackSubmissionButton), 5);
            viewTypes.Add(typeof(FeedItemStarRating), 6);
            
        }

        public override int GetItemViewType(int position)
        {
            if (viewTypes.ContainsKey(data[position].GetType()))
                return viewTypes[data[position].GetType()];
            else
                return 0; // Default to base layout (All types will populate this correctly)
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
                case 1:
                    View imageView = inflater.Inflate(Resource.Layout.ResultsCardImage, viewGroup, false);
                    CardImageViewHolder imageHolder = new CardImageViewHolder(imageView);
                    return imageHolder;
                case 2:
                    View percentView = inflater.Inflate(Resource.Layout.ResultsCardPercentage, viewGroup, false);
                    ResultViewPercentHolder percentHolder = new ResultViewPercentHolder(percentView);
                    return percentHolder;
                case 3:
                    View graphView = inflater.Inflate(Resource.Layout.ResultsCardGraph, viewGroup, false);
                    ResultViewGraphHolder graphHolder = new ResultViewGraphHolder(graphView);
                    return graphHolder;
                case 4:
                    View commentView = inflater.Inflate(Resource.Layout.ResultsCardPerson, viewGroup, false);
                    ResultViewPersonHolder commentHolder = new ResultViewPersonHolder(commentView);
                    return commentHolder;
                default:
                    View v = inflater.Inflate(Resource.Layout.ResultsCardText, viewGroup, false);
                    CardBaseViewHolder vh = new CardBaseViewHolder(v);
                    return vh;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            (viewHolder as CardBaseViewHolder).title.SetText(data[position].Title, TextView.BufferType.Normal);
            (viewHolder as CardBaseViewHolder).description.SetText(data[position].Description, TextView.BufferType.Normal);

            if(viewHolder.GetType() == typeof(CardImageViewHolder))
            {
                (viewHolder as CardImageViewHolder).LoadImage(((FeedItemImage)data[position]).Image, context);
            }
            else if (viewHolder.GetType() == typeof(ResultViewPercentHolder))
            {
                (viewHolder as ResultViewPercentHolder).AnimatePercentage(((FeedItemPercentage)data[position]).Percentage, 1200);
            }
            else if (viewHolder.GetType() == typeof(ResultViewGraphHolder))
            {
                try
                {
                    PlotModel model = ((FeedItemGraph)data[position]).CreatePlotModel();
                    (viewHolder as ResultViewGraphHolder).PlotGraph(model);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else if (viewHolder.GetType() == typeof(ResultViewPersonHolder))
            {
                (viewHolder as ResultViewPersonHolder).LoadUserAvatar(((FeedItemUser)data[position]).Account, context);
            }
        }
    }

    public class CardBaseViewHolder : RecyclerView.ViewHolder
    {
        public TextView title;
        public TextView description;

        public CardBaseViewHolder(View v)
            : base(v)
        {
            title = v.FindViewById<TextView>(Resource.Id.resultCard_title);
            description = v.FindViewById<TextView>(Resource.Id.resultCard_caption);
        }
    }

    public class CardImageViewHolder : CardBaseViewHolder
    {
        public ImageView image;

        public CardImageViewHolder(View v) : base(v)
        {
            image = v.FindViewById<ImageView>(Resource.Id.resultCard_image);
        }

        public async Task LoadImage(string imageLoc, Context context)
        {
            string localLoc = await Utils.FetchLocalCopy(imageLoc);

            if (string.IsNullOrEmpty(localLoc)) return;

            image.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(localLoc)));
        }
    }

    public class ResultViewPercentHolder : CardBaseViewHolder
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

    public class ResultViewPersonHolder : CardBaseViewHolder
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

    public class ResultViewGraphHolder : CardBaseViewHolder
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