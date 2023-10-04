using ZapSurgical.Data;

namespace ZapReport.Extensions
{
    public static class PatientExtensions
    {
        public static string PatientName(this Patient patient)
        {
            var patientName = $"{patient.LastName.Trim()}, {patient.FirstName.Trim()}";
            if (patient.MiddleName != null)
                patientName += patient.MiddleName;

            return patientName;
        }
    }
}
