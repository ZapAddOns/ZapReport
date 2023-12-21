using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using ZapClient.Data;
using ZapReport.Helpers;
using ZapSurgical.Data;
using ZapTranslation;

namespace ZapReport.Components
{
    public class kVImagesShort : PrintComponent
    {
        private PlanConfig _config;
        private PrintData _printData;

        public static new string ComponentName = "kVImagesShort";

        public static new string ComponentCaption = Translate.GetString(ComponentName);

        public kVImagesShort(PlanConfig config, PrintData printData)
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
                column.Item().Element(ComposekVImagesSummary);
            });
        }

        private void ComposekVImagesSummary(IContainer container)
        {
            int totalInTreatmentImages = 0;
            int totalAlignmentImages = 0;
            int totalAAImages = 0;
            double totalDoseInMicroGy = 0.0;
            Dictionary<(short, short, short), int> parameters = new Dictionary<(short, short, short), int>();

            foreach (var fraction in _printData.DeliveryData.Fractions)
            {
                if (_printData.DeliveredFraction != 0 && _printData.DeliveredFraction != fraction.ID)
                {
                    continue;
                }

                // AA images don't belong to a node
                totalAAImages += fraction.KVImages.Where(i => i.Node == null).Count();
                totalInTreatmentImages += fraction.KVImages.Where(i => i.Node != null).Count();
                totalDoseInMicroGy = fraction.KVImages.Sum(i => i.KVDoseMicroGy);

                foreach (var treatment in fraction.Treatments)
                {
                    foreach (var path in treatment.Paths)
                    {
                        var treatmentImages = fraction.KVImages.Where(i => i.PathUUID.Equals(path.PathUUID, StringComparison.OrdinalIgnoreCase) && i.Node != null);
                        //totalInTreatmentImages += treatmentImages.Count();
                        //totalDoseInMicroGy += CalcDoseForkVImages(treatmentImages);
                        //var alignmentImages = fraction.KVImages.Where(i => i.PathUUID.Equals(path.PathUUID, StringComparison.OrdinalIgnoreCase) && i.Node == null);
                        //totalAlignmentImages += alignmentImages.Count();
                        //totalDoseInMicroGy += CalcDoseForkVImages(alignmentImages);

                        foreach (var image in treatmentImages)
                        {
                            var index = (image.KV, image.MA, image.MS);

                            if (parameters.ContainsKey(index))
                            {
                                parameters[index]++;
                            }
                            else
                            {
                                parameters[index] = 1;
                            }
                        }
                    }
                }
            }

            int totalImages = totalInTreatmentImages + totalAAImages;
            //int totalTAImages = 2 * totalAlignmentImages - totalAAImages;
            //totalTAImages = totalTAImages > 0 ? totalTAImages : 0;

            var keyForMostCommonParameter = parameters.FirstOrDefault(x => x.Value == parameters.Values.Max()).Key;

            short mostCommonKV = keyForMostCommonParameter.Item1;
            short mostCommonMA = keyForMostCommonParameter.Item2;
            short mostCommonMS = keyForMostCommonParameter.Item3;

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(270);
                    columns.RelativeColumn();
                });

                table.Cell().Text(Translate.GetString("TotalkVImages")).Style(Style.Bold);
                table.Cell().Text($"{totalImages.ToString("0")}");
                table.Cell().Text("    " + Translate.GetString("TotalkVImagesForAA")).Style(Style.Bold);
                table.Cell().Text($"{totalAAImages.ToString("0")}");
                //table.Cell().Text("    " + Translate.GetString("TotalkVImagesForTA")).Style(Style.Bold);
                //table.Cell().Text($"{totalTAImages.ToString("0")}");
                table.Cell().Text("    " + Translate.GetString("TotalkVImagesForInTreatment")).Style(Style.Bold);
                table.Cell().Text($"{totalInTreatmentImages.ToString("0")}");
                table.Cell().Text(Translate.GetString("TotalkVDose")).Style(Style.Bold);
                table.Cell().Text($"{totalDoseInMicroGy.ToString("0.0")} µGy");
                table.Cell().Text(Translate.GetString("MostCommonParameter")).Style(Style.Bold);
                table.Cell().Text($"kV = {mostCommonKV.ToString("0")}, mA = {mostCommonMA.ToString("0")}, ms = {mostCommonMS.ToString("0")}");
            });
        }

        private double CalcDoseForkVImages(IEnumerable<KVImage> images)
        {
            double totalDose = 0.0;

            foreach(var image in images) 
            {
                totalDose += image.KVDoseMicroGy;
            }

            return totalDose;
        }
    }
}
