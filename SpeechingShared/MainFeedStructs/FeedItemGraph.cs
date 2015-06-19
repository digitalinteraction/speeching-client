using System;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace SpeechingShared
{
    public class TimeGraphPoint
    {
        public DateTime XVal;
        public double YVal;
    }

    /// <summary>
    /// Feed item which contains a percentage statistic
    /// </summary>
    public class FeedItemGraph : FeedItemBase
    {
        public string BottomAxisLabel;
        public int BottomAxisLength;
        public TimeGraphPoint[] DataPoints;
        public string LeftAxisLabel;
        public int LeftAxisLength;
        private PlotModel plotModel;

        /// <summary>
        /// Returns an OxyPlot PlotModel from the current data
        /// </summary>
        /// <returns></returns>
        public PlotModel CreatePlotModel()
        {
            if (plotModel != null) return plotModel;

            PlotModel model = new PlotModel();

            model.Axes.Add(new DateTimeAxis
            {
                StringFormat = "dd MMM",
                IsPanEnabled = false,
                IsZoomEnabled = false,
                IntervalType = DateTimeIntervalType.Auto,
                MajorStep = 1,
                MinorStep = 0.5
            });
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                TickStyle = TickStyle.Inside,
                Maximum = 5.4, // Make sure the whole scale is visible at all times
                Minimum = -0.4,
                MinimumRange = 5,
                MajorStep = 1,
                MinorStep = 0.5,
                IsPanEnabled = false,
                IsZoomEnabled = false
            });

            model.DefaultFontSize = 21;

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