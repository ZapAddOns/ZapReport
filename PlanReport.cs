using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using ZapReport.Components;
using ZapReport.Extensions;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport
{
    /// <summary>
    /// This class generates a PDF report for a treatment plan using QuestPDF.
    /// It dynamically composes the report content based on provided flags and components.
    /// The report includes a header, footer, and optional physician/physicist signatures.
    /// </summary>
    public class PlanReport : IDocument
    {
        private readonly PlanConfig _config;
        private readonly PrintData _printData;
        private readonly List<string> _listOfFlags;
        private readonly Dictionary<string, Type> _listOfPrintComponents;
        private readonly string _caption;
        private readonly bool _printPhysicianSign;
        private readonly bool _printPhysicistSign;

        public PlanReport(
            PlanConfig config,
            PrintData printData,
            Dictionary<string, Type> listOfPrintComponents,
            List<string> listOfFlags,
            string caption,
            bool? printPhysicianSign,
            bool? printPhysicistSign)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _printData = printData ?? throw new ArgumentNullException(nameof(printData));
            _listOfFlags = listOfFlags ?? throw new ArgumentNullException(nameof(listOfFlags));
            _listOfPrintComponents = listOfPrintComponents ?? throw new ArgumentNullException(nameof(listOfPrintComponents));
            _caption = caption ?? string.Empty;
            _printPhysicianSign = printPhysicianSign ?? false;
            _printPhysicistSign = printPhysicistSign ?? false;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.DefaultTextStyle(Style.Default);

                page.MarginLeft(63);
                page.MarginRight(57);
                page.MarginTop(57);
                page.MarginBottom(42);

                page.Header().Component(new Header(_config, _printData, _caption));
                page.Content().Element(ComposeContent);
                page.Footer().Component(new Footer());
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(0).Column(column =>
            {
                foreach (var entry in _listOfFlags)
                {
                    if (entry.Equals("Signs") || entry.Equals(Translate.GetString("Signs")))
                    {
                        column.Item().ShowEntire().Component(new Signs(_config, _printData, _printPhysicianSign, _printPhysicistSign));
                    }
                    else if (entry.Equals("PageBreak"))
                    {
                        column.Item().PageBreak();
                    }
                    else if (_listOfPrintComponents.TryGetValue(entry, out var componentType))
                    {
                        column.Item().Height(15);
                        var component = Activator.CreateInstance(componentType, _config, _printData) as PrintComponent;
                        if (component != null)
                            column.Item().Component(component);
                    }
                }
            });
        }

        public DocumentMetadata GetMetadata()
        {
            var result = DocumentMetadata.Default;

            result.Author = _printData.Physicist?.Name ?? string.Empty;
            result.Subject = Translate.GetString("PDFSubjectPlan");
            result.Title = string.Format(
                Translate.GetString("PDFTitle"),
                _printData.Patient.MedicalId?.Trim() ?? string.Empty,
                _printData.Patient.PatientName());
            result.Creator = Translate.GetString("PDFCreator");
            result.Producer = Translate.GetString("PDFProducer");

            return result;
        }

        public DocumentSettings GetSettings()
        {
            var result = DocumentSettings.Default;

            result.PdfA = true;

            return result;
        }
    }
}
