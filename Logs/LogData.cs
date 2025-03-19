using NLog;
using System;
using System.Collections.Generic;
using ZapReport.Objects;

namespace ZapReport.Logs
{
    internal class LogData
    {
        readonly static Logger _logger = LogManager.GetCurrentClassLogger();

        public static List<LogFractionEntry> CreateLogData(string rootPath, string planName, ZapClient.Data.Fraction fraction)
        {
            List<LogFractionEntry> fractionLogData = null;

            // Create a list with start and end time for all treatments of this fractions
            var dates = new List<(DateTime, DateTime)>();

            foreach (var treatment in fraction.Treatments)
            {
                // Add one second, so that all milliseconds are included
                dates.Add((treatment.StartTime, treatment.EndTime.AddSeconds(1)));
            }

            // Create log entries for all treatments in this fraction
            var logFiles = LogFiles.SortFiles(LogFiles.CreateListOfFiles(rootPath, dates));
            var logEntries = new LogEntries(logFiles);

            logEntries.CreateLogEntries(planName, dates);
            var logForTreatment = logEntries.GetEntriesForPlanAndDate(planName, dates);

            if (fractionLogData == null)
                fractionLogData = logForTreatment;

            return fractionLogData;
        }
    }
}
