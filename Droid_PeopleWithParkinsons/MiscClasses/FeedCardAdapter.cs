using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using OxyPlot;
using OxyPlot.Xamarin.Android;
using RadialProgress;
using SpeechingShared;

namespace DroidSpeeching
{
    public class FeedCardAdapter : RecyclerView.Adapter
    {
        private readonly Context context;
        private readonly Dictionary<Type, int> viewTypes;
        public List<IFeedItem> Data;

        public FeedCardAdapter(List<IFeedItem> feedData, Context context)
        {
            Data = feedData;
            this.context = context;

            viewTypes = new Dictionary<Type, int>
            {
                {typeof (FeedItemBase), 0},
                {typeof (FeedItemImage), 1},
                {typeof (FeedItemPercentage), 2},
                {typeof (FeedItemGraph), 3},
                {typeof (FeedItemUser), 4},
                {typeof (FeedItemActivity), 5},
                {typeof (FeedbackSubmissionButton), 6},
                {typeof (FeedItemStarRating), 7}
            };
        }

        public override int ItemCount
        {
            get { return Data.Count; }
        }

        public override int GetItemViewType(int position)
        {
            if (viewTypes.ContainsKey(Data[position].GetType()))
                return viewTypes[Data[position].GetType()];
            return 0; // Default to base layout (All types will populate this correctly)
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
                case 7:
                    View starView = inflater.Inflate(Resource.Layout.FeedCardStarRating, viewGroup, false);
                    CardRatingViewHolder starHolder = new CardRatingViewHolder(starView);
                    return starHolder;
                default:
                    View v = inflater.Inflate(Resource.Layout.FeedCardText, viewGroup, false);
                    CardBaseViewHolder vh = new CardBaseViewHolder(v);
                    return vh;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            ((CardBaseViewHolder) viewHolder).Title.SetText(Data[position].Title, TextView.BufferType.Normal);
            ((CardBaseViewHolder) viewHolder).Description.SetText(Data[position].Description,
                TextView.BufferType.Normal);

            if (viewHolder.GetType() == typeof (CardImageViewHolder))
            {
                ((CardImageViewHolder) viewHolder).LoadImage(((FeedItemImage) Data[position]).Image, context);
            }
            else if (viewHolder.GetType() == typeof (CardPercentViewHolder))
            {
                ((CardPercentViewHolder) viewHolder).AnimatePercentage(
                    ((FeedItemPercentage) Data[position]).Percentage, 1200);
            }
            else if (viewHolder.GetType() == typeof (CardGraphViewHolder))
            {
                PlotModel model = ((FeedItemGraph) Data[position]).CreatePlotModel();
                ((CardGraphViewHolder) viewHolder).PlotGraph(model);
            }
            else if (viewHolder.GetType() == typeof (CardPersonViewHolder))
            {
                ((CardPersonViewHolder) viewHolder).LoadData(((FeedItemUser) Data[position]).UserAccount, context);
            }
            else if (viewHolder.GetType() == typeof (CardActivityViewHolder))
            {
                ((CardActivityViewHolder) viewHolder).LoadData((FeedItemActivity) Data[position], context);
            }
            else if (viewHolder.GetType() == typeof (CardRatingViewHolder))
            {
                ((CardRatingViewHolder) viewHolder).RatingBar.Rating = ((FeedItemStarRating) Data[position]).Rating;
            }

            if (Data[position].Interaction == null || Data[position].Interaction.Type == FeedItemInteraction.InteractionType.None)
            {
                ((CardBaseViewHolder) viewHolder).Interact.Visibility = ViewStates.Gone;
                return;
            }

            ((CardBaseViewHolder) viewHolder).Interact.Visibility = ViewStates.Visible;
            ((CardBaseViewHolder) viewHolder).Interact.Text = Data[position].Interaction.Label;

            FeedItemInteraction interaction = Data[position].Interaction;

            switch (interaction.Type)
            {
                case FeedItemInteraction.InteractionType.Url:
                    ((CardBaseViewHolder) viewHolder).Interact.Click += delegate
                    {
                        Intent i = new Intent(Intent.ActionView,
                            Android.Net.Uri.Parse(interaction.Value));
                        context.StartActivity(i);
                    };
                    break;

                case FeedItemInteraction.InteractionType.Assessment:
                    ((CardBaseViewHolder) viewHolder).Interact.Click +=
                        delegate
                        {
                            int actId = int.Parse(interaction.Value);

                            if (!AndroidUtils.IsConnected() && !AndroidUtils.IsActivityAvailableOffline(actId, context))
                            {
                                AndroidUtils.OfflineAlert(context,
                                    "This assessment has not been downloaded yet and requires an Internet connection to prepare!");
                                return;
                            }

                            try
                            {
                                Intent intent = new Intent(context, typeof(AssessmentActivity));
                                intent.PutExtra("ActivityId", actId);
                                context.StartActivity(intent);
                            }
                            catch (Exception ex)
                            {
                                AndroidUtils.OfflineAlert(context,
                                    "Error launching assessment activity");
                                return;
                            }
                            
                        };
                    break;

                case FeedItemInteraction.InteractionType.Activity:
                    ((CardBaseViewHolder) viewHolder).Interact.Click += delegate
                    {
                        int actId = int.Parse(interaction.Value);

                        if (!AndroidUtils.IsConnected() && !AndroidUtils.IsActivityAvailableOffline(actId, context))
                        {
                            AndroidUtils.OfflineAlert(context,
                                "This practiceActivity has not been downloaded yet and requires an Internet connection to prepare!");
                            return;
                        }

                        Intent intent = new Intent(context, typeof (ScenarioActivity));
                        intent.PutExtra("ActivityId", actId);
                        context.StartActivity(intent);
                    };
                    break;
            }
        }
    }

