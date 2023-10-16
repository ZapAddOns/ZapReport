using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Drawing.Imaging;
using System.IO;
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
                column.Item().Image(GeneratePlot);
                column.Item().PageBreak();
            });
        }

        private byte[] GeneratePlot(Size size)
        {
            var factor = 72;
            var plot = new ScottPlot.Plot((int)size.Height * factor, (int)size.Width * factor);

            plot.Title(ComponentCaption, true, System.Drawing.Color.Black, 16);
            plot.XAxis.Label(Translate.GetString("Dose") + " [cGy]", System.Drawing.Color.Black, size: 12, fontName: "Arial");
            plot.YAxis.Label(Translate.GetString("Volume"), System.Drawing.Color.Black, size: 12, fontName: "Arial");
            plot.YAxis.TickLabelFormat((d) => $"{d * 100:0} %");

            if (_printData.PlanDVData.DVHOverallMaxDose == 0)
            {
                // There is no information for the max dose, so draw nothing
                return null;
            }

            plot.SetAxisLimitsY(0, 1.05);
            plot.SetAxisLimitsX(0, _printData.PlanDVData.DVHOverallMaxDose);

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

                    var color = System.Drawing.Color.Black;

                    if (contour != null)
                    {
                        var colorArray = contour.ColorAsArray(_config);
                        color = System.Drawing.Color.FromArgb(colorArray[1], colorArray[2], colorArray[3]);

                        plot.AddScatterLines(dvData.DVHDoseValues, dvData.DVHVolumePercentValues, color);
                    }
                }
            }

            // plt.AddTooltip(label: "Special Point", x: 17, y: ys[17]);

            var bitmap = plot.Render((int)size.Height, (int)size.Width, false, 10);

            bitmap.RotateFlip(System.Drawing.RotateFlipType.Rotate270FlipNone);

            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                stream.Seek(0, SeekOrigin.Begin);

                return stream.ToArray();
            }
        }
    }
}
