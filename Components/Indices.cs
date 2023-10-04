using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Linq;
using ZapClient.Data;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class Indices : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "Indices";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public Indices(PlanConfig config, PrintData printData)
        {
            _config = config;
            _printData = printData;
        }

        public override void Compose(IContainer container)
        {
            if (_printData.PlanVOIData?.VOISet?.VOIs == null)
            {
                return;
            }

            container.EnsureSpace(100).Column(column =>
            {
                column.Item().PaddingBottom(10).Text(ComponentCaption).Style(Style.Title);
                column.Item().Element(ComposeIndicesTable);
            });
        }

        private void ComposeIndicesTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                });

                table.Header(header =>
                {
                    header.Cell().Element(Style.TableHeaderLeft).Text(Translate.GetString("Name"));
                    header.Cell().Element(Style.TableHeaderCenter).Text(Translate.GetString("Coverage"));
                    header.Cell().Element(Style.TableHeaderCenter).Text(Translate.GetString("CI"));
                    header.Cell().Element(Style.TableHeaderCenter).Text(Translate.GetString("nCI"));
                    header.Cell().Element(Style.TableHeaderCenter).Text(Translate.GetString("GI"));
                });

                foreach (var contour in _printData.PlanVOIData.VOISet.VOIs.Where(v => v.Type == VOIContourType.Target).OrderBy(v => v.TotalVolume)) //.OrderBy(v => v == null ? "zzzz" : (9 - v.Type).ToString("0") + v.Name))
                {
                    if (_printData.PlanDVData.DVData == null)
                    {
                        continue;
                    }

                    var print = true;

                    // Check, if this contour should be printed
                    foreach (var entry in _config.DoNotPrintPTVsWith)
                    {
                        if (contour.Name.ToUpper().Contains(entry.ToUpper()))
                            print = false;
                    }

                    if (!print)
                        continue;

                    var dv = _printData.PlanDVData.DVData.Where(sd => sd.VOIUUID == contour.UUID).FirstOrDefault();

                    table.Cell().Element(Style.TableContentLeft).Text($"{contour.Name}");

                    if (dv is null)
                    {
                        table.Cell().Element(Style.TableContentCenter).Text(" ");
                        table.Cell().Element(Style.TableContentCenter).Text(" ");
                        table.Cell().Element(Style.TableContentCenter).Text(" ");
                        table.Cell().Element(Style.TableContentCenter).Text(" ");
                    }
                    else
                    {
                        table.Cell().Element(Style.TableContentCenter).Text($"{dv.Coverage.ToString("0.00 %")}");
                        table.Cell().Element(Style.TableContentCenter).Text($"{dv.CI.ToString("0.00")}");
                        table.Cell().Element(Style.TableContentCenter).Text($"{dv.nCI.ToString("0.00")}");
                        table.Cell().Element(Style.TableContentCenter).Text($"{dv.GI.ToString("0.00")}");
                    }
                }
            });
        }
    }
}