    public class CardBaseViewHolder : RecyclerView.ViewHolder
    {
        public TextView Description;
        public Button Interact;
        public TextView Title;

        public CardBaseViewHolder(View v)
            : base(v)
        {
            Title = v.FindViewById<TextView>(Resource.Id.resultCard_title);
            Description = v.FindViewById<TextView>(Resource.Id.resultCard_caption);
            Interact = v.FindViewById<Button>(Resource.Id.resultCard_interaction);
        }

        public static async void LoadImageIntoCircle(string imageUrl, ImageView imageView, Context context)
        {
            try
            {
                string imageLoc = await Utils.FetchLocalCopy(imageUrl, typeof (User));

                if (string.IsNullOrEmpty(imageLoc)) return;

                Bitmap thisBitmap = MediaStore.Images.Media.GetBitmap(
                    context.ContentResolver,
                    Android.Net.Uri.FromFile(new Java.IO.File(imageLoc)));

                RoundedDrawable avatar = new RoundedDrawable(thisBitmap);
                imageView.SetImageDrawable(avatar);
            }
            catch (Exception except)
            {
                Console.WriteLine(except);
            }
        }
    }

    public class CardImageViewHolder : CardBaseViewHolder
    {
        public ImageView Image;

        public CardImageViewHolder(View v) : base(v)
        {
            Image = v.FindViewById<ImageView>(Resource.Id.resultCard_image);
        }

        public async void LoadImage(string imageLoc, Context context)
        {
            string localLoc = null;

            try
            {
                localLoc = await Utils.FetchLocalCopy(imageLoc);

                if (string.IsNullOrEmpty(localLoc)) throw new Exception("Failed to load local copy of image file");

                Image.SetImageBitmap(BitmapFactory.DecodeFile(localLoc));
            }
            catch (Exception except)
            {
                if (File.Exists(localLoc))
                {
                    if (localLoc != null) File.Delete(localLoc);
                }
                Console.WriteLine(except);
            }
        }
    }

    public class CardPercentViewHolder : CardBaseViewHolder
    {
        public RadialProgressView Percent;

        public CardPercentViewHolder(View v)
            : base(v)
        {
            Percent = v.FindViewById<RadialProgressView>(Resource.Id.resultCard_percent);
        }

        public async void AnimatePercentage(float toVal, float millis)
        {
            int waitTime = (int) (millis/toVal);
            float current = 0;
            while (current < toVal)
            {
                current++;
                Percent.Value = current;
                await Task.Delay(waitTime);
            }
        }
    }

    public class CardPersonViewHolder : CardBaseViewHolder
    {
        private readonly ImageView avatarView;
        public TextView Username;

        public CardPersonViewHolder(View v)
            : base(v)
        {
            avatarView = v.FindViewById<ImageView>(Resource.Id.resultCard_avatar);
            Username = v.FindViewById<TextView>(Resource.Id.resultCard_username);
        }

        public void LoadData(User user, Context context)
        {
            Username.Text = user.Name;
            LoadImageIntoCircle(user.Avatar, avatarView, context);
        }
    }

    public class CardRatingViewHolder : CardBaseViewHolder
    {
        public RatingBar RatingBar;

        public CardRatingViewHolder(View v) : base(v)
        {
            RatingBar = v.FindViewById<RatingBar>(Resource.Id.resultCard_ratingBar);
        }
    }

    public class CardActivityViewHolder : CardBaseViewHolder
    {
        private readonly TextView activityName;
        private readonly ImageView icon;
        private readonly TextView rationaleTease;
        private readonly TextView rationaleView;

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

            for (int i = 1; i < reasons.Length; i++)
            {
                rationaleView.Text += "\n" + reasons[i];
            }
        }

        public async void LoadData(FeedItemActivity data, Context context)
        {
            activityName.Text = data.PracticeActivity.Title;
            SetRationale(data.Rationale);

            bool success = true;

            if (data.PracticeActivity.LocalIcon == null && !(await data.PracticeActivity.PrepareIcon()))
            {
                // Icon download attempt failed...
                success = false;
            }

            if (success)
            {
                LoadImageIntoCircle(data.PracticeActivity.LocalIcon, icon, context);
            }
        }
    }

    public class CardGraphViewHolder : CardBaseViewHolder
    {
        public PlotView PlotView;

        public CardGraphViewHolder(View v)
            : base(v)
        {
            PlotView = v.FindViewById<PlotView>(Resource.Id.resultCard_graph);
        }

        public void PlotGraph(PlotModel plotModel)
        {
            if (plotModel.PlotView != null)
            {
                PlotView = (PlotView)plotModel.PlotView;
            }
            else
            {
                PlotView.Model = plotModel;
            }
        }
    }
}