namespace ZapReport.Extensions
{
    public static class IntExtension
    {
        public static string ToColorString(this int[] array)
        {
            return $"#{array[1].ToString("X")}{array[2].ToString("X")}{array[3].ToString("X")}";
        }

        public static string ToColorString(this byte[] array)
        {
            // Convert a ARGB array to a hex color without A
            return $"#{array[1].ToString("X2")}{array[2].ToString("X2")}{array[3].ToString("X2")}";
        }
    }
}
