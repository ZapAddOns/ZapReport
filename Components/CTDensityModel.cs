using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Linq;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class CTDensityModel : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "CTDensityModel";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public CTDensityModel(PlanConfig config, PrintData printData)
        {
            _config = config;
            _printData = printData;
        }

        public override void Compose(IContainer container)
        {
            container.EnsureSpace(100).Column(column =>
            {
                column.Item().PaddingBottom(10).Text(ComponentCaption).Style(Style.Title);
                column.Item().Element(ComposeIsocenterTable);
            });
        }

        private void ComposeIsocenterTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(100);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("CTValue"));
                    header.Cell().Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("Density"));
                    header.Cell().Border(Style.BorderSize).Element(Style.TableHeaderLeft).Text(Translate.GetString("Material"));
                });

                foreach (var entry in _printData.PlanSystemData.DensityModel.Data.OrderBy(d => d.CTValue))
                {
                    table.Cell().Element(Style.TableContentCenter).Text($"{entry.CTValue.ToString("0")}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{entry.Density.ToString("0.000")}");
                    table.Cell().Element(Style.TableContentLeft).Text($"{entry.Material}");
                }
            });
        }
    }
}
