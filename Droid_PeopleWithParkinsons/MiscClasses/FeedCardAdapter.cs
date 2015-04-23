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
            viewTypes.Add(typeof(FeedItemActivity), 5);
            viewTypes.Add(typeof(FeedbackSubmissionButton), 6);
            viewTypes.Add(typeof(FeedItemStarRating), 7);
            
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
                    View imageView = inflater.Inflate(Resource.Layout.FeedCardImage, viewGroup, false);
                    CardImageViewHolder imageHolder = new CardImageViewHolder(imageView);
                    return imageHolder;
                case 2:
                    View percentView = inflater.Inflate(Resource.Layout.FeedCardPercentage, viewGroup, false);
                    CardPercentViewHolder percentHolder = new CardPercentViewHolder(percentView);
                    return percentHolder;
                case 3:
                    View graphView = inflater.Inflate(Resource.Layout.FeedCardGraph, viewGroup, false);
                    CardGraphViewHolder graphHolder = new CardGraphViewHolder(graphView);
                    return graphHolder;
                case 4:
                    View commentView = inflater.Inflate(Resource.Layout.FeedCardPerson, viewGroup, false);
                    CardPersonViewHolder commentHolder = new CardPersonViewHolder(commentView);
                    return commentHolder;
                case 5:
                    View activityView = inflater.Inflate(Resource.Layout.FeedCardActivity, viewGroup, false);
                    CardActivityViewHolder activityHolder = new CardActivityViewHolder(activityView);
                    return activityHolder;
                default:
                    View v = inflater.Inflate(Resource.Layout.FeedCardText, viewGroup, false);
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
            else if (viewHolder.GetType() == typeof(CardPercentViewHolder))
            {
                (viewHolder as CardPercentViewHolder).AnimatePercentage(((FeedItemPercentage)data[position]).Percentage, 1200);
            }
            else if (viewHolder.GetType() == typeof(CardGraphViewHolder))
            {
                try
                {
                    PlotModel model = ((FeedItemGraph)data[position]).CreatePlotModel();
                    (viewHolder as CardGraphViewHolder).PlotGraph(model);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else if (viewHolder.GetType() == typeof(CardPersonViewHolder))
            {
                (viewHolder as CardPersonViewHolder).LoadData(((FeedItemUser)data[position]).UserAccount, context);
            }
            else if(viewHolder.GetType() == typeof(CardActivityViewHolder))
            {
                (viewHolder as CardActivityViewHolder).LoadData((FeedItemActivity)data[position], context);
            }

            if (data[position].Interaction == null)
            {
                (viewHolder as CardBaseViewHolder).interact.Visibility = ViewStates.Gone;
                return;
            }

            (viewHolder as CardBaseViewHolder).interact.Visibility = ViewStates.Visible;
            (viewHolder as CardBaseViewHolder).interact.Text = data[position].Interaction.label;

            FeedItemInteraction interaction = data[position].Interaction;

            if (interaction.type == FeedItemInteraction.InteractionType.URL)
            {
                (viewHolder as CardBaseViewHolder).interact.Click += delegate
                {
                    Intent i = new Intent(Intent.ActionView,
                        Android.Net.Uri.Parse(interaction.value));
                    context.StartActivity(i);
                };
            }
            else if(interaction.type == FeedItemInteraction.InteractionType.ASSESSMENT)
            {
                (viewHolder as CardBaseViewHolder).interact.Click += delegate
                {
                    context.StartActivity(typeof(AssessmentActivity));
                };
            }
            else if(interaction.type == FeedItemInteraction.InteractionType.ACTIVITY)
            {
                (viewHolder as CardBaseViewHolder).interact.Click += delegate
                {
                    Intent intent = new Intent(context, typeof(ScenarioActivity));
                    intent.PutExtra("ActivityId", int.Parse(interaction.value));
                    context.StartActivity(intent);
                };
            }
        }
    }

    public class CardBaseViewHolder : RecyclerView.ViewHolder
    {
        public TextView title;
        public TextView description;
        public Button interact;

        public CardBaseViewHolder(View v)
            : base(v)
        {
            title = v.FindViewById<TextView>(Resource.Id.resultCard_title);
            description = v.FindViewById<TextView>(Resource.Id.resultCard_caption);
            interact = v.FindViewById<Button>(Resource.Id.resultCard_interaction);
        }

        public static async Task LoadImageIntoCircle( string imageUrl, ImageView imageView, Context context)
        {
            string imageLoc = await Utils.FetchLocalCopy(imageUrl, typeof(User));

            if (string.IsNullOrEmpty(imageLoc)) return;

            Bitmap thisBitmap = MediaStore.Images.Media.GetBitmap(
                        context.ContentResolver,
                        Android.Net.Uri.FromFile(new Java.IO.File(imageLoc)));

            RoundedDrawable avatar = new RoundedDrawable(thisBitmap);
            imageView.SetImageDrawable(avatar);
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

    public class CardPercentViewHolder : CardBaseViewHolder
    {
        public RadialProgressView percent;

        public CardPercentViewHolder(View v)
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

    public class CardPersonViewHolder : CardBaseViewHolder
    {
        private ImageView avatarView;
        public TextView username;

        public CardPersonViewHolder(View v)
            : base(v)
        {
            avatarView = v.FindViewById<ImageView>(Resource.Id.resultCard_avatar);
            username = v.FindViewById<TextView>(Resource.Id.resultCard_username);
        }

        public void LoadData(User user, Context context)
        {
            username.Text = user.name;
            LoadImageIntoCircle(user.avatar, avatarView, context);
        }
        
    }

    public class CardActivityViewHolder : CardBaseViewHolder
    {
        private ImageView icon;
        private TextView activityName;

        private TextView rationaleView;
        private TextView rationaleTease;

        public CardActivityViewHolder(View v)
            : base(v)
        {
            icon = v.FindViewById<ImageView>(Resource.Id.resultCard_icon);
            activityName = v.FindViewById<TextView>(Resource.Id.resultCard_activityTitle);
            rationaleView = v.FindViewById<TextView>(Resource.Id.resultCard_rationale);
            rationaleTease = v.FindViewById<TextView>(Resource.Id.resultCard_rationaleTease);

            rationaleTease.Visibility = ViewStates.Gone;
            rationaleView.Visibility = ViewStates.Gone;
        }

        private void SetRationale(string[] reasons)
        {
            if (reasons == null || reasons.Length == 0) return;

            rationaleTease.Visibility = ViewStates.Visible;
            rationaleView.Visibility = ViewStates.Visible;

            rationaleView.Text = reasons[0];

            for(int i = 1; i < reasons.Length; i++)
            {
                rationaleView.Text += "\n" + reasons[i];
            }
        }

        public void LoadData(FeedItemActivity data, Context context)
        {
            activityName.Text = data.Activity.Title;
            LoadImageIntoCircle(data.Activity.Icon, icon, context);
            SetRationale(data.Rationale);
        }
    }

    public class CardGraphViewHolder : CardBaseViewHolder
    {
        public PlotView plotView;

        public CardGraphViewHolder(View v)
            : base(v)
        {
            plotView = v.FindViewById<PlotView>(Resource.Id.resultCard_graph);
        }

        public void PlotGraph(PlotModel plotModel)
        {
            plotView.Model = plotModel;
        }
    }
}