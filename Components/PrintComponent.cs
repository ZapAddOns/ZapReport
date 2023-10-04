using QuestPDF.Infrastructure;

namespace ZapReport.Components
{
    public abstract class PrintComponent : IComponent
    {
        public static string ComponentName;

        public static string ComponentCaption;

        public abstract void Compose(IContainer container);
    }
}
