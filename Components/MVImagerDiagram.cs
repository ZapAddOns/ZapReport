﻿using NLog;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.IO;
using ZapReport.Helpers;
using ZapReport.Logs;
using ZapReport.Objects;
using ZapTranslation;

namespace ZapReport.Components
{
    public class MVImagerDiagram : PrintComponent
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "MVImagerDiagram";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public MVImagerDiagram(PlanConfig config, PrintData printData)
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

            var rootPath = _config.LogFolder;

            // Exists dirctory
            if (!Directory.Exists(rootPath))
            {
                _logger.Warn($"Path {rootPath} couldn't be found");

                return;
            }

            var foundLogs = false;

            // Create all diagrams for the fractions
            foreach (var fraction in _printData.DeliveryData.Fractions)
            {
                // Is this the fraction we want to print or do we print them all
                if (_printData.DeliveredFraction > 0 && fraction.ID != _printData.DeliveredFraction)
                    continue;

                if (fraction.LogData == null)
                {
                    fraction.LogData = LogData.CreateLogData(rootPath, _printData.PlanData.PlanName, fraction);
                }

                foundLogs = foundLogs | fraction.LogData != null;
            }

            if (!foundLogs)
            {
                _logger.Warn($"No log data found for any fraction");

                return;
            }

            container.EnsureSpace(400).Column(column =>
            {
                var size = new Size(2100, 1300);

                column.Item().Text(ComponentCaption).Style(Helpers.Style.Title);

                foreach (var fraction in _printData.DeliveryData.Fractions)
                {
                    // Is this the fraction we want to print or do we print them all
                    if (_printData.DeliveredFraction > 0 && fraction.ID != _printData.DeliveredFraction)
                        continue;

                    if (fraction.LogData == null)
                        continue;

                    var date = fraction.StartTime;
                    var text = string.Format(Translate.GetString("MVImagerDiagramCaption"), fraction.ID, date.ToShortDateString(), date.ToShortTimeString());
                    var svg = Diagrams.GenerateMVImagerPlot(size, fraction, text);

                    if (svg != null)
                    {
                        column.Item().PaddingTop(10).Svg(SvgImage.FromText(svg)).FitWidth();
                    }
                }
            });
        }
    }
}
