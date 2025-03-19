using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ZapReport.Helpers;

namespace ZapReport.Objects
{
    public class LogEntries
    {
        readonly Logger _logger = LogManager.GetCurrentClassLogger();

        int _fileIndex;
        List<LogFile> _files;
        LogFile _activeFile;
        Dictionary<string, List<LogFractionEntry>> _logEntries;

        public LogEntries(List<LogFile> files)
        {
            _files = files;
            _fileIndex = -1;

            _logEntries = new Dictionary<string, List<LogFractionEntry>>();
        }

        public List<string> PlanNames { get => _logEntries.Keys.ToList(); }

        public void CreateLogEntries(string planName, List<(DateTime, DateTime)> dates)
        {
            var text = string.Empty;

            foreach (var date in dates)
            {
                if (text == string.Empty)
                    text = $"Create log entries for plan {planName} from {date.Item1} to {date.Item2}";
                else
                    text += $", from {date.Item1} to {date.Item2}";
            }

            _logger.Info(text);

            // Clear all log entries in case this function is called again
            _logEntries = new Dictionary<string, List<LogFractionEntry>>();

            var fraction = GetNextFraction(planName, dates);

            while (fraction != null)
            {
                if (fraction.IsTreatment)
                {
                    if (!_logEntries.ContainsKey(fraction.PlanName))
                    {
                        _logEntries[fraction.PlanName] = new List<LogFractionEntry>();
                    }

                    _logEntries[fraction.PlanName].Add(fraction);
                }

                fraction = GetNextFraction(planName, dates);
            }

            UpdateIsocenterValues();
        }

        public List<string> GetDatesForPlan(string planName)
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(planName))
            {
                return result;
            }

            if (!_logEntries.ContainsKey(planName))
            {
                return result;
            }

            foreach (var entry in _logEntries[planName])
            {
                var date = entry.Time.Date.ToShortDateString();

                if (!result.Contains(date))
                {
                    result.Add(date);
                }
            }

