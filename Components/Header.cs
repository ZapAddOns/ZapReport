using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;
using ZapReport.Extensions;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class Header : IComponent
    {
        private PlanConfig _config;
        private PrintData _printData;
        private string _title;

        public Header(PlanConfig config, PrintData printData, string title)
        {
            _config = config;
            _printData = printData;
            _title = title;
        }

        public void Compose(IContainer container)
        {
            container
                .PaddingBottom(20)
                .Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text(_title).Style(Style.Title.FontSize(24).Bold());
                        column.Item().Row(innerRow =>
                        {
                            innerRow.ConstantItem(50).Column(innerColumn =>
                            {
                                innerColumn.Item().Text(Translate.GetString("Patient")).Style(Style.Default.SemiBold());
                                innerColumn.Item().Text(Translate.GetString("Plan")).Style(Style.Default.SemiBold());
                            });
                            innerRow.RelativeItem().Column(innerColumn =>
                            {
                                innerColumn.Item().Text($"{_printData.Patient.PatientName()} ({_printData.Patient.MedicalId.Trim()}), {Translate.GetString(_printData.Patient.Sex.ToString())}, {_printData.Patient.BirthDate.ToString("d")}").Style(Style.Default);
                                innerColumn.Item().Text($"{_printData.Plan.PlanName.Trim()}").Style(Style.Default);
                            });
                        });
                    });
                    var logo = System.IO.Path.Combine("logos", "logo.png");
                    if (File.Exists(logo))
                    {
                        // Only some logos need a translation ;-)
                        row.ConstantItem(120).TranslateY(-22).Image(logo).FitArea();
                    }
                    else
                    {
                        row.ConstantItem(120);
                    }
                });
        }
    }
}
