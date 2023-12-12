using System;
using System.Collections.Generic;

namespace ZapReport.Objects
{
    public class LogIsocenterEntry
    {
        public DateTime Time { get; set; }

        public int ID { get; set; }

        public int Index { get; set; }

        public double ColliSize { get; set; }

        public List<LogBeamEntry> Beams { get; set; } = new List<LogBeamEntry>();

        public int TotalBeams { get; set; }

        public double TotalTimeInSeconds { get; set; }

        public double Rotation_Roll { get; set; }

        public double Rotation_Pitch { get; set; }

        public double Rotation_Yaw { get; set; }

        public bool IsAutoAlignment { get; set; }
    }
}
