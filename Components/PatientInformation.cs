using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using ZapReport.Extensions;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class PatientInformation : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "PatientInformation";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public PatientInformation(PlanConfig config, PrintData printData)
        {
            _config = config;
            _printData = printData;
        }

        public override void Compose(IContainer container)
        {
            container.EnsureSpace(100).Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(innerColumn =>
                    {
                        innerColumn.Item().PaddingBottom(10).Text(ComponentCaption).Style(Style.Title);
                        innerColumn.Item().Element(ComposeDetails);
                    });

                    if (_printData.PictureOfPatient != null && _printData.PictureOfPatient.Length > 0)
                    {
                        row.ConstantItem(100)
                            .AlignTop()
                            .Image(_printData.PictureOfPatient);
                    }
                });
            });
        }

        public void ComposeDetails(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(130);
                    columns.RelativeColumn();
                });

                table.Cell().Text(Translate.GetString("MedicalId")).Style(Style.Bold);
                table.Cell().Text(_printData.Patient.MedicalId.Trim());
                table.Cell().Text(Translate.GetString("PatientName")).Style(Style.Bold);
                table.Cell().Text(_printData.Patient.PatientName());
                table.Cell().Text(Translate.GetString("Sex")).Style(Style.Bold);
                table.Cell().Text(Translate.GetString(_printData.Patient.Sex.ToString()));
                table.Cell().Text(Translate.GetString("Birthdate")).Style(Style.Bold);
                table.Cell().Text(_printData.Patient.BirthDate.ToString("d"));
            });
        }

    }
}
