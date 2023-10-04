using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Linq;
using ZapReport.Extensions;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class VOIs : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "VOIs";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public VOIs(PlanConfig config, PrintData printData)
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
                column.Item().Element(ComposeContourTable);
                column.Item().Height(7);
                column.Item().PaddingBottom(10).Text(Translate.GetString("CommentPointDose")).Style(Style.Default.FontSize(9));
            });
        }

        private void ComposeContourTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(16);
                    columns.RelativeColumn();
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(65);

                });

                table.Header(header =>
                {
                    header.Cell().ColumnSpan(2).RowSpan(2).Element(Style.TableHeaderLeft).AlignLeft().Text(Translate.GetString("Name"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("Volume"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("MinDose"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("MeanDose"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("MaxDose"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("MaxPointDose"));
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[mm³]");
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[cGy]");
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[cGy]");
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[cGy]");
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[cGy]");
                });

                foreach (var contour in _printData.PlanVOIData.VOISet.VOIs.OrderBy(v => v == null ? "zzzz" : (9 - (int)v.Type).ToString("0") + v.Name))
                {
                    if (_printData.PlanDVData.DVData == null)
                    {
                        continue;
                    }

                    var print = true;

                    // Check, if this contour should be printed
                    foreach (var entry in _config.DoNotPrintVOIsWith)
                    {
                        if (contour.Name.ToUpper().Contains(entry.ToUpper()))
                            print = false;
                    }

                    if (!print)
                        continue;

                    var dv = _printData.PlanDVData.DVData.Where(sd => sd.VOIUUID == contour.UUID).FirstOrDefault();

                    table.Cell().Background(contour.ColorAsArray(_config).ToColorString()).Element(Style.TableContentCenter).Text(contour.TypeAsString());
                    table.Cell().Element(Style.TableContentLeft).Text($"{contour.Name}");
                    table.Cell().Element(Style.TableContentRight).Text($"{contour.TotalVolume.ToString("#,#0.0")}");

                    if (dv is null)
                    {
                        table.Cell().Element(Style.TableContentRight).Text(" ");
                        table.Cell().Element(Style.TableContentRight).Text(" ");
                        table.Cell().Element(Style.TableContentRight).Text(" ");
                        table.Cell().Element(Style.TableContentRight).Text(" ");
                    }
                    else
                    {
                        table.Cell().Element(Style.TableContentRight).Text($"{dv.MinDose.ToString("#,#0.0")}");
                        table.Cell().Element(Style.TableContentRight).Text($"{dv.MeanDose.ToString("#,#0.0")}");
                        table.Cell().Element(Style.TableContentRight).Text($"{dv.MaxDose.ToString("#,#0.0")}");
                        var pointDose = Utilities.GetDoseForVolume(dv.DVHVolumeValues, dv.DVHDoseValues, 35);
                        // Is the lowest given volume smaller than point dose volume, but dose bigger than max dose
                        if (pointDose > dv.MaxDose)
                            pointDose = dv.MaxDose;
                        if (pointDose > 0)
                            table.Cell().Element(Style.TableContentRight).Text($"{pointDose.ToString("#,#0.0")}");
                        else
                            table.Cell().Element(Style.TableContentRight).Text(" ");
                    }
                }
            });
        }
    }
}
