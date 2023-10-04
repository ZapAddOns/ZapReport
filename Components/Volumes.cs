using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class Volumes : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "Volumes";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public Volumes(PlanConfig config, PrintData printData)
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
                column.Item().Element(ComposeVolumesTable);
            });
        }

        private void ComposeVolumesTable(IContainer container)
        {
            double totalPTVVolume = 0.0;

            foreach (var contour in _printData.PlanVOIData.VOISet.VOIs.Where(v => v.TypeAsString() == "T"))
            {
                if (_printData.PlanDVData.DVData == null)
                {
                    continue;
                }

                var add = true;

                // Check, if this contour should be added to sum
                foreach (var entry in _config.DoNotPrintVOIsWith)
                {
                    if (contour.Name.ToUpper().Contains(entry.ToUpper()))
                        add = false;
                }

                if (!add)
                    continue;

                totalPTVVolume += contour.TotalVolume;
            }

            double V10BrainVolume = 0.0;
            int V10DoseVolume = 0;
            double V12BrainVolume = 0.0;
            int V12DoseVolume = 0;

            // Get volumes from structure Brain
            var structureBrain = _printData.PlanVOIData.VOISet.VOIs.Where((v) => v.Name.ToLower().Equals(_config.StructureForVolumes, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            if (structureBrain != null)
            {
                var structureDataBrain = _printData.PlanDVData.DVData.Where((d) => d.VOIUUID == structureBrain.UUID).FirstOrDefault();

                var reverseDose = structureDataBrain.DVHDoseValues.Reverse().ToArray();
                var reverseVolume = structureDataBrain.DVHVolumeValues.Reverse().ToArray();

                V10BrainVolume = Utilities.GetVolumeForDose(reverseDose, reverseVolume, 1000);
                V12BrainVolume = Utilities.GetVolumeForDose(reverseDose, reverseVolume, 1200);
            }

            // Get volumes from dose volume 
            if (_printData.PlanDoseVolumeGrid != null)
            {
                for (var k = 0; k < _printData.PlanDoseVolumeGrid.GridSize[0]; k++)
                {
                    for (var j = 0; j < _printData.PlanDoseVolumeGrid.GridSize[1]; j++)
                    {
                        for (var i = 0; i < _printData.PlanDoseVolumeGrid.GridSize[2]; i++)
                        {
                            var dose = _printData.PlanDoseVolumeGrid.Data[k][j][i];
                            if (dose >= 1000.0f)
                                V10DoseVolume++;
                            if (dose >= 1200.0f)
                                V12DoseVolume++;
                        }
                    }
                }
            }

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(150);
                    columns.ConstantColumn(150);
                });

                table.Header(header =>
                {
                    header.Cell().Element(Style.TableHeaderLeft).Text(" ");
                    header.Cell().Element(Style.TableHeaderCenter).Text(Translate.GetString("FromStructureBrain"));
                    header.Cell().Element(Style.TableHeaderCenter).Text(Translate.GetString("FromDoseVolume"));
                });

                table.Cell().Element(Style.TableContentLeft).Text($"{Translate.GetString("SumOfPTVs")}");
                table.Cell().Element(Style.TableContentCenter).Text($"{totalPTVVolume.ToString("#,#0.0 mm³")}");
                table.Cell().Element(Style.TableContentCenter).Text(" ");

                table.Cell().Element(Style.TableContentLeft).Text($"{Translate.GetString("V10Volume")}");
                table.Cell().Element(Style.TableContentCenter).Text(V10BrainVolume > 0 ? $"{(V10BrainVolume / 1000.0).ToString("0.0 cm³")}" : " ");
                table.Cell().Element(Style.TableContentCenter).Text(V10DoseVolume > 0 ? $"{(V10DoseVolume / 1000.0).ToString("0.0 cm³")}" : " ");

                table.Cell().Element(Style.TableContentLeft).Text($"{Translate.GetString("V12Volume")}");
                table.Cell().Element(Style.TableContentCenter).Text(V12BrainVolume > 0 ? $"{(V12BrainVolume / 1000.0).ToString("0.0 cm³")}" : " ");
                table.Cell().Element(Style.TableContentCenter).Text(V12DoseVolume > 0 ? $"{(V12DoseVolume / 1000.0).ToString("0.0 cm³")}" : " ");
            });
        }
    }
}
