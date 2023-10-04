using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using ZapReport.Helpers;
using ZapTranslation;

namespace ZapReport.Components
{
    public class Footer : IComponent
    {
        public Footer()
        {
        }

        public void Compose(IContainer container)
        {
            container
                .PaddingTop(15)
                .Row(row =>
                {
                    row
                        .RelativeItem()
                        .AlignLeft()
                        .Text($"{DateTime.Now} / {Translate.GetString("Version")} {System.Reflection.Assembly.GetEntryAssembly().GetName().Version}")
                        .Style(Style.Footer);
                    row.RelativeItem()
                        .AlignRight()
                        .Text(text =>
                        {
                            text.CurrentPageNumber().Style(Style.Footer);
                            text.Span("/").Style(Style.Footer);
                            text.TotalPages().Style(Style.Footer);
                        });
                });
        }
    }
}
