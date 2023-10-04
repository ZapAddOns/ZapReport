using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class FractionSummary : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "FractionSummary";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public FractionSummary(PlanConfig config, PrintData printData)
        {
            _config = config;
            _printData = printData;
        }

        public override void Compose(IContainer container)
        {
            if (_printData.DeliveryData == null || _printData.DeliveryData.Fractions == null || _printData.DeliveryData.Fractions.Count <= 0)
            {
                return;
            }

            container.EnsureSpace(100).Column(column =>
            {
                column.Item().PaddingBottom(10).Text(ComponentCaption).Style(Style.Title);
                column.Item().Element(ComposeFractionSummaryTable);
            });
        }

        private void ComposeFractionSummaryTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(60);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.ConstantColumn(60);
                });

                table.Header(header =>
                {
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("FractionId"));
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("NonZeroBeams"));
                    header.Cell().BorderTop(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text(Translate.GetString("FractionMU"));
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("FractionStart"));
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("FractionEnd"));
                    header.Cell().RowSpan(2).Border(Style.BorderSize).Element(Style.TableHeaderCenter).Text(Translate.GetString("FractionTotal")); // Time from loading plan to extracting patient
                    header.Cell().BorderBottom(Style.BorderSize).Element(Style.TableHeaderCenterNoBorder).Text("[" + Translate.GetString("MU") + "]");
                });

                var totalBeams = 0;
                var totalMUs = 0.0;
                //DateTime? beginOfTreatment = null;
                //DateTime? endOfTreatment = null;
                var overallTime = new TimeSpan();

                foreach (var fraction in _printData.DeliveryData.Fractions)
                {
                    if (_printData.DeliveredFraction > 0 && fraction.ID != _printData.DeliveredFraction)
                        continue;

                    totalBeams += fraction.TotalBeam;
                    totalMUs += fraction.TotalDose;

                    table.Cell().Element(Style.TableContentCenter).Text($"{fraction.ID.ToString("0")}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{fraction.TotalBeam.ToString("0")}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{fraction.TotalDose.ToString("#,##0.0")}");

                    var startTime = string.Empty;
                    var endTime = string.Empty;
                    TimeSpan totalTime = new TimeSpan();

                    foreach (var treatment in fraction.Treatments)
                    {
                        startTime += !string.IsNullOrEmpty(startTime) ? Environment.NewLine : "";
                        endTime += !string.IsNullOrEmpty(startTime) ? Environment.NewLine : "";

                        startTime += treatment.StartTime.ToString();
                        endTime += treatment.EndTime.ToString();

                        totalTime += treatment.EndTime - treatment.StartTime;

                        //beginOfTreatment = beginOfTreatment != null && beginOfTreatment < treatment.StartTime ? beginOfTreatment : treatment.StartTime;
                        //endOfTreatment = endOfTreatment != null && endOfTreatment > treatment.EndTime ? endOfTreatment : treatment.EndTime;
                    }

                    table.Cell().Element(Style.TableContentCenter).Text($"{startTime}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{endTime}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{totalTime.ToString()}");

                    overallTime += totalTime;
                }

            //var fractionsInBeamSet = _printData.DeliveryData.Fractions.Where(f => f.ID == _printData.DeliveredFraction).Count();
            //var fractionsInTreatmentReport = _printData.TreatmentReportData.Where(f => f.Fraction.ID == _printData.DeliveredFraction).Count();

            if ((_printData.DeliveredFraction == 0 && _printData.DeliveryData.Fractions.Count > 1)) // || (fractionsInBeamSet != fractionsInTreatmentReport))
            {
                    table.Cell().Element(Style.TableContentCenter).Text($"{Translate.GetString("FractionsAll")}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{totalBeams.ToString("0")}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{totalMUs.ToString("#,##0.0")}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{_printData.DeliveryData.StartTime}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{_printData.DeliveryData.EndTime}");
                    table.Cell().Element(Style.TableContentCenter).Text($"{overallTime.ToString()}");
                }
            });
        }
    }
}
