using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using ZapReport.Extensions;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class PlanInformation : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "PlanInformation";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public PlanInformation(PlanConfig config, PrintData printData)
        {
            _config = config;
            _printData = printData;
        }

        public override void Compose(IContainer container)
        {
            if (_printData.Plan == null)
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

                table.Cell().Text(Translate.GetString("Planname")).Style(Style.Bold);
                table.Cell().Text(_printData.Plan.PlanName.Trim());
                table.Cell().Text(Translate.GetString("Status")).Style(Style.Bold);
                table.Cell().Text(_printData.Plan.Status.AsString() + (_printData.Plan.IsSimulation ? ", " + Translate.GetString("Simulation") : ""));
                if (_printData.Plan.Status == ZapSurgical.Data.PlanStatus.Deliverable || _printData.Plan.Status == ZapSurgical.Data.PlanStatus.PartiallyDelivered || _printData.Plan.Status == ZapSurgical.Data.PlanStatus.FullyDelivered)
                {
                    var approver = _printData.PlanData.Approver?.Trim();
                    table.Cell().Text(Translate.GetString("Approved")).Style(Style.Bold);
                    table.Cell().Text((approver ?? Translate.GetString("Unknown")) + " " + Translate.GetString("at") + $" {_printData.Plan.ApprovalTime}");
                }
                table.Cell().Text(Translate.GetString("SavedDate")).Style(Style.Bold);
                table.Cell().Text(_printData.Plan.LastSavedTime.ToString("G"));
                if (_printData.PrimarySeries != null)
                {
                    table.Cell().Text(Translate.GetString("PrimarySeries")).Style(Style.Bold);
                    var primarySeries = $"{_printData.PrimarySeries.Modality}, {_printData.PrimarySeries.SeriesDescription.Trim().FromUnicode()}, {_printData.PrimarySeries.ScanDateTime.ToString("d")}, {_printData.PrimarySeries.NumOfInstances.ToString("0")} " + Translate.GetString("Images");
                    table.Cell().Text(primarySeries);
                }
                table.Cell().Text(Translate.GetString("DVSpacing")).Style(Style.Bold);
                table.Cell().Text(_printData.PlanData.DoseVolumePixelSpacing.ToString("#,#0.0 mm"));

                if (_printData.SecondarySeries != null && _printData.SecondarySeries.Count >= 1 && _printData.SecondarySeries[0] != null)
                {
                    table.Cell().Text(Translate.GetString("SecondarySeries")).Style(Style.Bold);
                    foreach (var series in _printData.SecondarySeries)
                    {
                        if (series == null)
                        {
                            continue;
                        }

                        var secondarySeries = $"{series.Modality}, {series.SeriesDescription.Trim().FromUnicode()}, {series.ScanDateTime.ToString("d")}, {series.NumOfInstances.ToString("0")} " + Translate.GetString("Images");
                        table.Cell().Text(secondarySeries);
                    }
                }

                if (_printData.PlanSummary != null)
                {
                    table.Cell().Text(" ");
                    table.Cell().Text(" ");
                    table.Cell().Text(Translate.GetString("Fractions")).Style(Style.Bold);
                    table.Cell().Text(_printData.PlanSummary.TotalFractions.ToString());
                    table.Cell().Text(Translate.GetString("Isocenters")).Style(Style.Bold);
                    table.Cell().Text(_printData.PlanSummary.TotalIsocenters.ToString());
                    table.Cell().Text(Translate.GetString("Beams")).Style(Style.Bold);
                    table.Cell().Text(_printData.PlanSummary.TotalBeamsWithMU.ToString());
                    table.Cell().Text(Translate.GetString("MUPerFraction")).Style(Style.Bold);
                    table.Cell().Text((_printData.PlanSummary.TotalMUs / _printData.PlanSummary.TotalFractions).ToString("#,#0.0") ?? " ");
                    table.Cell().Text(Translate.GetString("TotalMU")).Style(Style.Bold);
                    table.Cell().Text(_printData.PlanSummary.TotalMUs.ToString("#,#0.0"));
                    table.Cell().Text(Translate.GetString("PrescriptionDose")).Style(Style.Bold);
                    table.Cell().Text(_printData.PlanSummary.PrescribedDose.ToString("#,#0.0 cGy"));
                    table.Cell().Text(Translate.GetString("PrescriptionIsodose")).Style(Style.Bold);
                    table.Cell().Text((_printData.PlanSummary.PrescribedPercent / 100.0).ToString("#,#0.0 %"));
                    table.Cell().Text(Translate.GetString("MaximumDose")).Style(Style.Bold);
                    table.Cell().Text(_printData.PlanDVData?.DVHOverallMaxDose.ToString("#,#0.0 cGy") ?? " ");
                    table.Cell().Text(Translate.GetString("TreatmentTime")).Style(Style.Bold);
                    table.Cell().Text(TimeSpan.FromSeconds(_printData.PlanSummary.TotalTreatmentTime).ToString("hh\\:mm\\:ss"));
                }
            });
        }
    }
}
