using System.Text.RegularExpressions;

namespace ZapReport.Objects
{
    internal class LogRegEx
    {
        // RegEx for decimal numbers with or without scientific notation
        // [+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?

        // Regexs for getting information from the different log file lines.
        public static Regex RegexFractionStart = new Regex(@"(\d{2}.\d{2}.\d{4})\s(\d{2}:\d{2}:\d{2}\.\d{3}).*\{.EventType.:.FractionLoaded.,.EventInfo.:\{.PlanName.:.(.*).,.FractionId.:(\d*)\}\}", RegexOptions.Compiled);
        public static Regex RegexIsocenterStart = new Regex(@"(\d{2}.\d{2}.\d{4})\s(\d{2}:\d{2}:\d{2}\.\d{3}).*\{.EventType.:.IsoCenterStarted.,.EventInfo.:\{.IsoCenterId.:(\d*),.PathIndex.:(\d*),.BeamsRemaining.:(\d*)\}\}", RegexOptions.Compiled);
        public static Regex RegexRotations = new Regex(@"(\d{2}.\d{2}.\d{4})\s(\d{2}:\d{2}:\d{2}\.\d{3}).*Adding new rotation data points at X:\s*(\d*),\s*Pitch\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?),\s*Roll\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?),\s*Yaw\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?),\s*isocenterLabel\s*=\s*(\d*),\s*isAutoAlignment:\s*(\w*).*", RegexOptions.Compiled);
        public static Regex RegexIsocenterEnd = new Regex(@"(\d{2}.\d{2}.\d{4})\s(\d{2}:\d{2}:\d{2}\.\d{3}).*\{.EventType.:.IsoCenterCompleted.,.EventInfo.:\{.IsoCenterId.:(\d*),.PathIndex.:(\d*),.TotalTimeInSeconds.:([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?)\}\}", RegexOptions.Compiled);
        public static Regex RegexFractionEnd = new Regex(@"(\d{2}.\d{2}.\d{4})\s(\d{2}:\d{2}:\d{2}\.\d{3}).*\{.EventType.:.FractionCompleted.,.EventInfo.:\{.PlanName.:.(.*).,.FractionId.:(\d*),.TotalTimeInSeconds.:([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?)\}\}", RegexOptions.Compiled);

        // The information about one beam is distributed over more than one line. 
        public static Regex RegexMeasured = new Regex(@"(\d{2}.\d{2}.\d{4})\s(\d{2}:\d{2}:\d{2}\.\d{3}).*TUIController.OnMVImagerDoseMeasured.*intensity:\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?)\s*estDose:\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?).*fieldSizeMm:\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?).*id:\s*(-?\d*).*isValid:\s*(\w*)", RegexOptions.Compiled);
        public static Regex RegexDoseChecker = new Regex(@"(\d{2}.\d{2}.\d{4})\s(\d{2}:\d{2}:\d{2}\.\d{3}).*MVImageDoseChecker:\s*node:\s*(\d*).*beamDoseLIN:\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?).*nominalDoseIMG:\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?).*beamDoseIMG:\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?).*beamPercErr:\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?).*FlaggedErroneous:\s*(\w*).*", RegexOptions.Compiled);
        public static Regex RegexCumulative = new Regex(@"(\d{2}.\d{2}.\d{4})\s(\d{2}:\d{2}:\d{2}\.\d{3}).*MVImageDoseChecker:\s*Cumulative\s*\(LIN IMG DIFF PCENT\)\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?)\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?)\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?)\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?)", RegexOptions.Compiled);
        public static Regex RegexMVImageEnd = new Regex(@"(\d{2}.\d{2}.\d{4})\s(\d{2}:\d{2}:\d{2}\.\d{3}).*MVImageDoseChecker:\s*Removed\s*nodeIndex.*from\s*MVImage\s*checklist", RegexOptions.Compiled);
        //public static Regex RegexSystemData = new Regex(@"(\d{2}.\d{2}.\d{4})\s(\d{2}:\d{2}:\d{2}\.\d{3}).*SystemDeliveryData\s*TreatmentType:>\s*(\w*),\s*isTreatment:>\s*(\w*),\s*MV:>\s*(\d*\.?\d*)\s*MU,\s*Collimator:>\s*(\d*\.?\d*)\s*mm,\s*Axial:>\s*(\d*\.?\d*),\s*Oblique:>\s*(\d*\.?\d*),\s*PlanName:>\s*(.*)", RegexOptions.Compiled);
        public static Regex RegexSystemData = new Regex(@"(\d{2}.\d{2}.\d{4})\s(\d{2}:\d{2}:\d{2}\.\d{3}).*SystemDeliveryData\s*TreatmentType:>\s*(\w*),\s*isTreatment:>\s*(\w*),\s*MV:>\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?)\s*MU,\s*Collimator:>\s*([\w\.]*)\s*mm,\s*Axial:>\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?),\s*Oblique:>\s*([+-]?[0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*[eE][+-]?[0-9]+?),\s*PlanName:>\s*(.*)", RegexOptions.Compiled);
        public static Regex RegexGantryMoveCompleted = new Regex(@"(\d{2}.\d{2}.\d{4})\s(\d{2}:\d{2}:\d{2}\.\d{3}).*OnGantryMoveCompleted.*", RegexOptions.Compiled);
        public static Regex RegexEnd = new Regex(@"(\d{2}.\d{2}.\d{4})\s(\d{2}:\d{2}:\d{2}\.\d{3}).*PlanRecorder: Saved ongoing plan data in file.*", RegexOptions.Compiled);
    }
}
