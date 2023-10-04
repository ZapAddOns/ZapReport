using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class Signs : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;
        private bool _printPhysicianSign;
        private bool _printPhysicistSign;

        public static new string ComponentName = "Signs";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public Signs(PlanConfig config, PrintData printData, bool printPhysicianSign, bool printPhysicistSign)
        {
            _config = config;
            _printData = printData;
            _printPhysicianSign = printPhysicianSign;
            _printPhysicistSign = printPhysicistSign;
        }

        public override void Compose(IContainer container)
        {
            container.ShowEntire().Column(column =>
                {
                    column.Item().MinHeight(60).Row(row =>
                    {
                        row.RelativeItem(2).AlignBottom().Column(innerColumn =>
                        {
                            innerColumn.Item().Text(Translate.GetString("Physician"));
                            innerColumn.Item().Text(_printData.Physician?.Name ?? " ").Style(Style.Bold);
                        });
                        row.RelativeItem().AlignBottom().Column(innerColumn =>
                        {
                            innerColumn.Item().Text(Translate.GetString("Date"));
                            innerColumn.Item().Text(_printData.DatePhysician.ToString("d")).Style(Style.Bold);
                        });
                        var filename = Path.Combine("signs", (_printData.Physician?.UserID ?? string.Empty) + ".png");
                        if (_printData.Physician != null && _printPhysicianSign && File.Exists(filename))
                        {
                            row.ConstantItem(150)
                                .MinHeight(80)
                                .BorderBottom(Style.BorderSize)
                                .TranslateY(10)
                                .AlignBottom()
                                .Image(filename);
                        }
                        else
                        {
                            row.ConstantItem(150)
                                .MinHeight(80)
                                .BorderBottom(Style.BorderSize);
                        }
                    });
                    column.Item().MinHeight(60).Row(row =>
                    {
                        row.RelativeItem(2).AlignBottom().Column(innerColumn =>
                        {
                            innerColumn.Item().Text(Translate.GetString("Physicist"));
                            innerColumn.Item().Text(_printData.Physicist?.Name ?? " ").Style(Style.Bold);
                        });
                        row.RelativeItem().AlignBottom().Column(innerColumn =>
                        {
                            innerColumn.Item().Text(Translate.GetString("Date"));
                            innerColumn.Item().Text(_printData.DatePhysicist.ToString("d")).Style(Style.Bold);
                        });
                        var filename = Path.Combine("signs", (_printData.Physicist?.UserID ?? string.Empty) + ".png");
                        if (_printData.Physicist != null && _printPhysicistSign && File.Exists(filename))
                        {
                            row.ConstantItem(150)
                                .MinHeight(80)
                                .BorderBottom(Style.BorderSize)
                                .TranslateY(10)
                                .AlignBottom()
                                .Image(filename);
                        }
                        else
                        {
                            row.ConstantItem(150)
                                .MinHeight(80)
                                .BorderBottom(Style.BorderSize);
                        }
                    });
                });
        }
    }
}
