using QuestPDF.Infrastructure;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZapClient.Data;
using ZapReport.Objects;
using ZapTranslation;

namespace ZapReport.Helpers
{
    internal class Diagrams
    {
        public static byte[] GenerateRotationsPlot(Size size, Fraction fraction, string caption)
        {
            var logData = ((List<LogFractionEntry>)fraction.LogData).FirstOrDefault();

            if (logData == null)
            { 
                return null;
            }

            // Size given in 0.1 mm
            var plot = new ScottPlot.Plot((int)size.Width, (int)size.Height);

            plot.Title(caption, false, System.Drawing.Color.Black, 16);

            plot.XAxis.Label(Translate.GetString("Isocenters"), System.Drawing.Color.Black, size: 12, fontName: "Arial");
            plot.YAxis.Label(Translate.GetString("Deviation") + " [°]", System.Drawing.Color.Black, size: 12, fontName: "Arial");
            plot.YAxis.TickLabelFormat((d) => $"{d}");

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

            var indexAA = 0;

            foreach (var isocenter in logData.Isocenters)
            {
                var index = isocenter.Value.Index - 1;

                ticks[index] = isocenter.Value.Index;
                labels[index] = isocenter.Value.ID.ToString("0");

                pitchXs[index] = isocenter.Value.Index;
                pitchYs[index] = isocenter.Value.Rotation_Pitch;
                rollXs[index] = isocenter.Value.Index;
                rollYs[index] = isocenter.Value.Rotation_Roll;
                yawXs[index] = isocenter.Value.Index;
                yawYs[index] = isocenter.Value.Rotation_Yaw;

                if (isocenter.Value.IsAutoAlignment)
                {
                    pitchAAXs[indexAA] = isocenter.Value.Index;
                    pitchAAYs[indexAA] = isocenter.Value.Rotation_Pitch;
                    rollAAXs[indexAA] = isocenter.Value.Index;
                    rollAAYs[indexAA] = isocenter.Value.Rotation_Roll;
                    yawAAXs[indexAA] = isocenter.Value.Index;
                    yawAAYs[indexAA] = isocenter.Value.Rotation_Yaw;

                    indexAA++;
                }
            }

            var maxY = Math.Max(Math.Max(pitchYs.Max(), rollYs.Max()), yawYs.Max());
            var upperLimitY = Math.Ceiling(maxY);
            var minY = Math.Min(Math.Min(pitchYs.Min(), rollYs.Min()), yawYs.Min());
            var lowerLimitY = Math.Floor(minY);

            var legendLocation = Math.Abs(upperLimitY - maxY) > Math.Abs(lowerLimitY - minY) ? Alignment.UpperRight : Alignment.LowerRight;

            plot.SetAxisLimitsX(0, logData.Isocenters.Count + 1);
            plot.SetAxisLimitsY(lowerLimitY, upperLimitY);

            Array.Resize<double>(ref pitchAAXs, indexAA);
            Array.Resize<double>(ref pitchAAYs, indexAA);
            Array.Resize<double>(ref rollAAXs, indexAA);
            Array.Resize<double>(ref rollAAYs, indexAA);
            Array.Resize<double>(ref yawAAXs, indexAA);
            Array.Resize<double>(ref yawAAYs, indexAA);

            var colorPitch = System.Drawing.Color.FromArgb(255, 0, 0);
            var colorRoll = System.Drawing.Color.FromArgb(0, 255, 0);
            var colorYaw = System.Drawing.Color.FromArgb(0, 0, 255);
            var colorAA = System.Drawing.Color.FromArgb(0, 0, 0);

            plot.XAxis.ManualTickPositions(ticks, labels);
            plot.YAxis.ManualTickSpacing(0.5);

            var pitchPlot = plot.AddScatterLines(pitchXs, pitchYs, colorPitch, label: Translate.GetString("Pitch"));
            pitchPlot.MarkerShape = MarkerShape.filledCircle;
            pitchPlot.MarkerSize = 7;

            var pitchAAPlot = plot.AddScatterLines(pitchAAXs, pitchAAYs, colorAA, 0);
            pitchAAPlot.MarkerShape = MarkerShape.openCircle;
            pitchAAPlot.MarkerSize = 12;

            var rollPlot = plot.AddScatterLines(rollXs, rollYs, colorRoll, label: Translate.GetString("Roll"));
            rollPlot.MarkerShape = MarkerShape.filledTriangleUp;
            rollPlot.MarkerSize = 11;

            var rollAAPlot = plot.AddScatterLines(rollAAXs, rollAAYs, colorAA, 0);
            rollAAPlot.MarkerShape = MarkerShape.openCircle;
            rollAAPlot.MarkerSize = 12;

            var yawPlot = plot.AddScatterLines(yawXs, yawYs, colorYaw, label: Translate.GetString("Yaw"));
            yawPlot.MarkerShape = MarkerShape.filledSquare;
            yawPlot.MarkerSize = 7;

            var yawAAPlot = plot.AddScatterLines(yawAAXs, yawAAYs, colorAA, 0, label: Translate.GetString("AutoAlignment"));
            yawAAPlot.MarkerShape = MarkerShape.openCircle;
            yawAAPlot.MarkerSize = 12;

            var legend = plot.Legend();

            legend.Orientation = Orientation.Horizontal;
            legend.Location = legendLocation;
            legend.Padding = 10;

            var bitmap = plot.Render((int)size.Width / 3, (int)size.Height / 3, false, 10);

            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                stream.Seek(0, SeekOrigin.Begin);

                return stream.ToArray();
            }
        }

