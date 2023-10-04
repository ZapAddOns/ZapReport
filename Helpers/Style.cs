using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ZapReport.Helpers
{
    public static class Style
    {
        public static float BorderSize = 0.5f;
        public static int MinHeight = 18;

        public static TextStyle Default = new TextStyle().FontFamily(Fonts.Arial).FontSize(11);
        public static TextStyle Bold = Default.Bold();
        public static TextStyle Title = Default.FontSize(16).Bold();
        public static TextStyle Footer = Default.FontSize(9);

        public static IContainer TableHeaderCenter(IContainer container)
        {
            return container
                .DefaultTextStyle(Bold)
                .Border(BorderSize)
                .Background(Colors.Grey.Lighten4)
                .MinHeight(MinHeight)
                .AlignCenter()
                .AlignMiddle();
        }

        public static IContainer TableHeaderCenterNoBorder(IContainer container)
        {
            return container
                .DefaultTextStyle(Bold)
                .Background(Colors.Grey.Lighten4)
                .BorderVertical(BorderSize)
                .MinHeight(MinHeight)
                .AlignCenter()
                .AlignMiddle();
        }

        public static IContainer TableHeaderLeft(IContainer container)
        {
            return container
                .DefaultTextStyle(Bold)
                .Background(Colors.Grey.Lighten4)
                .Border(BorderSize)
                .MinHeight(MinHeight)
                .AlignLeft()
                .AlignMiddle()
                .PaddingLeft(5);
        }

        public static IContainer TableContentCenter(IContainer container)
        {
            return container
                .DefaultTextStyle(Default)
                .Border(BorderSize)
                .MinHeight(MinHeight)
                .AlignCenter()
                .AlignMiddle();
        }

        public static IContainer TableContentRight(IContainer container)
        {
            return container
                .DefaultTextStyle(Default)
                .Border(BorderSize)
                .MinHeight(MinHeight)
                .AlignRight()
                .AlignMiddle()
                .PaddingRight(5);
        }

        public static IContainer TableContentLeft(IContainer container)
        {
            return container
                .DefaultTextStyle(Default)
                .Border(BorderSize)
                .MinHeight(MinHeight)
                .AlignLeft()
                .AlignMiddle()
                .PaddingLeft(5);
        }
    }
}
