namespace ZapReport.Extensions
{
    public static class StringExtensions
    {
        public static string FromUnicode(this string s)
        {
            return System.Text.RegularExpressions.Regex.Replace(s, @"\\u(....)", match => ((char)int.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber)).ToString());
        }
    }
}
