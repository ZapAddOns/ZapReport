using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Linq;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class kVImages : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        private double _totalImages = 0;
        private double _totalDoseInMicroGy = 0.0;

        public static new string ComponentName = "kVImages";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public kVImages(PlanConfig config, PrintData printData)
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

            container.EnsureSpace(100).Column(column =>
            {
                column.Item().PaddingBottom(10).Text(ComponentCaption).Style(Style.Title);
                column.Item().Element(ComposekVImagesTable);
            });
        }

        private void ComposekVImagesTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(80);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("Fraction"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("AxialDegrees"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("ObliqueDegrees"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("DoseInMicroGy"));
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("Type"));
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("ImageAquired"));
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[°]");
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[°]");
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[µGy]");
                });

                foreach (var fraction in _printData.DeliveryData.Fractions.OrderBy(e => e.StartTime))
                {
                    if (_printData.DeliveredFraction != 0 && _printData.DeliveredFraction != fraction.ID)
                    {
                        continue;
                    }

                    foreach (var image in fraction.KVImages)
                    {
                        if (image.Type.Equals("MV"))
                        {
                            continue;
                        }

                        table.Cell().Element(Style.TableContentCenter).Text($"{fraction.ID.ToString("0")}");
                        table.Cell().Element(Style.TableContentCenter).Text($"{image.AxialDegrees.ToString("0")}");
                        table.Cell().Element(Style.TableContentCenter).Text($"{image.ObliqueDegrees.ToString("0")}");
                        table.Cell().Element(Style.TableContentRight).Text($"{image.KVDoseMicroGy.ToString("0.00")}");
                        table.Cell().Element(Style.TableContentCenter).Text(image.Node == null ? "AA" : $"{image.TreatmentType}");
                        table.Cell().Element(Style.TableContentCenter).Text($"{image.Timestamp}");

                        _totalDoseInMicroGy += image.KVDoseMicroGy;
                        _totalImages++;
                    }
                }
            });
        }
    }
}
