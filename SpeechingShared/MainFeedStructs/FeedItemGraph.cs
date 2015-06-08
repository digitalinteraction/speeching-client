using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;

namespace SpeechingShared
{
    public class TimeGraphPoint
    {
        public double YVal;
        public DateTime XVal;
    }

    /// <summary>
    /// Feed item which contains a percentage statistic
    /// </summary>
    public class FeedItemGraph : FeedItemBase
    {
        public string BottomAxisLabel;
        public int BottomAxisLength;
        public string LeftAxisLabel;
        public int LeftAxisLength;
        public TimeGraphPoint[] DataPoints;

        private PlotModel plotModel;
        

        /// <summary>
        /// Returns an OxyPlot PlotModel from the current data
        /// </summary>
        /// <returns></returns>
        public PlotModel CreatePlotModel()
        {
            if(plotModel != null) return plotModel;

            PlotModel model = new PlotModel();

            model.Axes.Add(new DateTimeAxis
            {
                IntervalType = DateTimeIntervalType.Days,
                StringFormat = "dd MMM",
                IsPanEnabled = false,
                IsZoomEnabled = false
            });
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                TickStyle = TickStyle.Inside,
                Maximum = 5.4,
                Minimum = -0.4,
                MajorStep = 1,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MinimumRange = 5,
            });
            
            LineSeries series = new LineSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.Red,
                MarkerFill = OxyColors.Red,
                Color = OxyColors.DarkRed,
                Smooth = true
            };

            foreach (TimeGraphPoint t in DataPoints)
            {
                series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(t.XVal), t.YVal));
            }

            model.Series.Add(series);

            plotModel = model;
            return model;
        }
    }
}