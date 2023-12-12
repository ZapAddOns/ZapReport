using System;

namespace ZapReport.Objects
{
    public class LogBeamEntry
    {
        public int Num;

        public DateTime Time { get; set; }

        public double Axial { get; set; }

        public double Oblique { get; set; }

        public int Node { get; set; }

        public double Intensity { get; set; }

        public double FieldSizeInMm { get; set; }

        public double PlannedMU { get; set; }

        public double DeliveredMU { get; set; }

        public double ImagerMU { get; set; }

        public double CumulativeDeliveredMU { get; set; }

        public double CumulativeImagerMU { get; set; }

        public double DifferenceMU { get => ImagerMU - DeliveredMU; }

        public double DifferencePercent { get => DeliveredMU == 0.0 ? 0.0 : (ImagerMU - DeliveredMU) / DeliveredMU * 100.0; }

        public double CumulativeDifferenceMU { get => CumulativeImagerMU - CumulativeDeliveredMU; }

        public double CumulativeDifferencePercent { get => CumulativeDeliveredMU == 0.0 ? 0.0 : (CumulativeImagerMU - CumulativeDeliveredMU) / CumulativeDeliveredMU * 100.0; }

        public bool IsValid { get; set; }

        public bool IsFlagged { get; set; }
    }
}
