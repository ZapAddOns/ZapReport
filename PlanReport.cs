using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using ZapReport.Components;
using ZapReport.Extensions;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport
{
    public class PlanReport : IDocument
    {
        private PlanConfig _config;
        private PrintData _printData;
        private List<string> _listOfFlags;
        private Dictionary<string, Type> _listOfPrintComponents;
        private string _caption;
        private bool _printPhysicianSign;
        private bool _printPhysicistSign;

        public PlanReport(PlanConfig config, PrintData printData, Dictionary<string, Type> listOfPrintComponents, List<string> listOfFlags, string caption, bool? printPhysicianSign, bool? printPhysicistSign)
        {
            _config = config;
            _printData = printData;
            _listOfFlags = listOfFlags;
            _listOfPrintComponents = listOfPrintComponents;
            _caption = caption;
            _printPhysicianSign = (bool)printPhysicianSign;
            _printPhysicistSign = (bool)printPhysicistSign;
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
                var firstEntry = _listOfFlags.First();

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
                    else if (_listOfPrintComponents.ContainsKey(entry)) 
                    {
                        column.Item().Height(15);
                        column.Item().Component((PrintComponent)Activator.CreateInstance(_listOfPrintComponents[entry], new object[] { _config, _printData }));
                    }
                }
            });
        }

        public DocumentMetadata GetMetadata()
        {
            var result = DocumentMetadata.Default;

            result.Author = _printData.Physicist?.Name ?? "";
            result.Subject = Translate.GetString("PDFSubjectPlan");
            result.Title = String.Format(Translate.GetString("PDFTitle"), _printData.Patient.MedicalId.Trim(), _printData.Patient.PatientName());
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
