using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class AAImages : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "AAImages";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public AAImages(PlanConfig config, PrintData printData)
        {
            _config = config;
            _printData = printData;
        }

        public override void Compose(IContainer container)
        {
            if (_printData.DeliveryData == null || _printData.DeliveryData.Fractions == null || _printData.DeliveryData.Fractions.Count <= 0)
            {
                return;
            }

            var rootPath = "C:\\Temp"; // "C:\\ZapSurgical\\AlignmentImages";

            // Exists dirctory
            if (!Directory.Exists(rootPath))
            {
                return;
            }

            var imageFound = false;

            // Load all image files for the fractions
            foreach (var fraction in _printData.DeliveryData.Fractions)
            {
                // Is this the fraction we want to print or do we print them all
                if (_printData.DeliveredFraction > 0 && fraction.ID != _printData.DeliveredFraction)
                    continue;

                var path = Path.Combine(rootPath, fraction.UUID);

                if (Directory.Exists(path))
                {
                    // Directory for fraction exists, so read all images in this directory
                    var files = Directory.GetFiles(path, "*.png");

                    imageFound = imageFound || files.Length > 0;

                    fraction.AAImages.AddRange(files);
                }
            }

            if (!imageFound)
            {
                return;
            }

            container.EnsureSpace(100).Column(column =>
            {
                column.Item().Text(ComponentCaption).Style(Style.Title);
                column.Item().Element(ComposeAAImagesBody);
            });
        }

        private void ComposeAAImagesBody(IContainer container)
        {
            container.Column(c =>
            {
                foreach (var fraction in _printData.DeliveryData.Fractions)
                {
                    // Is this the fraction we want to print or do we print them all
                    if (_printData.DeliveredFraction > 0 && fraction.ID != _printData.DeliveredFraction)
                        continue;

                    foreach (var file in fraction.AAImages)
                    {
                        var date = File.GetCreationTime(file);
                        var text = string.Format(Translate.GetString("AAImagesCaption"), fraction.ID, date.ToShortDateString(), date.ToShortTimeString());

                        c.Item().PaddingTop(10).Text(text);
                        c.Item().PaddingTop(10).Image(file).FitWidth();
                    }
                }
            });
        }
    }
}
