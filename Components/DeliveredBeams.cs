using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using ZapReport.Extensions;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class DeliveredBeams : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "DeliveredBeams";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public DeliveredBeams(PlanConfig config, PrintData printData)
        {
            _config = config;
            _printData = printData;
        }

        public override void Compose(IContainer container)
        {
            if (_printData.DeliveryData == null)
            {
                return;
            }

            if (_printData.PlanBeamData?.IsocenterSet?.Isocenters == null)
            {
                return;
            }

            container.EnsureSpace(100).Column(column =>
            {
                column.Item().PaddingBottom(10).Text(ComponentCaption).Style(Style.Title);
                column.Item().Element(ComposeDeliveredBeamsTable);
            });
        }

        private void ComposeDeliveredBeamsTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(55);
                    columns.ConstantColumn(55);
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(80);
                    columns.RelativeColumn();
                    columns.ConstantColumn(65);
                });

                table.Header(header =>
                {
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("Fraction"));
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("IsocenterId"));
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("NodeId"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("PlanMU"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("DeliveredMU"));
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("DeliveredTime"));
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("kVImage"));
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[" + Translate.GetString("MU") + "]");
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[" + Translate.GetString("MU") + "]");
                });

                foreach (var fraction in _printData.DeliveryData.Fractions)
                {
                    if (_printData.DeliveredFraction != 0 && _printData.DeliveredFraction != fraction.ID)
                    {
                        continue;
                    }

                    var isocenterID = 1;

                    foreach (var path in fraction.PathSet)
                    {
                        var nodeID = 1;

                        foreach (var beam in path.Beams.OrderBy(b => b.TreatmentTime))
                        {
                            table.Cell().Element(Style.TableContentCenter).Text($"{fraction.ID.ToString("0")}");
                            table.Cell().Element(Style.TableContentCenter).Text($"{isocenterID.ToString("0")}");
                            table.Cell().Element(Style.TableContentCenter).Text($"{nodeID.ToString("0")}");
                            table.Cell().Element(Style.TableContentRight).Text($"{beam.PlanMU.ToString("0.00")}");
                            table.Cell().Element(Style.TableContentRight).Text($"{beam.DeliveredMU.ToString("0.00")}");
                            table.Cell().Element(Style.TableContentCenter).Text($"{beam.DeliveryTime.ToMinSec()}");

                            var images = fraction.KVImages.Where(i => i.PathUUID.Equals(path.PathUUID, StringComparison.OrdinalIgnoreCase) && i.Node?.NodeID == beam.NodeID).Count();

                            table.Cell().Element(Style.TableContentCenter).Text($"{(images > 0 ? images.ToString("0") : "")}");

                            nodeID++;
                        }

                        isocenterID++;
                    }
                }
            });
        }
    }
}
