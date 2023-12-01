using NLog;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using ZapReport.Helpers;
using ZapReport.Objects;
using ZapTranslation;

namespace ZapReport.Components
{
    public class Measurements : PrintComponent
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "Measurements";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public Measurements(PlanConfig config, PrintData printData)
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

            var imageFound = false;

            // Get data for AA images

            var rootPath = _config.AAImagesFolder;

            // Exists dirctory
            if (!Directory.Exists(rootPath))
            {
                _logger.Warn($"Path {rootPath} couldn't be found");
            }
            else
            {
                // Load all image files for the fractions
                foreach (var fraction in _printData.DeliveryData.Fractions)
                {
                    // Is this the fraction we want to print or do we print them all
                    if (_printData.DeliveredFraction > 0 && fraction.ID != _printData.DeliveredFraction)
                        continue;

                    var path = Path.Combine(rootPath, fraction.UUID);

                    if (Directory.Exists(path))
                    {
                        // Directory for fraction exists, so save all filenames in this directory
                        var files = Directory.GetFiles(path, "*.png");

                        imageFound = imageFound || files.Length > 0;

                        fraction.AAImages.AddRange(files);
                    }
                }
            }

            if (!imageFound)
            {
                _logger.Warn($"No AA image found for any fraction");
            }

            // Get data for rotations diagram and MV imager diagram

            rootPath = _config.LogFolder;

            var foundLogs = false;

            // Exists dirctory
            if (!Directory.Exists(rootPath))
            {
                _logger.Warn($"Path {rootPath} couldn't be found");
            }
            else
            {
                // Create all diagrams for the fractions
                foreach (var fraction in _printData.DeliveryData.Fractions)
                {
                    // Is this the fraction we want to print or do we print them all
                    if (_printData.DeliveredFraction > 0 && fraction.ID != _printData.DeliveredFraction)
                        continue;

                    if (fraction.LogData == null)
                    {
                        var files = LogFiles.SortFiles(LogFiles.CreateListOfFiles(rootPath, fraction.StartTime));
                        var entries = new LogEntries(files);

                        entries.CreateLogEntries(_printData.PlanData.PlanName, fraction.StartTime);

                        fraction.LogData = entries.GetEntriesForPlanAndDate(_printData.PlanData.PlanName, fraction.StartTime);
                    }

                    foundLogs = foundLogs | fraction.LogData != null;
                }
            }

            if (!foundLogs)
            {
                _logger.Warn($"No log data found for any fraction");
            }

            container.EnsureSpace(600).Column(column =>
            {
                var size = new Size(2000, 600);
                var text = string.Empty;
                var date = DateTime.Now;

                column.Item().Text(ComponentCaption).Style(Helpers.Style.Title);

                foreach (var fraction in _printData.DeliveryData.Fractions)
                {
                    // Is this the fraction we want to print or do we print them all
                    if (_printData.DeliveredFraction > 0 && fraction.ID != _printData.DeliveredFraction)
                        continue;

                    // Print AA images

                    foreach (var file in fraction.AAImages)
                    {
                        date = File.GetCreationTime(file);
                        text = string.Format(Translate.GetString("AAImagesCaption"), fraction.ID, date.ToShortDateString(), date.ToShortTimeString());

                        column.Item().ShowEntire().Column(c =>
                        {
                            c.Item().PaddingTop(10).Text(text);
                            c.Item().PaddingTop(10).Image(file).FitWidth();
                        });
                    }

                    if (fraction.LogData == null)
                        continue;

                    // Print rotations diagram

                    date = fraction.StartTime;
                    text = string.Format(Translate.GetString("RotationDiagramCaption"), fraction.ID, date.ToShortDateString(), date.ToShortTimeString());
                    var image = Diagrams.GenerateRotationsPlot(size, fraction, text);

                    if (image != null)
                    {
                        column.Item().PaddingTop(10).Image(image).FitWidth();
                    }

                    // Print MVImager diagram

                    text = string.Format(Translate.GetString("MVImagerDiagramCaption"), fraction.ID, date.ToShortDateString(), date.ToShortTimeString());
                    image = Diagrams.GenerateMVImagerPlot(size, fraction, text);

                    if (image != null)
                    {
                        column.Item().PaddingTop(10).Image(image).FitWidth();
                    }
                }
            });
        }
    }
}
