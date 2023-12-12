using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace ZapReport.Objects
{
    public static class LogFiles
    {

        #region Static Functions

        public static List<LogFile> CreateListOfFiles(string name, DateTime date)
        {
            var result = new List<LogFile>();

            if (Directory.Exists(name))
            {
                // Name is a directory
                result.AddRange(GetAllTreatmentViewFilesInDir(name, date));
            }
            else if (IsZipFile(name))
            {
                result.AddRange(GetAllTreatmentViewFilesInZip(name, date));
            }
            else if (File.Exists(name))
            {
                result.AddRange(new List<LogFile> { new LogFile(name, "TXT", "") });
            }

            return result;
        }

        public static List<LogFile> SortFiles(List<LogFile> files)
        {
            if (files.Count <= 1)
            {
                return new List<LogFile>(files);
            }

            files.Sort(new LogFileComparer(_regexFilename));

            return new List<LogFile>(files);
        }

        #endregion

        #region Private Static Functions

        private static Regex _regexFilename = new Regex(@"TreatmentView_(\d{4}-\d{2}-\d{2})\.log\.?(\d*)", RegexOptions.Compiled);

        private static bool IsZipFile(string filename)
        {
            if (!File.Exists(filename))
            {
                return false;
            }

            var buffer = new byte[4];
            FileStream file;

            try
            {
                file = File.Open(filename, FileMode.Open, FileAccess.Read);
            }
            catch
            {
                return false;
            }

            file.Read(buffer, 0, 4);
            file.Close();

            return buffer[0] == 0x50 && buffer[1] == 0x4b && buffer[2] == 0x03 && buffer[3] == 0x04;
        }

        private static List<LogFile> GetAllTreatmentViewFilesInDir(string path, DateTime date)
        {
            var result = new List<LogFile>();

            foreach (var filename in Directory.EnumerateFiles(path))
            {
                if (IsZipFile(filename))
                {
                    result.AddRange(GetAllTreatmentViewFilesInZip(filename, date));
                }
                if (IsCorrectFilename(filename, date))
                {
                    result.Add(new LogFile(Path.Combine(path, filename), "TXT", ""));
                }
            }

            foreach (var directory in Directory.EnumerateDirectories(path))
            {
                result.AddRange(GetAllTreatmentViewFilesInDir(directory, date));
            }

            return result;
        }

        private static List<LogFile> GetAllTreatmentViewFilesInZip(string filename, DateTime date)
        {
            var result = new List<LogFile>();

            using (ZipArchive archive = ZipFile.OpenRead(filename))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (IsCorrectFilename(entry.Name, date))
                    {
                        result.Add(new LogFile(entry.FullName, "ZIP", filename));
                    }
                }
            }

            return result;
        }

        private static bool IsCorrectFilename(string filename, DateTime date)
        {
            if (!_regexFilename.IsMatch(filename))
            {
                return false;
            }

            var match = _regexFilename.Match(filename);

            if (date != null)
            {
                return match.Groups[1].Value == date.ToString("yyyy-MM-dd");
            }

            return true;
        }

        #endregion
    }
}