            return result;
        }

        public List<LogFractionEntry> GetEntriesForPlanAndDate(string planName, List<(DateTime, DateTime)> dates)
        {
            var result = new List<LogFractionEntry>();

            foreach (var date in dates)
            {
                if (!_logEntries.ContainsKey(planName))
                {
                    _logger.Log(LogLevel.Info, $"No log entries found for plan \"{planName}\"");

                    return result;
                }

                var startDate = date.Item1;
                var endDate = date.Item2;

                foreach (var entry in _logEntries[planName])
                {
                    if (entry.Time > startDate && entry.Time < endDate && entry != null)
                    {
                        result.Add(entry);
                    }
                }
            }

            return result;
        }

        private void UpdateIsocenterValues()
        {
            foreach (var plan in _logEntries.Keys)
            {
                var beamNum = 1;
                var lastDate = DateTime.MinValue;
                var lastBeamNum = -1;

                foreach (var fraction in _logEntries[plan])
                {
                    foreach (var isocenter in fraction.Isocenters)
                    {
                        foreach (var beam in isocenter.Beams)
                        {
                            if (beam.Node < lastBeamNum)
                            {
                                beamNum++;
                            }

                            if (fraction.Time.Date != lastDate)
                            {
                                beamNum = 1;
                                lastDate = fraction.Time.Date;
                            }

                            beam.Num = beamNum;

                            lastBeamNum = beam.Node;
                        }
                    }
                }
            }
        }

        private LogFractionEntry GetNextFraction(string planName, List<(DateTime, DateTime)> dates)
        {
            LogFractionEntry fraction;
            LogIsocenterEntry isocenter = null;
            Match match;
            int pos = 0;

            var line = GetNextLine(dates);
            pos++;

            while (line != null)
            {
                if (LogRegEx.RegexFractionStart.IsMatch(line))
                {
                    match = LogRegEx.RegexFractionStart.Match(line);

                    if (match.Groups[3].Value == planName && Utilities.CheckDate(match.Groups[1].Value, match.Groups[2].Value, dates))
                    { 
                        break; 
                    }
                }

                line = GetNextLine(dates);
                pos++;
            }

            if (line == null)
            {
                return null;
            }

            match = LogRegEx.RegexFractionStart.Match(line);

            // We now have the first line of a fraction

            fraction = new LogFractionEntry();

            fraction.Time = DateTime.ParseExact(match.Groups[1].Value + " " + match.Groups[2].Value, "MM.dd.yyyy HH:mm:ss.FFF", CultureInfo.InvariantCulture);
            fraction.PlanName = match.Groups[3].Value;
            fraction.Fraction = int.Parse(match.Groups[4].Value);

            _logger.Log(LogLevel.Info, $"Start of fraction {fraction.Fraction} of plan \"{fraction.PlanName}\"");

            while (line != null)
            {
                if (LogRegEx.RegexRotations.IsMatch(line))
                {
                    match = LogRegEx.RegexRotations.Match(line);

                    isocenter = new LogIsocenterEntry();

                    isocenter.Time = DateTime.ParseExact(match.Groups[1].Value + " " + match.Groups[2].Value, "MM.dd.yyyy HH:mm:ss.FFF", CultureInfo.InvariantCulture);
                    isocenter.Rotation_Pitch = double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                    isocenter.Rotation_Roll = double.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                    isocenter.Rotation_Yaw = double.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
                    isocenter.ID = int.Parse(match.Groups[7].Value);
                    isocenter.IsAutoAlignment = match.Groups[8].Value.ToUpper() == "TRUE" ? true : false;

                    fraction.Isocenters.Add(isocenter);

                    _logger.Log(LogLevel.Info, $"Found isocenter with ID {isocenter.ID} and {isocenter.Rotation_Pitch}/{isocenter.Rotation_Roll}/{isocenter.Rotation_Yaw} (P/R/Y) at {isocenter.Time}");
                }

                if (LogRegEx.RegexMeasured.IsMatch(line) && isocenter != null)
                {
                    var beam = GetNextContent(line, dates, ref fraction, ref isocenter);

                    if (beam != null)
                    {
                        isocenter.Beams.Add(beam);
                        isocenter.TotalBeams++;
                        fraction.TotalBeams++;
                    }

                    _logger.Log(LogLevel.Info, $"Found beam {beam.Node}/{beam.Axial}/{beam.Oblique}/{beam.PlannedMU} at {beam.Time}");
                }

                if (LogRegEx.RegexIsocenterEnd.IsMatch(line) && isocenter != null)
                {
                    match = LogRegEx.RegexIsocenterEnd.Match(line);

                    isocenter.TotalTimeInSeconds = double.Parse(match.Groups[5].Value);

                    _logger.Log(LogLevel.Info, $"Found end of isocenter with ID {isocenter.ID} at {DateTime.ParseExact(match.Groups[1].Value + " " + match.Groups[2].Value, "MM.dd.yyyy HH:mm:ss.FFF", CultureInfo.InvariantCulture)}");
                }

                if (LogRegEx.RegexFractionEnd.IsMatch(line))
                {
                    _logger.Log(LogLevel.Info, $"End of fraction {fraction.Fraction} of plan \"{fraction.PlanName}\" found at {DateTime.ParseExact(match.Groups[1].Value + " " + match.Groups[2].Value, "MM.dd.yyyy HH:mm:ss.FFF", CultureInfo.InvariantCulture)}");

                    return fraction;
                }

                line = GetNextLine(dates);
                pos++;
            }

            _logger.Log(LogLevel.Error, $"No end of fraction found. Perhaps a makeup fraction?");

            return fraction;
        }


        private LogBeamEntry GetNextContent(string activeLine, List<(DateTime, DateTime)> dates, ref LogFractionEntry fraction, ref LogIsocenterEntry isocenter)
        {
            LogBeamEntry beam;

            var line = activeLine;

            while (line != null && !LogRegEx.RegexMeasured.IsMatch(line))
            {
                line = GetNextLine(dates);
            }

            if (line == null)
            {
                return null;
            }

            // We now have the first line of a content

            beam = new LogBeamEntry();

            while (line != null)
            {
                if (LogRegEx.RegexMeasured.IsMatch(line))
                {
                    var match = LogRegEx.RegexMeasured.Match(line);

                    beam.Time = DateTime.ParseExact(match.Groups[1].Value + " " + match.Groups[2].Value, "MM.dd.yyyy HH:mm:ss.FFF", CultureInfo.InvariantCulture);
                    beam.Intensity = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                    beam.FieldSizeInMm = double.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                    beam.Node = int.Parse(match.Groups[6].Value);
                    beam.IsValid = match.Groups[7].Value.ToUpper() == "TRUE" ? true : false;
                }

                if (LogRegEx.RegexDoseChecker.IsMatch(line))
                {
                    var match = LogRegEx.RegexDoseChecker.Match(line);

                    beam.DeliveredMU = double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                    beam.ImagerMU = double.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                    beam.IsFlagged = match.Groups[8].Value.ToUpper() == "YES" ? true : false;
                }

                if (LogRegEx.RegexCumulative.IsMatch(line))
                {
                    var match = LogRegEx.RegexCumulative.Match(line);

                    beam.CumulativeDeliveredMU = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                    beam.CumulativeImagerMU = double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                }

                if (LogRegEx.RegexSystemData.IsMatch(line))
                {
                    var match = LogRegEx.RegexSystemData.Match(line);

                    fraction.TreatmentType = match.Groups[3].Value;
                    fraction.IsTreatment = match.Groups[4].Value.ToUpper() == "TRUE" ? true : false;
                    beam.PlannedMU = double.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                    if (double.TryParse(match.Groups[6].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double colliSize))
                        isocenter.ColliSize = colliSize;
                    beam.Axial = double.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture);
                    beam.Oblique = double.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture);
                    fraction.PlanName = match.Groups[9].Value;

                    return beam;
                }

                line = GetNextLine(dates);
            }

            return null;
        }

        private string GetNextLine(List<(DateTime, DateTime)> dates)
        {
            if (_files == null || _files.Count == 0 || _fileIndex >= _files.Count) 
            {
                return null;
            }

            // Is this the first call?
            if (_fileIndex == -1)
            {
                _fileIndex = 0;
                _activeFile = _files[_fileIndex];
                _activeFile.Open();
            }

            var line = _activeFile?.GetNextLine(dates);

            // Are there no more lines to check in this file left?
            while (line == null)
            {
                _fileIndex++;

                if (_fileIndex >= _files.Count)
                {
                    // No more files left
                    _activeFile?.Close();

                    return null;
                }

                _activeFile?.Close();

                _activeFile = _files[_fileIndex];

                _activeFile.Open();

                line = _activeFile.GetNextLine(dates);
            }

            return line;
        }
    }
}
