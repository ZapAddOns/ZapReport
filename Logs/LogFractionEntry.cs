using System;
using System.Collections.Generic;

namespace ZapReport.Objects
{
    public class LogFractionEntry
    {
        public DateTime Time { get; set; }

        public string PlanName { get; set; } = string.Empty;

        public int Fraction { get; set; }

        public string TreatmentType { get; set; } = string.Empty;

        public bool IsTreatment { get; set; }

        public Dictionary<int, LogIsocenterEntry> Isocenters { get; set; } = new Dictionary<int, LogIsocenterEntry>();

        public int TotalBeams { get; set; }
    }
}
