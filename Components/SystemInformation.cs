using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class SystemInformation : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "SystemInformation";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public SystemInformation(PlanConfig config, PrintData printData)
        {
            _config = config;
            _printData = printData;
        }

        public override void Compose(IContainer container)
        {
            if (_printData.PlanSystemData == null)
            {
                return;
            }

            container.EnsureSpace(100).Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(innerColumn =>
                    {
                        innerColumn.Item().PaddingBottom(10).Text(ComponentCaption).Style(Style.Title);
                        innerColumn.Item().Element(ComposeDetails);
                    });
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

                table.Cell().Text(Translate.GetString("System")).Style(Style.Bold);
                table.Cell().Text(string.Format(Translate.GetString("SystemText"), _printData.PlanSystemData.System));
                table.Cell().Text(Translate.GetString("LINACEnergy")).Style(Style.Bold);
                table.Cell().Text(Translate.GetString("LINACEnergyText"));
                table.Cell().Text(Translate.GetString("Commisioned")).Style(Style.Bold);
                table.Cell().Text(_printData.PlanSystemData.Commissioning.CommissioningDate.ToString());
                table.Cell().Text(Translate.GetString("TPSVersion")).Style(Style.Bold);
                table.Cell().Text(_printData.PlanSystemData.TPSBuildVersion);
            });
        }
    }
}
