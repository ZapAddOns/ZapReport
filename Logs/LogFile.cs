using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using ZapReport.Helpers;

namespace ZapReport.Objects
{
    public class LogFile
    {
        readonly Logger _logger = LogManager.GetCurrentClassLogger();
        readonly string _file;
        readonly string _filetype;
        readonly string _zipPath;
        StreamReader _streamReader;
        ZipArchive _zipArchive;

        public LogFile(string filename, string filetype, string zipPath)
        {
            _file = filename;
            _filetype = filetype;
            _zipPath = zipPath;
        }

        public string Filename { get => Path.GetFileName(_file); }

        public bool Open()
        {
            try
            {
                switch (_filetype)
                {
                    case "TXT":
                        _streamReader = new StreamReader(File.Open(_file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        break;
                    case "ZIP":
                        _zipArchive = ZipFile.OpenRead(_zipPath);
                        _streamReader = new StreamReader(_zipArchive.GetEntry(_file).Open());
                        break;
                }
            }
            catch
            {
                _logger.Log(LogLevel.Error, $"File {Filename} could not be opened");

                return false;
            }

            _logger.Log(LogLevel.Info, $"File {Filename} opened");

            return true;
        }

        public bool Close()
        {
            try
            {
                _streamReader?.BaseStream.Close();
                _streamReader?.Close();
                _zipArchive?.Dispose();
            }
            catch
            {
                _logger.Log(LogLevel.Error, $"File {Filename} could not be closed");

                return false;
            }

            _logger.Log(LogLevel.Info, $"File {Filename} closed");

            return true;
        }

        public string GetNextLine(List<(DateTime, DateTime)> dates)
        {
            var line = _streamReader.ReadLine();

            if (line == null)
                return null;

            while (line != null)
            {
                if (LogRegEx.RegexDateTime.IsMatch(line))
                {
                    // Check, if lines date is valid
                    var match = LogRegEx.RegexDateTime.Match(line);
                    if (Utilities.CheckDate(match.Groups[1].Value, match.Groups[2].Value, dates))
                        return line;
                }

                line = _streamReader.ReadLine();
            }

            return null;
        }

        public override bool Equals(object obj)
        {
            var item = obj as LogFile;

            if (item == null)
            {
                return false;
            }

            return _file.Equals(item._file);
        }

        public override int GetHashCode()
        {
            return _file.GetHashCode();
        }
    }
}