        public static byte[] GenerateMVImagerPlot(Size size, Fraction fraction, string caption)
        {
            var logData = ((List<LogFractionEntry>)fraction.LogData).FirstOrDefault();

            if (logData == null)
            {
                return null;
            }

            // Size given in 0.1 mm
            var plot = new ScottPlot.Plot((int)size.Width, (int)size.Height);

            plot.Title(caption, false, System.Drawing.Color.Black, 16);

            plot.XAxis.Label(Translate.GetString("Beams"), System.Drawing.Color.Black, size: 12, fontName: "Arial");
            plot.YAxis.Label(Translate.GetString("Deviation") + " [%]", System.Drawing.Color.Black, size: 12, fontName: "Arial");
            plot.YAxis.TickLabelFormat((d) => $"{d}");

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
                foreach (var beam in isocenter.Value.Beams)
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

            var legendLocation = Math.Abs(upperLimitY - maxY) > Math.Abs(lowerLimitY - minY) ? Alignment.UpperRight : Alignment.LowerRight;

            plot.SetAxisLimitsX(0, length + 1);
            plot.SetAxisLimitsY(lowerLimitY, upperLimitY);

            Array.Resize<double>(ref flagsXs, indexFlags);
            Array.Resize<double>(ref flagsYs, indexFlags);

            var colorValues = System.Drawing.Color.FromArgb(0, 0, 0);
            var colorFlags = System.Drawing.Color.FromArgb(0, 0, 0);
            var colorCum = System.Drawing.Color.FromArgb(0, 255, 0);

            //plot.XAxis.ManualTickPositions(ticks, labels);
            //plot.YAxis.AutomaticTickPositions();

            var cumPlot = plot.AddScatterLines(cumXs, cumYs, colorCum, label: Translate.GetString("Cum"));
            cumPlot.MarkerShape = MarkerShape.filledCircle;
            cumPlot.MarkerSize = 4;

            var valuesPlot = plot.AddScatterLines(valuesXs, valuesYs, colorValues, label: Translate.GetString("Beam"));
            valuesPlot.MarkerShape = MarkerShape.filledSquare;
            valuesPlot.MarkerSize = 4;

            var flagsPlot = plot.AddScatterLines(flagsXs, flagsYs, colorFlags, 0, label: Translate.GetString("Flagged"));
            flagsPlot.MarkerShape = MarkerShape.eks;
            flagsPlot.MarkerSize = 8;

            var legend = plot.Legend();

            legend.Orientation = Orientation.Horizontal;
            legend.Location = legendLocation;
            legend.Padding = 10;

            var bitmap = plot.Render((int)size.Width / 3, (int)size.Height / 3, false, 10);

            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                stream.Seek(0, SeekOrigin.Begin);

                return stream.ToArray();
            }
        }
    }
}