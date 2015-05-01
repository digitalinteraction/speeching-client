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
            PlotModel model = new PlotModel();

            model.Axes.Add(new DateTimeAxis { IntervalType = DateTimeIntervalType.Days, StringFormat = "dd MMM"});
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Key = LeftAxisLabel, Maximum = LeftAxisLength });

            LineSeries series = new LineSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.Red,
                MarkerFill = OxyColors.Red,
                Color = OxyColors.DarkRed,
                Smooth = true

            };

            DateTime now = DateTime.Now;

            for (int i = 0; i < DataPoints.Length; i++)
            {
                series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(DataPoints[i].XVal), DataPoints[i].YVal));
            }

            model.Series.Add(series);

            plotModel = model;
            return model;
        }
    }
}