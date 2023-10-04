using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ZapReport.Extensions
{
    public static class IContainerExtensions
    {
        public static IContainer PageBreak(this IContainer container, ref bool pageBreak)
        {
            if (pageBreak)
            {
                container.PageBreak();
                pageBreak = false;
            }

            return container;
        }
    }
}
