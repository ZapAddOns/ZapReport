using NLog;
using QuestPDF.Infrastructure;
using ScottPlot;
using ScottPlot.TickGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using ZapReport.Objects;
using ZapTranslation;

namespace ZapReport.Helpers
{
    internal class Diagrams
    {
        readonly static Logger _logger = LogManager.GetCurrentClassLogger();

        public static string GenerateRotationsPlot(Size size, ZapClient.Data.Fraction fraction, string caption)
        {
            if (fraction.LogData == null)
            {
                _logger.Log(LogLevel.Info, $"No log data found for fraction {fraction.ID}");
                return null;
            }

            var logData = ((List<LogFractionEntry>)fraction.LogData).FirstOrDefault();

            if (logData == null)
            {
                _logger.Log(LogLevel.Warn, $"No rotation data for fraction {fraction.ID} found in log files");

                return null;
            }

            _logger.Log(LogLevel.Info, $"Found {logData.Isocenters.Count} isocenters");

            // Size given in 0.1 mm
            var plot = new Plot();

            plot.Axes.Title.Label.Text = caption;
            plot.Axes.Title.Label.ForeColor = ScottPlot.Color.Gray(0);
            plot.Axes.Title.Label.FontSize = 16;

            plot.Axes.Bottom.Label.Text = Translate.GetString("Isocenters");
            plot.Axes.Bottom.Label.ForeColor = ScottPlot.Color.Gray(0);
            plot.Axes.Bottom.Label.FontSize = 12;
            plot.Axes.Bottom.Label.FontName = "Arial";

            plot.Axes.Left.Label.Text = Translate.GetString("Deviation") + " [°]";
            plot.Axes.Left.Label.ForeColor = ScottPlot.Color.Gray(0);
            plot.Axes.Left.Label.FontSize = 12;
            plot.Axes.Left.Label.FontName = "Arial";
            plot.Axes.Left.TickGenerator = new NumericAutomatic { LabelFormatter = (d) => $"{d}" };

            var length = logData.Isocenters.Count;

            var ticks = new double[length];
            var labels = new string[length];

            var pitchXs = new double[length];
            var pitchYs = new double[length];
            var rollXs = new double[length];
            var rollYs = new double[length];
            var yawXs = new double[length];
            var yawYs = new double[length];

            var pitchAAXs = new double[length];
            var pitchAAYs = new double[length];
            var rollAAXs = new double[length];
            var rollAAYs = new double[length];
            var yawAAXs = new double[length];
            var yawAAYs = new double[length];

            var index = 0;
            var indexAA = 0;

            foreach (var isocenter in logData.Isocenters)
            {
                ticks[index] = index;
                labels[index] = isocenter.ID.ToString("0");

                pitchXs[index] = index;
                pitchYs[index] = isocenter.Rotation_Pitch;
                rollXs[index] = index;
                rollYs[index] = isocenter.Rotation_Roll;
                yawXs[index] = index;
                yawYs[index] = isocenter.Rotation_Yaw;

                if (isocenter.IsAutoAlignment)
                {
                    pitchAAXs[indexAA] = index;
                    pitchAAYs[indexAA] = isocenter.Rotation_Pitch;
                    rollAAXs[indexAA] = index;
                    rollAAYs[indexAA] = isocenter.Rotation_Roll;
                    yawAAXs[indexAA] = index;
                    yawAAYs[indexAA] = isocenter.Rotation_Yaw;

                    indexAA++;
                }

                index++;
            }

            var maxY = Math.Max(Math.Max(pitchYs.Max(), rollYs.Max()), yawYs.Max());
            var upperLimitY = Math.Ceiling(maxY);
            var minY = Math.Min(Math.Min(pitchYs.Min(), rollYs.Min()), yawYs.Min());
            var lowerLimitY = Math.Floor(minY);

            var legendLocation = Math.Abs(upperLimitY - maxY) > Math.Abs(lowerLimitY - minY) ? Alignment.UpperRight : Alignment.LowerRight;

            plot.Axes.Bottom.Min = 0;
            plot.Axes.Bottom.Max = logData.Isocenters.Count + 1;
            plot.Axes.Left.Min = lowerLimitY;
            plot.Axes.Left.Max = upperLimitY;

            Array.Resize<double>(ref pitchAAXs, indexAA);
            Array.Resize<double>(ref pitchAAYs, indexAA);
            Array.Resize<double>(ref rollAAXs, indexAA);
            Array.Resize<double>(ref rollAAYs, indexAA);
            Array.Resize<double>(ref yawAAXs, indexAA);
            Array.Resize<double>(ref yawAAYs, indexAA);

            var colorPitch = new ScottPlot.Color(255, 0, 0);
            var colorRoll = new ScottPlot.Color(0, 255, 0);
            var colorYaw = new ScottPlot.Color(0, 0, 255);
            var colorAA = new ScottPlot.Color(0, 0, 0);

            plot.Axes.Bottom.SetTicks(ticks, labels);
            plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericFixedInterval(0.5);

            var pitchPlot = plot.Add.Scatter(pitchXs, pitchYs);
            pitchPlot.Color = colorPitch;
            pitchPlot.LegendText = Translate.GetString("Pitch");
            pitchPlot.MarkerShape = MarkerShape.FilledCircle;
            pitchPlot.MarkerSize = 7;

            var pitchAAPlot = plot.Add.Scatter(pitchAAXs, pitchAAYs);
            pitchAAPlot.Color = colorAA;
            pitchAAPlot.LineStyle = LineStyle.None;
            pitchAAPlot.MarkerShape = MarkerShape.OpenCircle;
            pitchAAPlot.MarkerSize = 12;

            var rollPlot = plot.Add.Scatter(rollXs, rollYs);
            rollPlot.Color = colorRoll;
            rollPlot.LegendText = Translate.GetString("Roll");
            rollPlot.MarkerShape = MarkerShape.FilledTriangleUp;
            rollPlot.MarkerSize = 11;

            var rollAAPlot = plot.Add.Scatter(rollAAXs, rollAAYs);
            rollAAPlot.Color = colorAA;
            rollAAPlot.LineStyle = LineStyle.None;
            rollAAPlot.MarkerShape = MarkerShape.OpenCircle;
            rollAAPlot.MarkerSize = 12;

            var yawPlot = plot.Add.Scatter(yawXs, yawYs);
            yawPlot.Color = colorYaw;
            yawPlot.LegendText = Translate.GetString("Yaw");
            yawPlot.MarkerShape = MarkerShape.FilledSquare;
            yawPlot.MarkerSize = 7;

            var yawAAPlot = plot.Add.Scatter(yawAAXs, yawAAYs);
            yawAAPlot.Color = colorAA;
            yawAAPlot.LegendText = Translate.GetString("AutoAlignment");
            yawAAPlot.LineStyle = LineStyle.None;
            yawAAPlot.MarkerShape = MarkerShape.OpenCircle;
            yawAAPlot.MarkerSize = 12;

            plot.ShowLegend(Edge.Right);

            plot.Axes.Margins(horizontal: .1, vertical: .1);
 
            plot.ScaleFactor = 2;

            return plot.GetSvgHtml((int)size.Width, (int)size.Height);
        }

