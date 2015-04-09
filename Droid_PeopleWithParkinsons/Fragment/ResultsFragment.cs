using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using SpeechingCommon;
using RadialProgress;
using System.Threading.Tasks;
using DroidSpeeching;
using OxyPlot.Xamarin.Android;
using OxyPlot;

namespace DroidSpeeching
{
    public class ResultsFragment : Android.Support.V4.App.Fragment
    {
        RecyclerView recList;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            return inflater.Inflate(Resource.Layout.MainResultsFragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            recList = view.FindViewById<RecyclerView>(Resource.Id.mainResults_recyclerView);

            recList.HasFixedSize = true;

            LinearLayoutManager llm = new LinearLayoutManager(Activity);
            llm.Orientation = LinearLayoutManager.Vertical;
            recList.SetLayoutManager(llm);
        }

        public override void OnResume()
        {
            base.OnResume();
            InsertData();
        }

        private async void InsertData()
        {
            List<IResultItem> uploads = AppData.session.resultsToUpload;

            if (uploads == null || uploads.Count == 0) return;

            List<IFeedbackItem> feedback = await ServerData.FetchFeedbackFor(uploads[0].Id);

            ResultsCardAdapter adapter = new ResultsCardAdapter(feedback);
            recList.SetAdapter(adapter);
        }
    }

    public class ResultsCardAdapter : RecyclerView.Adapter
    {
        private List<IFeedbackItem> data;
        private Dictionary<Type, int> viewTypes;

        public ResultsCardAdapter(List<IFeedbackItem> feedback)
        {
            this.data = feedback;

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

            switch(viewType)
            {
                case 0 :
                    View percentView = inflater.Inflate(Resource.Layout.ResultsCardPercentage, viewGroup, false);
                    ResultViewPercentHolder percentHolder = new ResultViewPercentHolder(percentView);
                    return percentHolder;
                case 4  :
                    View graphView = inflater.Inflate(Resource.Layout.ResultsCardGraph, viewGroup, false);
                    ResultViewGraphHolder graphHolder = new ResultViewGraphHolder(graphView);
                    return graphHolder;
                default :
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
            else if(viewHolder.GetType() == typeof(ResultViewGraphHolder))
            {
                try
                {
                    PlotModel model = ((GraphFeedback)data[position]).CreatePlotModel();
                    (viewHolder as ResultViewGraphHolder).PlotGraph(model);
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }
    }

    public class ResultViewBaseHolder : RecyclerView.ViewHolder
    {
        public TextView title;
        public TextView caption;

        public ResultViewBaseHolder(View v) : base(v)
        {
            title = v.FindViewById<TextView>(Resource.Id.resultCard_title);
            caption = v.FindViewById<TextView>(Resource.Id.resultCard_caption);
        }
    }

    public class ResultViewPercentHolder : ResultViewBaseHolder
    {
        public RadialProgressView percent;

        public ResultViewPercentHolder(View v) : base(v)
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

    public class ResultViewGraphHolder : ResultViewBaseHolder
    {
        public PlotView plotView;

        public ResultViewGraphHolder(View v) : base(v)
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