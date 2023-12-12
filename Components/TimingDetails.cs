using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Linq;
using ZapReport.Extensions;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class TimingDetails : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "TimingDetails";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public TimingDetails(PlanConfig config, PrintData printData)
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

            if (_printData.DeliveryData.Fractions?.Count <= 0)
            {
                return;
            }

            container.EnsureSpace(100).Column(column =>
            {
                column.Item().PaddingBottom(10).Text(ComponentCaption).Style(Style.Title);
                column.Item().Element(ComposeTimingDetailsTable);
            });
        }

        private void ComposeTimingDetailsTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(50);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Element(Style.TableHeaderCenter).AlignLeft().Text(Translate.GetString("FractionId"));
                    header.Cell().Element(Style.TableHeaderCenter).Text(Translate.GetString("SetupTime"));
                    header.Cell().Element(Style.TableHeaderCenter).Text(Translate.GetString("GantryTime"));
                    header.Cell().Element(Style.TableHeaderCenter).Text(Translate.GetString("TableTime"));
                    header.Cell().Element(Style.TableHeaderCenter).Text(Translate.GetString("LinacTime"));
                    header.Cell().Element(Style.TableHeaderCenter).Text(Translate.GetString("ImageTime"));
                    header.Cell().Element(Style.TableHeaderCenter).Text(Translate.GetString("ProcessingTime"));
                    header.Cell().Element(Style.TableHeaderCenter).Text(Translate.GetString("DeliveryTime")); // Time from starting with AA to extracing patient
                });

                var totalSetup = 0.0;
                var totalDelivery = 0.0;
                var totalGantry = 0.0;
                var totalTable = 0.0;
                var totalLinac = 0.0;
                var totalImage = 0.0;
                var totalProcessing = 0.0;
                var total = 0.0;

                if (_printData?.DeliveryData?.Fractions != null) 
                {
                    foreach (var fraction in _printData.DeliveryData.Fractions.OrderBy(t => t.StartTime))
                    {
                        if (_printData.DeliveredFraction > 0 && fraction.ID != _printData.DeliveredFraction)
                        {
                            continue;
                        }

                        foreach (var treatment in fraction.Treatments)
                        {
                            // var processingTime = fraction.DeliveryTime > 0.0 ? fraction.DeliveryTime - fraction.SetupTime - fraction.GantryTime - fraction.TableTime - fraction.LinacTime - fraction.ImageTime : 0.0;
                            var processingTime = treatment.DeliveryTime > 0.0 ? treatment.DeliveryTime - treatment.GantryTime - treatment.TableTime - treatment.LinacTime : 0.0;

                            table.Cell().Element(Style.TableContentCenter).Text($"{treatment.Fraction.ID.ToString("0")}");
                            table.Cell().Element(Style.TableContentCenter).Text($"{treatment.SetupTime.ToHourMinSec()}");
                            table.Cell().Element(Style.TableContentCenter).Text($"{treatment.GantryTime.ToHourMinSec()}");
                            table.Cell().Element(Style.TableContentCenter).Text($"{treatment.TableTime.ToHourMinSec()}");
                            table.Cell().Element(Style.TableContentCenter).Text($"{treatment.LinacTime.ToHourMinSec()}");
                            table.Cell().Element(Style.TableContentCenter).Text($"{treatment.ImageTime.ToHourMinSec()}");
                            table.Cell().Element(Style.TableContentCenter).Text($"{processingTime.ToHourMinSec()}");
                            table.Cell().Element(Style.TableContentCenter).Text($"{treatment.DeliveryTime.ToHourMinSec()}");

                            totalSetup += treatment.SetupTime;
                            totalDelivery += treatment.DeliveryTime;
                            totalGantry += treatment.GantryTime;
                            totalTable += treatment.TableTime;
                            totalLinac += treatment.LinacTime;
                            totalImage += treatment.ImageTime;
                            totalProcessing += processingTime;
                            total += treatment.TotalTime;
                        }
                    }

                    //var fractionsInBeamSet = _printData.DeliveryData.Fractions.Where(f => f.ID == _printData.DeliveredFraction).Count();
                    //var fractionsInTreatmentReport = _printData.TreatmentReportData.Where(f => f.Fraction.ID == _printData.DeliveredFraction).Count();

                    if ((_printData.DeliveredFraction == 0 && _printData.DeliveryData.TotalTreatments > 1)) // || (fractionsInBeamSet != fractionsInTreatmentReport))
                    {
                        table.Cell().Element(Style.TableContentCenter).Text($"{Translate.GetString("FractionsAll")}");
                        table.Cell().Element(Style.TableContentCenter).Text($"{totalSetup.ToHourMinSec()}");
                        table.Cell().Element(Style.TableContentCenter).Text($"{totalGantry.ToHourMinSec()}");
                        table.Cell().Element(Style.TableContentCenter).Text($"{totalTable.ToHourMinSec()}");
                        table.Cell().Element(Style.TableContentCenter).Text($"{totalLinac.ToHourMinSec()}");
                        table.Cell().Element(Style.TableContentCenter).Text($"{totalImage.ToHourMinSec()}");
                        table.Cell().Element(Style.TableContentCenter).Text($"{totalProcessing.ToHourMinSec()}");
                        table.Cell().Element(Style.TableContentCenter).Text($"{totalDelivery.ToHourMinSec()}");
                    }
                }
            });
        }
    }
}
