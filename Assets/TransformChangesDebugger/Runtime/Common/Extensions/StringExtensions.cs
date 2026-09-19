namespace ImmersiveVRTools.Runtime.Common.Extensions
{
    public static class StringExtensions
    {
        public static string TrimExcess(this string str, int maxLenght, string postfixToIfTrimmed = "...")
        {
            var trimmedValueToReturn = TryTrimExcess(str, maxLenght, out var trimmedValue, postfixToIfTrimmed) ? trimmedValue : str;
            return trimmedValueToReturn;
        }

        public static bool TryTrimExcess(this string str, int maxLenght, out string trimmedValue, string postfixToIfTrimmed = "...")
        {
            var needsTrimming = str.Length > maxLenght;
            trimmedValue = needsTrimming ? str.Substring(0, maxLenght - postfixToIfTrimmed.Length) + postfixToIfTrimmed : str;

            return needsTrimming;
        }
    }
}