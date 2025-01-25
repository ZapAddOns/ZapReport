using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class Isocenters : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "Isocenters";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public Isocenters(PlanConfig config, PrintData printData)
        {
            _config = config;
            _printData = printData;
        }

        public override void Compose(IContainer container)
        {
            if (_printData.PlanBeamData?.IsocenterSet?.Isocenters == null)
            {
                return;
            }

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
                    columns.ConstantColumn(20);
                    columns.RelativeColumn();
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("Id"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("Collimator"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("X"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("Y"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("Z"));
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("Beams"));
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("MU"));
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("Alignment"));
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[mm]");
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[mm]");
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[mm]");
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[mm]");
                });

                double oldX = double.MinValue;
                double oldY = double.MinValue;
                double oldZ = double.MinValue;

                foreach (var isocenter in _printData.PlanBeamData.IsocenterSet.Isocenters.OrderBy(i => i.ID))
                {
                    var isocenterFromPlanData = _printData.PlanData.IsocenterSet.Isocenters.Where(i => i.ID == isocenter.IsocenterID).FirstOrDefault();

                    table.Cell().Element(Style.TableContentCenter).Text($"{isocenter.IsocenterID.ToString("0")}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{isocenter.Collimator.Size.ToString("0.0")}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{isocenterFromPlanData.CTTarget[0].ToString("#,#0.00")}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{isocenterFromPlanData.CTTarget[1].ToString("#,#0.00")}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{isocenterFromPlanData.CTTarget[2].ToString("#,#0.00")}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{isocenter.IsocenterBeamSet.Beams.Where(b => b.MU > 0).Count().ToString("0")}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{isocenter.IsocenterBeamSet.Beams.Where(b => b.MU > 0).Select(b => b.MU).Sum().ToString("0.0")}");

                    if (oldX == double.MinValue && oldY == double.MinValue && oldZ == double.MinValue)
                    {
                        oldX = isocenterFromPlanData.CTTarget[0];
                        oldY = isocenterFromPlanData.CTTarget[1];
                        oldZ = isocenterFromPlanData.CTTarget[2];

                        table.Cell().Element(Style.TableContentCenter).Text($"AA").Bold().FontColor(Colors.Red.Medium);
                    }
                    else
                    {
                        var distance = Math.Sqrt(Math.Pow(oldX - isocenterFromPlanData.CTTarget[0], 2) + Math.Pow(oldY - isocenterFromPlanData.CTTarget[1], 2) + Math.Pow(oldZ - isocenterFromPlanData.CTTarget[2], 2));

                        if (distance > 30)
                            table.Cell().Element(Style.TableContentCenter).Text($"AA").Bold().FontColor(Colors.Red.Medium);
                        else
                            table.Cell().Element(Style.TableContentCenter).Text($"TA");

                        oldX = isocenterFromPlanData.CTTarget[0];
                        oldY = isocenterFromPlanData.CTTarget[1];
                        oldZ = isocenterFromPlanData.CTTarget[2];
                    }
                }
            });
        }
    }
}
