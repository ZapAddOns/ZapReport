using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using ScottPlot.TickGenerators;
using System.Linq;
using ZapReport.Extensions;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class DoseVolumeHistograms : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "DoseVolumeHistograms";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public DoseVolumeHistograms(PlanConfig config, PrintData printData)
        {
            _config = config;
            _printData = printData;
        }

        public override void Compose(IContainer container)
        {
            container.Column(column =>
            {
                //column.Item().Width(0).Height(0).Canvas((canvas, size) => start = canvas.TotalMatrix.TransY / canvas.TotalMatrix.ScaleY);
                column.Item().PageBreak();
                column.Item().RotateLeft().Svg(GeneratePlot);
                column.Item().PageBreak();
            });
        }

        private string GeneratePlot(Size size)
        {
            var plot = new ScottPlot.Plot();

            plot.Axes.Title.Label.Text = ComponentCaption;
            plot.Axes.Title.Label.ForeColor = ScottPlot.Color.Gray(0);
            plot.Axes.Title.Label.FontSize = 16;

            plot.Axes.Bottom.Label.Text = Translate.GetString("Dose") + " [cGy]";
            plot.Axes.Bottom.Label.ForeColor = ScottPlot.Color.Gray(0);
            plot.Axes.Bottom.Label.FontSize = 12;
            plot.Axes.Bottom.Label.FontName = "Arial";

            plot.Axes.Left.Label.Text = Translate.GetString("Volume") + " [%]";
            plot.Axes.Left.Label.ForeColor = ScottPlot.Color.Gray(0);
            plot.Axes.Left.Label.FontSize = 12;
            plot.Axes.Left.Label.FontName = "Arial";
            plot.Axes.Left.TickGenerator = new NumericAutomatic { LabelFormatter = (d) => $"{d * 100:0}" };

            if (_printData.PlanDVData.DVHOverallMaxDose == 0)
            {
                // There is no information for the max dose, so draw nothing
                return null;
            }

            plot.Axes.Bottom.Min = 0;
            plot.Axes.Bottom.Max = _printData.PlanDVData.DVHOverallMaxDose;
            plot.Axes.Left.Min = 0;
            plot.Axes.Left.Max = 1.05;

            if (_printData.PlanDVData?.DVData != null)
            {
                foreach (var dvData in _printData.PlanDVData.DVData)
                {
                    var contour = _printData.PlanVOIData.VOISet.VOIs.Where(v => v.UUID == dvData.VOIUUID).FirstOrDefault();

                    var print = true;

                    // Check, if this contour should be printed
                    foreach (var entry in _config.DoNotPrintVOIsWith)
                    {
                        if (contour != null && contour.Name.ToUpper().Contains(entry.ToUpper()))
                            print = false;
                    }

                    if (!print)
                        continue;

                    var color = new ScottPlot.Color(0, 0, 0);

                    if (contour != null)
                    {
                        var colorArray = contour.ColorAsArray(_config);
                        color = ScottPlot.Color.FromColor(System.Drawing.Color.FromArgb(255, colorArray[1], colorArray[2], colorArray[3]));

                        var scatter = plot.Add.Scatter(dvData.DVHDoseValues, dvData.DVHVolumePercentValues);
                        scatter.Color = color;
                        scatter.MarkerShape = ScottPlot.MarkerShape.None;
                    }
                }
            }

            return plot.GetSvgHtml((int)size.Width, (int)size.Height);
        }
    }
}
