using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using ZapReport.Extensions;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class Beams : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "Beams";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public Beams(PlanConfig config, PrintData printData)
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
                column.Item().Element(ComposeBeamsTable);
            });
        }

        private void ComposeBeamsTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(110);
                    columns.ConstantColumn(110);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("IsocenterId"));
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("NodeId"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("AxialDegrees"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("ObliqueDegrees"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("PlanMU"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("FractionMU"));
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[°]");
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[°]");
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[" + Translate.GetString("MU") + "]");
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[" + Translate.GetString("MU") + "]");
                });

                foreach (var isocenter in _printData.PlanBeamData.IsocenterSet.Isocenters.OrderBy(i => i.IsocenterID))
                {
                    var nodeId = 1;

                    if (isocenter?.IsocenterBeamSet?.Beams != null)
                    {
                        foreach (var beam in isocenter.IsocenterBeamSet.Beams.Where(b => b.MU > 0).OrderBy(b => b.DeliveryIndex))
                        {
                            table.Cell().Element(Style.TableContentCenter).Text($"{isocenter.IsocenterID.ToString("0")}");
                            table.Cell().Element(Style.TableContentCenter).Text($"{beam.DeliveryIndex.ToString("0")}");
                            if (beam.RotationCorrectionStatus == ZapClient.Data.RotationCorrectionStatus.Uncorrected)
                            {
                                table.Cell().Element(Style.TableContentCenter).Text($"{beam.AxialAngle.ToDegrees().ToString("0")}");
                                table.Cell().Element(Style.TableContentCenter).Text($"{beam.ObliqueAngle.ToDegrees().ToString("0")}");
                            }
                            else
                            {
                                table.Cell().Element(Style.TableContentCenter).Text($"{beam.AxialAngle.ToDegrees().ToString("0")} ({beam.CorrectedAxial.ToString("0.0")})");
                                table.Cell().Element(Style.TableContentCenter).Text($"{beam.ObliqueAngle.ToDegrees().ToString("0")} ({beam.CorrectedOblique.ToString("0.0")})");
                            }
                            table.Cell().Element(Style.TableContentRight).Text($"{beam.MU.ToString("#,#0.0")}");
                            table.Cell().Element(Style.TableContentRight).Text($"{(beam.MU / _printData.PlanSummary.TotalFractions).ToString("#,#0.0")}");

                            nodeId++;
                        }
                    }
                }
            });
        }
    }
}