        public static string GenerateMVImagerPlot(Size size, ZapClient.Data.Fraction fraction, string caption)
        {
            if (fraction.LogData == null)
            {
                _logger.Log(LogLevel.Info, $"No log data found for fraction {fraction.ID}");
                return null;
            }

            var logData = ((List<LogFractionEntry>)fraction.LogData).FirstOrDefault();

            if (logData == null)
            {
                _logger.Log(LogLevel.Warn, $"No MV imager data for fraction {fraction.ID} found in log files");

                return null;
            }

            // Size given in 0.1 mm
            var plot = new ScottPlot.Plot();

            plot.Axes.Title.Label.Text = caption;
            plot.Axes.Title.Label.ForeColor = ScottPlot.Color.Gray(0);
            plot.Axes.Title.Label.FontSize = 16;

            plot.Axes.Bottom.Label.Text = Translate.GetString("Beams");
            plot.Axes.Bottom.Label.ForeColor = ScottPlot.Color.Gray(0);
            plot.Axes.Bottom.Label.FontSize = 12;
            plot.Axes.Bottom.Label.FontName = "Arial";

            plot.Axes.Left.Label.Text = Translate.GetString("Deviation") + " [%]";
            plot.Axes.Left.Label.ForeColor = ScottPlot.Color.Gray(0);
            plot.Axes.Left.Label.FontSize = 12;
            plot.Axes.Left.Label.FontName = "Arial";
            plot.Axes.Left.TickGenerator = new NumericAutomatic { LabelFormatter = (d) => $"{d}" };

            var length = logData.TotalBeams;

            //var ticks = new double[length];
            //var labels = new string[length];

            var valuesXs = new double[length];
            var valuesYs = new double[length];
            var cumXs = new double[length];
            var cumYs = new double[length];

            var flagsXs = new double[length];
            var flagsYs = new double[length];

            var index = 0;
            var indexFlags = 0;

            foreach (var isocenter in logData.Isocenters)
            {
                foreach (var beam in isocenter.Beams)
                {
                    //ticks[index] = index;
                    //labels[index] = isocenter.Value.ID.ToString("0");

                    valuesXs[index] = index;
                    valuesYs[index] = beam.DifferencePercent;
                    cumXs[index] = index;
                    cumYs[index] = beam.CumulativeDifferencePercent;

                    if (beam.IsFlagged)
                    {
                        flagsXs[indexFlags] = index;
                        flagsYs[indexFlags] = beam.DifferencePercent;

                        indexFlags++;
                    }

                    index++;
                }
            }

            var maxY = Math.Max(valuesYs.Max(), cumYs.Max());
            var upperLimitY = Math.Ceiling(maxY);
            var minY = Math.Min(valuesYs.Min(), cumYs.Min());
            var lowerLimitY = Math.Floor(minY);

            // If there is no data
            if (upperLimitY <= lowerLimitY)
            {
                return null;
            }

            var legendLocation = Math.Abs(upperLimitY - maxY) > Math.Abs(lowerLimitY - minY) ? Alignment.UpperRight : Alignment.LowerRight;

            plot.Axes.Bottom.Min = 0;
            plot.Axes.Bottom.Max = length + 1;
            plot.Axes.Left.Min = lowerLimitY;
            plot.Axes.Left.Max = upperLimitY;

            Array.Resize<double>(ref flagsXs, indexFlags);
            Array.Resize<double>(ref flagsYs, indexFlags);

            var colorValues = new ScottPlot.Color(0, 0, 0);
            var colorFlags = new ScottPlot.Color(0, 0, 0);
            var colorCum = new ScottPlot.Color(0, 255, 0);

            //plot.XAxis.ManualTickPositions(ticks, labels);
            //plot.YAxis.AutomaticTickPositions();

            var cumPlot = plot.Add.Scatter(cumXs, cumYs);
            cumPlot.Color = colorCum;
            cumPlot.LegendText = Translate.GetString("Cum") + $" ({cumYs.Last():0.0} %)";
            cumPlot.MarkerShape = MarkerShape.FilledCircle;
            cumPlot.MarkerSize = 4;

            var valuesPlot = plot.Add.Scatter(valuesXs, valuesYs);
            valuesPlot.Color = colorValues;
            valuesPlot.LegendText = Translate.GetString("Beam");
            valuesPlot.MarkerShape = MarkerShape.FilledSquare;
            valuesPlot.MarkerSize = 4;

            var flagsPlot = plot.Add.Scatter(flagsXs, flagsYs);
            flagsPlot.Color = colorFlags;
            flagsPlot.LegendText = Translate.GetString("Flagged");
            flagsPlot.LineStyle = LineStyle.None;
            flagsPlot.MarkerShape = MarkerShape.Eks;
            flagsPlot.MarkerSize = 8;

            plot.ShowLegend(Edge.Right);

            plot.Axes.Margins(horizontal: .05, vertical: 0.1);

            plot.ScaleFactor = 2;

            return plot.GetSvgHtml((int)size.Width, (int)size.Height);
        }
    }
}